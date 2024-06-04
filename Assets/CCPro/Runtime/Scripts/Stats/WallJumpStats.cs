
using Lightbug.CharacterControllerPro.Demo;
using Lightbug.Utilities;
using UnityEngine;

namespace IndieGabo.HandyFSM.CCPro
{
    [CreateAssetMenu(fileName = "WallJumpStats", menuName = "HandyFSM/CCPro/Stats/Wall Jump")]
    public class WallJumpStats : ScriptableObject
    {

        [SerializeField]
        private bool _canWallJump;

        [SerializeField]
        private float _duration;

        [SerializeField]
        private float _awayFromWallVelocity;

        [SerializeField]
        private float _jumpVelocity;


        public bool CanWallJump => _canWallJump;
        public float Duration => _duration;
        public float AwayFromWallVelocity => _awayFromWallVelocity;
        public float JumpVelocity => _jumpVelocity;
    }
}
