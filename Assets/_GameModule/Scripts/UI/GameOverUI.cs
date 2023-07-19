using FlatFury.MainModule.Scripts.Controllers;
using FlatFury.MainModule.Scripts.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FlatFury.GameModule.Scripts.UI
{
    /// <summary>
    /// The <see cref="GameOverUI"/> class.
    /// This class represents the game over UI: victory / defeat / disconnect visuals.
    /// </summary>
    internal sealed class GameOverUI : MonoBehaviour
    {
        #region Variables

        [SerializeField] private Button _lobbyButton;
        [SerializeField] private RectTransform _victory;
        [SerializeField] private RectTransform _defeat;
        [SerializeField] private RectTransform _disconnect;
        [SerializeField] private TextMeshProUGUI _winnerText;
        [SerializeField] private TextMeshProUGUI _winnerScore;

        #endregion

        #region Show Methods

        /// <summary>
        /// Prepares the visuals to be shown.
        /// </summary>
        /// <param name="player"></param>
        private void Show(PlayerData player)
        {
            gameObject.SetActive(true);

            _winnerText.text = player.PlayerName;
            _winnerScore.text = $"{player.score}";

            _disconnect.gameObject.SetActive(false);
            _defeat.gameObject.SetActive(false);
            _victory.gameObject.SetActive(false);
            _lobbyButton.onClick.AddListener(GameMultiplayer.Instance.EndGame);
        }

        /// <summary>
        /// Shows the victory visuals for the specified player.
        /// </summary>
        /// <param name="winner">player</param>
        public void ShowVictory(PlayerData winner)
        {
            Show(winner);
            _victory.gameObject.SetActive(true);
        }

        /// <summary>
        /// Shows the defeat visuals with the actual winner.
        /// </summary>
        /// <param name="winner">player</param>
        public void ShowDefeat(PlayerData winner)
        {
            Show(winner);
            _defeat.gameObject.SetActive(true);
        }

        /// <summary>
        /// Shows the disconnect visuals for the specified player.
        /// </summary>
        /// <param name="player">player</param>
        public void ShowDisconnect(PlayerData player)
        {
            Show(player);
            _disconnect.gameObject.SetActive(true);
        }

        #endregion
    }
}
