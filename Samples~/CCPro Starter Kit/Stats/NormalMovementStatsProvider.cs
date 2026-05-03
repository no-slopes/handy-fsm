using Lightbug.CharacterControllerPro.Demo;
using UnityEngine;

namespace IndieGabo.HandyFSM.CCPro
{
    public class NormalMovementStatsProvider : StatsProvider<NormalMovementStats>
    {
        #region Getters

        public PlanarMovementParameters PlanarMovement => CurrentStats.PlanarMovement;
        public VerticalMovementParameters VerticalMovement => CurrentStats.VerticalMovement;
        public CrouchParameters Crouch => CurrentStats.Crouch;
        public LookingDirectionParameters LookingDirection => CurrentStats.LookingDirection;
        public float WallJumpReducedControlDuration => CurrentStats.WallJumpReducedControlDuration;

        public string GroundedAnimationParameter => CurrentStats.GroundedAnimationParameter;
        public string StableAnimationParameter => CurrentStats.StableAnimationParameter;
        public string VerticalSpeedAnimationParameter => CurrentStats.VerticalSpeedAnimationParameter;
        public string PlanarSpeedAnimationParameter => CurrentStats.PlanarSpeedAnimationParameter;
        public string HorizontalAxisAnimationParameter => CurrentStats.HorizontalAxisAnimationParameter;
        public string VerticalAxisAnimationParameter => CurrentStats.VerticalAxisAnimationParameter;
        public string HeightAnimationParameter => CurrentStats.HeightAnimationParameter;

        #endregion
    }
}