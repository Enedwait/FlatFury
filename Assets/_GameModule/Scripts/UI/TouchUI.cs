using UnityEngine;

namespace FlatFury.GameModule.Scripts.UI
{
    /// <summary>
    /// The <see cref="TouchUI"/> class.
    /// This class represents the touchscreen UI.
    /// </summary>
    internal sealed class TouchUI : MonoBehaviour
    {
        #region Unity Methods

        private void Awake()
        {
            // check if the game is not launched on mobile device
            if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer) // then remove touchscreen UI
                Destroy(gameObject);
        }

        #endregion
    }
}
