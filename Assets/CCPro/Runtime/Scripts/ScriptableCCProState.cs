using System;
using UnityEngine;
using UnityEngine.Events;

namespace IndieGabo.HandyFSM.CCPro
{
    public abstract class ScriptableCCProState : ScriptableState, ICCProState
    {
        #region Inspector

        [SerializeField]
        private bool _overrideRuntimeAnimatorController;

        [SerializeField]
        private RuntimeAnimatorController _runtimeAnimatorController;

        #endregion

        #region Fields

        #endregion

        #region  Getters

        #endregion

        #region Cycle Methods

        public void PreCharacterSimulation(float dt) { OnPreCharacterSimulationAction?.Invoke(dt); }
        public void PostCharacterSimulation(float dt) { OnPostCharacterSimulationAction?.Invoke(dt); }
        public void PreFixedTick() { OnPreFixedTickAction?.Invoke(); }
        public void PostFixedTick() { OnPostFixedTickAction?.Invoke(); }

        protected UnityAction<float> OnPreCharacterSimulationAction { get; private set; }
        protected UnityAction<float> OnPostCharacterSimulationAction { get; private set; }
        protected UnityAction OnPreFixedTickAction { get; private set; }
        protected UnityAction OnPostFixedTickAction { get; private set; }

        public bool OverrideAnimatorController => _overrideRuntimeAnimatorController;
        public RuntimeAnimatorController RuntimeAnimatorController => _runtimeAnimatorController;

        #endregion

        #region Actions


        /// <summary>
        /// Loads methods as actions to be called during the state's lifecycle.
        /// </summary>
        protected override void LoadActions()
        {
            base.LoadActions();
            Type stateType = GetType();
            OnPreCharacterSimulationAction = GetDelegate<UnityAction<float>>(stateType, "OnPreCharacterSimulation");
            OnPostCharacterSimulationAction = GetDelegate<UnityAction<float>>(stateType, "OnPostCharacterSimulation");
            OnPreFixedTickAction = GetDelegate<UnityAction>(stateType, "OnPreFixedTick");
            OnPostFixedTickAction = GetDelegate<UnityAction>(stateType, "OnPostFixedTick");
        }

        #endregion
    }
}