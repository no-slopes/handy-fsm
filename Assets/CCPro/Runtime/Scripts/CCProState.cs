using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public void PreCharacterSimulation(float dt) { OnPreCharacterSimulationAction?.Invoke(dt); }
        public void PostCharacterSimulation(float dt) { OnPostCharacterSimulationAction?.Invoke(dt); }
        public void PreFixedTick() { OnPreFixedTickAction?.Invoke(); }
        public void PostFixedTick() { OnPostFixedTickAction?.Invoke(); }

        protected UnityAction<float> OnPreCharacterSimulationAction { get; private set; }
        protected UnityAction<float> OnPostCharacterSimulationAction { get; private set; }
        protected UnityAction OnPreFixedTickAction { get; private set; }
        protected UnityAction OnPostFixedTickAction { get; private set; }

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
        }

        #endregion
    }
}