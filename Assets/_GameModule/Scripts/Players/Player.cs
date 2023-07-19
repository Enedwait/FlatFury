using System;
using FlatFury.GameModule.Scripts.Input;
using FlatFury.GameModule.SOs.Units;
using FlatFury.MainModule.Scripts.Controllers;
using FlatFury.MainModule.Scripts.Data;
using FlatFury.MainModule.Scripts.Helpers;
using Unity.Netcode;
using UnityEngine;

namespace FlatFury.GameModule.Scripts.Players
{
    /// <summary>
    /// The <see cref="Player"/> class.
    /// This class represents both the player and the unit under player control.
    /// </summary>
    internal sealed class Player : NetworkBehaviour
    {
        #region Variables

        private NetworkVariable<float> _playerHealth;
        private NetworkVariable<long> _score;
        
        [SerializeField] private UnitSO _unitSO;
        [SerializeField] private SpriteRenderer _model;
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private Transform _projectileSpawnPoint;
        [SerializeField] private Projectile _projectilePrefab;
        [SerializeField] private PlayerVisual _playerVisual;

        private Vector2 _movement = Vector2.zero;
        private PlayerData _playerData;
        private float _fireCooldownElapsed = 0f;
        private float _fireCooldown = 0f;
        private Collider2D[] _colliders;

        #endregion

        #region Properties

        /// <summary> Gets the unit data. </summary>
        public UnitSO UnitSO => _unitSO;
        
        /// <summary> Gets the player data. </summary>
        public PlayerData PlayerData => _playerData;

        /// <summary> Gets the player name. </summary>
        public string Name => _playerData.PlayerName;

        /// <summary> Indicates whether player can fire or not at the current time. </summary>
        public bool CanFire { get; private set; }

        /// <summary> Gets the player health. </summary>
        public float Health 
        {
            get => _playerHealth.Value;
            private set { if (IsServer) _playerHealth.Value = value; OnUpdated(); }
        }

        /// <summary> Gets the player max health. </summary>
        public float MaxHealth => _unitSO.Health;

        /// <summary> Gets the player score. </summary>
        public long Score
        {
            get => _score.Value;
            private set { if (IsServer) _score.Value = value; OnUpdated(); }
        }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _playerHealth = new NetworkVariable<float>(MaxHealth, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
            _score = new NetworkVariable<long>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        }

        private void Start()
        {
            _playerVisual.Set(this);

            _fireCooldown = 1f / _projectilePrefab.ProjectileSO.FireRate;
            _fireCooldownElapsed = 0f;

            CanFire = true;
            Health = MaxHealth;

            UpdatePlayerData();

            OnCreated();
            OnUpdated();
        }

        private void Update()
        {
            if (!IsOwner)
                return;

            HandleAttack();
        }

        private void LateUpdate()
        {
            ValidatePlayer();
        }
        private void FixedUpdate()
        {
            if (!IsOwner)
                return;

            HandleMovement();
            HandleRotation();
        }

        #endregion

        #region Validate Player

        private int _counter = 0;
        private int _counterMax = 15;

        /// <summary>
        /// Validates the player data by call counter.
        /// </summary>
        private void ValidatePlayer()
        {
            if (_counter == 0)
                if (string.IsNullOrWhiteSpace(Name) || _model.color == new Color(0, 0, 0, 0))
                    ValidatePlayerServerRpc();

            _counter++;
            if (_counter >= _counterMax)
                _counter = 0;
        }

        /// <summary>
        /// Requests the player data validation on server.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void ValidatePlayerServerRpc() => ValidatePlayerClientRpc();

        /// <summary>
        /// Requests the player data validation across all the clients
        /// </summary>
        [ClientRpc]
        private void ValidatePlayerClientRpc() => LevelManager.Instance.ValidatePlayer(this);

        /// <summary>
        /// Updates the player data.
        /// </summary>
        public void UpdatePlayerData()
        {
            _playerData = GameMultiplayer.Instance.GetPlayerDataByClientId(OwnerClientId);
            _playerData.Color = GameMultiplayer.Instance.GetPlayerColor(_playerData.colorId);

            if (_model)
                _model.color = _playerData.Color;
        }

        /// <summary>
        /// Updates the player data with the specified player data.
        /// </summary>
        /// <param name="playerData">player data</param>
        public void UpdatePlayerData(PlayerData playerData)
        {
            _playerData = playerData;
            _playerData.Color = GameMultiplayer.Instance.GetPlayerColor(_playerData.colorId);
            _model.color = _playerData.Color;
            OnUpdated();
        }

        #endregion

        #region Movement

        /// <summary>
        /// Handles the player's rotation.
        /// </summary>
        private void HandleRotation()
        {
            if (_movement == Vector2.zero)
                return;

            Quaternion rotation = Quaternion.Euler(0,0, _rigidbody2D.rotation);
            Quaternion angle = Quaternion.Euler(0, 0, MathHelper.Atan2(_movement));

            if (rotation != angle)
                _rigidbody2D.rotation = Quaternion.LerpUnclamped(rotation, angle, _unitSO.RotateSpeed * Time.deltaTime).eulerAngles.z;
        }

        /// <summary>
        /// Handles the player's movement.
        /// </summary>
        private void HandleMovement()
        {
            if (GameInput.Instance.IsMouse)
            {
                Vector3 worldPoint = LevelManager.Instance.MainCamera.ScreenToWorldPoint(new Vector3(GameInput.Instance.CursorPosition.x, GameInput.Instance.CursorPosition.y, LevelManager.Instance.MainCamera.transform.position.z), Camera.MonoOrStereoscopicEye.Mono);
                Debug.Log($"{worldPoint}");
                worldPoint.z = 0;
                _movement = (worldPoint - transform.position).normalized;
            }
            else
                _movement = GameInput.Instance.GetMovementNormalized();

            if (_movement != Vector2.zero)
                _rigidbody2D.AddForce(_movement * _unitSO.MoveSpeed * Time.deltaTime, ForceMode2D.Force);
        }

        #endregion

        #region Attack

        /// <summary>
        /// Handles the player's attack.
        /// </summary>
        private void HandleAttack()
        {
            FireCooldown();

            if (!GameInput.Instance.IsFiring)
                return;

            if (CanFire) 
                Fire();
        }

        /// <summary>
        /// Opens fire.
        /// </summary>
        private void Fire()
        {
            _fireCooldownElapsed = 0f;
            CanFire = false;

            // open fake fire on owner locally
            Projectile.SpawnFake(PlayerData.clientId, _projectilePrefab, _projectileSpawnPoint.position, _projectileSpawnPoint.rotation, LevelManager.Instance.ProjectileRoot);

            // open real fire on server
            FireServerRpc(_projectileSpawnPoint.position, _projectileSpawnPoint.rotation);
        }

        /// <summary>
        /// Requests server to open real fire!
        /// </summary>
        /// <param name="position">position</param>
        /// <param name="rotation">rotation</param>
        /// <param name="serverRpcParams">parameters</param>
        [ServerRpc(RequireOwnership = false)]
        private void FireServerRpc(Vector3 position, Quaternion rotation, ServerRpcParams serverRpcParams = default)
        {
            Projectile projectile = Instantiate(_projectilePrefab, position, rotation, LevelManager.Instance.ProjectileRoot);
            projectile.ClientId = serverRpcParams.Receive.SenderClientId;

            // open fake fire on all the clients locally except initial caller
            FakeFireClientRpc(position, rotation, serverRpcParams.Receive.SenderClientId);
        }

        /// <summary>
        /// Requests clients to open fake fire.
        /// </summary>
        /// <param name="position">position</param>
        /// <param name="rotation">rotation</param>
        /// <param name="excludeClientId">initial client id</param>
        /// <param name="clientRpcParams">parameters</param>
        [ClientRpc]
        private void FakeFireClientRpc(Vector3 position, Quaternion rotation, ulong excludeClientId, ClientRpcParams clientRpcParams = default)
        {
            if (IsOwner && OwnerClientId == excludeClientId)
                return;

            Projectile.SpawnFake(PlayerData.clientId, _projectilePrefab, position, rotation, LevelManager.Instance.ProjectileRoot);
        }

        /// <summary>
        /// Processes open fire cooldown.
        /// </summary>
        private void FireCooldown()
        {
            if (CanFire)
                return;

            if (_fireCooldownElapsed < _fireCooldown)
            {
                _fireCooldownElapsed += Time.deltaTime;
                CanFire = false;
            }
            else
            {
                CanFire = true;
            }
        }

        #endregion

        #region Other

        /// <summary>
        /// Takes the specified amount of damage.
        /// </summary>
        /// <param name="damage">damage</param>
        public void Damage(float damage)
        {
            Health -= damage;

            if (Health <= 0f)
                Kill();
        }

        /// <summary>
        /// Kills the player.
        /// </summary>
        private void Kill()
        {
            Destroy(gameObject);
            OnKilled();
        }

        /// <summary>
        /// Picks up the specified coin.
        /// </summary>
        /// <param name="coin">coin</param>
        public void PickUp(Coin coin)
        {
            Score += coin.CoinSO.Value;
            GameMultiplayer.Instance.UpdatePlayerScore(OwnerClientId, Score);
        }

        #endregion

        #region Events Handling

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            _playerHealth.OnValueChanged += (value, newValue) => { Health = newValue; };
            _score.OnValueChanged += (value, newValue) => { Score = newValue; };
        }

        /// <summary>
        /// Raises the 'Created' event when the player is created.
        /// </summary>
        private void OnCreated() => AnyCreated?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises the 'Updated' event when the player is updated.
        /// </summary>
        private void OnUpdated() => Updated?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises the 'Killed' event when the player is killed.
        /// </summary>
        private void OnKilled() => AnyKilled?.Invoke(this, EventArgs.Empty);

        #endregion

        #region Events

        /// <summary> Occurs when the specific player is updated. </summary>
        public event EventHandler Updated;

        /// <summary> Occurs when any player is created. </summary>
        public static event EventHandler AnyCreated;

        /// <summary> Occurs when any player is killed. </summary>
        public static event EventHandler AnyKilled;

        #endregion
    }
}
