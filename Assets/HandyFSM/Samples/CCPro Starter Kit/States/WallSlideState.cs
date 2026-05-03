using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Demo;
using Lightbug.CharacterControllerPro.Implementation;
using Lightbug.Utilities;
using UnityEngine;

namespace IndieGabo.HandyFSM.CCPro
{
    /// <summary>
    /// Recreates the CCPro 2D wall slide behaviour.
    /// The state validates that the actor is still attached to the same wall,
    /// applies either grab or slide motion, and hands the exit wall normal to
    /// the next state when a wall jump is requested.
    /// </summary>
    [CreateAssetMenu(fileName = "WallSlideState", menuName = "HandyFSM/CCPro/States/Wall Slide")]
    public class WallSlideState : ScriptableCCProState
    {
        #region Runtime State

        /// <summary>
        /// Tracks whether the state is leaving through a wall jump request.
        /// </summary>
        protected bool _wallJump = false;

        /// <summary>
        /// Stores the actor size so temporary wall-slide resizing can be restored on exit.
        /// </summary>
        protected Vector2 _initialSize = Vector2.zero;

        /// <summary>
        /// Caches the wall normal that produced the wall jump exit.
        /// </summary>
        protected Vector2 _remanescenteNormal;

        #endregion

        #region Configuration Access

        /// <summary>
        /// Gets the stats provider that tunes slide, grab, and wall jump behavior.
        /// </summary>
        protected WallSlideStatsProvider WallSlideStats { get; private set; }

        /// <summary>
        /// Gets whether the player is currently grabbing the wall instead of free-sliding.
        /// </summary>
        protected bool IsGrabbing => CharacterActions.run.value && WallSlideStats.EnableGrab;

        #endregion

        #region Exposed Data

        /// <summary>
        /// Gets the wall normal captured when leaving through a wall jump.
        /// </summary>
        public Vector2 RemanescenteNormal => _remanescenteNormal;

        #endregion

        #region Transition Evaluation

        /// <summary>
        /// Evaluates whether the state should return to normal locomotion.
        /// </summary>
        /// <returns>
        /// True when the actor lost the wall, reached the ground, started crouching,
        /// or requested a wall jump.
        /// </returns>
        protected bool CheckNormalMovementTransition()
        {
            // Any condition that invalidates a meaningful wall slide immediately hands
            // control back to locomotion.
            if (CharacterActions.crouch.value
                || CharacterActor.IsGrounded
                || !CharacterActor.WallCollision
                || !CheckCenterRay())
            {
                return true;
            }

            if (CharacterActions.jump.Started)
            {
                // The actual wall jump impulse is applied on exit so locomotion receives
                // the actor after the launch velocity has already been written.
                _wallJump = true;
                return true;
            }

            return false;
        }

        #endregion

        #region State Lifecycle

        /// <summary>
        /// Resolves configuration and wires the exit transition back to locomotion.
        /// </summary>
        protected virtual void OnInit()
        {
            // Stats live on the actor branch so this ScriptableState can stay reusable.
            WallSlideStats = CCProBrain.GetComponentInChildren<WallSlideStatsProvider>();

            if (WallSlideStats == null)
            {
                ThrowStateFailure(
                    "WallSlideState requires a WallSlideStatsProvider on the FSM branch.");
            }

            AddTransition(CheckNormalMovementTransition, Brain.GetState<NormalMovementState>());
            SortTransitions();
        }

        /// <summary>
        /// Validates whether the actor can enter the wall slide state.
        /// </summary>
        /// <param name="fromState">The state the machine is leaving.</param>
        /// <returns>True when the actor is descending against a valid wall.</returns>
        public override bool CanEnter(IState fromState)
        {
            // Ascending contacts are ignored so the state only models the downward wall
            // interaction seen in the original 2D demo.
            if (CharacterActor.IsAscending)
            {
                return false;
            }

            if (!CharacterActor.WallCollision)
            {
                return false;
            }

            if (WallSlideStats.FilterByTag)
            {
                if (!CharacterActor.WallContact.gameObject.CompareTag(WallSlideStats.WallTag))
                    return false;
            }

            if (!CheckCenterRay())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Initializes the slide state and aligns the actor to the contacted wall.
        /// </summary>
        protected virtual void OnEnter()
        {
            _remanescenteNormal = Vector2.zero;
            _wallJump = false;

            // Wall slide writes velocity directly, so root motion would only fight the state.
            CharacterActor.UseRootMotion = false;

            // The incoming velocity is partially preserved to keep the transition from
            // locomotion or falling from feeling abrupt.
            CharacterActor.Velocity *= WallSlideStats.InitialInertia;

            // Facing opposite to the wall keeps the sprite readable while attached.
            CharacterActor.SetYaw(-CharacterActor.WallContact.normal);

            if (WallSlideStats.ModifySize)
            {
                // The body can be compressed while sliding and restored on exit.
                _initialSize = CharacterActor.BodySize;
                CharacterActor.SetSize(
                    new Vector2(_initialSize.x, WallSlideStats.Height),
                    CharacterActor.SizeReferenceType.Center);
            }
        }

        /// <summary>
        /// Restores temporary wall-slide state and applies the wall jump impulse when requested.
        /// </summary>
        protected virtual void OnExit()
        {
            if (_wallJump)
            {
                _wallJump = false;

                // The next state can inspect the exit normal if it needs context about
                // which wall launched the character.
                _remanescenteNormal = CharacterActor.WallContact.normal;

                // Turning around before applying the launch keeps the actor orientation in
                // sync with the outgoing jump direction.
                CharacterActor.TurnAround();

                CharacterActor.Velocity = WallSlideStats.JumpVerticalVelocity
                    * CharacterActor.Up
                    + WallSlideStats.JumpNormalVelocity
                    * CharacterActor.WallContact.normal;
            }

            if (WallSlideStats.ModifySize)
            {
                CharacterActor.SizeReferenceType sizeReferenceType = CharacterActor.IsGrounded ?
                    CharacterActor.SizeReferenceType.Bottom : CharacterActor.SizeReferenceType.Top;

                // The original body size is restored using the correct reference point for
                // the current grounded state.
                CharacterActor.SetSize(_initialSize, sizeReferenceType);
            }
        }

        /// <summary>
        /// Applies either wall climb motion or passive slide acceleration.
        /// </summary>
        protected virtual void OnFixedTick()
        {
            float dt = Time.deltaTime;

            if (IsGrabbing)
            {
                // The horizontal climb axis is projected onto the wall plane so player
                // input moves along the wall instead of pushing into it.
                Vector3 rightDirection = Vector3.ProjectOnPlane(MovementReferenceRight, CharacterActor.WallContact.normal);
                rightDirection.Normalize();

                Vector3 upDirection = CharacterActor.Up;
                Vector3 targetVelocity = WallSlideStats.EnableClimb
                    ? CharacterActions.movement.value.x
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
                // Free sliding only accelerates downward along the actor up axis.
                CharacterActor.VerticalVelocity += dt * WallSlideStats.SlideAcceleration * -CharacterActor.Up;
            }
        }

        /// <summary>
        /// Synchronizes animation parameters after fixed simulation finishes.
        /// </summary>
        protected virtual void OnPostFixedTick()
        {
            if (!CharacterActor.IsAnimatorValid())
            {
                return;
            }

            // Animator values are sent after simulation so they describe the final state
            // of the current frame rather than stale pre-simulation data.
            CharacterActor.Animator.SetFloat(WallSlideStats.HorizontalVelocityParameter, CharacterActor.LocalVelocity.x);
            CharacterActor.Animator.SetFloat(WallSlideStats.VerticalVelocityParameter, CharacterActor.LocalVelocity.y);
            CharacterActor.Animator.SetBool(WallSlideStats.GrabParameter, IsGrabbing);
            CharacterActor.Animator.SetBool(WallSlideStats.MovementDetectedParameter, CharacterActions.movement.Detected);
        }

        /// <summary>
        /// Updates IK look-at data so the character gaze follows wall-climb motion.
        /// </summary>
        /// <param name="layerIndex">Animator IK layer index provided by the runtime hook.</param>
        protected virtual void OnTickIK(int layerIndex)
        {
            if (!CharacterActor.IsAnimatorValid())
            {
                return;
            }

            if (IsGrabbing && CharacterActions.movement.Detected)
            {
                // Looking toward the current velocity direction gives the grab/climb pose
                // a more intentional animation target.
                CharacterActor.Animator.SetLookAtWeight(Mathf.Clamp01(CharacterActor.Velocity.magnitude), 0f, 0.2f);
                CharacterActor.Animator.SetLookAtPosition(CharacterActor.Position + CharacterActor.Velocity);
            }
            else
            {
                CharacterActor.Animator.SetLookAtWeight(0f);
            }
        }

        #endregion

        #region Contact Validation

        /// <summary>
        /// Casts a short ray toward the contacted wall to verify the actor is still centered on it.
        /// </summary>
        /// <returns>True when the ray confirms the current wall contact.</returns>
        protected virtual bool CheckCenterRay()
        {
            // The filter mirrors the actor collision layers so the validation ray tests
            // against the same surface set as the main controller.
            HitInfoFilter filter = new HitInfoFilter(
                CharacterActor.PhysicsComponent.CollisionLayerMask,
                true,
                true
            );

            // The cast length is slightly wider than the body width so the state can
            // verify the actor is still meaningfully attached to the same wall.
            CharacterActor.PhysicsComponent.Raycast(
                out HitInfo centerRayHitInfo,
                CharacterActor.Center,
                -CharacterActor.WallContact.normal * 1.2f * CharacterActor.BodySize.x,
                in filter
            );

            return centerRayHitInfo.hit && centerRayHitInfo.transform.gameObject == CharacterActor.WallContact.gameObject;
        }

        #endregion
    }
}