namespace nickmaltbie.OpenKCC.Character
{
    /// <summary>
    /// Current player input state, is the player allowed to give input or not
    /// </summary>
    public enum PlayerInputState
    {
        Allow,
        Deny
    }

    /// <summary>
    /// Manager for player input information as a static class
    /// </summary>
    public static class PlayerInputManager
    {
        /// <summary>
        /// Current state of the player movement action
        /// </summary>
        public static PlayerInputState playerMovementState = PlayerInputState.Allow;

        /// <summary>
        /// Minimum mouse sensitivity value multiplier
        /// </summary>
        public static readonly float minimumMouseSensitivity = 0.05f;
        /// <summary>
        /// Maximum mouse sensitivity value multiplier
        /// </summary>
        public static readonly float maximumMouseSensitivity = 1.0f;

        /// <summary>
        /// Mouse sensitivity multiplier (should be between 0.05 and 1.0 hopefully)
        /// </summary>
        public static float mouseSensitivity = 0.25f;

        /// <summary>
        /// Default mouse sensitivity
        /// </summary>
        public const float DefaultMouseSensitivity = 0.25f;
    }
}
