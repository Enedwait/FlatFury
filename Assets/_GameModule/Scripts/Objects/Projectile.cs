using FlatFury.GameModule.Scripts.Players;
using FlatFury.GameModule.SOs.Projectiles;
using FlatFury.MainModule.Scripts.Controllers;
using FlatFury.MainModule.Scripts.Data;
using Unity.Netcode;
using UnityEngine;

namespace FlatFury.GameModule.Scripts
{
    /// <summary>
    /// The <see cref="Projectile"/> class.
    /// This class represent the projectile.
    /// </summary>
    internal sealed class Projectile : NetworkBehaviour
    {
        #region Variables

        [SerializeField] private ProjectileSO _projectileSo;
        [SerializeField] private SpriteRenderer _model;
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private Player _player;

        private bool _isFake = false;

        #endregion

        #region Properties

        /// <summary> Gets the projectile data. </summary>
        public ProjectileSO ProjectileSO => _projectileSo;

        /// <summary> Gets the actual client id (owner id) of the projectile. </summary>
        public ulong ClientId { get; set; }

        #endregion

        #region Unity Methods

        private void Start()
        {
            PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataByClientId(ClientId);
            _model.color = GameMultiplayer.Instance.GetPlayerColor(playerData.colorId);
            
            if (!_isFake)
                Hide();

            _rigidbody2D.velocity = transform.right * _projectileSo.Speed;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Hides the projectile.
        /// </summary>
        public void Hide()
        {
            if (_model)
                _model.enabled = false;
        }

        /// <summary>
        /// Spawns a fake local projectile.
        /// </summary>
        /// <param name="clientId">client id</param>
        /// <param name="prefab">projectile prefab</param>
        /// <param name="position">position</param>
        /// <param name="rotation">rotation</param>
        /// <param name="parent">parent</param>
        public static void SpawnFake(ulong clientId, Projectile prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            Projectile projectile = Instantiate(prefab, position, rotation, parent);
            projectile._isFake = true;
            projectile.ClientId = clientId;
        }

        #endregion

        #region Events Handling

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.attachedRigidbody)
            {
                Player player = collider.attachedRigidbody.GetComponent<Player>();
                if (player)
                {
                    if (player.PlayerData.clientId == ClientId)
                        return;

                    // check if it's not a fake
                    if (!_isFake) // then it's a REAL PROJECTILE! BAM!
                        player.Damage(_projectileSo.Damage);
                }
            }
            
            Destroy(gameObject);
        }

        #endregion
    }
}
