using Lightbug.CharacterControllerPro.Demo;
using UnityEngine;

namespace IndieGabo.HandyFSM.CCPro
{
    public class NormalMovementStatsProvider : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private NormalMovementStats _defaultStats;

        #endregion

        #region Fields

        protected NormalMovementStats _currentStats;

        #endregion

        #region Getters

        public NormalMovementStats CurrentStats => _currentStats != null ? _currentStats : _defaultStats;

        public PlanarMovementParameters PlanarMovement => CurrentStats.PlanarMovement;
        public VerticalMovementParameters VerticalMovement => CurrentStats.VerticalMovement;
        public CrouchParameters Crouch => CurrentStats.Crouch;
        public LookingDirectionParameters LookingDirection => CurrentStats.LookingDirection;

        public string GroundedAnimationParameter => CurrentStats.GroundedAnimationParameter;
        public string StableAnimationParameter => CurrentStats.StableAnimationParameter;
        public string VerticalSpeedAnimationParameter => CurrentStats.VerticalSpeedAnimationParameter;
        public string PlanarSpeedAnimationParameter => CurrentStats.PlanarSpeedAnimationParameter;
        public string HorizontalAxisAnimationParameter => CurrentStats.HorizontalAxisAnimationParameter;
        public string VerticalAxisAnimationParameter => CurrentStats.VerticalAxisAnimationParameter;
        public string HeightAnimationParameter => CurrentStats.HeightAnimationParameter;

        #endregion

        #region Behaviour

        protected virtual void Awake()
        {
            _currentStats = _defaultStats;
        }

        #endregion
    }
}