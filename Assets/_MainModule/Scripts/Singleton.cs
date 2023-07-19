using UnityEngine;

namespace FlatFury.MainModule.Scripts
{
    /// <summary>
    /// The <see cref="Singleton"/> class.
    /// This class represents the singleton.
    /// <remarks>Can be used only on ONE CLASS in game.</remarks>
    /// </summary>
    [DisallowMultipleComponent]
    public class Singleton : MonoBehaviour
    {
        /// <summary> Gets the singleton instance of the <see cref="Singleton"/> class. </summary>
        public static Singleton Instance { get; private set; }

        private void Awake()
        {
            if (Instance) Destroy(gameObject);
            else Instance = this;
        }
    }
}
