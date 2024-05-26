using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandyFSM
{
    public class Configuration : ScriptableObject
    {
        #region Inspector

        [SerializeField]
        private Transform _owner;

        [SerializeField]
        private InitializationMode _initializationMode = InitializationMode.Automatic;

        [SerializeField]
        private ScriptableState _defaultScriptableState;

        [SerializeField]
        private List<ScriptableState> _scriptableStates;

        [SerializeField]
        private List<Signal> _signals = new();

        [SerializeField]
        private List<string> _triggers = new();

        [SerializeField]
        private UnityEvent<MachineStatus> _statusChanged;

        [SerializeField]
        private UnityEvent<IState, IState> _stateChanged;

        #endregion

        #region Properties

        public Transform Owner { get => _owner; set => _owner = value; }
        public InitializationMode InitializationMode { get => _initializationMode; set => _initializationMode = value; }
        public ScriptableState DefaultScriptableState { get => _defaultScriptableState; set => _defaultScriptableState = value; }
        public List<ScriptableState> ScriptableStates { get => _scriptableStates; set => _scriptableStates = value; }
        public List<Signal> Signals { get => _signals; set => _signals = value; }
        public List<string> Triggers { get => _triggers; set => _triggers = value; }
        public UnityEvent<MachineStatus> StatusChanged { get => _statusChanged; set => _statusChanged = value; }
        public UnityEvent<IState, IState> StateChanged { get => _stateChanged; set => _stateChanged = value; }

        #endregion
    }

}