using Lightbug.CharacterControllerPro.Demo;
using UnityEngine;

namespace IndieGabo.HandyFSM.CCPro
{
    public class WallSlideStatsProvider : StatsProvider<WallSlideStats>
    {
        #region Getters

        public bool EnableGrab => CurrentStats.enableGrab;
        public bool EnableClimb => CurrentStats.enableClimb;

        public float WallClimbHorizontalSpeed => CurrentStats.wallClimbHorizontalSpeed;
        public float WallClimbVerticalSpeed => CurrentStats.wallClimbVerticalSpeed;
        public float WallClimbAcceleration => CurrentStats.wallClimbAcceleration;

        #endregion
    }
}