using FlatFury.GameModule.SOs.Coins;
using Unity.Netcode;
using UnityEngine;

namespace FlatFury.GameModule.Scripts
{
    /// <summary>
    /// The <see cref="Coin"/> class.
    /// This class represent the coin.
    /// </summary>
    internal sealed class Coin : NetworkBehaviour
    {
        #region Variables

        [SerializeField] private CoinSO _coinSo;
        [SerializeField] private SpriteRenderer _model;

        #endregion

        #region Properties

        /// <summary> Gets the coin data. </summary>
        public CoinSO CoinSO => _coinSo;

        #endregion

        #region Unity Methods

        private void Start()
        {
            _model.color = _coinSo.Color;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Hides the coin.
        /// </summary>
        public void Hide()
        {
            if (_model)
                _model.enabled = false;
        }

        #endregion

        #region Events Handling

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
                NetworkObject.TrySetParent(CoinManager.Instance.CoinRoot);
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.attachedRigidbody)
                CoinManager.Instance.Process(this, collider.attachedRigidbody.transform);
        }

        #endregion
    }
}
