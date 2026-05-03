namespace IndieGabo.HandyFSM.CCPro
{
    /// <summary>
    /// Exposes the current dash stats to the CCPro sample states.
    /// </summary>
    public class DashStatsProvider : StatsProvider<DashStats>
    {
        #region Properties

        /// <summary>
        /// Gets the initial dash velocity.
        /// </summary>
        public float InitialVelocity => CurrentStats.InitialVelocity;

        /// <summary>
        /// Gets the dash duration in seconds.
        /// </summary>
        public float Duration => CurrentStats.Duration;

        /// <summary>
        /// Gets the normalized dash speed curve.
        /// </summary>
        public UnityEngine.AnimationCurve MovementCurve => CurrentStats.MovementCurve;

        /// <summary>
        /// Gets the amount of extra dashes available while airborne.
        /// </summary>
        public int AvailableNotGroundedDashes => CurrentStats.AvailableNotGroundedDashes;

        /// <summary>
        /// Gets whether dash speed should ignore material multipliers.
        /// </summary>
        public bool IgnoreSpeedMultipliers => CurrentStats.IgnoreSpeedMultipliers;

        /// <summary>
        /// Gets whether the dash should force the actor into a not-grounded state.
        /// </summary>
        public bool ForceNotGrounded => CurrentStats.ForceNotGrounded;

        /// <summary>
        /// Gets whether the dash should stop when contacting a wall.
        /// </summary>
        public bool CancelOnContact => CurrentStats.CancelOnContact;

        #endregion
    }
}