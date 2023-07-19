using System;
using UnityEngine;

namespace FlatFury.GameModule.Scripts.Input
{
    /// <summary>
    /// The <see cref="GameInput"/> class.
    /// This class represent the game input wrapper for the input system.
    /// </summary>
    internal sealed class GameInput : IDisposable
    {
        #region Singleton

        /// <summary> Gets the singleton instance of the <see cref="GameInput"/> class. </summary>
        public static GameInput Instance { get; private set; }

        #endregion

        #region Variables

        private PlayerControls _playerControls;

        #endregion

        #region Properties

        /// <summary> Gets a value indicating whether the Fire button is pressed or not. </summary>
        public bool IsFiring => _playerControls.GameActions.Fire.IsPressed();

        /// <summary> Gets the value indicating whether the mouse is used or not. </summary>
        public bool IsMouse => _playerControls.GameActions.MouseMove.IsPressed();

        /// <summary> Gets the current cursor position (if available). </summary>
        public Vector2 CursorPosition => _playerControls.GameActions.MouseMove.ReadValue<Vector2>();

        #endregion

        #region Init

        static GameInput()
        {
            Instance = new GameInput();
            Instance.Init();
        }

        private GameInput(){}


        /// <summary>
        /// Initializes the data.
        /// </summary>
        public void Init()
        {
            _playerControls = new PlayerControls();
            _playerControls.GameActions.Enable();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Disposes the data.
        /// </summary>
        public void Dispose()
        {
            _playerControls?.Dispose();
        }

        /// <summary>
        /// Retrieves the normalized movement value.
        /// </summary>
        /// <returns>normalized movement</returns>
        public Vector2 GetMovementNormalized()
        {
            Vector2 inputVector = _playerControls.GameActions.Move.ReadValue<Vector2>();
            return inputVector.normalized;
        }

        #endregion
    }
}
