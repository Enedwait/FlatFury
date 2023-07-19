using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FlatFury.MainModule.Scripts.UI
{
    /// <summary>
    /// The <see cref="ProgressBarUI"/> class.
    /// This class represent the progress bar visual.
    /// </summary>
    internal sealed class ProgressBarUI : MonoBehaviour
    {
        #region Variables

        [SerializeField] private Slider _slider;
        [SerializeField] private Image _fill;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Gradient _gradient;
        [SerializeField, Range(0f, 1f)] private float _value;

        #endregion

        #region Properties

        /// <summary> Gets the progress value. </summary>
        public float Value => _slider.value;

        /// <summary> Gets the max progress value. </summary>
        public float MaxValue => _slider.maxValue;

        /// <summary> Gets the scaled progress value. (e.g. [0, 1]) </summary>
        public float ScaledValue => Value / MaxValue;

        #endregion

        #region Unity Methods

#if UNITY_EDITOR
        private void OnValidate()
        {
            Add(0);
        }
#endif

        private void Start()
        {
            Add(0);

            _slider.onValueChanged.AddListener(arg0 => Add(0));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds specified value to the current progress value.
        /// </summary>
        /// <param name="value">value</param>
        public void Add(float value)
        {
            _slider.value = _value += value;
            UpdateUI();
        }

        /// <summary>
        /// Sets the progress value to the specified value / max value.
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="maxValue">max value</param>
        public void Set(float value, float maxValue)
        {
            _slider.value = _value = value;
            _slider.maxValue = maxValue;
            UpdateUI();
        }

        /// <summary>
        /// Updates UI.
        /// </summary>
        public void UpdateUI()
        {
            float value = Value / MaxValue;
            _text.text = $"{(int)(value  * 100f)}%";
            _fill.color = _gradient.Evaluate(value);
        }

        #endregion
    }
}
