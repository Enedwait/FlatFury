using FlatFury.MainModule.Scripts.UI;
using TMPro;
using UnityEngine;

namespace FlatFury.GameModule.Scripts.Players
{
    /// <summary>
    /// The <see cref="PlayerVisual"/> class.
    /// This class represents the in-game player visual elements (e.g. name, health bar, etc.)
    /// </summary>
    internal sealed class PlayerVisual : MonoBehaviour
    {
        #region Variables

        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private ProgressBarUI _healthBar;

        #endregion

        #region Properties

        /// <summary> Gets the player associated with this visual. </summary>
        public Player Player { get; private set; }

        #endregion

        #region Unity Methods

        private void LateUpdate()
        {
            transform.rotation = Quaternion.identity;
        }

        private void OnDestroy()
        {
            if (Player)
                Player.Updated -= OnPlayerUpdated;

            Player = null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the player to be associated with this visual.
        /// </summary>
        /// <param name="player"></param>
        public void Set(Player player)
        {
            Player = player;
            Player.Updated += OnPlayerUpdated;
            UpdateVisual();
        }

        /// <summary>
        /// Updates the visual elements.
        /// </summary>
        public void UpdateVisual()
        {
            _nameText.text = Player.Name;
            _healthBar.Set(Player.Health, Player.MaxHealth);
        }

        #endregion

        #region Events Handling

        /// <summary>
        /// Handles the 'PlayerUpdated' event of the associated player.
        /// Updates visual.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void OnPlayerUpdated(object sender, System.EventArgs e) => UpdateVisual();

        #endregion
    }
}
