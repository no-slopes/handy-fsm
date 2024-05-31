using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Demo;
using Lightbug.CharacterControllerPro.Implementation;
using Lightbug.Utilities;
using UnityEngine;

namespace IndieGabo.HandyFSM.CCPro
{
    [CreateAssetMenu(fileName = "WallSlideState", menuName = "HandyFSM/CCPro/States/Wall Slide")]
    public class WallSlideState : ScriptableCCProState
    {
        #region Fields

        protected bool _wallJump = false;
        protected bool _released = false;
        protected Vector2 _initialSize = Vector2.zero;

        #endregion

        #region Properties

        protected WallSlideStatsProvider WallSlideStats { get; private set; }
        protected bool IsGrabbing => !CharacterActor.Is2D ? IsGrabbing3D() : IsGrabbing2D();

        #endregion

        #region Transitions

        protected bool CheckNormalMovementTransition()
        {
            if (CharacterActions.crouch.value || CharacterActor.IsGrounded || !CharacterActor.WallCollision || !CheckCenterRay())
            {
                return true;
            }
            else if (!IsGrabbing && WallSlideStats.SlideAcceleration == 0)
            {
                _released = true;
                return true;
            }
            else if (CharacterActions.jump.Started)
            {
                _wallJump = true;
                return true;
            }

            return false;
        }

        #endregion

        #region Cycle

        protected virtual void OnInit()
        {
            WallSlideStats = CCProBrain.GetComponentInChildren<WallSlideStatsProvider>();
            AddTransition(CheckNormalMovementTransition, Brain.GetState<NormalMovementState>());
        }

        public override bool CanEnter(IState fromState)
        {
            if (CharacterActor.IsAscending)
                return false;

            if (!CharacterActor.WallCollision)
                return false;

            if (WallSlideStats.FilterByTag)
                if (!CharacterActor.WallContact.gameObject.CompareTag(WallSlideStats.WallTag))
                    return false;

            if (!CheckCenterRay())
                return false;

            return true;
        }

        protected virtual void OnEnter()
        {
            CharacterActor.UseRootMotion = false;

            CharacterActor.Velocity *= WallSlideStats.InitialInertia;
            CharacterActor.SetYaw(-CharacterActor.WallContact.normal);

            if (WallSlideStats.ModifySize)
            {
                _initialSize = CharacterActor.BodySize;
                CharacterActor.SetSize(new Vector2(_initialSize.x, WallSlideStats.Height), CharacterActor.SizeReferenceType.Center);
            }
        }
        protected virtual void OnExit()
        {
            if (_wallJump)
            {
                _wallJump = false;

                // Do a 180 degrees turn.
                CharacterActor.TurnAround();

                // Apply the wall jump velocity.
                CharacterActor.Velocity = WallSlideStats.JumpVerticalVelocity
                    * CharacterActor.Up
                    + WallSlideStats.JumpNormalVelocity
                    * CharacterActor.WallContact.normal;
            }

            if (_released)
            {
                _released = false;

                // Apply the wall jump velocity.
                CharacterActor.Velocity = WallSlideStats.JumpVerticalVelocity
                    * CharacterActor.Up
                    + WallSlideStats.JumpNormalVelocity
                    * CharacterActor.WallContact.normal;
            }

            if (WallSlideStats.ModifySize)
            {
                CharacterActor.SizeReferenceType sizeReferenceType = CharacterActor.IsGrounded ?
                    CharacterActor.SizeReferenceType.Bottom : CharacterActor.SizeReferenceType.Top;
                CharacterActor.SetSize(_initialSize, sizeReferenceType);
            }
        }

        protected virtual void OnFixedTick()
        {
            float dt = Time.deltaTime;
            if (IsGrabbing)
            {
                Vector3 rightDirection = Vector3.ProjectOnPlane(CCProBrain.MovementReferenceRight, CharacterActor.WallContact.normal);
                rightDirection.Normalize();

                Vector3 upDirection = CharacterActor.Up;
                Vector3 targetVelocity = WallSlideStats.EnableClimb ? CharacterActions.movement.value.x
                    * WallSlideStats.WallClimbHorizontalSpeed
                    * rightDirection
                    + CharacterActions.movement.value.y
                    * WallSlideStats.WallClimbVerticalSpeed
                    * upDirection
                    : Vector3.zero;

                CharacterActor.Velocity = Vector3.MoveTowards(
                    CharacterActor.Velocity,
                    targetVelocity,
                    WallSlideStats.WallClimbAcceleration * dt
                );
            }
            else
            {
                CharacterActor.VerticalVelocity += dt * WallSlideStats.SlideAcceleration * -CharacterActor.Up;
            }
        }

        protected virtual void OnPostFixedTick()
        {
            if (!CharacterActor.IsAnimatorValid())
                return;

            CharacterActor.Animator.SetFloat(WallSlideStats.HorizontalVelocityParameter, CharacterActor.LocalVelocity.x);
            CharacterActor.Animator.SetFloat(WallSlideStats.VerticalVelocityParameter, CharacterActor.LocalVelocity.y);
            CharacterActor.Animator.SetBool(WallSlideStats.GrabParameter, IsGrabbing);
            CharacterActor.Animator.SetBool(WallSlideStats.MovementDetectedParameter, CharacterActions.movement.Detected);
        }

        protected virtual void OnTickIK(int layerIndex)
        {
            if (!CharacterActor.IsAnimatorValid())
                return;

            if (IsGrabbing && CharacterActions.movement.Detected)
            {
                CharacterActor.Animator.SetLookAtWeight(Mathf.Clamp01(CharacterActor.Velocity.magnitude), 0f, 0.2f);
                CharacterActor.Animator.SetLookAtPosition(CharacterActor.Position + CharacterActor.Velocity);
            }
            else
            {
                CharacterActor.Animator.SetLookAtWeight(0f);
            }

        }

        #endregion

        #region  Evaluations

        protected virtual bool CheckCenterRay()
        {
            HitInfoFilter filter = new HitInfoFilter(
                CharacterActor.PhysicsComponent.CollisionLayerMask,
                false,
                true
            );

            CharacterActor.PhysicsComponent.Raycast(
                out HitInfo centerRayHitInfo,
                CharacterActor.Center,
                -CharacterActor.WallContact.normal * 1.2f * CharacterActor.BodySize.x,
                in filter
            );

            return centerRayHitInfo.hit && centerRayHitInfo.transform.gameObject == CharacterActor.WallContact.gameObject;
        }

        protected virtual bool IsGrabbing2D()
        {
            if (!WallSlideStats.EnableGrab) return false;
            Vector2 input = CharacterActions.movement.value;
            float distance = CharacterActor.WallContact.point.x - CharacterActor.Position.x;
            float sideSign = Mathf.Sign(distance) * Mathf.Sign(CCProBrain.MovementReferenceRight.x);
            return Mathf.Sign(input.x) == sideSign;
        }

        protected virtual bool IsGrabbing3D()
        {
            return CharacterActions.run.value && WallSlideStats.EnableGrab;
        }

        #endregion
    }
}