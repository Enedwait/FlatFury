using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlatFury.MainModule.Scripts.Controllers;
using FlatFury.MainModule.Scripts.CustomEventArgs;
using FlatFury.MainModule.Scripts.Data;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FlatFury.LobbyModule.Scripts.Managers
{
    /// <summary>
    /// The <see cref="LobbyManager"/> class
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class LobbyManager : MonoBehaviour
    {
        #region Singleton

        /// <summary> Gets the singleton instance of the <see cref="LobbyManager"/> class. </summary>
        public static LobbyManager Instance { get; private set; }

        #endregion

        #region Consts

        /// <summary> Relay join code key </summary>
        private const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";

        #endregion

        #region Variables

        [SerializeField] private float _lobbyHeartbeatTimeout = 15f;
        [SerializeField] private float _lobbyUpdateTimeout = 1.1f;

        private float _heartbeatTimer = 0;
        private float _lobbyUpdateTimer = 0;
        private string _playerName;
        private long _playerNameNumMax = 10000;
        private string _relayJoinCode;

        #endregion

        #region Properties

        /// <summary> Gets the joined lobby. </summary>
        public Lobby Lobby { get; private set; }

        /// <summary> Gets the sign in flag. </summary>
        public bool IsSignedIn { get; private set; }

        /// <summary> Indicates if the current player is the lobby host. </summary>
        public bool IsHost => Lobby != null && Lobby.HostId == AuthenticationService.Instance.PlayerId;

        /// <summary> Indicates if the current player is the lobby client. </summary>
        public bool IsClient => Lobby != null && Lobby.HostId != AuthenticationService.Instance.PlayerId;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Authenticate();
        }

        private void Update()
        {
            HandleLobbyHeartbeat();
            HandleLobbyPollForUpdates();
        }

        #endregion

        #region Auth

        /// <summary>
        /// Authenticates the player.
        /// </summary>
        private async void Authenticate()
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
                return;

            InitializationOptions options = new InitializationOptions();
            options.SetProfile($"FlatWarPlayer{Random.Range(0, _playerNameNumMax)}"); // for testing on same PC

            await UnityServices.InitializeAsync(options);

            AuthenticationService.Instance.SignedIn += OnSignedIn;
            AuthenticationService.Instance.SignedOut += OnSignedOut;

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        #endregion

        #region Methods

        private int _step;
        private int _maxStep;

        /// <summary>
        /// Gets the current lobby progress.
        /// </summary>
        /// <returns>progress</returns>
        private float GetProgress() => (float)_step++ / (float)_maxStep;

        /// <summary>
        /// Gets the player.
        /// </summary>
        /// <returns>Player</returns>
        private Player GetPlayer()
        {
            if (string.IsNullOrWhiteSpace(_playerName))
                _playerName = AuthenticationService.Instance.Profile;

            return new Player()
            {
                Data = new Dictionary<string, PlayerDataObject>()
                {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName) }
                }
            };
        }

        #endregion

        #region Relay Methods

        /// <summary>
        /// Allocates Relay.
        /// </summary>
        /// <returns></returns>
        private async Task<Allocation> AllocateRelay()
        {
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(GameMultiplayer.MAX_PLAYERS - 1);

                return allocation;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return default;
            }
        }
        
        /// <summary>
        /// Gets the Relay join code.
        /// </summary>
        /// <param name="allocation">allocation.</param>
        /// <returns>relay join code</returns>
        private async Task<string> GetRelayJoinCode(Allocation allocation)
        {
            try
            {
                return await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return default;
            }
        }

        /// <summary>
        /// Joins the Relay by the specified join code.
        /// </summary>
        /// <param name="joinCode">join code</param>
        /// <returns>allocation</returns>
        private async Task<JoinAllocation> JoinRelay(string joinCode)
        {
            try
            {
                return await RelayService.Instance.JoinAllocationAsync(joinCode);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return default;
            }
        }

        #endregion

        #region Lobby

        /// <summary>
        /// Handles the lobby heartbeat.
        /// </summary>
        private void HandleLobbyHeartbeat()
        {
            if (!IsHost)
                return;

            _heartbeatTimer += Time.deltaTime;
            if (_heartbeatTimer > _lobbyHeartbeatTimeout)
            {
                _heartbeatTimer = 0;
                LobbyService.Instance.SendHeartbeatPingAsync(Lobby.Id);
            }
        }

        /// <summary>
        /// Polls the lobby for updates.
        /// </summary>
        private async void HandleLobbyPollForUpdates()
        {
            if (Lobby == null)
                return;

            _lobbyUpdateTimer += Time.deltaTime;
            if (_lobbyUpdateTimer > _lobbyUpdateTimeout)
            {
                _lobbyUpdateTimer = 0;
                Lobby = await LobbyService.Instance.GetLobbyAsync(Lobby.Id);
            }
        }

        /// <summary>
        /// Creates the lobby with the specified name and saves player name.
        /// </summary>
        /// <param name="lobbyName">lobby name</param>
        /// <param name="playerName">player name</param>
        public async void CreateLobby(string lobbyName, string playerName)
        {
            _playerName = playerName;
            _step = 0;
            _maxStep = 7;

            try
            {
                OnProgressChanged(GetProgress(), "Initialization...");
                SceneController.Load(SceneController.Scene.Loading);

                CreateLobbyOptions options = new CreateLobbyOptions()
                {
                    Player = GetPlayer()
                };

                OnProgressChanged(GetProgress(), "$Creating lobby...");
                Lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, GameMultiplayer.MAX_PLAYERS, options);
                Debug.Log($"Lobby \'{Lobby.Name}\' created.");
                LobbyCreated?.Invoke(this, EventArgs.Empty);

                PlayerPrefs.SetString(PlayerPrefsConsts.LastCreatedLobby, lobbyName);

                OnProgressChanged(GetProgress(), "Allocating relay...");
                Allocation allocation = await AllocateRelay();

                OnProgressChanged(GetProgress(), "Getting codes...");
                _relayJoinCode = await GetRelayJoinCode(allocation);

                OnProgressChanged(GetProgress(), "Updating data...");
                await LobbyService.Instance.UpdateLobbyAsync(Lobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject> { { KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, _relayJoinCode) } }
                });

                OnProgressChanged(GetProgress(), "Starting Host...");
                UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetRelayServerData(new RelayServerData(allocation, "dtls"));

                GameMultiplayer.Instance.StartHost();

                OnProgressChanged(GetProgress(), "Waiting for players...");
                //SceneController.LoadNetwork(SceneController.Scene.Game);
            }
            catch (LobbyServiceException ex)
            {
                Lobby = null;
                Debug.LogError(ex);
            }
        }

        /// <summary>
        /// (Quick) Joins the lobby with the specified name and saves player name.
        /// </summary>
        /// <param name="lobbyName">lobby name</param>
        /// <param name="playerName">player name</param>
        public async void JoinLobby(string lobbyName, string playerName)
        {
            _playerName = playerName;
            _step = 0;
            _maxStep = 4;

            try
            {
                OnProgressChanged(GetProgress(), "Initialization...");
                SceneController.Load(SceneController.Scene.Loading);

                QuickJoinLobbyOptions options = new QuickJoinLobbyOptions()
                {
                    Player = GetPlayer(),
                    Filter = new List<QueryFilter>()
                    {
                        new QueryFilter(QueryFilter.FieldOptions.Name, lobbyName, QueryFilter.OpOptions.EQ),
                        new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                    },
                };

                OnProgressChanged(GetProgress(), "Joining lobby...");
                Lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
                Debug.Log($"Joined \'{Lobby.Name}\' lobby.");
                LobbyJoined?.Invoke(this, EventArgs.Empty);

                PlayerPrefs.SetString(PlayerPrefsConsts.LastJoinedLobby, lobbyName);

                OnProgressChanged(GetProgress(), "Joining relay...");
                _relayJoinCode = Lobby.Data[KEY_RELAY_JOIN_CODE].Value;
                JoinAllocation allocation = await JoinRelay(_relayJoinCode);

                UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetRelayServerData(new RelayServerData(allocation, "dtls"));

                OnProgressChanged(GetProgress(), $"Connecting to host...");
                GameMultiplayer.Instance.StartClient();
            }
            catch (LobbyServiceException ex)
            {
                Lobby = null;
                Debug.LogError(ex);
            }
            catch (Exception ex)
            {
                Lobby = null;
                Debug.LogError(ex);
            }
        }

        /// <summary>
        /// Deletes or leaves the lobby. It depends on the caller's status. 
        /// </summary>
        public void DeleteOrLeaveLobby()
        {
            if (IsHost) DeleteLobby();
            else LeaveLobby();
        }

        /// <summary>
        /// Leaves the lobby.
        /// </summary>
        public async void LeaveLobby()
        {
            if (Lobby == null)
                return;

            try
            {
                await LobbyService.Instance.RemovePlayerAsync(Lobby.Id, AuthenticationService.Instance.PlayerId);
                Lobby = null;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        /// <summary>
        /// Deletes the lobby.
        /// </summary>
        public async void DeleteLobby()
        {
            if (!IsHost)
                return;

            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(Lobby.Id);
                Lobby = null;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        #endregion

        #region Events Handling

        /// <summary>
        /// Handles the signed in event.
        /// </summary>
        private void OnSignedIn()
        {
            Debug.Log($"Signed in : {AuthenticationService.Instance.PlayerId}.");
            IsSignedIn = true;
        }

        /// <summary>
        /// Handles the signed out event.
        /// </summary>
        private void OnSignedOut()
        {
            Debug.Log($"Signed out.");
            IsSignedIn = false;
        }

        private void OnDestroy()
        {
            DeleteOrLeaveLobby();
        }

        /// <summary>
        /// Raises the 'ProgressChanged' event.
        /// </summary>
        /// <param name="progress">progress</param>
        /// <param name="status">status</param>
        private void OnProgressChanged(float progress, string status) => ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(progress, status));

        #endregion

        #region Events

        /// <summary> Occurs when the lobby is created. </summary>
        public event EventHandler LobbyCreated;

        /// <summary> Occurs when the lobby is joined. </summary>
        public event EventHandler LobbyJoined;

        /// <summary> Occurs when the progress is changed </summary>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        #endregion
    }
}
