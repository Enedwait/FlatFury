using System;
using TMPro;
using UnityEngine;

namespace FlatFury.LobbyModule.Scripts.UI
{
    /// <summary>
    /// The <see cref="InputBlockUI"/> class.
    /// This class represents the custom input field.
    /// </summary>
    internal class InputBlockUI : MonoBehaviour
    {
        #region Variables

        [SerializeField] protected TextMeshProUGUI text;
        [SerializeField] protected string caption;
        [SerializeField] protected TextMeshProUGUI placeholder;
        [SerializeField] protected string placeholderText;
        [SerializeField] protected TMP_InputField input;

        #endregion

        #region Properties

        /// <summary> Gets or sets the value of the input field. </summary>
        public string Value { get => input.text; set => input.text = value; }

        #endregion

        #region Unity Methods

#if (UNITY_EDITOR)
        protected virtual void OnValidate()
        {
            text.text = caption;
            placeholder.text = placeholderText;
        }
#endif

        protected virtual void Start()
        {
            text.text = caption;
            placeholder.text = placeholderText;

            input.onValueChanged.AddListener((name) => { ValueChanged?.Invoke(this, name); });
        }

        #endregion

        #region Methods

        /// <summary>
        /// Enables input.
        /// </summary>
        public void EnableInput() => input.interactable = true;

        /// <summary>
        /// Disables input.
        /// </summary>
        public void DisableInput() => input.interactable = false;

        #endregion

        #region Events

        /// <summary> Occurs when the value is changed inside the input field. </summary>
        public event EventHandler<string> ValueChanged;

        #endregion
    }
}
