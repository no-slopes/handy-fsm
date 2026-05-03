namespace IndieGabo.HandyFSM
{
    /// <summary>
    /// Describes how HandyFSM should compute movement reference data when the
    /// optional Character Controller Pro integration is enabled.
    /// </summary>
    public enum CharacterControllerProMovementReferenceMode
    {
        /// <summary>
        /// Uses world-space axes as the movement reference.
        /// </summary>
        World = 0,

        /// <summary>
        /// Uses an external transform as the movement reference.
        /// </summary>
        External = 1,

        /// <summary>
        /// Uses the initial character orientation as the movement reference.
        /// </summary>
        Character = 2
    }
}