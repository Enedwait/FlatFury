using FlatFury.LobbyModule.Scripts.Managers;
using FlatFury.MainModule.Scripts.Controllers;
using FlatFury.MainModule.Scripts.CustomEventArgs;
using FlatFury.MainModule.Scripts.Helpers;
using FlatFury.MainModule.Scripts.UI;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace FlatFury.LoadingModule.Scripts.UI
{
    /// <summary>
    /// The <see cref="LoadingUI"/> class.
    /// This class represents an overall UI of the Loading scene.
    /// </summary>
    internal sealed class LoadingUI : MonoBehaviour
    {
        #region Variables

        [SerializeField] private ProgressBarUI _loadingBar;
        [SerializeField] private Image _loadingIcon;
        [SerializeField] private float _loadingIconMaxRotateSpeed = 1f;
        [SerializeField] private TextMeshProUGUI _loadingText;
        [SerializeField] private TextMeshProUGUI _lobbyNameText;
        [SerializeField] private TextMeshProUGUI _playerNameText;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private Button _toLobbyButton;

        [SerializeField] private float _loadingIconPulseSpeed = 0.001f;
        [SerializeField, Range(0f, 1f)] private float _loadingIconMinHue = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _loadingIconMaxHue = 0.7f;

        private bool _pulseAdd = true;

        #endregion

        #region Unity Methods

        private void Start()
        {
            _lobbyNameText.text = "";
            _toLobbyButton.onClick.AddListener(GameMultiplayer.Instance.EndGame);

            UpdateProgress(0f, "Initializing...");

            if (GameMultiplayer.Instance)
            {
                _playerNameText.text = $"Ready {GameMultiplayer.Instance.PlayerName}";

                GameMultiplayer.Instance.GameStarted += OnGameStarted;
            }

            LobbyManager.Instance.ProgressChanged += OnProgressChanged;
            LobbyManager.Instance.LobbyCreated += OnLobbyPrepared;
            LobbyManager.Instance.LobbyJoined += OnLobbyPrepared;
        }

        private void Update()
        {
            _loadingIcon.transform.eulerAngles = new Vector3(_loadingIcon.transform.eulerAngles.x, _loadingIcon.transform.eulerAngles.y, _loadingIcon.transform.eulerAngles.z - Random.Range(0.1f, 1f) * _loadingIconMaxRotateSpeed * Time.deltaTime);
            _loadingIcon.color = _loadingIcon.color.PulseHue(_loadingIconPulseSpeed, _loadingIconMinHue, _loadingIconMaxHue, ref _pulseAdd);

            if (string.IsNullOrWhiteSpace(_lobbyNameText.text) && LobbyManager.Instance != null && LobbyManager.Instance.Lobby != null)
            {
                _lobbyNameText.text = LobbyManager.Instance.Lobby.Name;
            }
        }

        private void OnDestroy()
        {
            _toLobbyButton.onClick.RemoveAllListeners();

            GameMultiplayer.Instance.GameStarted -= OnGameStarted;

            LobbyManager.Instance.ProgressChanged -= OnProgressChanged;
            LobbyManager.Instance.LobbyCreated -= OnLobbyPrepared;
            LobbyManager.Instance.LobbyJoined -= OnLobbyPrepared;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the status text of the loading UI.
        /// </summary>
        /// <param name="text">status text</param>
        public void UpdateProgress(string text)
        {
            if (!_statusText.text.Equals(text))
                _statusText.text = text;
        }

        /// <summary>
        /// Updates the status text and the progress of the loading UI.
        /// </summary>
        /// <param name="value">progress [0, 1]</param>
        /// <param name="text">status text</param>
        public void UpdateProgress(float value, string text)
        {
            _loadingBar.Set(value, 1f);
            if (!_statusText.text.Equals(text))
                _statusText.text = text;
        }

        #endregion

        #region Events Handling

        /// <summary>
        /// Handles the 'ProgressChanged' event.
        /// Updates progress.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void OnProgressChanged(object sender, ProgressChangedEventArgs e) => UpdateProgress(e.Progress, e.Status);

        /// <summary>
        /// Handles the 'GameStarted' event.
        /// Updates progress and destroys the loading UI.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void OnGameStarted(object sender, System.EventArgs e)
        {
            UpdateProgress(1, "READY");
            Destroy(gameObject, 5f);
        }

        /// <summary>
        /// Handles the 'LobbyPrepared' event.
        /// Updates the lobby name.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void OnLobbyPrepared(object sender, System.EventArgs e)
        {
            Lobby lobby = LobbyManager.Instance.Lobby;
            if (lobby != null)
                _lobbyNameText.text = lobby.Name;
        }

        #endregion
    }
}
