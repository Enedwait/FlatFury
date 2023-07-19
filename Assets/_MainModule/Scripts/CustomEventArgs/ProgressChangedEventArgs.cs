namespace FlatFury.MainModule.Scripts.CustomEventArgs
{
    /// <summary>
    /// The <see cref="ProgressChangedEventArgs"/> class.
    /// This class represents a custom events args for the ease of use with progress values and statuses.
    /// </summary>
    internal class ProgressChangedEventArgs : System.EventArgs
    {
        #region Properties

        /// <summary> Gets the status. </summary>
        public string Status { get; }

        /// <summary> Gets the progress. </summary>
        public float Progress { get; }

        #endregion

        #region Init

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressChangedEventArgs"/> class.
        /// </summary>
        /// <param name="progress">progress</param>
        /// <param name="status">status</param>
        public ProgressChangedEventArgs(float progress, string status)
        {
            Progress = progress;
            Status = status;
        }

        #endregion
    }
}
