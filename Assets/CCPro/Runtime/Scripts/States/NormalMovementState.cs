using System;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Demo;
using Lightbug.CharacterControllerPro.Implementation;
using Lightbug.Utilities;
using UnityEngine;

namespace IndieGabo.HandyFSM.CCPro
{
    [CreateAssetMenu(fileName = "NormalMovementState", menuName = "HandyFSM/CCPro/States/Normal Movement")]
    public class NormalMovementState : ScriptableCCProState
    {
        protected static readonly string WantsToRunSignal = "wantsToRun";

        protected int _notGroundedJumpsLeft = 0;
        protected bool _isAllowedToCancelJump = false;
        protected bool _wantToRun = false;
        protected float _currentPlanarSpeedLimit = 0f;

        protected bool _groundedJumpAvailable = true;
        protected Vector3 _jumpDirection = default;

        protected Vector3 _targetLookingDirection = default;
        protected float _targetHeight = 1f;

        protected bool _wantToCrouch = false;
        protected bool _isCrouched = false;

        protected PlanarMovementParameters.PlanarMovementProperties _currentMotion = new();

        protected NormalMovementStatsProvider MovementStats { get; private set; }
        protected PlanarMovementParameters PlanarMovement => MovementStats.PlanarMovement;
        protected VerticalMovementParameters VerticalMovement => MovementStats.VerticalMovement;
        protected CrouchParameters Crouch => MovementStats.Crouch;
        protected LookingDirectionParameters LookingDirection => MovementStats.LookingDirection;
        protected bool UnstableGroundedJumpAvailable => !VerticalMovement.canJumpOnUnstableGround && CharacterActor.CurrentState == CharacterActorState.UnstableGrounded;

        bool _reducedAirControlFlag = false;
        float _reducedAirControlInitialTime = 0f;
        float _reductionDuration = 0.5f;

        public enum JumpResult
        {
            Invalid,
            Grounded,
            NotGrounded
        }

        #region Events	

        /// <summary>
        /// Event triggered when the character jumps.
        /// </summary>
        public event System.Action OnJumpPerformed;

        /// <summary>
        /// Event triggered when the character jumps from the ground.
        /// </summary>
        public event System.Action<bool> OnGroundedJumpPerformed;

        /// <summary>
        /// Event triggered when the character jumps while.
        /// </summary>
        public event System.Action<int> OnNotGroundedJumpPerformed;

        #endregion

        #region Transitions

        protected Func<bool> WallSlideConditions => () => CharacterActor.WallCollision && !CharacterActor.IsAscending && !CharacterActor.IsGrounded && !CharacterActions.crouch.value;

        #endregion

        protected virtual void OnInit()
        {
            MovementStats = CCProBrain.GetComponentInChildren<NormalMovementStatsProvider>();

            _notGroundedJumpsLeft = MovementStats.VerticalMovement.availableNotGroundedJumps;
            _targetHeight = CCProBrain.CharacterActor.DefaultBodySize.y;
            float minCrouchHeightRatio = CCProBrain.CharacterActor.BodySize.x / CCProBrain.CharacterActor.BodySize.y;
            MovementStats.Crouch.heightRatio = Mathf.Max(minCrouchHeightRatio, MovementStats.Crouch.heightRatio);

            AddTransition(WallSlideConditions, Brain.GetState<WallSlideState>());
        }

        protected virtual void OnEnter()
        {
            CharacterActor.alwaysNotGrounded = false;

            _targetLookingDirection = CharacterActor.Forward;

            if (CCProBrain.PreviousState is WallSlideState)
            {
                Debug.Log("WallSlideState");
                // "availableNotGroundedJumps + 1" because the update code will consume one jump!
                _notGroundedJumpsLeft = VerticalMovement.availableNotGroundedJumps + 1;

                // Reduce the amount of air control (acceleration and deceleration) for 0.5 seconds.
                ReduceAirControl(0.15f);
            }

            _currentPlanarSpeedLimit = Mathf.Max(CharacterActor.PlanarVelocity.magnitude, PlanarMovement.baseSpeedLimit);

            CharacterActor.UseRootMotion = false;
            CCProBrain.CharacterActor.OnTeleport += OnTeleport;
        }

        protected virtual void OnExit()
        {
            _reducedAirControlFlag = false;
            CCProBrain.CharacterActor.OnTeleport += OnTeleport;
        }

        protected virtual void OnFixedTick()
        {
            float dt = Time.deltaTime;
            HandleSize(dt);
            HandleVelocity(dt);
            HandleRotation(dt);
        }

        protected virtual void OnPreCharacterSimulation(float dt)
        {
            // Pre/PostCharacterSimulation methods are useful to update all the Animator parameters. 
            // Why? Because the CharacterActor component will end up modifying the velocity of the actor.
            if (!CharacterActor.IsAnimatorValid())
                return;

            CCProBrain.Animator.SetBool(MovementStats.GroundedAnimationParameter, CharacterActor.IsGrounded);
            CCProBrain.Animator.SetBool(MovementStats.StableAnimationParameter, CharacterActor.IsStable);
            CCProBrain.Animator.SetFloat(MovementStats.HorizontalAxisAnimationParameter, CharacterActions.movement.value.x);
            CCProBrain.Animator.SetFloat(MovementStats.VerticalAxisAnimationParameter, CharacterActions.movement.value.y);
            CCProBrain.Animator.SetFloat(MovementStats.HeightAnimationParameter, CharacterActor.BodySize.y);
        }

        public virtual void OnPostCharacterSimulation(float dt)
        {
            // Pre/PostCharacterSimulation methods are useful to update all the Animator parameters. 
            // Why? Because the CharacterActor component will end up modifying the velocity of the actor.
            if (!CharacterActor.IsAnimatorValid())
                return;

            // Parameters associated with velocity are sent after the simulation.
            // The PostSimulationUpdate (CharacterActor) might update velocity once more (e.g. if a "bad step" has been detected).
            CCProBrain.Animator.SetFloat(MovementStats.VerticalSpeedAnimationParameter, CharacterActor.LocalVelocity.y);
            CCProBrain.Animator.SetFloat(MovementStats.PlanarSpeedAnimationParameter, CharacterActor.PlanarVelocity.magnitude);
        }

        void OnTeleport(Vector3 position, Quaternion rotation)
        {
            _targetLookingDirection = CCProBrain.CharacterActor.Forward;
            _isAllowedToCancelJump = false;
        }

        /// <summary>
        /// Reduces the amount of acceleration and deceleration (not grounded state) until the character reaches the apex of the jump 
        /// (vertical velocity close to zero). This can be useful to prevent the character from accelerating/decelerating too quickly (e.g. right after performing a wall jump).
        /// </summary>
        void ReduceAirControl(float reductionDuration = 0.5f)
        {
            _reducedAirControlFlag = true;
            _reducedAirControlInitialTime = Time.time;
            _reductionDuration = reductionDuration;
        }

        void SetMotionValues(Vector3 targetPlanarVelocity)
        {
            float angleCurrentTargetVelocity = Vector3.Angle(CharacterActor.PlanarVelocity, targetPlanarVelocity);

            switch (CharacterActor.CurrentState)
            {
                case CharacterActorState.StableGrounded:

                    _currentMotion.acceleration = PlanarMovement.stableGroundedAcceleration;
                    _currentMotion.deceleration = PlanarMovement.stableGroundedDeceleration;
                    _currentMotion.angleAccelerationMultiplier = PlanarMovement.stableGroundedAngleAccelerationBoost.Evaluate(angleCurrentTargetVelocity);

                    break;
                case CharacterActorState.UnstableGrounded:
                    _currentMotion.acceleration = PlanarMovement.unstableGroundedAcceleration;
                    _currentMotion.deceleration = PlanarMovement.unstableGroundedDeceleration;
                    _currentMotion.angleAccelerationMultiplier = PlanarMovement.unstableGroundedAngleAccelerationBoost.Evaluate(angleCurrentTargetVelocity);

                    break;
                case CharacterActorState.NotGrounded:
                    if (_reducedAirControlFlag)
                    {
                        float time = Time.time - _reducedAirControlInitialTime;
                        if (time <= _reductionDuration)
                        {
                            _currentMotion.acceleration = (PlanarMovement.notGroundedAcceleration / _reductionDuration) * time;
                            _currentMotion.deceleration = (PlanarMovement.notGroundedDeceleration / _reductionDuration) * time;
                        }
                        else
                        {
                            _reducedAirControlFlag = false;

                            _currentMotion.acceleration = PlanarMovement.notGroundedAcceleration;
                            _currentMotion.deceleration = PlanarMovement.notGroundedDeceleration;
                        }

                    }
                    else
                    {
                        _currentMotion.acceleration = PlanarMovement.notGroundedAcceleration;
                        _currentMotion.deceleration = PlanarMovement.notGroundedDeceleration;
                    }

                    _currentMotion.angleAccelerationMultiplier = PlanarMovement.notGroundedAngleAccelerationBoost.Evaluate(angleCurrentTargetVelocity);

                    break;
            }

            // Material values
            if (MaterialController != null)
            {
                if (CharacterActor.IsGrounded)
                {
                    _currentMotion.acceleration *= MaterialController.CurrentSurface.accelerationMultiplier * MaterialController.CurrentVolume.accelerationMultiplier;
                    _currentMotion.deceleration *= MaterialController.CurrentSurface.decelerationMultiplier * MaterialController.CurrentVolume.decelerationMultiplier;
                }
                else
                {
                    _currentMotion.acceleration *= MaterialController.CurrentVolume.accelerationMultiplier;
                    _currentMotion.deceleration *= MaterialController.CurrentVolume.decelerationMultiplier;
                }
            }
        }

        /// <summary>
        /// Processes the lateral movement of the character (stable and unstable state), that is, walk, run, crouch, etc. 
        /// This movement is tied directly to the "movement" character action.
        /// </summary>
        protected virtual void ProcessPlanarMovement(float dt)
        {
            //SetMotionValues();

            float speedMultiplier = MaterialController != null ?
            MaterialController.CurrentSurface.speedMultiplier * MaterialController.CurrentVolume.speedMultiplier : 1f;


            bool needToAccelerate = CustomUtilities.Multiply(CCProBrain.InputMovementReference, _currentPlanarSpeedLimit).sqrMagnitude >= CharacterActor.PlanarVelocity.sqrMagnitude;

            Vector3 targetPlanarVelocity = default;
            switch (CharacterActor.CurrentState)
            {
                case CharacterActorState.NotGrounded:

                    if (CharacterActor.WasGrounded)
                        _currentPlanarSpeedLimit = Mathf.Max(CharacterActor.PlanarVelocity.magnitude, PlanarMovement.baseSpeedLimit);

                    targetPlanarVelocity = CustomUtilities.Multiply(CCProBrain.InputMovementReference, speedMultiplier, _currentPlanarSpeedLimit);

                    break;
                case CharacterActorState.StableGrounded:
                    // Run ------------------------------------------------------------
                    if (PlanarMovement.runInputMode == InputMode.Toggle)
                    {
                        if (CharacterActions.run.Started)
                            _wantToRun = !_wantToRun;
                    }
                    else
                    {
                        _wantToRun = CharacterActions.run.value;
                    }

                    if (_wantToCrouch || !PlanarMovement.canRun)
                        _wantToRun = false;

                    if (_isCrouched)
                    {
                        _currentPlanarSpeedLimit = PlanarMovement.baseSpeedLimit * Crouch.speedMultiplier;
                    }
                    else
                    {
                        _currentPlanarSpeedLimit = _wantToRun ? PlanarMovement.boostSpeedLimit : PlanarMovement.baseSpeedLimit;
                    }

                    targetPlanarVelocity = CustomUtilities.Multiply(CCProBrain.InputMovementReference, speedMultiplier, _currentPlanarSpeedLimit);

                    break;
                case CharacterActorState.UnstableGrounded:
                    _currentPlanarSpeedLimit = PlanarMovement.baseSpeedLimit;
                    targetPlanarVelocity = CustomUtilities.Multiply(CCProBrain.InputMovementReference, speedMultiplier, _currentPlanarSpeedLimit);
                    break;
            }

            SetMotionValues(targetPlanarVelocity);


            float acceleration = _currentMotion.acceleration;


            if (needToAccelerate)
            {
                acceleration *= _currentMotion.angleAccelerationMultiplier;
            }
            else
            {
                acceleration = _currentMotion.deceleration;
            }

            CharacterActor.PlanarVelocity = Vector3.MoveTowards(
                CharacterActor.PlanarVelocity,
                targetPlanarVelocity,
                acceleration * dt
            );
        }

        protected virtual void ProcessGravity(float dt)
        {
            if (!VerticalMovement.useGravity)
                return;


            VerticalMovement.UpdateParameters();

            float gravityMultiplier = 1f;

            if (MaterialController != null)
                gravityMultiplier = CharacterActor.LocalVelocity.y >= 0 ?
                    MaterialController.CurrentVolume.gravityAscendingMultiplier :
                    MaterialController.CurrentVolume.gravityDescendingMultiplier;

            float gravity = gravityMultiplier * VerticalMovement.gravity;


            if (!CharacterActor.IsStable)
                CharacterActor.VerticalVelocity += CustomUtilities.Multiply(-CharacterActor.Up, gravity, dt);
        }

        protected JumpResult CanJump()
        {
            JumpResult jumpResult = JumpResult.Invalid;

            if (!VerticalMovement.canJump)
                return jumpResult;

            if (_isCrouched)
                return jumpResult;

            if (VerticalMovement.canJumpDown && CharacterActor.IsGrounded && CharacterActions.movement.Down && CharacterActions.jump.Started)
            {
                return jumpResult;
            }

            switch (CharacterActor.CurrentState)
            {
                case CharacterActorState.StableGrounded:
                    if (CharacterActions.jump.StartedElapsedTime <= VerticalMovement.preGroundedJumpTime && _groundedJumpAvailable)
                        jumpResult = JumpResult.Grounded;
                    break;

                case CharacterActorState.NotGrounded:

                    if (CharacterActions.jump.Started)
                    {
                        // First check if the "grounded jump" is available. If so, execute a "coyote jump".
                        if (CharacterActor.NotGroundedTime <= VerticalMovement.postGroundedJumpTime && _groundedJumpAvailable)
                        {
                            jumpResult = JumpResult.Grounded;
                        }
                        else if (_notGroundedJumpsLeft != 0)  // Do a not grounded jump
                        {
                            jumpResult = JumpResult.NotGrounded;
                        }
                    }

                    break;
                case CharacterActorState.UnstableGrounded:

                    if (CharacterActions.jump.StartedElapsedTime <= VerticalMovement.preGroundedJumpTime && VerticalMovement.canJumpOnUnstableGround)
                        jumpResult = JumpResult.Grounded;

                    break;
            }

            return jumpResult;
        }

        protected virtual void ProcessJump(float dt)
        {
            ProcessRegularJump(dt);
            ProcessJumpDown(dt);
        }

        #region JumpDown

        protected virtual bool ProcessJumpDown(float dt)
        {
            if (!VerticalMovement.canJumpDown)
                return false;

            if (!CharacterActor.IsStable)
                return false;

            if (!CharacterActor.IsGroundAOneWayPlatform)
                return false;

            if (VerticalMovement.filterByTag)
            {
                if (!CharacterActor.GroundObject.CompareTag(VerticalMovement.jumpDownTag))
                    return false;
            }

            if (!ProcessJumpDownAction())
                return false;

            JumpDown(dt);

            return true;
        }

        protected virtual bool ProcessJumpDownAction()
        {
            return (_isCrouched || CharacterActions.movement.Down) && CharacterActions.jump.Started;
        }

        protected virtual void JumpDown(float dt)
        {
            float groundDisplacementExtraDistance = 0f;

            Vector3 groundDisplacement = CustomUtilities.Multiply(CharacterActor.GroundVelocity, dt);

            if (!CharacterActor.IsGroundAscending)
                groundDisplacementExtraDistance = groundDisplacement.magnitude;

            CharacterActor.ForceNotGrounded();

            CharacterActor.Position -=
                CustomUtilities.Multiply(
                    CharacterActor.Up,
                    CharacterConstants.ColliderMinBottomOffset + VerticalMovement.jumpDownDistance + groundDisplacementExtraDistance
                );

            CharacterActor.VerticalVelocity -= CustomUtilities.Multiply(CharacterActor.Up, VerticalMovement.jumpDownVerticalVelocity);
        }

        #endregion


        #region Jump

        protected virtual void ProcessRegularJump(float dt)
        {
            if (CharacterActor.IsGrounded)
            {
                _notGroundedJumpsLeft = VerticalMovement.availableNotGroundedJumps;

                _groundedJumpAvailable = true;
            }

            if (_isAllowedToCancelJump)
            {
                if (VerticalMovement.cancelJumpOnRelease)
                {
                    if (CharacterActions.jump.StartedElapsedTime >= VerticalMovement.cancelJumpMaxTime || CharacterActor.IsFalling)
                    {
                        _isAllowedToCancelJump = false;
                    }
                    else if (!CharacterActions.jump.value && CharacterActions.jump.StartedElapsedTime >= VerticalMovement.cancelJumpMinTime)
                    {
                        // Get the velocity mapped onto the current jump direction
                        Vector3 projectedJumpVelocity = Vector3.Project(CharacterActor.Velocity, _jumpDirection);

                        CharacterActor.Velocity -= CustomUtilities.Multiply(projectedJumpVelocity, 1f - VerticalMovement.cancelJumpMultiplier);

                        _isAllowedToCancelJump = false;
                    }
                }
            }
            else
            {
                JumpResult jumpResult = CanJump();

                switch (jumpResult)
                {
                    case JumpResult.Grounded:
                        _groundedJumpAvailable = false;

                        break;
                    case JumpResult.NotGrounded:
                        _notGroundedJumpsLeft--;

                        break;

                    case JumpResult.Invalid:
                        return;
                }

                // Events ---------------------------------------------------
                if (CharacterActor.IsGrounded)
                {

                    OnGroundedJumpPerformed?.Invoke(true);
                }
                else
                {
                    OnNotGroundedJumpPerformed?.Invoke(_notGroundedJumpsLeft);
                }

                OnJumpPerformed?.Invoke();

                // Define the jump direction ---------------------------------------------------
                _jumpDirection = SetJumpDirection();

                // Force "not grounded" state.     
                if (CharacterActor.IsGrounded)
                    CharacterActor.ForceNotGrounded();

                // First remove any velocity associated with the jump direction.
                CharacterActor.Velocity -= Vector3.Project(CharacterActor.Velocity, _jumpDirection);
                CharacterActor.Velocity += CustomUtilities.Multiply(_jumpDirection, VerticalMovement.jumpSpeed);

                if (VerticalMovement.cancelJumpOnRelease)
                    _isAllowedToCancelJump = true;
            }
        }

        /// <summary>
        /// Returns the jump direction vector whenever the jump action is started.
        /// </summary>
        protected virtual Vector3 SetJumpDirection()
        {
            return CharacterActor.Up;
        }

        #endregion        

        void ProcessVerticalMovement(float dt)
        {
            ProcessGravity(dt);
            ProcessJump(dt);
        }


        protected virtual void HandleRotation(float dt)
        {
            // _flip2D.EvaluateAndFlipHorizontally(CharacterActions.movement.value.x);
            HandleLookingDirection(dt);
        }

        void HandleLookingDirection(float dt)
        {
            if (!LookingDirection.changeLookingDirection)
                return;

            switch (LookingDirection.lookingDirectionMode)
            {
                case LookingDirectionParameters.LookingDirectionMode.Movement:

                    switch (CharacterActor.CurrentState)
                    {
                        case CharacterActorState.NotGrounded:

                            SetTargetLookingDirection(LookingDirection.notGroundedLookingDirectionMode);

                            break;
                        case CharacterActorState.StableGrounded:

                            SetTargetLookingDirection(LookingDirection.stableGroundedLookingDirectionMode);

                            break;
                        case CharacterActorState.UnstableGrounded:

                            SetTargetLookingDirection(LookingDirection.unstableGroundedLookingDirectionMode);

                            break;
                    }

                    break;

                case LookingDirectionParameters.LookingDirectionMode.ExternalReference:

                    if (!CharacterActor.CharacterBody.Is2D)
                        _targetLookingDirection = CCProBrain.MovementReferenceForward;

                    break;

                case LookingDirectionParameters.LookingDirectionMode.Target:

                    _targetLookingDirection = (LookingDirection.target.position - CharacterActor.Position);
                    _targetLookingDirection.Normalize();

                    break;
            }

            Quaternion targetDeltaRotation = Quaternion.FromToRotation(CharacterActor.Forward, _targetLookingDirection);
            Quaternion currentDeltaRotation = Quaternion.Slerp(Quaternion.identity, targetDeltaRotation, LookingDirection.speed * dt);

            if (CharacterActor.CharacterBody.Is2D)
                CharacterActor.SetYaw(_targetLookingDirection);
            else
                CharacterActor.SetYaw(currentDeltaRotation * CharacterActor.Forward);
        }

        void SetTargetLookingDirection(LookingDirectionParameters.LookingDirectionMovementSource lookingDirectionMode)
        {
            if (lookingDirectionMode == LookingDirectionParameters.LookingDirectionMovementSource.Input)
            {
                if (CCProBrain.InputMovementReference != Vector3.zero)
                    _targetLookingDirection = CCProBrain.InputMovementReference;
                else
                    _targetLookingDirection = CharacterActor.Forward;
            }
            else
            {
                if (CharacterActor.PlanarVelocity != Vector3.zero)
                    _targetLookingDirection = Vector3.ProjectOnPlane(CharacterActor.PlanarVelocity, CharacterActor.Up);
                else
                    _targetLookingDirection = CharacterActor.Forward;
            }
        }


        protected virtual void HandleSize(float dt)
        {
            // Get the crouch input state 
            if (Crouch.enableCrouch)
            {
                if (Crouch.inputMode == InputMode.Toggle)
                {
                    if (CharacterActions.crouch.Started)
                        _wantToCrouch = !_wantToCrouch;
                }
                else
                {
                    _wantToCrouch = CharacterActions.crouch.value;
                }

                if (!Crouch.notGroundedCrouch && !CharacterActor.IsGrounded)
                    _wantToCrouch = false;

                if (CharacterActor.IsGrounded && _wantToRun)
                    _wantToCrouch = false;
            }
            else
            {
                _wantToCrouch = false;
            }

            if (_wantToCrouch)
                HandleCrouch(dt);
            else
                StandUp(dt);
        }

        void HandleCrouch(float dt)
        {
            CharacterActor.SizeReferenceType sizeReferenceType = CharacterActor.IsGrounded ?
                CharacterActor.SizeReferenceType.Bottom : Crouch.notGroundedReference;

            bool validSize = CharacterActor.CheckAndInterpolateHeight(
                CharacterActor.DefaultBodySize.y * Crouch.heightRatio,
                Crouch.sizeLerpSpeed * dt, sizeReferenceType);

            if (validSize)
                _isCrouched = true;
        }

        void StandUp(float dt)
        {
            CharacterActor.SizeReferenceType sizeReferenceType = CharacterActor.IsGrounded ?
                CharacterActor.SizeReferenceType.Bottom : Crouch.notGroundedReference;

            bool validSize = CharacterActor.CheckAndInterpolateHeight(
                CharacterActor.DefaultBodySize.y,
                Crouch.sizeLerpSpeed * dt, sizeReferenceType);

            if (validSize)
                _isCrouched = false;
        }


        protected virtual void HandleVelocity(float dt)
        {
            ProcessVerticalMovement(dt);
            ProcessPlanarMovement(dt);
        }
    }
}