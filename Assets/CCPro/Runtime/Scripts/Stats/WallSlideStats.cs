
using Lightbug.CharacterControllerPro.Demo;
using Lightbug.Utilities;
using UnityEngine;

namespace IndieGabo.HandyFSM.CCPro
{
    [CreateAssetMenu(fileName = "WallSlideStats", menuName = "HandyFSM/CCPro/WallSlideStats")]
    public class WallSlideStats : ScriptableObject
    {

        [Header("Filter")]

        [SerializeField]
        protected bool filterByTag = true;

        [Condition("filterByTag", ConditionAttribute.ConditionType.IsTrue)]
        [SerializeField]
        protected string wallTag = "WallSlide";


        [Header("Slide")]

        [SerializeField]
        protected float slideAcceleration = 10f;

        [Range(0f, 1f)]
        [SerializeField]
        protected float initialIntertia = 0.4f;

        [Header("Grab")]

        public bool enableGrab = true;

        public bool enableClimb = true;

        [Condition("enableClimb", ConditionAttribute.ConditionType.IsTrue)]
        public float wallClimbHorizontalSpeed = 1f;

        [Condition("enableClimb", ConditionAttribute.ConditionType.IsTrue)]
        public float wallClimbVerticalSpeed = 3f;

        [Condition("enableClimb", ConditionAttribute.ConditionType.IsTrue)]
        public float wallClimbAcceleration = 10f;



        [Header("Size")]

        [SerializeField]
        protected bool modifySize = true;

        [Condition("modifySize", ConditionAttribute.ConditionType.IsTrue)]
        [SerializeField]
        protected float height = 1.5f;

        [Header("Jump")]

        [SerializeField]
        protected float jumpNormalVelocity = 5f;

        [SerializeField]
        protected float jumpVerticalVelocity = 10f;

        [Header("Animation")]

        [SerializeField]
        protected string horizontalVelocityParameter = "HorizontalVelocity";

        [SerializeField]
        protected string verticalVelocityParameter = "VerticalVelocity";

        [SerializeField]
        protected string grabParameter = "Grab";

        [SerializeField]
        protected string movementDetectedParameter = "MovementDetected";
    }
}
