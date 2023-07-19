using System;
using System.Collections.Generic;
using FlatFury.LobbyModule.Scripts.Managers;
using FlatFury.MainModule.Scripts.Data;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FlatFury.MainModule.Scripts.Controllers
{
    /// <summary>
    /// The <see cref="GameMultiplayer"/> class.
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class GameMultiplayer : NetworkBehaviour
    {
        #region Singleton

        /// <summary> Gets the singleton instance of the <see cref="GameMultiplayer"/> class. </summary>
        public static GameMultiplayer Instance { get; private set; }

        #endregion

        #region Consts

        /// <summary> The maximum amount of players per game session. </summary>
        public const int MAX_PLAYERS = 4;

        #endregion

        #region Variables

        [SerializeField] private List<Color> _playerColors = new List<Color>();
        [SerializeField, Range(1, MAX_PLAYERS)] private int _minCountOfPlayersToBegin = 2;

        private NetworkList<PlayerData> _playerDataList;

        #endregion

        #region Properties

        /// <summary> Gets the current player name. </summary>
        public string PlayerName { get; private set; }

        /// <summary> Indicates whether the game is started or not. </summary>
        public bool IsGameStarted { get; private set; }

        /// <summary> Indicates whether the game is ended or not. </summary>
        public bool IsGameEnded { get; private set; }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            PlayerName = PlayerPrefs.GetString(PlayerPrefsConsts.PlayerName, $"PlayerName{Random.Range(0, 100)}");

            _playerDataList = new NetworkList<PlayerData>();
            _playerDataList.OnListChanged += OnPlayerDataListChanged;
        }

        #endregion

        #region Player Data

        /// <summary>
        /// Saves the player name in system.
        /// </summary>
        /// <param name="playerName">player name</param>
        public void SetPlayerName(string playerName)
        {
            if (!string.IsNullOrWhiteSpace(playerName))
            {
                PlayerName = playerName;
                PlayerPrefs.SetString(PlayerPrefsConsts.PlayerName, PlayerName);
            }
        }

        /// <summary>
        /// Gets the player id if authenticated or player name otherwise.
        /// </summary>
        /// <returns>player id or player name.</returns>
        public string GetPlayerId()
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
                if (AuthenticationService.Instance == null && !AuthenticationService.Instance.IsAuthorized && !string.IsNullOrWhiteSpace(AuthenticationService.Instance.PlayerId))
                    return AuthenticationService.Instance.PlayerId;

            return PlayerName;
        }

        /// <summary>
        /// Checks whether the specified player is connected already.
        /// </summary>
        /// <param name="clientId">client id</param>
        /// <returns><value>True</value> if the client is connected; otherwise <value>False</value>.</returns>
        public bool IsPlayerConnectedByClientId(ulong clientId)
        {
            foreach (var playerData in _playerDataList)
                if (playerData.clientId == clientId)
                    return true;

            return false;
        }

        /// <summary>
        /// Gets the index of the player data by client's id.
        /// </summary>
        /// <param name="clientId">client id</param>
        /// <returns>the index of the player data.</returns>
        public int GetPlayerDataIndexByClientId(ulong clientId)
        {
            for(int i = 0; i < _playerDataList.Count; i++)
                if (_playerDataList[i].clientId == clientId)
                    return i;

            return -1;
        }

        /// <summary>
        /// Gets the player data by client's id.
        /// </summary>
        /// <param name="clientId">client id</param>
        /// <returns>the player data.</returns>
        public PlayerData GetPlayerDataByClientId(ulong clientId)
        {
            foreach (PlayerData playerData in _playerDataList)
                if (playerData.clientId == clientId)
                    return playerData;

            return default;
        }

        /// <summary>
        /// Checks whether the specified color (by it's id) is available for use or not.
        /// </summary>
        /// <param name="colorId">color id</param>
        /// <returns><value>True</value> if the color available; otherwise <value>False</value>.</returns>
        public bool IsColorAvailable(int colorId)
        {
            foreach (var playerData in _playerDataList)
                if (playerData.colorId == colorId)
                    return false;

            return true;
        }

        /// <summary>
        /// Gets the player color by color id.
        /// </summary>
        /// <param name="colorId">color id</param>
        /// <returns>the player color.</returns>
        public Color GetPlayerColor(int colorId) => _playerColors[colorId];

        public Color GetPlayerColorByClientId(ulong clientId)
        {
            foreach (PlayerData playerData in _playerDataList)
                if (playerData.clientId == clientId)
                    return GetPlayerColor(playerData.colorId);

            return _playerColors[0];
        }

        /// <summary>
        /// Acquires the first available color id to choose for.
        /// </summary>
        /// <returns>first available color id.</returns>
        public int GetAvailableColorId()
        {
            for (int i = 0; i < _playerColors.Count; i++)
            {
                bool found = false;
                foreach (var playerData in _playerDataList)
                    if (playerData.colorId == i)
                    {
                        found = true;
                        break;
                    }

                if (!found)
                    return i;
            }

            return 0;
        }

        /// <summary>
        /// Acquires the random available color id to choose for.
        /// </summary>
        /// <returns>random available color id.</returns>
        public int GetRandomAvailableColorId()
        {
            int step = 0;
            int maxStep = _playerColors.Count * _playerColors.Count;

            while (step++ <= maxStep)
            {
                int colorId = Random.Range(0, _playerColors.Count);
                bool found = false;
                foreach (var playerData in _playerDataList)
                    if (playerData.colorId == colorId)
                    {
                        found = true;
                        break;
                    }

                if (!found)
                    return colorId;
            }

            return GetAvailableColorId();
        }

        /// <summary>
        /// Updates the player score.
        /// </summary>
        /// <param name="clientId">client id</param>
        /// <param name="score">score</param>
        public void UpdatePlayerScore(ulong clientId, long score)
        {
            if (!IsServer)
                return;

            int index = GetPlayerDataIndexByClientId(clientId);

            PlayerData playerData = _playerDataList[index];
            playerData.score = score;

            _playerDataList[index] = playerData;
        }

        #endregion

        #region Host

        /// <summary>
        /// Starts the host.
        /// </summary>
        public void StartHost()
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += OnConnectionApproval;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
            NetworkManager.Singleton.StartHost();
        }

        /// <summary>
        /// Handles the player's connection attempt.
        /// </summary>
        /// <param name="request">request</param>
        /// <param name="response">response</param>
        private void OnConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYERS)
            {
                response.Approved = false; // Max amount of players reached
                return;
            }
            
            if (IsGameEnded)
            {
                response.Approved = false; // Game ended
                return;
            }

            response.Approved = true;
        }

        /// <summary>
        /// Handles the player (client) connect event.
        /// </summary>
        /// <param name="clientId">client id</param>
        private void OnClientConnected(ulong clientId)
        {
            PlayerData playerData = new PlayerData { clientId = clientId, colorId = GetRandomAvailableColorId(), playerName = PlayerName };
            _playerDataList.Add(playerData);

            SetPlayerNameServerRpc(PlayerName);

            BeginGame(clientId);
        }

        /// <summary>
        /// Handles the player (client) disconnect event.
        /// </summary>
        /// <param name="clientId">client id</param>
        private void OnClientDisconnect(ulong clientId)
        {
            for (int i = 0; i < _playerDataList.Count; i++)
                if (_playerDataList[i].clientId == clientId)
                {
                    _playerDataList.RemoveAt(i);
                    break;
                }
        }

        #endregion

        #region Client-Side

        /// <summary>
        /// Starts the client.
        /// </summary>
        public void StartClient()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += Client_OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += Client_OnClientDisconnect;
            NetworkManager.Singleton.StartClient();
        }

        /// <summary>
        /// Handles the player (client) connect event from client side.
        /// </summary>
        /// <param name="clientId">client id</param>
        private void Client_OnClientConnected(ulong clientId)
        {
            SetPlayerNameServerRpc(PlayerName);
        }

        /// <summary>
        /// Handles the player (client) disconnect event from client-side.
        /// </summary>
        /// <param name="clientId">client id</param>
        private void Client_OnClientDisconnect(ulong clientId)
        {
            DisconnectedFromHost?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region ServerRPCs

        /// <summary>
        /// Sets the player name via server call.
        /// </summary>
        /// <param name="playerName">player name</param>
        /// <param name="serverRpcParams">parameters</param>
        [ServerRpc(RequireOwnership = false)]
        private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
        {
            int index = GetPlayerDataIndexByClientId(serverRpcParams.Receive.SenderClientId);

            PlayerData playerData = _playerDataList[index];
            playerData.playerName = playerName;

            _playerDataList[index] = playerData;
        }

        #endregion

        #region Game

        /// <summary>
        /// Begins the game actually (e.g. loads the game scene).
        /// </summary>
        public void BeginGame(ulong clientId)
        {
            // check if the game is started already
            if (IsGameStarted)
            {
                // then it's a late-joining client arrived
                LateClientConnected?.Invoke(this, clientId);
                return;
            }

            // check if it's not possible to start the game (e.g. minimum amount of players not reached)
            if (_playerDataList.Count < _minCountOfPlayersToBegin)
                return;

            // start the game
            IsGameStarted = true;
            GameStarted?.Invoke(this, EventArgs.Empty);
            SceneController.LoadNetwork(SceneController.Scene.Game);
        }

        public event EventHandler<ulong> LateClientConnected;

        /// <summary>
        /// Begins the end game sequence.
        /// </summary>
        public void BeginEndGame()
        {
            IsGameEnded = true;

            if (LobbyManager.Instance)
                LobbyManager.Instance.DeleteOrLeaveLobby();
        }

        /// <summary>
        /// Finalizes the game process (e.g. disconnects and returns back to lobby).
        /// </summary>
        public void EndGame()
        {
            IsGameStarted = false;
            IsGameEnded = false;

            if (LobbyManager.Instance)
                LobbyManager.Instance.DeleteOrLeaveLobby();

            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= Client_OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= Client_OnClientDisconnect;
                NetworkManager.Singleton.ConnectionApprovalCallback -= OnConnectionApproval;
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
                
                NetworkManager.Singleton.Shutdown();
            }

            PauseController.Resume();

            Instance = null;
            Destroy(gameObject);


            SceneController.ToLobby();
        }

        #endregion

        #region Events Handling

        /// <summary>
        /// Raises the 'PlayerDataListChanged' event of the player data list.
        /// </summary>
        /// <param name="changeEvent">change event</param>
        private void OnPlayerDataListChanged(NetworkListEvent<PlayerData> changeEvent) => PlayerDataListChanged?.Invoke(this, EventArgs.Empty);

        #endregion

        #region Events

        /// <summary> Occurs on the client-side when the client is disconnected from the host. </summary>
        public event EventHandler DisconnectedFromHost;

        /// <summary> Occurs when the player data list changed. </summary>
        public event EventHandler PlayerDataListChanged;

        /// <summary> Occurs when the game is started. </summary>
        public event EventHandler GameStarted;

        #endregion
    }
}
