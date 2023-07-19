using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using FlatFury.GameModule.Scripts.Players;
using FlatFury.GameModule.Scripts.UI;
using FlatFury.MainModule.Scripts.Controllers;
using FlatFury.MainModule.Scripts.Data;
using FlatFury.MainModule.Scripts.Helpers;
using Unity.Netcode;
using UnityEngine;

namespace FlatFury.GameModule.Scripts
{
    /// <summary>
    /// The <see cref="LevelManager"/> class.
    /// This class represents the level manager which is responsible for handling actual level gameplay.
    /// </summary>
    internal sealed class LevelManager : NetworkBehaviour
    {
        #region State Enum

        /// <summary> The <see cref="State"/> enumeration of actual game states. </summary>
        public enum State { Playing, GameOver, Disconnect }

        #endregion

        #region Singleton

        /// <summary> Gets the singleton instance of the <see cref="LevelManager"/> class. </summary>
        public static LevelManager Instance { get; private set; }

        #endregion

        #region Variables

        [SerializeField] private Camera _mainCamera;
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        [SerializeField] private Player _playerPrefab;
        [SerializeField] private Bounds _playerSpawnBounds;
        [SerializeField] private Transform _playerRoot;
        [SerializeField] private Transform _projectileRoot;

        #endregion

        #region Properties

        /// <summary> Gets the current game state. </summary>
        public State GameState { get; private set; }

        /// <summary> Gets the players of the game. </summary>
        public Dictionary<ulong, Player> Players { get; private set; }

        /// <summary> Gets the projectile root. </summary>
        public Transform ProjectileRoot => _projectileRoot;

        /// <summary> Gets the main camera. </summary>
        public Camera MainCamera => _mainCamera;

        /// <summary> Gets the virtual camera. </summary>
        public CinemachineVirtualCamera VirtualCamera => _virtualCamera;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            Instance = this;
            Players = new Dictionary<ulong, Player>();
            GameState = State.Playing;
        }

        private void Start()
        {
            Player.AnyCreated += OnPlayerCreated;
            Player.AnyKilled += OnPlayerKilled;

            GameMultiplayer.Instance.PlayerDataListChanged += OnPlayerDataListChanged;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(_playerSpawnBounds.center, _playerSpawnBounds.size);
        }

        private new void OnDestroy()
        {
            base.OnDestroy();

            if (Players != null)
            {
                Players.Clear();
                Players = null;
            }

            Player.AnyCreated -= OnPlayerCreated;
            Player.AnyKilled -= OnPlayerKilled;

            if (NetworkManager.Singleton)
            {
                if (NetworkManager.SceneManager != null)
                    NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
            }

            if (GameMultiplayer.Instance)
            {
                GameMultiplayer.Instance.LateClientConnected -= OnLateClientConnected;
                GameMultiplayer.Instance.DisconnectedFromHost -= OnDisconnectedFromHost;
                GameMultiplayer.Instance.PlayerDataListChanged -= OnPlayerDataListChanged;
            }
        }

        #endregion

        #region Validate Players

        /// <summary>
        /// Validates all the players with their respectful player data.
        /// Updates the players in the end.
        /// </summary>
        public void ValidatePlayers()
        {
            foreach (var player in Players.ToList())
            {
                if (!GameMultiplayer.Instance.IsPlayerConnectedByClientId(player.Key))
                {
                    Players.Remove(player.Key);
                    continue;
                }

                player.Value?.UpdatePlayerData(GameMultiplayer.Instance.GetPlayerDataByClientId(player.Value.OwnerClientId));
            }
        }

        /// <summary>
        /// Validates the specified player with his respectful player data.
        /// Updates the player in the end.
        /// </summary>
        /// <param name="player">player</param>
        public void ValidatePlayer(Player player)
        {
            PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataByClientId(player.OwnerClientId);
            player.UpdatePlayerData(playerData);
        }

        #endregion

        #region Spawn Player

        /// <summary>
        /// Spawns a player with the specified client id.
        /// </summary>
        /// <param name="clientId">client id</param>
        private void SpawnPlayer(ulong clientId)
        {
            // check if the player not added already
            if (Players == null ||  Players.ContainsKey(clientId))
                return;

            Player player = Instantiate(_playerPrefab);
            Players.Add(clientId, player);

            // find location to spawn
            Vector2 spawnPoint = GetAvailableSpawnPosition(player);
            player.transform.position = spawnPoint;

            player.NetworkObject.SpawnAsPlayerObject(clientId, true);
        }

        /// <summary>
        /// Gets the available position to spawn a player.
        /// </summary>
        /// <param name="newPlayer">player</param>
        /// <returns>the available position</returns>
        private Vector2 GetAvailableSpawnPosition(Player newPlayer)
        {
            int maxStep = 100;
            int step = 0;

            Vector2 spawnPoint = Vector2.zero;
            while (step++ <= maxStep)
            {
                spawnPoint = _playerSpawnBounds.GetRandomPointInBounds();
                bool isValid = true;

                // avoid spawn inside or near other players
                foreach (var player in Players.ToList())
                {
                    if (!player.Value)
                    {
                        Players.Remove(player.Key);
                        continue;
                    }
                    
                    if (Vector2.Distance(spawnPoint, (Vector2)player.Value.transform.position) < player.Value.UnitSO.NonSpawnRadius.MaxRange(newPlayer.UnitSO.NonSpawnRadius))
                    {
                        isValid = false;
                        break;
                    }
                }

                if (CoinManager.Instance && CoinManager.Instance.Coins != null && CoinManager.Instance.Coins.Count > 0)
                {
                    // avoid spawn inside or near coins
                    foreach (var coin in CoinManager.Instance.Coins)
                        if (Vector2.Distance(spawnPoint, coin.transform.position) < coin.CoinSO.NonSpawnRadius.MaxRange(newPlayer.UnitSO.NonSpawnRadius))
                        {
                            isValid = false;
                            break;
                        }
                }

                if (isValid)
                    break;
            }

            return spawnPoint;
        }

        #endregion

        #region End Game

        /// <summary>
        /// Initiates end game for all the clients with the specified winner id.
        /// </summary>
        /// <param name="winnerId">winner id</param>
        /// <param name="clientRpcParams">parameters</param>
        [ClientRpc]
        private void EndGameClientRpc(ulong winnerId, ClientRpcParams clientRpcParams = default)
        {
            EndGame(winnerId);
        }

        /// <summary>
        /// Ends the game with the specified winner.
        /// </summary>
        /// <param name="winnerId"></param>
        private void EndGame(ulong winnerId)
        {
            GameState = State.GameOver;

            GameMultiplayer.Instance.BeginEndGame();
            PauseController.Pause();

            PlayerData winner = GameMultiplayer.Instance.GetPlayerDataByClientId(winnerId);
            GameUI.Instance.ShowGameOver(winner);
        }

        /// <summary>
        /// Ends the game if the client has been disconnected from host (client-side).
        /// </summary>
        private void EndGameIfDisconnected()
        {
            if (GameState == State.GameOver)
                return;

            GameState = State.Disconnect;
            GameMultiplayer.Instance.BeginEndGame();
            PauseController.Pause();
            
            GameUI.Instance.ShowDisconnect();
        }

        #endregion

        #region Events Handling

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
                GameMultiplayer.Instance.LateClientConnected += OnLateClientConnected;
            }
            else
            {
                GameMultiplayer.Instance.DisconnectedFromHost += OnDisconnectedFromHost;
            }
        }

        /// <summary>
        /// Handles the 'LateClientConnected' event when new player arrives after game started.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="clientId">client id</param>
        private void OnLateClientConnected(object sender, ulong clientId) => SpawnPlayer(clientId);

        /// <summary>
        /// Handles the 'DisconnectedFromHost' event when new player disconnects from host (occurs on client-side).
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void OnDisconnectedFromHost(object sender, EventArgs e) => EndGameIfDisconnected();

        private void OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            foreach (ulong clientId in clientsCompleted)
                SpawnPlayer(clientId);
        }

        /// <summary>
        /// Handles the 'PlayerCreated' event if any player created.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void OnPlayerCreated(object sender, System.EventArgs e)
        {
            if (sender is Player newPlayer)
            {
                // check if it's the server
                if (IsServer) // then change player parent
                {
                    newPlayer.transform.parent = _playerRoot;
                }
                
                // check if the player is owner
                if (newPlayer.IsOwner) // then assign camera and UI
                {
                    GameUI.Instance.Set(newPlayer);
                    VirtualCamera.Follow = newPlayer.transform;
                }
            }
        }

        /// <summary>
        /// Handles the 'PlayerKilled' event if any player killed.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void OnPlayerKilled(object sender, System.EventArgs e)
        {
            if (!IsServer)
                return;

            if (sender is Player player)
            {
                // remove player from the list of active players
                Players.Remove(player.PlayerData.clientId);
            }

            // check if there is only last man standing 
            if (Players.Count == 1) // then end game
            {
                if (GameState == State.Disconnect)
                    return;

                GameState = State.GameOver;
                GameMultiplayer.Instance.BeginEndGame();
                ulong winnerId = Players.FirstOrDefault().Value.PlayerData.clientId;
                EndGameClientRpc(winnerId);
            }
        }

        /// <summary>
        /// Handles the 'PlayerDataListChanged' event.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void OnPlayerDataListChanged(object sender, System.EventArgs e) => ValidatePlayers();

        #endregion
    }
}
