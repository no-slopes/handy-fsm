using System;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Demo;
using Lightbug.CharacterControllerPro.Implementation;
using UnityEngine;
using UnityEngine.Events;

namespace IndieGabo.HandyFSM.CCPro
{
    public abstract class ScriptableCCProState : ScriptableState, ICCProState
    {
        #region Fields

        #endregion

        #region  Getters

        protected FSMBrain CCProBrain => _brain;
        protected Animator Animator => _brain?.Animator;
        protected CharacterActor CharacterActor => _brain?.CharacterActor as CharacterActor;
        protected CharacterBrain CharacterBrain => _brain?.CharacterBrain as CharacterBrain;
        protected MaterialController MaterialController => _brain?.MaterialController as MaterialController;
        protected CharacterActions CharacterActions => CharacterBrain?.CharacterActions ?? default;
        protected Vector3 InputMovementReference => _brain?.InputMovementReference ?? Vector3.zero;
        protected Vector3 MovementReferenceForward => _brain?.MovementReferenceForward ?? Vector3.forward;
        protected Vector3 MovementReferenceRight => _brain?.MovementReferenceRight ?? Vector3.right;

        #endregion

        #region Cycle Methods
        public override void Initialize(FSMBrain brain)
        {
            _brain = brain;
            ValidateConfiguration();
            SortTransitions();
            Type type = GetType();
            LoadActions(type);

            try
            {
                OnInitAction?.Invoke();
            }
            catch (StateFailureException exception)
            {
                throw exception.WithState(this);
            }
        }

        public void PreCharacterSimulation(float dt)
        {
            InvokeCCProAction(OnPreCharacterSimulationAction, dt);
        }

        public void PostCharacterSimulation(float dt)
        {
            InvokeCCProAction(OnPostCharacterSimulationAction, dt);
        }

        public void PreFixedTick()
        {
            InvokeCCProAction(OnPreFixedTickAction);
        }

        public void PostFixedTick()
        {
            InvokeCCProAction(OnPostFixedTickAction);
        }

        public virtual void TickIK(int layerIndex)
        {
            InvokeCCProAction(OnTickIKAction, layerIndex);
        }

        protected UnityAction<float> OnPreCharacterSimulationAction { get; private set; }
        protected UnityAction<float> OnPostCharacterSimulationAction { get; private set; }
        protected UnityAction OnPreFixedTickAction { get; private set; }
        protected UnityAction OnPostFixedTickAction { get; private set; }
        protected UnityAction<int> OnTickIKAction { get; private set; }

        #endregion

        #region Actions


        /// <summary>
        /// Loads methods as actions to be called during the state's lifecycle.
        /// </summary>
        protected override void LoadActions(Type type)
        {
            base.LoadActions(type);
            OnPreCharacterSimulationAction = GetDelegate<UnityAction<float>>(type, "OnPreCharacterSimulation");
            OnPostCharacterSimulationAction = GetDelegate<UnityAction<float>>(type, "OnPostCharacterSimulation");
            OnPreFixedTickAction = GetDelegate<UnityAction>(type, "OnPreFixedTick");
            OnPostFixedTickAction = GetDelegate<UnityAction>(type, "OnPostFixedTick");
            OnTickIKAction = GetDelegate<UnityAction<int>>(type, "OnTickIK");
        }

        private void ValidateConfiguration()
        {
            if (_brain == null)
            {
                throw new StateFailureException(
                    "Character Controller Pro states require a valid FSMBrain instance.");
            }

            if (!_brain.UseCharacterControllerPro)
            {
                throw new StateFailureException(
                    "Character Controller Pro support is disabled on the FSMBrain.");
            }

            if (CharacterActor == null)
            {
                throw new StateFailureException(
                    "Character Controller Pro support requires a CharacterActor reference.");
            }

            if (CharacterBrain == null)
            {
                throw new StateFailureException(
                    "Character Controller Pro support requires a CharacterBrain reference.");
            }
        }

        private void InvokeCCProAction(UnityAction action)
        {
            try
            {
                action?.Invoke();
            }
            catch (StateFailureException exception)
            {
                throw exception.WithState(this);
            }
        }

        private void InvokeCCProAction(UnityAction<float> action, float value)
        {
            try
            {
                action?.Invoke(value);
            }
            catch (StateFailureException exception)
            {
                throw exception.WithState(this);
            }
        }

        private void InvokeCCProAction(UnityAction<int> action, int value)
        {
            try
            {
                action?.Invoke(value);
            }
            catch (StateFailureException exception)
            {
                throw exception.WithState(this);
            }
        }

        #endregion
    }
}