
using Lightbug.CharacterControllerPro.Demo;
using UnityEngine;

namespace IndieGabo.HandyFSM.CCPro
{
        [CreateAssetMenu(fileName = "WallSlideStats", menuName = "HandyFSM/CCPro/Stats/Normal Movement")]
        public class NormalMovementStats : ScriptableObject
        {
                [SerializeField] protected PlanarMovementParameters _planarMovement = new();
                [SerializeField] protected VerticalMovementParameters _verticalMovement = new();
                [SerializeField] protected CrouchParameters _crouch = new();
                [SerializeField] protected LookingDirectionParameters _lookingDirection = new();

                [Header("Animation")]
                [SerializeField]
                protected string _groundedAnimationParameter = "Grounded";

                [SerializeField]
                protected string _stableAnimationParameter = "Stable";

                [SerializeField]
                protected string _verticalSpeedAnimationParameter = "VerticalSpeed";

                [SerializeField]
                protected string _planarSpeedAnimationParameter = "PlanarSpeed";

                [SerializeField]
                protected string _horizontalAxisAnimationParameter = "HorizontalAxis";

                [SerializeField]
                protected string _verticalAxisAnimationParameter = "VerticalAxis";

                [SerializeField]
                protected string _heightAnimationParameter = "Height";

                #region Getters

                public PlanarMovementParameters PlanarMovement => _planarMovement;
                public VerticalMovementParameters VerticalMovement => _verticalMovement;
                public CrouchParameters Crouch => _crouch;
                public LookingDirectionParameters LookingDirection => _lookingDirection;

                public string GroundedAnimationParameter => _groundedAnimationParameter;
                public string StableAnimationParameter => _stableAnimationParameter;
                public string VerticalSpeedAnimationParameter => _verticalSpeedAnimationParameter;
                public string PlanarSpeedAnimationParameter => _planarSpeedAnimationParameter;
                public string HorizontalAxisAnimationParameter => _horizontalAxisAnimationParameter;
                public string VerticalAxisAnimationParameter => _verticalAxisAnimationParameter;
                public string HeightAnimationParameter => _heightAnimationParameter;

                #endregion
        }
}
