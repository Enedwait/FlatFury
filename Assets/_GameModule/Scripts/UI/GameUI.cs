using FlatFury.GameModule.Scripts.Players;
using FlatFury.MainModule.Scripts.Controllers;
using FlatFury.MainModule.Scripts.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FlatFury.GameModule.Scripts.UI
{
    /// <summary>
    /// The <see cref="GameUI"/> class.
    /// This class represents the actual game UI. Or HUD.
    /// </summary>
    internal sealed class GameUI : MonoBehaviour
    {
        #region Singleton

        /// <summary> Gets the singleton instance of the <see cref="GameUI"/> class. </summary>
        public static GameUI Instance { get; private set; }

        #endregion

        #region Variables

        [SerializeField] private TextMeshProUGUI _playerNameText;
        [SerializeField] private TextMeshProUGUI _playerScoreText;
        [SerializeField] private Button _toLobbyButton;
        [SerializeField] private GameOverUI _gameOver;

        private Player _player;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (Instance) Destroy(this);
            else Instance = this;
        }

        private void Start()
        {
            _toLobbyButton.onClick.AddListener(GameMultiplayer.Instance.EndGame);
        }

        public void Reset()
        {
            _playerNameText.text = "---";
            _playerScoreText.text = "---";
        }

        private void OnDestroy()
        {
            _toLobbyButton.onClick.RemoveAllListeners();

            if (_player)
                _player.Updated -= PlayerUpdated;

            _player = null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the player to be displayed on UI.
        /// </summary>
        /// <param name="player">player</param>
        public void Set(Player player)
        {
            // check if the player already assigned
            if (_player) // then detach this player
            {
                _player.Updated -= PlayerUpdated;
                _player = null;
            }

            _player = player;
            if (!player)
            {
                Reset();
                return;
            }

            _player.Updated += PlayerUpdated;
        }

        /// <summary>
        /// Shows the game over screen with the specified winner.
        /// </summary>
        /// <param name="winner">winner.</param>
        public void ShowGameOver(PlayerData winner)
        {
            if (winner.clientId.Equals(_player.PlayerData.clientId)) ShowVictory(winner);
            else ShowDefeat(winner);
        }

        /// <summary>
        /// Shows victory screen with the specified winner.
        /// </summary>
        /// <param name="winner">winner.</param>
        private void ShowVictory(PlayerData winner)
        {
            _gameOver.ShowVictory(winner);
        }

        /// <summary>
        /// Shows defeat screen with the specified winner.
        /// </summary>
        /// <param name="winner">winner.</param>
        public void ShowDefeat(PlayerData winner)
        {
            _gameOver.ShowDefeat(winner);
        }

        /// <summary>
        /// Shows disconnect screen for the disconnected player.
        /// </summary>
        public void ShowDisconnect()
        {
            _player.UpdatePlayerData();
            _gameOver.ShowDisconnect(_player.PlayerData);
        }

        /// <summary>
        /// Updates UI accordingly.
        /// </summary>
        private void UpdateUI()
        {
            _playerNameText.text = _player.Name;
            _playerScoreText.text = $"{_player.Score}";
        }

        #endregion

        #region Events Handling

        /// <summary>
        /// Handles the 'PlayerUpdated' event.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void PlayerUpdated(object sender, System.EventArgs e) => UpdateUI();

        #endregion
    }
}
