
using Lightbug.CharacterControllerPro.Demo;
using Lightbug.Utilities;
using UnityEngine;

namespace IndieGabo.HandyFSM.CCPro
{
    [CreateAssetMenu(fileName = "WallSlideStats", menuName = "HandyFSM/CCPro/Stats/Wall Slide")]
    public class WallSlideStats : ScriptableObject
    {

        [Header("Filter")]

        [SerializeField]
        public bool filterByTag = true;

        [Condition("filterByTag", ConditionAttribute.ConditionType.IsTrue)]
        [SerializeField]
        public string wallTag = "WallSlide";


        [Header("Slide")]

        [SerializeField]
        public float slideAcceleration = 10f;

        [Range(0f, 1f)]
        [SerializeField]
        public float initialIntertia = 0.4f;

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
        public bool modifySize = true;

        [Condition("modifySize", ConditionAttribute.ConditionType.IsTrue)]
        [SerializeField]
        public float height = 1.5f;

        [Header("Jump")]

        [SerializeField]
        public float jumpNormalVelocity = 5f;

        [SerializeField]
        public float jumpVerticalVelocity = 10f;

        [Header("Animation")]

        [SerializeField]
        public string horizontalVelocityParameter = "HorizontalVelocity";

        [SerializeField]
        public string verticalVelocityParameter = "VerticalVelocity";

        [SerializeField]
        public string grabParameter = "Grab";

        [SerializeField]
        public string movementDetectedParameter = "MovementDetected";
    }
}
