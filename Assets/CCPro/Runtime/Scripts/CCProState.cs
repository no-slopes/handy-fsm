using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Demo;
using Lightbug.CharacterControllerPro.Implementation;
using UnityEngine;
using UnityEngine.Events;

namespace IndieGabo.HandyFSM.CCPro
{
    /// <summary>
    /// Represents a State controlled by the StateMachine class.
    /// </summary>
    [Serializable]
    public abstract class CCProState : State, ICCProState
    {

        #region Fields

        protected CCProFSMBrain _ccProbrain;

        #endregion

        #region Getters

        protected CCProFSMBrain CCProBrain => _ccProbrain;
        protected CharacterActor CharacterActor => _ccProbrain.CharacterActor;
        protected MaterialController MaterialController => _ccProbrain.MaterialController;
        protected CharacterActions CharacterActions => _ccProbrain.CharacterBrain.CharacterActions;

        #endregion

        #region Cycle Methods

        public override void Initialize(FSMBrain brain)
        {
            _brain = brain;
            _ccProbrain = _brain as CCProFSMBrain;
            SortTransitions();
            Type type = GetType();
            LoadActions(type);
            OnInitAction?.Invoke();
        }

        public virtual void PreCharacterSimulation(float dt) { OnPreCharacterSimulationAction?.Invoke(dt); }
        public virtual void PostCharacterSimulation(float dt) { OnPostCharacterSimulationAction?.Invoke(dt); }
        public virtual void PreFixedTick() { OnPreFixedTickAction?.Invoke(); }
        public virtual void PostFixedTick() { OnPostFixedTickAction?.Invoke(); }
        public virtual void TickIK(int layerIndex) { OnTickIKAction?.Invoke(layerIndex); }

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

        #endregion
    }
}