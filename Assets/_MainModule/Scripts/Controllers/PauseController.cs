using System;
using UnityEngine;

namespace FlatFury.MainModule.Scripts.Controllers
{
    /// <summary>
    /// The <see cref="PauseController"/> class.
    /// This class represents a common singleton pause controller. It allows to pause or resume a game.
    /// Paused event is raised if the game is paused via singleton.
    /// Resume event is raised if the game is resumed via singleton.
    /// </summary> 
    public class PauseController
    {
        #region Properties

        /// <summary> Gets the singleton instance of the <see cref="PauseController"/> class. </summary>
        public static PauseController Instance { get; protected set; }

        /// <summary> Gets a value indicating whether the game is paused or not. </summary>
        public static bool IsGamePaused { get; protected set; }

        #endregion

        #region Methods

        /// <summary>
        /// Pauses the game if it's not already paused. Raises 'Paused' event.
        /// </summary>
        public static void Pause()
        {
            if (!IsGamePaused)
            {
                Time.timeScale = 0f; // pauses the game time
                AudioListener.pause = true; // pauses the audio
                IsGamePaused = true;
                Paused?.Invoke(Instance, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Resumes the game if it's not already resumed. Raises 'Resumed' event.
        /// </summary>
        /// <param name="timeScale">time scale to be set on resume.</param>
        public static void Resume(float timeScale = 1f)
        {
            if (IsGamePaused)
            {
                Time.timeScale = timeScale; // resumes the game time
                AudioListener.pause = false; // resumes the audio
                IsGamePaused = false;
                Resumed?.Invoke(Instance, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Pauses or resumes the game depending on the actual game's state.
        /// </summary>
        public static void PauseOrResume()
        {
            if (IsGamePaused) Resume();
            else Pause();
        }

        #endregion

        #region Events

        /// <summary> Occurs when the game is paused. </summary>
        public static event EventHandler Paused;

        /// <summary> Occurs when the game is resumed. </summary>
        public static event EventHandler Resumed;

        #endregion
    }
}
