
using UnityEngine;

namespace IndieGabo.HandyFSM.CCPro
{
    [CreateAssetMenu(fileName = "WallJumpState", menuName = "HandyFSM/CCPro/States/Wall Jump")]
    public class WallJumpState : ScriptableCCProState
    {
        #region Fields

        private WallSlideState _wallSlideState;
        private Vector2 _currentNormal;

        #endregion

        #region Cycle

        protected virtual void OnInit()
        {
            _wallSlideState = Brain.GetState<WallSlideState>();
        }

        protected virtual void OnEnter()
        {
            _currentNormal = _wallSlideState.RemanescenteNormal;
            // _notGroundedJumpsLeft = VerticalMovement.availableNotGroundedJumps + 1;
        }

        #endregion
    }
}