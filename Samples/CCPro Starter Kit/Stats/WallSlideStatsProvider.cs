using Lightbug.CharacterControllerPro.Demo;
using UnityEngine;

namespace IndieGabo.HandyFSM.CCPro
{
    public class WallSlideStatsProvider : StatsProvider<WallSlideStats>
    {
        #region Getters

        public bool FilterByTag => CurrentStats.filterByTag;
        public string WallTag => CurrentStats.wallTag;

        public float SlideAcceleration => CurrentStats.slideAcceleration;
        public float InitialInertia => CurrentStats.initialIntertia;

        public bool EnableGrab => CurrentStats.enableGrab;
        public bool EnableClimb => CurrentStats.enableClimb;

        public float WallClimbHorizontalSpeed => CurrentStats.wallClimbHorizontalSpeed;
        public float WallClimbVerticalSpeed => CurrentStats.wallClimbVerticalSpeed;
        public float WallClimbAcceleration => CurrentStats.wallClimbAcceleration;

        public bool ModifySize => CurrentStats.modifySize;
        public float Height => CurrentStats.height;
        public float JumpNormalVelocity => CurrentStats.jumpNormalVelocity;
        public float JumpVerticalVelocity => CurrentStats.jumpVerticalVelocity;

        public string HorizontalVelocityParameter => CurrentStats.horizontalVelocityParameter;
        public string VerticalVelocityParameter => CurrentStats.verticalVelocityParameter;
        public string GrabParameter => CurrentStats.grabParameter;
        public string MovementDetectedParameter => CurrentStats.movementDetectedParameter;

        #endregion
    }
}