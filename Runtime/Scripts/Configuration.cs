using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandyFSM
{
    [System.Serializable]
    public class Configuration
    {
        #region Inspector

        [SerializeField]
        private InitializationMode _initalizationMode = InitializationMode.Automatic;

        [SerializeField]
        private List<ScriptableState> _scriptableStates;

        [SerializeField]
        private ScriptableState _defaultScriptableState;

        [SerializeField]
        private UnityEvent<MachineStatus> _statusChanged;

        [SerializeField]
        private UnityEvent<IState, IState> _stateChanged;

        #endregion

        #region Properties

        public InitializationMode InitalizationMode { get => _initalizationMode; set => _initalizationMode = value; }
        public List<ScriptableState> ScriptableStates { get => _scriptableStates; set => _scriptableStates = value; }
        public ScriptableState DefaultScriptableState { get => _defaultScriptableState; set => _defaultScriptableState = value; }
        public UnityEvent<MachineStatus> StatusChanged { get => _statusChanged; set => _statusChanged = value; }
        public UnityEvent<IState, IState> StateChanged { get => _stateChanged; set => _stateChanged = value; }

        #endregion
    }

}