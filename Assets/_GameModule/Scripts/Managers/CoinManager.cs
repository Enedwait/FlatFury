using System.Collections.Generic;
using FlatFury.GameModule.Scripts.Players;
using FlatFury.MainModule.Scripts.Helpers;
using Unity.Netcode;
using UnityEngine;

namespace FlatFury.GameModule.Scripts
{
    /// <summary>
    /// The <see cref="CoinManager"/> class.
    /// This class represents the coin manager which is responsible for spawning and processing the coins.
    /// </summary>
    internal sealed class CoinManager : NetworkBehaviour
    {
        #region Singleton

        /// <summary> Gets the singleton instance of the <see cref="CoinManager"/> class. </summary>
        public static CoinManager Instance { get; private set; }

        #endregion

        #region Variables

        [SerializeField] private Transform _coinRoot;
        [SerializeField] private Bounds _spawnBounds;
        [SerializeField] private Coin _coinPrefab;
        [SerializeField] private int _coinCount = 7;
        
        private List<Coin> _spawnedCoins = new List<Coin>();

        #endregion

        #region Properties

        /// <summary> Gets the coin root. </summary>
        public Transform CoinRoot => _coinRoot;

        /// <summary> Gets the list of existing coins. </summary>
        public List<Coin> Coins => _spawnedCoins;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (Instance) Destroy(this);
            else Instance = this;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initiates multiple coin spawn process.
        /// </summary>
        private void Initiate()
        {
            if (!IsServer)
                return;

            for (int i = 0; i < _coinCount; i++)
                SpawnCoin();
        }

        /// <summary>
        /// Spawns the new coin
        /// </summary>
        public void SpawnCoin()
        {
            if (!IsServer)
                return;

            int step = 0;
            int maxStep = 100;

            Vector3 origin = Vector3.zero;
            // seek for a spawn origin
            while (step++ <= maxStep)
            {
                bool isValid = true;
                origin = _spawnBounds.GetRandomPointInBounds();
                
                // avoid spawn inside or near players
                foreach (var player in LevelManager.Instance.Players)
                    if (Vector3.Distance(player.Value.transform.position, origin) < player.Value.UnitSO.NonSpawnRadius.MaxRange(_coinPrefab.CoinSO.NonSpawnRadius))
                    {
                        isValid = false;
                        break;
                    }

                // avoid spawn inside or near other coins
                foreach (var spawnedCoin in _spawnedCoins)
                    if (Vector3.Distance(spawnedCoin.transform.position, origin) < spawnedCoin.CoinSO.NonSpawnRadius)
                    {
                        isValid = false;
                        break;
                    }

                if (isValid)
                    break;
            }
            
            Coin coin = Instantiate(_coinPrefab, origin, Quaternion.identity);
            _spawnedCoins.Add(coin);

            coin.NetworkObject.Spawn(true);
        }

        /// <summary>
        /// Processes the specified coin with the specified target.
        /// If the target is <see cref="Player"/> then the current coin should be collected and the new coin should be spawned.
        /// </summary>
        /// <remarks>Only server can process it in full.</remarks>
        /// <param name="coin">coin</param>
        /// <param name="target">target</param>
        public void Process(Coin coin, Transform target)
        {
            Player player = target.GetComponent<Player>();
            if (player)
            {
                // check if it's not the server
                if (!IsServer) // then we have a "fake" coin which should be hidden to respect UX of the current player.
                {
                    coin.Hide();
                    return;
                }

                player.PickUp(coin);
                _spawnedCoins.Remove(coin);
                Destroy(coin.gameObject);

                SpawnCoin();
            }
        }

        #endregion

        #region Events Handling

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            Initiate();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(_spawnBounds.center, _spawnBounds.size);
        }

        #endregion
    }
}
