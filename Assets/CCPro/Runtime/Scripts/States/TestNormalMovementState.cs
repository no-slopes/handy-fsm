using System;
using UnityEngine;

namespace IndieGabo.HandyFSM.CCPro
{
    [CreateAssetMenu(fileName = "TestNormalMovementState", menuName = "HandyFSM/CCPro/TestNormalMovementState")]
    public class TestNormalMovementState : NormalMovementState
    {
        #region Transitions

        protected virtual bool CheckWallSlideConditions()
        {
            if (!CharacterActor.IsGrounded || CharacterActions.crouch.value) return false;

            return false;
        }

        #endregion

        protected override void OnInit()
        {
            base.OnInit();

            AddTransition(CheckWallSlideConditions, Brain.GetState<WallSlideState>());
        }
    }
}