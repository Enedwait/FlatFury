using FlatFury.LobbyModule.Scripts.Managers;
using FlatFury.MainModule.Scripts.Controllers;
using FlatFury.MainModule.Scripts.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FlatFury.LobbyModule.Scripts.UI
{
    /// <summary>
    /// The <see cref="LobbyUI"/> class.
    /// </summary>
    internal sealed class LobbyUI : MonoBehaviour
    {
        #region Variables

        [SerializeField] private InputBlockUI _playerNameInputBlock;
        [SerializeField] private InputBlockUI _createLobbyInputBlock;
        [SerializeField] private Button _createLobbyButton;
        [SerializeField] private InputBlockUI _joinLobbyInputBlock;
        [SerializeField] private Button _joinLobbyButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private TextMeshProUGUI _versionText;

        #endregion

        #region Properties

        /// <summary> Gets the entered player name. </summary>
        public string PlayerName => _playerNameInputBlock.Value;

        #endregion

        #region Unity Methods

        private void Start()
        {
            _versionText.text = $"v.{Application.version}";

            _createLobbyButton.onClick.AddListener(() => { DisableControlsIfCreated(); GameMultiplayer.Instance.SetPlayerName(PlayerName); LobbyManager.Instance.CreateLobby(_createLobbyInputBlock.Value, PlayerName); });
            _joinLobbyButton.onClick.AddListener(() => { GameMultiplayer.Instance.SetPlayerName(PlayerName); LobbyManager.Instance.JoinLobby(_joinLobbyInputBlock.Value, PlayerName); });
            _exitButton.onClick.AddListener(() => { LobbyManager.Instance.LeaveLobby(); Application.Quit(); });

            _playerNameInputBlock.Value = GameMultiplayer.Instance.PlayerName;
            _createLobbyInputBlock.Value = PlayerPrefs.GetString(PlayerPrefsConsts.LastCreatedLobby);
            _joinLobbyInputBlock.Value = PlayerPrefs.GetString(PlayerPrefsConsts.LastJoinedLobby);
        }

        private void LateUpdate()
        {
            if (string.IsNullOrWhiteSpace(PlayerName))
            {
                _createLobbyInputBlock.DisableInput();
                _createLobbyButton.interactable = false;
                _joinLobbyInputBlock.DisableInput();
                _joinLobbyButton.interactable = false;
            }
            else
            {
                _createLobbyInputBlock.EnableInput();
                _createLobbyButton.interactable = !string.IsNullOrWhiteSpace(_createLobbyInputBlock.Value);
                _joinLobbyInputBlock.EnableInput();
                _joinLobbyButton.interactable = !string.IsNullOrWhiteSpace(_joinLobbyInputBlock.Value);
            }
        }

        private void OnDestroy()
        {
            _createLobbyButton.onClick.RemoveAllListeners();
            _joinLobbyButton.onClick.RemoveAllListeners();
            _exitButton.onClick.RemoveAllListeners();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Disables controls if Create Lobby button clicked.
        /// </summary>
        private void DisableControlsIfCreated()
        {
            _createLobbyButton.interactable = false;
            _joinLobbyButton.interactable = false;
        }

        #endregion
    }
}
