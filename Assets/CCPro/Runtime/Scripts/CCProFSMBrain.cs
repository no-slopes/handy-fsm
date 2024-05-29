using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Implementation;
using UnityEngine;

namespace IndieGabo.HandyFSM.CCPro
{
    /// <summary>
    /// An implementation of the FSMBrain for the Character Controller Pro (CCPro) Asset.
    /// </summary>
    [AddComponentMenu("HandyFSM/CCProFSMBrain")]
    public class CCProFSMBrain : FSMBrain
    {
        #region Inspector

        [SerializeField]
        protected Animator _animator;

        [SerializeField]
        protected CharacterActor _characterActor;

        [SerializeField]
        MovementReferenceParameters _movementReferenceParameters = new();

        #endregion

        #region Fields

        #endregion

        #region Getters

        public Animator Animator => _animator;
        public CharacterActor CharacterActor => _characterActor;
        public MovementReferenceParameters MovementReferenceParameters => _movementReferenceParameters;
        bool CanCurrentStateOverrideAnimatorController()
        {
            if (CurrentState is not ICCProState state) return false;
            return state.OverrideAnimatorController && state.RuntimeAnimatorController != null && _animator != null;
        }

        #endregion

        #region Properties

        public bool UseRootMotion
        {
            get => CharacterActor.UseRootMotion;
            set => CharacterActor.UseRootMotion = value;
        }

        public bool UpdateRootPosition
        {
            get => CharacterActor.UpdateRootPosition;
            set => CharacterActor.UpdateRootPosition = value;
        }

        public bool UpdateRootRotation
        {
            get => CharacterActor.UpdateRootRotation;
            set => CharacterActor.UpdateRootRotation = value;
        }

        #endregion

        #region FSM Brain Messages

        /// <summary>
        /// Before the brain is initialized.
        /// </summary>
        protected virtual void BeforeInitialized()
        {

        }

        /// <summary>
        /// After the brain is initialized. It is not turned on yet.
        /// </summary>
        protected virtual void AfterInitialized()
        {
            _movementReferenceParameters.UpdateData(Vector2.right);
        }

        #endregion

        #region Behaviour

        protected virtual void OnEnable()
        {
            CharacterActor.OnPreSimulation += PreCharacterSimulation;
            CharacterActor.OnPostSimulation += PostCharacterSimulation;

            if (Animator != null)
                CharacterActor.OnAnimatorIKEvent += OnAnimatorIK;
        }

        protected override void OnDisable()
        {
            CharacterActor.OnPreSimulation -= PreCharacterSimulation;
            CharacterActor.OnPostSimulation -= PostCharacterSimulation;

            if (Animator != null)
                CharacterActor.OnAnimatorIKEvent -= OnAnimatorIK;

            base.OnDisable();
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update() { }
        protected override void LateUpdate() { }

        protected override void FixedUpdate()
        {
            if (!IsOn) return;

            _movementReferenceParameters.UpdateData(Vector2.right);

            EvaluateTransition();
            (_currentState as ICCProState)?.PreFixedTick();
            _currentState?.FixedTick();
            (_currentState as ICCProState)?.PostFixedTick();
        }

        #endregion       

        #region Machine's Logic
        /// <summary>
        /// Changes the state.
        /// </summary>
        /// <param name="state">The new state to change to.</param>
        protected override void ChangeState(IState state)
        {
            // Do not change state if it is the same as the current state or null
            if (state == _currentState || state == null) return;

            // Define the previous state
            _previousState = _currentState;

            // Invoke the exit action of the current state
            _currentState?.Exit();

            // Change the current state
            _currentState = state;

            if (CanCurrentStateOverrideAnimatorController())
                Animator.runtimeAnimatorController = (CurrentState as ICCProState).RuntimeAnimatorController;

            // Announce the new state
            _stateChanged.Invoke(_currentState, _previousState);

            // Invoke the enter action of the new state
            _currentState.Enter();

            // Update the current state name
            _currentStateName = CurrentState.Name;
        }

        /// <summary>
        /// Handles the tick event.
        /// </summary>
        protected override void EvaluateTransition()
        {
            if (_currentState == null) return;

            // Evaluate the next state
            if (_currentState.ShouldTransition(out IState targetState))
            {
                RequestStateChange(targetState);
            }
        }

        #endregion

        #region Actor Stuff

        void PreCharacterSimulation(float dt) => (CurrentState as ICCProState).PreCharacterSimulation(dt);
        void PostCharacterSimulation(float dt) => (CurrentState as ICCProState).PostCharacterSimulation(dt);

        /// <summary>
        /// Sets a flag that resets all the IK weights (hands and feet) during the next OnAnimatorIK call.
        /// </summary>
        public void ResetIKWeights()
        {
            CharacterActor.ResetIKWeights();
        }

        #endregion

        #region Animation stuff        

        void OnAnimatorIK(int layerIndex)
        {
            CurrentState?.TickIK(layerIndex);
        }

        #endregion
    }
}
