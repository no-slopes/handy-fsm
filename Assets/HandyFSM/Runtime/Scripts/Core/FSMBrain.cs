using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;

namespace IndieGabo.HandyFSM
{
    /// <summary>
    /// The state machine base class
    /// </summary>
    [AddComponentMenu("HandyFSM/FSMBrain")]
    public class FSMBrain : MonoBehaviour
    {
#if UNITY_EDITOR
        // [ContextMenu("Open Visualizer")]
        // void DoSomething()
        // {
        //     var window = MachineStateVisualizerWindow.OpenEditorWindow();
        //     window.MachineSelectorField.value = this;
        // }
#endif

        #region Inspector

        /// <summary>
        /// The current machine's status of the MachineStatus enum type. 
        /// </summary>
        [SerializeField]
        protected MachineStatus _status = MachineStatus.Off;

        /// <summary>
        /// The current state name
        /// </summary>
        [SerializeField]
        protected string _currentStateName = "None";

        [SerializeField]
        protected Transform _owner;

        /// <summary>
        /// Enables Simple Blackboard integration for this machine when the package is installed.
        /// </summary>
        [SerializeField]
        protected bool _useSimpleBlackboard;

        /// <summary>
        /// The Simple Blackboard container used by this machine and its states.
        /// </summary>
        [SerializeField]
        protected Component _blackboardContainer;

        /// <summary>
        /// Enables Character Controller Pro integration for this machine when
        /// the package is installed.
        /// </summary>
        [SerializeField]
        protected bool _useCharacterControllerPro;

        /// <summary>
        /// Optional animator reference used by the Character Controller Pro integration.
        /// </summary>
        [SerializeField]
        protected Animator _animator;

        /// <summary>
        /// The Character Controller Pro actor used by this machine.
        /// </summary>
        [SerializeField]
        protected Component _characterActor;

        /// <summary>
        /// The Character Controller Pro material controller used by this machine.
        /// </summary>
        [SerializeField]
        protected Component _materialController;

        /// <summary>
        /// The Character Controller Pro character brain used by this machine.
        /// </summary>
        [SerializeField]
        protected Component _characterBrain;

        /// <summary>
        /// Determines how movement reference data should be generated for the
        /// optional Character Controller Pro integration.
        /// </summary>
        [SerializeField]
        protected CharacterControllerProMovementReferenceMode _movementReferenceMode =
            CharacterControllerProMovementReferenceMode.World;

        /// <summary>
        /// Optional external movement reference transform used by the Character
        /// Controller Pro integration.
        /// </summary>
        [SerializeField]
        protected Transform _externalReference;

        [SerializeField]
        protected InitializationMode _initializationMode = InitializationMode.Automatic;

        [SerializeField]
        protected bool _transitionsOnUpdate;

        [SerializeField]
        protected bool _transitionsOnLateUpdate;

        [SerializeField]
        protected bool _transitionsOnFixedUpdate;

        /// <summary>
        /// Enables editor-side history capture for state debugging.
        /// </summary>
        [SerializeField]
        protected bool _saveHistory;

        [SerializeField]
        protected ScriptableState _defaultScriptableState;

        [SerializeField]
        protected List<ScriptableState> _scriptableStates;

        [SerializeField]
        protected UnityEvent<MachineStatus> _statusChanged;

        [SerializeField]
        protected UnityEvent<IState, IState> _stateChanged;

        #endregion

        #region Fields        

        protected IState _defaultState;
        protected IState _firstEnteredState;

        protected IState _currentState;
        protected IState _previousState;
        protected StateTransitionReport _lastTransitionReport;

        protected bool _isInitialized;
        protected bool _isCharacterControllerProInitialized;
        protected StateProvider _stateProvider;
        protected TriggersProvider _triggersProvider;

        protected Vector3 _inputMovementReference;
        protected Vector3 _movementReferenceForward = Vector3.forward;
        protected Vector3 _movementReferenceRight = Vector3.right;
        protected Vector3 _characterControllerInitialForward = Vector3.forward;
        protected Vector3 _characterControllerInitialRight = Vector3.right;

        protected readonly HashSet<IState> _faultedStates = new();

        private bool _isRecoveringFromStateFailure;
        private bool _isCharacterControllerProSubscribed;

        private Action<float> _characterControllerPreSimulationHandler;
        private Action<float> _characterControllerPostSimulationHandler;
        private Action<int> _characterControllerAnimatorIkHandler;

        private static readonly Dictionary<Type, CachedBrainLifecycleMethods>
            s_cachedBrainLifecycleMethods = new();

        private static readonly Dictionary<Type, CachedCharacterControllerProStateMethods>
            s_cachedCharacterControllerProStateMethods = new();

        #endregion

        #region Getters

        /// <summary>
        /// The state machine Owner trandform. If not defined on inspector it will be the Transform
        /// of the GameObject in which the script is attached
        /// </summary>
        public Transform Owner => _owner != null ? _owner : transform;

        /// <summary>
        /// Gets whether this machine should use the optional Simple Blackboard integration.
        /// </summary>
        public bool UseSimpleBlackboard =>
            _useSimpleBlackboard && IsSimpleBlackboardAvailable;

        /// <summary>
        /// Gets whether this machine should use the optional Character
        /// Controller Pro integration.
        /// </summary>
        public bool UseCharacterControllerPro =>
            _useCharacterControllerPro && IsCharacterControllerProAvailable;

        /// <summary>
        /// The configured Simple Blackboard container used by this machine.
        /// </summary>
        public Component BlackboardContainer =>
            UseSimpleBlackboard
                ? _blackboardContainer
                : null;

        /// <summary>
        /// Gets whether the Simple Blackboard package is available in the project.
        /// </summary>
        public static bool IsSimpleBlackboardAvailable => SimpleBlackboardBridge.IsAvailable;

        /// <summary>
        /// Gets the Simple Blackboard container type when the package is installed.
        /// </summary>
        public static Type SimpleBlackboardContainerType =>
            SimpleBlackboardBridge.ContainerType;

        /// <summary>
        /// Gets whether the Character Controller Pro package is available in the project.
        /// </summary>
        public static bool IsCharacterControllerProAvailable =>
            CharacterControllerProBridge.IsAvailable;

        /// <summary>
        /// Gets the Character Controller Pro actor type when the package is installed.
        /// </summary>
        public static Type CharacterActorType => CharacterControllerProBridge.CharacterActorType;

        /// <summary>
        /// Gets the Character Controller Pro material controller type when the package is installed.
        /// </summary>
        public static Type MaterialControllerType =>
            CharacterControllerProBridge.MaterialControllerType;

        /// <summary>
        /// Gets the Character Controller Pro character brain type when the package is installed.
        /// </summary>
        public static Type CharacterBrainType => CharacterControllerProBridge.CharacterBrainType;

        /// <summary>
        /// Gets whether this brain currently exposes a valid Simple Blackboard instance.
        /// </summary>
        public bool HasBlackboard =>
            UseSimpleBlackboard
            && SimpleBlackboardBridge.TryGetBlackboard(_blackboardContainer, out _);

        /// <summary>
        /// Gets the animator used by the optional Character Controller Pro integration.
        /// </summary>
        public Animator Animator => UseCharacterControllerPro ? _animator : null;

        /// <summary>
        /// Gets the configured Character Controller Pro actor component.
        /// </summary>
        public Component CharacterActor => UseCharacterControllerPro ? _characterActor : null;

        /// <summary>
        /// Gets the configured Character Controller Pro material controller component.
        /// </summary>
        public Component MaterialController =>
            UseCharacterControllerPro ? _materialController : null;

        /// <summary>
        /// Gets the configured Character Controller Pro character brain component.
        /// </summary>
        public Component CharacterBrain => UseCharacterControllerPro ? _characterBrain : null;

        /// <summary>
        /// Gets the current input movement reference computed for the optional
        /// Character Controller Pro integration.
        /// </summary>
        public Vector3 InputMovementReference =>
            UseCharacterControllerPro ? _inputMovementReference : Vector3.zero;

        /// <summary>
        /// Gets or sets the external reference used by the optional Character
        /// Controller Pro integration.
        /// </summary>
        public Transform ExternalReference
        {
            get => _externalReference;
            set => _externalReference = value;
        }

        /// <summary>
        /// Gets or sets the movement reference mode used by the optional
        /// Character Controller Pro integration.
        /// </summary>
        public CharacterControllerProMovementReferenceMode MovementReferenceMode
        {
            get => _movementReferenceMode;
            set => _movementReferenceMode = value;
        }

        /// <summary>
        /// Gets the forward vector used by the optional Character Controller Pro integration.
        /// </summary>
        public Vector3 MovementReferenceForward =>
            UseCharacterControllerPro ? _movementReferenceForward : Vector3.forward;

        /// <summary>
        /// Gets the right vector used by the optional Character Controller Pro integration.
        /// </summary>
        public Vector3 MovementReferenceRight =>
            UseCharacterControllerPro ? _movementReferenceRight : Vector3.right;

        /// <summary>
        /// Gets or sets whether root motion should be enabled on the configured
        /// Character Controller Pro actor.
        /// </summary>
        public bool UseRootMotion
        {
            get
            {
                return UseCharacterControllerPro
                    && CharacterControllerProBridge.TryGetUseRootMotion(
                        _characterActor,
                        out bool value)
                    && value;
            }
            set
            {
                if (!UseCharacterControllerPro)
                {
                    return;
                }

                CharacterControllerProBridge.TrySetUseRootMotion(_characterActor, value);
            }
        }

        /// <summary>
        /// Gets or sets whether root position updates should be enabled on the
        /// configured Character Controller Pro actor.
        /// </summary>
        public bool UpdateRootPosition
        {
            get
            {
                return UseCharacterControllerPro
                    && CharacterControllerProBridge.TryGetUpdateRootPosition(
                        _characterActor,
                        out bool value)
                    && value;
            }
            set
            {
                if (!UseCharacterControllerPro)
                {
                    return;
                }

                CharacterControllerProBridge.TrySetUpdateRootPosition(_characterActor, value);
            }
        }

        /// <summary>
        /// Gets or sets whether root rotation updates should be enabled on the
        /// configured Character Controller Pro actor.
        /// </summary>
        public bool UpdateRootRotation
        {
            get
            {
                return UseCharacterControllerPro
                    && CharacterControllerProBridge.TryGetUpdateRootRotation(
                        _characterActor,
                        out bool value)
                    && value;
            }
            set
            {
                if (!UseCharacterControllerPro)
                {
                    return;
                }

                CharacterControllerProBridge.TrySetUpdateRootRotation(_characterActor, value);
            }
        }

        /// <summary>
        /// Gets the blackboard exposed by the configured container.
        /// </summary>
        public object Blackboard =>
            TryGetActiveBlackboard(out object blackboard)
                ? blackboard
                : null;

        /// <summary>
        /// If the machine is already
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// If the machine is on
        /// </summary>
        public bool IsOn => _status == MachineStatus.On;

        /// <summary>
        /// If the machine is paused
        /// </summary>
        public bool IsPaused => _status == MachineStatus.Paused;

        /// <summary>
        /// If the machine is off
        /// </summary>
        public bool IsOff => _status == MachineStatus.Off;

        /// <summary>
        /// If the machine is working. Either On or Paused
        /// </summary>
        public bool IsWorking => IsOn || IsPaused;

        /// <summary>
        /// A getter for the machine's Status
        /// </summary>
        public MachineStatus Status => _status;

        /// <summary>
        /// This is the current active state for the this State Machine
        /// </summary>
        public IState CurrentState => _currentState;

        /// <summary>
        /// This is the immediate previous state the machine was in.
        /// </summary>
        public IState PreviousState => _previousState;

        /// <summary>
        /// Gets the reason that produced the latest successful state transition.
        /// </summary>
        public StateTransitionReason LastTransitionReason => _lastTransitionReport.Reason;

        /// <summary>
        /// Gets the latest successful transition report.
        /// </summary>
        public StateTransitionReport LastTransitionReport => _lastTransitionReport;

        /// <summary>
        /// Getter for the machine's default state
        /// </summary>
        public IState DefaultState => _defaultState;

        /// <summary>
        /// Gets the first state that entered successfully after the machine was turned on.
        /// </summary>
        public IState FirstEnteredState => _firstEnteredState;

        /// <summary>
        /// The triggers registered in this machine brain
        /// </summary>
        public TriggersProvider Triggers => _triggersProvider;

        /// <summary>
        /// Gets whether history capture is explicitly enabled for editor debugging.
        /// </summary>
        public bool SaveHistory => _saveHistory;

        /// <summary>
        /// Gets whether history capture should run in the current execution context.
        /// </summary>
        public bool ShouldCaptureHistory
        {
            get
            {
#if UNITY_EDITOR
                return _saveHistory;
#else
                return Debug.isDebugBuild;
#endif
            }
        }

        /// <summary>
        /// If CurrentStateName should be shown in the inspector
        /// </summary>
        protected bool ShowCurrentState => _status != MachineStatus.Off;

        // Events

        /// <summary>
        /// Whenever the machine status changes
        /// </summary>
        public UnityEvent<MachineStatus> StatusChanged => _statusChanged;

        /// <summary>
        /// Whenever the current state changes
        /// </summary>
        public UnityEvent<IState, IState> StateChanged => _stateChanged;

        /// <summary>
        /// Tries to read a typed value from the configured blackboard.
        /// </summary>
        /// <typeparam name="T">The value type to read.</typeparam>
        /// <param name="propertyName">The blackboard property name.</param>
        /// <param name="value">The resolved value if found.</param>
        /// <returns>True if the value exists and matches the requested type.</returns>
        public bool TryGetBlackboardValue<T>(string propertyName, out T value)
        {
            if (!TryGetActiveBlackboard(out object blackboard))
            {
                value = default;
                return false;
            }

            return SimpleBlackboardBridge.TryGetValue(
                blackboard,
                propertyName,
                out value);
        }

        /// <summary>
        /// Writes a typed value into the configured blackboard.
        /// </summary>
        /// <typeparam name="T">The value type to write.</typeparam>
        /// <param name="propertyName">The blackboard property name.</param>
        /// <param name="value">The value to store.</param>
        /// <returns>True if the value was written successfully.</returns>
        public bool SetBlackboardValue<T>(string propertyName, T value)
        {
            if (!TryGetActiveBlackboard(out object blackboard))
            {
                return false;
            }

            return SimpleBlackboardBridge.SetValue(
                blackboard,
                propertyName,
                value);
        }

        /// <summary>
        /// Tries to read an untyped value from the configured blackboard.
        /// </summary>
        /// <param name="propertyName">The blackboard property name.</param>
        /// <param name="value">The resolved value if found.</param>
        /// <returns>True if the value exists.</returns>
        public bool TryGetBlackboardObject(string propertyName, out object value)
        {
            if (!TryGetActiveBlackboard(out object blackboard))
            {
                value = null;
                return false;
            }

            return SimpleBlackboardBridge.TryGetObjectValue(
                blackboard,
                propertyName,
                out value);
        }

        /// <summary>
        /// Gets whether the configured blackboard contains a property.
        /// </summary>
        /// <param name="propertyName">The blackboard property name.</param>
        /// <returns>True if the property exists.</returns>
        public bool HasBlackboardValue(string propertyName)
        {
            if (!TryGetActiveBlackboard(out object blackboard))
            {
                return false;
            }

            return SimpleBlackboardBridge.ContainsValue(
                blackboard,
                propertyName);
        }

        /// <summary>
        /// Resolves the active blackboard only when the optional integration is enabled.
        /// </summary>
        /// <param name="blackboard">The resolved blackboard instance.</param>
        /// <returns>True if the optional blackboard is enabled and available.</returns>
        private bool TryGetActiveBlackboard(out object blackboard)
        {
            blackboard = null;

            return UseSimpleBlackboard
                && SimpleBlackboardBridge.TryGetBlackboard(
                    _blackboardContainer,
                    out blackboard);
        }

        #endregion

        #region Behaviour   

        protected virtual void Awake()
        {
            _status = MachineStatus.Off;
            _firstEnteredState = null;
            _lastTransitionReport = StateTransitionReport.Unknown;
            _faultedStates.Clear();
            _isRecoveringFromStateFailure = false;
            EnsureCharacterControllerProHandlers();
            ResetCharacterControllerProRuntimeState();

            _stateProvider = new StateProvider(this);
            _stateProvider.LoadStatesFromScriptablesList(_scriptableStates, false);

            _triggersProvider = new TriggersProvider(this);

            Type machineType = GetType();
            CachedBrainLifecycleMethods lifecycleMethods =
                GetCachedBrainLifecycleMethods(machineType);

            lifecycleMethods.InvokeBeforeInitialized(this);

            if (_defaultState == null && _defaultScriptableState != null)
                _defaultState = _stateProvider.Get(_defaultScriptableState.GetType());

            _stateProvider.InitializeAllStates();

            _isInitialized = true;

            lifecycleMethods.InvokeAfterInitialized(this);
        }

        protected virtual void OnEnable()
        {
            TrySubscribeCharacterControllerProCallbacks();
        }

        protected virtual void Start()
        {
            InitializeCharacterControllerProSupport();

            if (_initializationMode != InitializationMode.Automatic) return;

            if (_defaultState == null)
            {
                Debug.LogError($"The machine {name} is marked to initialize automatically but was unable to resolve a default state.", this);
                return;
            }

            TurnOn(_defaultState);
        }

        protected virtual void Update()
        {
            if (UseCharacterControllerPro)
            {
                return;
            }

            if (_status != MachineStatus.On) return;

            if (_transitionsOnUpdate)
                ExecuteMachineOperation(EvaluateTransition, _currentState, "Update transition evaluation");

            ExecuteStateOperation(_currentState, state => state.Tick(), "Update tick");
        }

        protected virtual void LateUpdate()
        {
            if (UseCharacterControllerPro)
            {
                return;
            }

            if (_status != MachineStatus.On) return;

            if (_transitionsOnLateUpdate)
                ExecuteMachineOperation(EvaluateTransition, _currentState, "LateUpdate transition evaluation");

            ExecuteStateOperation(_currentState, state => state.LateTick(), "LateUpdate tick");
        }

        protected virtual void FixedUpdate()
        {
            if (_status != MachineStatus.On) return;

            if (UseCharacterControllerPro)
            {
                UpdateCharacterControllerProSupport();

                ExecuteMachineOperation(
                    EvaluateTransition,
                    _currentState,
                    "FixedUpdate transition evaluation");

                TryExecuteCharacterControllerProPreFixedTick(_currentState);
                ExecuteStateOperation(_currentState, state => state.FixedTick(), "FixedUpdate tick");
                TryExecuteCharacterControllerProPostFixedTick(_currentState);
                return;
            }

            if (_transitionsOnFixedUpdate)
                ExecuteMachineOperation(EvaluateTransition, _currentState, "FixedUpdate transition evaluation");

            ExecuteStateOperation(_currentState, state => state.FixedTick(), "FixedUpdate tick");
        }

        protected virtual void OnDisable()
        {
            TryUnsubscribeCharacterControllerProCallbacks();
            Stop();
        }

        #endregion

        #region Machine Engine

        /// <summary>
        /// Turns the machine on and enters the given state
        /// </summary>
        /// <param name="stateType"></param>
        public virtual void TurnOn(Type stateType)
        {
            if (!_stateProvider.TryGet(stateType, out IState state))
            {
                Debug.LogError($"Trying to turn machine on but {stateType.Name} is not loaded.", this);
                return;
            }

            TurnOn(state);
        }

        /// <summary>
        /// Turns the machine on and enters the given state
        /// </summary>
        /// <param name="state"></param>
        public virtual void TurnOn(IState state)
        {
            if (IsWorking)
            {
                Debug.LogError($"Trying to turn machine on but it is already working", this);
                return;
            }

            if (!_stateProvider.IsLoaded(state))
            {
                Debug.LogError($"Trying to turn machine on but {nameof(state)} is not loaded.", this);
                return;
            }

            _firstEnteredState = null;
            _previousState = null;
            _lastTransitionReport = StateTransitionReport.Unknown;
            _faultedStates.Clear();
            _isRecoveringFromStateFailure = false;
            InitializeCharacterControllerProSupport();

            ChangeStatus(MachineStatus.On);
            ChangeState(
                state,
                new StateTransitionReport(StateTransitionReason.InitialEntry));
        }

        /// <summary>
        /// Pauses the machine
        /// </summary>
        public virtual void Resume()
        {
            if (!IsPaused) return;
            ChangeStatus(MachineStatus.On);
        }

        /// <summary>
        /// Pauses the machine
        /// </summary>
        public virtual void Pause()
        {
            if (!IsOn) return;
            ChangeStatus(MachineStatus.Paused);
        }

        /// <summary>
        /// Stops the machine
        /// </summary>
        public virtual void Stop()
        {
            if (!IsWorking) return;

            ExecuteStateOperation(_currentState, state => state.Exit(), "Stop exit");
            _currentState = null;
            _previousState = null;
            _firstEnteredState = null;
            _lastTransitionReport = StateTransitionReport.Unknown;
            _faultedStates.Clear();
            _isRecoveringFromStateFailure = false;
            ResetCharacterControllerProRuntimeState();

            ChangeStatus(MachineStatus.Off);
        }

        /// <summary>
        /// Initializes the optional Character Controller Pro support when the
        /// package is installed and the integration toggle is enabled.
        /// </summary>
        protected void InitializeCharacterControllerProSupport()
        {
            ResetCharacterControllerProRuntimeState();

            if (!UseCharacterControllerPro)
            {
                return;
            }

            if (!CharacterControllerProBridge.TryGetForward(
                    _characterActor,
                    out _characterControllerInitialForward))
            {
                _characterControllerInitialForward = Vector3.forward;
            }

            if (!CharacterControllerProBridge.TryGetRight(
                    _characterActor,
                    out _characterControllerInitialRight))
            {
                _characterControllerInitialRight = Vector3.right;
            }

            _isCharacterControllerProInitialized = true;
            UpdateCharacterControllerProMovementReference(Vector2.zero);
        }

        /// <summary>
        /// Updates the optional Character Controller Pro cached movement data.
        /// </summary>
        protected void UpdateCharacterControllerProSupport()
        {
            if (!UseCharacterControllerPro)
            {
                return;
            }

            if (!_isCharacterControllerProInitialized)
            {
                InitializeCharacterControllerProSupport();
            }

            if (!CharacterControllerProBridge.TryGetMovementInput(
                    _characterBrain,
                    out Vector2 movementInput))
            {
                movementInput = Vector2.zero;
            }

            UpdateCharacterControllerProMovementReference(movementInput);
        }

        /// <summary>
        /// Updates the cached movement reference vectors used by the optional
        /// Character Controller Pro integration.
        /// </summary>
        /// <param name="movementInput">The current movement input vector.</param>
        protected void UpdateCharacterControllerProMovementReference(Vector2 movementInput)
        {
            if (!UseCharacterControllerPro)
            {
                _inputMovementReference = Vector3.zero;
                _movementReferenceForward = Vector3.forward;
                _movementReferenceRight = Vector3.right;
                return;
            }

            if (!CharacterControllerProBridge.TryGetUp(_characterActor, out Vector3 up))
            {
                up = Vector3.up;
            }

            switch (_movementReferenceMode)
            {
                case CharacterControllerProMovementReferenceMode.Character:
                    _movementReferenceForward = _characterControllerInitialForward;
                    _movementReferenceRight = _characterControllerInitialRight;
                    break;

                case CharacterControllerProMovementReferenceMode.External:
                    if (_externalReference != null)
                    {
                        _movementReferenceForward =
                            Vector3.Normalize(
                                Vector3.ProjectOnPlane(_externalReference.forward, up));

                        _movementReferenceRight =
                            Vector3.Normalize(
                                Vector3.ProjectOnPlane(_externalReference.right, up));
                    }
                    else
                    {
                        _movementReferenceForward = Vector3.forward;
                        _movementReferenceRight = Vector3.right;
                    }
                    break;

                default:
                    _movementReferenceForward = Vector3.forward;
                    _movementReferenceRight = Vector3.right;
                    break;
            }

            if (!CharacterControllerProBridge.TryGetIs2D(_characterActor, out bool is2D))
            {
                is2D = false;
            }

            if (is2D)
            {
                _inputMovementReference = _movementReferenceRight * movementInput.x;
                return;
            }

            Vector3 rawMovementReference =
                _movementReferenceRight * movementInput.x
                + _movementReferenceForward * movementInput.y;

            _inputMovementReference = Vector3.ClampMagnitude(rawMovementReference, 1f);
        }

        /// <summary>
        /// Resets the cached Character Controller Pro runtime data.
        /// </summary>
        protected void ResetCharacterControllerProRuntimeState()
        {
            _isCharacterControllerProInitialized = false;
            _inputMovementReference = Vector3.zero;
            _movementReferenceForward = Vector3.forward;
            _movementReferenceRight = Vector3.right;
            _characterControllerInitialForward = Vector3.forward;
            _characterControllerInitialRight = Vector3.right;
        }

        /// <summary>
        /// Caches the delegates used to subscribe to Character Controller Pro
        /// actor callbacks.
        /// </summary>
        protected void EnsureCharacterControllerProHandlers()
        {
            _characterControllerPreSimulationHandler ??=
                OnCharacterControllerProPreSimulation;

            _characterControllerPostSimulationHandler ??=
                OnCharacterControllerProPostSimulation;

            _characterControllerAnimatorIkHandler ??=
                OnCharacterControllerProAnimatorIk;
        }

        /// <summary>
        /// Subscribes the machine to Character Controller Pro actor callbacks.
        /// </summary>
        protected void TrySubscribeCharacterControllerProCallbacks()
        {
            if (_isCharacterControllerProSubscribed || !UseCharacterControllerPro)
            {
                return;
            }

            if (!CharacterControllerProBridge.IsCharacterActor(_characterActor))
            {
                return;
            }

            EnsureCharacterControllerProHandlers();

            CharacterControllerProBridge.SubscribePreSimulation(
                _characterActor,
                _characterControllerPreSimulationHandler);

            CharacterControllerProBridge.SubscribePostSimulation(
                _characterActor,
                _characterControllerPostSimulationHandler);

            if (_animator != null)
            {
                CharacterControllerProBridge.SubscribeAnimatorIk(
                    _characterActor,
                    _characterControllerAnimatorIkHandler);
            }

            _isCharacterControllerProSubscribed = true;
        }

        /// <summary>
        /// Unsubscribes the machine from Character Controller Pro actor callbacks.
        /// </summary>
        protected void TryUnsubscribeCharacterControllerProCallbacks()
        {
            if (!_isCharacterControllerProSubscribed)
            {
                return;
            }

            CharacterControllerProBridge.UnsubscribePreSimulation(
                _characterActor,
                _characterControllerPreSimulationHandler);

            CharacterControllerProBridge.UnsubscribePostSimulation(
                _characterActor,
                _characterControllerPostSimulationHandler);

            if (_animator != null)
            {
                CharacterControllerProBridge.UnsubscribeAnimatorIk(
                    _characterActor,
                    _characterControllerAnimatorIkHandler);
            }

            _isCharacterControllerProSubscribed = false;
        }

        /// <summary>
        /// Resets all IK weights on the configured Character Controller Pro actor.
        /// </summary>
        public void ResetIKWeights()
        {
            if (!UseCharacterControllerPro)
            {
                return;
            }

            CharacterControllerProBridge.TryResetIKWeights(_characterActor);
        }

        /// <summary>
        /// Changes the status of the machine
        /// </summary>
        /// <param name="status"></param>
        public virtual void ChangeStatus(MachineStatus status)
        {
            _status = status;
            _statusChanged?.Invoke(_status);

            if (status == MachineStatus.Off)
            {
                _currentStateName = "None";
            }
        }

        /// <summary>
        /// Caches optional lifecycle hooks on derived brain types.
        /// </summary>
        private sealed class CachedBrainLifecycleMethods
        {
            private readonly MethodInfo _afterInitializedMethod;
            private readonly MethodInfo _beforeInitializedMethod;

            public CachedBrainLifecycleMethods(Type type)
            {
                _beforeInitializedMethod = GetMethod(type, "BeforeInitialized");
                _afterInitializedMethod = GetMethod(type, "AfterInitialized");
            }

            public void InvokeBeforeInitialized(FSMBrain brain)
            {
                _beforeInitializedMethod?.Invoke(brain, null);
            }

            public void InvokeAfterInitialized(FSMBrain brain)
            {
                _afterInitializedMethod?.Invoke(brain, null);
            }

            private static MethodInfo GetMethod(Type type, string methodName)
            {
                return type.GetMethod(
                    methodName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    Type.EmptyTypes,
                    null);
            }
        }

        private static CachedBrainLifecycleMethods GetCachedBrainLifecycleMethods(Type type)
        {
            if (s_cachedBrainLifecycleMethods.TryGetValue(
                    type,
                    out CachedBrainLifecycleMethods lifecycleMethods))
            {
                return lifecycleMethods;
            }

            lifecycleMethods = new CachedBrainLifecycleMethods(type);
            s_cachedBrainLifecycleMethods.Add(type, lifecycleMethods);
            return lifecycleMethods;
        }

        /// <summary>
        /// Caches optional Character Controller Pro state hooks on derived state types.
        /// </summary>
        private sealed class CachedCharacterControllerProStateMethods
        {
            private readonly Action<IState> _postFixedTickAction;
            private readonly Action<IState, float> _postSimulationAction;
            private readonly Action<IState> _preFixedTickAction;
            private readonly Action<IState, float> _preSimulationAction;
            private readonly Action<IState, int> _tickIkAction;

            public CachedCharacterControllerProStateMethods(Type type)
            {
                _preFixedTickAction = CreateStateAction(type, "PreFixedTick");
                _postFixedTickAction = CreateStateAction(type, "PostFixedTick");
                _preSimulationAction = CreateStateAction<float>(type, "PreCharacterSimulation");
                _postSimulationAction = CreateStateAction<float>(type, "PostCharacterSimulation");
                _tickIkAction = CreateStateAction<int>(type, "TickIK");
            }

            public void InvokePostFixedTick(IState state)
            {
                _postFixedTickAction?.Invoke(state);
            }

            public void InvokePostSimulation(IState state, float dt)
            {
                _postSimulationAction?.Invoke(state, dt);
            }

            public void InvokePreFixedTick(IState state)
            {
                _preFixedTickAction?.Invoke(state);
            }

            public void InvokePreSimulation(IState state, float dt)
            {
                _preSimulationAction?.Invoke(state, dt);
            }

            public void InvokeTickIk(IState state, int layerIndex)
            {
                _tickIkAction?.Invoke(state, layerIndex);
            }

            private static Action<IState> CreateStateAction(Type type, string methodName)
            {
                MethodInfo method = type.GetMethod(
                    methodName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    Type.EmptyTypes,
                    null);

                if (method == null)
                {
                    return null;
                }

                ParameterExpression stateParameter = Expression.Parameter(typeof(IState), "state");
                MethodCallExpression callExpression = Expression.Call(
                    Expression.Convert(stateParameter, type),
                    method);

                return Expression.Lambda<Action<IState>>(
                    callExpression,
                    stateParameter)
                    .Compile();
            }

            private static Action<IState, T> CreateStateAction<T>(Type type, string methodName)
            {
                MethodInfo method = type.GetMethod(
                    methodName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(T) },
                    null);

                if (method == null)
                {
                    return null;
                }

                ParameterExpression stateParameter = Expression.Parameter(typeof(IState), "state");
                ParameterExpression valueParameter = Expression.Parameter(typeof(T), "value");
                MethodCallExpression callExpression = Expression.Call(
                    Expression.Convert(stateParameter, type),
                    method,
                    valueParameter);

                return Expression.Lambda<Action<IState, T>>(
                    callExpression,
                    stateParameter,
                    valueParameter)
                    .Compile();
            }
        }

        private static CachedCharacterControllerProStateMethods GetCachedCharacterControllerProStateMethods(
            Type type)
        {
            if (s_cachedCharacterControllerProStateMethods.TryGetValue(
                    type,
                    out CachedCharacterControllerProStateMethods cachedMethods))
            {
                return cachedMethods;
            }

            cachedMethods = new CachedCharacterControllerProStateMethods(type);
            s_cachedCharacterControllerProStateMethods.Add(type, cachedMethods);
            return cachedMethods;
        }

        /// <summary>
        /// Resolves Simple Blackboard types and methods without introducing a hard package dependency.
        /// </summary>
        internal static class SimpleBlackboardBridge
        {
            private const string ContainerTypeName =
                "Zor.SimpleBlackboard.Components.SimpleBlackboardContainer, Zor.SimpleBlackboard";

            private const string BlackboardTypeName =
                "Zor.SimpleBlackboard.Core.Blackboard, Zor.SimpleBlackboard";

            private const string PropertyNameTypeName =
                "Zor.SimpleBlackboard.Core.BlackboardPropertyName, Zor.SimpleBlackboard";

            private static readonly Dictionary<string, object> s_propertyNames =
                new(StringComparer.Ordinal);

            private static readonly Dictionary<Type, Delegate> s_tryGetValueDelegates = new();
            private static readonly Dictionary<Type, Delegate> s_setValueDelegates = new();

            private static Type s_containerType;
            private static Type s_blackboardType;
            private static Type s_propertyNameListType;
            private static Type s_propertyNameType;
            private static ConstructorInfo s_propertyNameConstructor;
            private static PropertyInfo s_blackboardProperty;
            private static PropertyInfo s_propertyNameNameProperty;
            private static MethodInfo s_getPropertyNamesMethod;
            private static MethodInfo s_getValueTypeMethod;
            private static MethodInfo s_recreateBlackboardMethod;
            private static MethodInfo s_tryGetStructValueMethodDefinition;
            private static MethodInfo s_tryGetClassValueMethodDefinition;
            private static MethodInfo s_setStructValueMethodDefinition;
            private static MethodInfo s_setClassValueMethodDefinition;
            private static MethodInfo s_tryGetObjectValueMethod;
            private static MethodInfo s_containsValueMethod;
            private static TryGetObjectValueDelegate s_tryGetObjectValueDelegate;
            private static ContainsValueDelegate s_containsValueDelegate;

            /// <summary>
            /// Gets whether the Simple Blackboard runtime types were resolved successfully.
            /// </summary>
            public static bool IsAvailable =>
                ContainerType != null
                && BlackboardType != null
                && PropertyNameType != null
                && PropertyNameConstructor != null
                && BlackboardProperty != null;

            /// <summary>
            /// Gets the resolved Simple Blackboard container type.
            /// </summary>
            public static Type ContainerType =>
                s_containerType ??= Type.GetType(ContainerTypeName);

            /// <summary>
            /// Tries to resolve the runtime blackboard object from a container component.
            /// </summary>
            /// <param name="container">The candidate container component.</param>
            /// <param name="blackboard">The resolved blackboard instance.</param>
            /// <returns>True if a valid blackboard was resolved.</returns>
            public static bool TryGetBlackboard(Component container, out object blackboard)
            {
                blackboard = null;

                if (!IsAvailable || container == null || !ContainerType.IsInstanceOfType(container))
                {
                    return false;
                }

                blackboard = BlackboardProperty.GetValue(container);
                return blackboard != null;
            }

            /// <summary>
            /// Tries to read a typed value from a resolved blackboard instance.
            /// </summary>
            /// <typeparam name="T">The value type to read.</typeparam>
            /// <param name="blackboard">The resolved blackboard instance.</param>
            /// <param name="propertyName">The blackboard property name.</param>
            /// <param name="value">The resolved value if found.</param>
            /// <returns>True if the value exists and matches the requested type.</returns>
            public static bool TryGetValue<T>(
                object blackboard,
                string propertyName,
                out T value)
            {
                value = default;

                if (!TryGetPropertyName(propertyName, out object boxedPropertyName)
                    || blackboard == null)
                {
                    return false;
                }

                if (!s_tryGetValueDelegates.TryGetValue(typeof(T), out Delegate cachedDelegate))
                {
                    cachedDelegate = CreateTryGetValueDelegate<T>();
                    s_tryGetValueDelegates.Add(typeof(T), cachedDelegate);
                }

                if (cachedDelegate is not TryGetValueDelegate<T> tryGetDelegate)
                {
                    return false;
                }

                return tryGetDelegate(blackboard, boxedPropertyName, out value);
            }

            /// <summary>
            /// Writes a typed value into a resolved blackboard instance.
            /// </summary>
            /// <typeparam name="T">The value type to write.</typeparam>
            /// <param name="blackboard">The resolved blackboard instance.</param>
            /// <param name="propertyName">The blackboard property name.</param>
            /// <param name="value">The value to write.</param>
            /// <returns>True if the value was written successfully.</returns>
            public static bool SetValue<T>(object blackboard, string propertyName, T value)
            {
                if (!TryGetPropertyName(propertyName, out object boxedPropertyName)
                    || blackboard == null)
                {
                    return false;
                }

                if (!s_setValueDelegates.TryGetValue(typeof(T), out Delegate cachedDelegate))
                {
                    cachedDelegate = CreateSetValueDelegate<T>();
                    s_setValueDelegates.Add(typeof(T), cachedDelegate);
                }

                if (cachedDelegate is not SetValueDelegate<T> setValueDelegate)
                {
                    return false;
                }

                setValueDelegate(blackboard, boxedPropertyName, value);
                return true;
            }

            /// <summary>
            /// Tries to read an untyped value from a resolved blackboard instance.
            /// </summary>
            /// <param name="blackboard">The resolved blackboard instance.</param>
            /// <param name="propertyName">The blackboard property name.</param>
            /// <param name="value">The resolved value if found.</param>
            /// <returns>True if the property exists.</returns>
            public static bool TryGetObjectValue(
                object blackboard,
                string propertyName,
                out object value)
            {
                value = null;

                if (!TryGetPropertyName(propertyName, out object boxedPropertyName)
                    || blackboard == null)
                {
                    return false;
                }

                EnsureMethods();

                if (s_tryGetObjectValueDelegate == null)
                {
                    return false;
                }

                return s_tryGetObjectValueDelegate(
                    blackboard,
                    boxedPropertyName,
                    out value);
            }

            /// <summary>
            /// Gets whether a resolved blackboard contains a property.
            /// </summary>
            /// <param name="blackboard">The resolved blackboard instance.</param>
            /// <param name="propertyName">The blackboard property name.</param>
            /// <returns>True if the property exists.</returns>
            public static bool ContainsValue(object blackboard, string propertyName)
            {
                if (!TryGetPropertyName(propertyName, out object boxedPropertyName)
                    || blackboard == null)
                {
                    return false;
                }

                EnsureMethods();

                return s_containsValueDelegate != null
                    && s_containsValueDelegate(blackboard, boxedPropertyName);
            }

            /// <summary>
            /// Recreates the runtime blackboard owned by a container component.
            /// </summary>
            /// <param name="container">The candidate Simple Blackboard container.</param>
            /// <returns>True when the container recreated its runtime blackboard.</returns>
            public static bool RecreateBlackboard(Component container)
            {
                if (!IsAvailable || container == null || !ContainerType.IsInstanceOfType(container))
                {
                    return false;
                }

                s_recreateBlackboardMethod ??=
                    ContainerType.GetMethod("RecreateBlackboard", BindingFlags.Instance | BindingFlags.Public);

                if (s_recreateBlackboardMethod == null)
                {
                    return false;
                }

                s_recreateBlackboardMethod.Invoke(container, Array.Empty<object>());
                return true;
            }

            /// <summary>
            /// Tries to enumerate the available blackboard property names and their value types.
            /// </summary>
            /// <param name="blackboard">The resolved blackboard instance.</param>
            /// <param name="propertyMetadata">Receives the discovered property metadata.</param>
            /// <returns>True when the blackboard metadata APIs were resolved and invoked.</returns>
            public static bool TryGetPropertyMetadata(
                object blackboard,
                Dictionary<string, Type> propertyMetadata)
            {
                propertyMetadata?.Clear();

                if (!IsAvailable
                    || blackboard == null
                    || propertyMetadata == null
                    || !BlackboardType.IsInstanceOfType(blackboard))
                {
                    return false;
                }

                EnsureMetadataMethods();

                if (s_getPropertyNamesMethod == null
                    || s_getValueTypeMethod == null
                    || s_propertyNameNameProperty == null)
                {
                    return false;
                }

                IList propertyNames = CreatePropertyNameListInstance();

                if (propertyNames == null)
                {
                    return false;
                }

                s_getPropertyNamesMethod.Invoke(blackboard, new object[] { propertyNames });

                foreach (object propertyName in propertyNames)
                {
                    string name = s_propertyNameNameProperty.GetValue(propertyName) as string;

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    Type valueType = s_getValueTypeMethod.Invoke(blackboard, new[] { propertyName }) as Type;

                    if (valueType != null)
                    {
                        propertyMetadata[name] = valueType;
                    }
                }

                return true;
            }

            private static Type BlackboardType =>
                s_blackboardType ??= Type.GetType(BlackboardTypeName);

            private static Type PropertyNameType =>
                s_propertyNameType ??= Type.GetType(PropertyNameTypeName);

            private static ConstructorInfo PropertyNameConstructor =>
                s_propertyNameConstructor ??=
                    PropertyNameType?.GetConstructor(new[] { typeof(string) });

            private static PropertyInfo BlackboardProperty =>
                s_blackboardProperty ??=
                    ContainerType?.GetProperty("blackboard", BindingFlags.Instance | BindingFlags.Public);

            private static bool TryGetPropertyName(string propertyName, out object boxedPropertyName)
            {
                boxedPropertyName = null;

                if (!IsAvailable || string.IsNullOrWhiteSpace(propertyName))
                {
                    return false;
                }

                if (s_propertyNames.TryGetValue(propertyName, out boxedPropertyName))
                {
                    return true;
                }

                boxedPropertyName = PropertyNameConstructor.Invoke(new object[] { propertyName });
                s_propertyNames.Add(propertyName, boxedPropertyName);
                return true;
            }

            private static void EnsureMethods()
            {
                if (s_tryGetStructValueMethodDefinition != null)
                {
                    return;
                }

                MethodInfo[] methods = BlackboardType?.GetMethods(
                    BindingFlags.Instance | BindingFlags.Public);

                if (methods == null)
                {
                    return;
                }

                foreach (MethodInfo method in methods)
                {
                    if (method.Name == "TryGetStructValue"
                        && method.IsGenericMethodDefinition)
                    {
                        s_tryGetStructValueMethodDefinition = method;
                    }

                    if (method.Name == "TryGetClassValue"
                        && method.IsGenericMethodDefinition)
                    {
                        s_tryGetClassValueMethodDefinition = method;
                    }

                    if (method.Name == "SetStructValue"
                        && method.IsGenericMethodDefinition)
                    {
                        s_setStructValueMethodDefinition = method;
                    }

                    if (method.Name == "SetClassValue"
                        && method.IsGenericMethodDefinition)
                    {
                        s_setClassValueMethodDefinition = method;
                    }

                    if (method.Name == "TryGetObjectValue"
                        && !method.IsGenericMethod
                        && method.GetParameters().Length == 2)
                    {
                        s_tryGetObjectValueMethod = method;
                    }

                    if (method.Name == "ContainsObjectValue"
                        && !method.IsGenericMethod
                        && method.GetParameters().Length == 1)
                    {
                        s_containsValueMethod = method;
                    }
                }

                if (s_tryGetObjectValueDelegate == null
                    && s_tryGetObjectValueMethod != null)
                {
                    s_tryGetObjectValueDelegate =
                        CreateTryGetObjectValueDelegate(s_tryGetObjectValueMethod);
                }

                if (s_containsValueDelegate == null && s_containsValueMethod != null)
                {
                    s_containsValueDelegate =
                        CreateContainsValueDelegate(s_containsValueMethod);
                }
            }

            private static void EnsureMetadataMethods()
            {
                if (s_getPropertyNamesMethod != null
                    && s_getValueTypeMethod != null
                    && s_propertyNameNameProperty != null)
                {
                    return;
                }

                s_getPropertyNamesMethod ??=
                    BlackboardType?.GetMethod(
                        "GetPropertyNames",
                        BindingFlags.Instance | BindingFlags.Public);

                s_getValueTypeMethod ??=
                    BlackboardType?.GetMethod(
                        "GetValueType",
                        BindingFlags.Instance | BindingFlags.Public,
                        null,
                        new[] { PropertyNameType },
                        null);

                s_propertyNameNameProperty ??=
                    PropertyNameType?.GetProperty("name", BindingFlags.Instance | BindingFlags.Public);
            }

            private static IList CreatePropertyNameListInstance()
            {
                Type propertyNameType = PropertyNameType;

                if (propertyNameType == null)
                {
                    return null;
                }

                s_propertyNameListType ??= typeof(List<>).MakeGenericType(propertyNameType);
                return Activator.CreateInstance(s_propertyNameListType) as IList;
            }

            private static Delegate CreateTryGetValueDelegate<T>()
            {
                EnsureMethods();

                MethodInfo methodDefinition = typeof(T).IsValueType
                    ? s_tryGetStructValueMethodDefinition
                    : s_tryGetClassValueMethodDefinition;

                if (methodDefinition == null)
                {
                    return new TryGetValueDelegate<T>(FallbackTryGetValue);
                }

                MethodInfo closedMethod = methodDefinition.MakeGenericMethod(typeof(T));
                return CreateTryGetDelegate<T>(closedMethod);
            }

            private static Delegate CreateSetValueDelegate<T>()
            {
                EnsureMethods();

                MethodInfo methodDefinition = typeof(T).IsValueType
                    ? s_setStructValueMethodDefinition
                    : s_setClassValueMethodDefinition;

                if (methodDefinition == null)
                {
                    return new SetValueDelegate<T>(FallbackSetValue);
                }

                MethodInfo closedMethod = methodDefinition.MakeGenericMethod(typeof(T));
                return CreateSetDelegate<T>(closedMethod);
            }

            private static TryGetObjectValueDelegate CreateTryGetObjectValueDelegate(
                MethodInfo methodInfo)
            {
                ParameterExpression blackboardParameter =
                    Expression.Parameter(typeof(object), "blackboard");

                ParameterExpression propertyNameParameter =
                    Expression.Parameter(typeof(object), "propertyName");

                ParameterExpression valueParameter =
                    Expression.Parameter(typeof(object).MakeByRefType(), "value");

                MethodCallExpression body = Expression.Call(
                    Expression.Convert(blackboardParameter, BlackboardType),
                    methodInfo,
                    Expression.Unbox(propertyNameParameter, PropertyNameType),
                    valueParameter);

                return Expression.Lambda<TryGetObjectValueDelegate>(
                    body,
                    blackboardParameter,
                    propertyNameParameter,
                    valueParameter)
                    .Compile();
            }

            private static ContainsValueDelegate CreateContainsValueDelegate(
                MethodInfo methodInfo)
            {
                ParameterExpression blackboardParameter =
                    Expression.Parameter(typeof(object), "blackboard");

                ParameterExpression propertyNameParameter =
                    Expression.Parameter(typeof(object), "propertyName");

                MethodCallExpression body = Expression.Call(
                    Expression.Convert(blackboardParameter, BlackboardType),
                    methodInfo,
                    Expression.Unbox(propertyNameParameter, PropertyNameType));

                return Expression.Lambda<ContainsValueDelegate>(
                    body,
                    blackboardParameter,
                    propertyNameParameter)
                    .Compile();
            }

            private static bool FallbackTryGetValue<T>(
                object blackboard,
                object propertyName,
                out T value)
            {
                value = default;
                return false;
            }

            private static void FallbackSetValue<T>(
                object blackboard,
                object propertyName,
                T value)
            {
            }

            private static TryGetValueDelegate<T> CreateTryGetDelegate<T>(MethodInfo methodInfo)
            {
                ParameterExpression blackboardParameter =
                    System.Linq.Expressions.Expression.Parameter(typeof(object), "blackboard");

                ParameterExpression propertyNameParameter =
                    System.Linq.Expressions.Expression.Parameter(typeof(object), "propertyName");

                ParameterExpression valueParameter =
                    System.Linq.Expressions.Expression.Parameter(typeof(T).MakeByRefType(), "value");

                System.Linq.Expressions.MethodCallExpression body =
                    System.Linq.Expressions.Expression.Call(
                        System.Linq.Expressions.Expression.Convert(
                            blackboardParameter,
                            BlackboardType),
                        methodInfo,
                        System.Linq.Expressions.Expression.Unbox(
                            propertyNameParameter,
                            PropertyNameType),
                        valueParameter);

                return System.Linq.Expressions.Expression.Lambda<TryGetValueDelegate<T>>(
                    body,
                    blackboardParameter,
                    propertyNameParameter,
                    valueParameter)
                    .Compile();
            }

            private static SetValueDelegate<T> CreateSetDelegate<T>(MethodInfo methodInfo)
            {
                ParameterExpression blackboardParameter =
                    System.Linq.Expressions.Expression.Parameter(typeof(object), "blackboard");

                ParameterExpression propertyNameParameter =
                    System.Linq.Expressions.Expression.Parameter(typeof(object), "propertyName");

                ParameterExpression valueParameter =
                    System.Linq.Expressions.Expression.Parameter(typeof(T), "value");

                System.Linq.Expressions.MethodCallExpression body =
                    System.Linq.Expressions.Expression.Call(
                        System.Linq.Expressions.Expression.Convert(
                            blackboardParameter,
                            BlackboardType),
                        methodInfo,
                        System.Linq.Expressions.Expression.Unbox(
                            propertyNameParameter,
                            PropertyNameType),
                        valueParameter);

                return System.Linq.Expressions.Expression.Lambda<SetValueDelegate<T>>(
                    body,
                    blackboardParameter,
                    propertyNameParameter,
                    valueParameter)
                    .Compile();
            }

            private delegate bool TryGetValueDelegate<T>(
                object blackboard,
                object propertyName,
                out T value);

            private delegate bool TryGetObjectValueDelegate(
                object blackboard,
                object propertyName,
                out object value);

            private delegate bool ContainsValueDelegate(
                object blackboard,
                object propertyName);

            private delegate void SetValueDelegate<T>(
                object blackboard,
                object propertyName,
                T value);
        }

        /// <summary>
        /// Resolves Character Controller Pro types, callbacks, and members
        /// without introducing a hard package dependency into the core runtime assembly.
        /// </summary>
        internal static class CharacterControllerProBridge
        {
            private const string CharacterActorTypeName =
                "Lightbug.CharacterControllerPro.Core.CharacterActor, com.lightbug.character-controller-pro";

            private const string CharacterBrainTypeName =
                "Lightbug.CharacterControllerPro.Implementation.CharacterBrain, com.lightbug.character-controller-pro";

            private const string MaterialControllerTypeName =
                "Lightbug.CharacterControllerPro.Demo.MaterialController, com.lightbug.character-controller-pro";

            private static EventInfo s_onAnimatorIkEvent;
            private static EventInfo s_onPostSimulationEvent;
            private static EventInfo s_onPreSimulationEvent;

            private static Func<object, object> s_getCharacterActions;
            private static Func<object, Vector3> s_getForward;
            private static Func<object, bool> s_getIs2D;
            private static Func<object, Vector2> s_getMovementValue;
            private static Func<object, Vector3> s_getRight;
            private static Func<object, Vector3> s_getUp;
            private static Func<object, bool> s_getUpdateRootPosition;
            private static Func<object, bool> s_getUpdateRootRotation;
            private static Func<object, bool> s_getUseRootMotion;

            private static Action<object> s_resetIkWeights;
            private static Action<object, bool> s_setUpdateRootPosition;
            private static Action<object, bool> s_setUpdateRootRotation;
            private static Action<object, bool> s_setUseRootMotion;

            private static Type s_characterActorType;
            private static Type s_characterBrainType;
            private static Type s_materialControllerType;

            public static bool IsAvailable =>
                CharacterActorType != null
                && CharacterBrainType != null
                && MaterialControllerType != null;

            public static Type CharacterActorType =>
                s_characterActorType ??= Type.GetType(CharacterActorTypeName);

            public static Type CharacterBrainType =>
                s_characterBrainType ??= Type.GetType(CharacterBrainTypeName);

            public static Type MaterialControllerType =>
                s_materialControllerType ??= Type.GetType(MaterialControllerTypeName);

            public static bool IsCharacterActor(Component component)
            {
                return component != null
                    && CharacterActorType != null
                    && CharacterActorType.IsInstanceOfType(component);
            }

            public static void SubscribeAnimatorIk(Component actor, Action<int> callback)
            {
                EnsureEvents();
                s_onAnimatorIkEvent?.AddEventHandler(actor, callback);
            }

            public static void SubscribePostSimulation(Component actor, Action<float> callback)
            {
                EnsureEvents();
                s_onPostSimulationEvent?.AddEventHandler(actor, callback);
            }

            public static void SubscribePreSimulation(Component actor, Action<float> callback)
            {
                EnsureEvents();
                s_onPreSimulationEvent?.AddEventHandler(actor, callback);
            }

            public static bool TryGetForward(Component actor, out Vector3 value)
            {
                EnsureMembers();
                return TryGetValue(actor, s_getForward, out value);
            }

            public static bool TryGetIs2D(Component actor, out bool value)
            {
                EnsureMembers();
                return TryGetValue(actor, s_getIs2D, out value);
            }

            public static bool TryGetMovementInput(Component brain, out Vector2 value)
            {
                EnsureMembers();
                value = Vector2.zero;

                if (brain == null || s_getCharacterActions == null || s_getMovementValue == null)
                {
                    return false;
                }

                object characterActions = s_getCharacterActions(brain);

                if (characterActions == null)
                {
                    return false;
                }

                value = s_getMovementValue(characterActions);
                return true;
            }

            public static bool TryGetRight(Component actor, out Vector3 value)
            {
                EnsureMembers();
                return TryGetValue(actor, s_getRight, out value);
            }

            public static bool TryGetUp(Component actor, out Vector3 value)
            {
                EnsureMembers();
                return TryGetValue(actor, s_getUp, out value);
            }

            public static bool TryGetUpdateRootPosition(Component actor, out bool value)
            {
                EnsureMembers();
                return TryGetValue(actor, s_getUpdateRootPosition, out value);
            }

            public static bool TryGetUpdateRootRotation(Component actor, out bool value)
            {
                EnsureMembers();
                return TryGetValue(actor, s_getUpdateRootRotation, out value);
            }

            public static bool TryGetUseRootMotion(Component actor, out bool value)
            {
                EnsureMembers();
                return TryGetValue(actor, s_getUseRootMotion, out value);
            }

            public static bool TryResetIKWeights(Component actor)
            {
                EnsureMembers();

                if (actor == null || s_resetIkWeights == null)
                {
                    return false;
                }

                s_resetIkWeights(actor);
                return true;
            }

            public static bool TrySetUpdateRootPosition(Component actor, bool value)
            {
                EnsureMembers();
                return TrySetValue(actor, s_setUpdateRootPosition, value);
            }

            public static bool TrySetUpdateRootRotation(Component actor, bool value)
            {
                EnsureMembers();
                return TrySetValue(actor, s_setUpdateRootRotation, value);
            }

            public static bool TrySetUseRootMotion(Component actor, bool value)
            {
                EnsureMembers();
                return TrySetValue(actor, s_setUseRootMotion, value);
            }

            public static void UnsubscribeAnimatorIk(Component actor, Action<int> callback)
            {
                EnsureEvents();
                s_onAnimatorIkEvent?.RemoveEventHandler(actor, callback);
            }

            public static void UnsubscribePostSimulation(Component actor, Action<float> callback)
            {
                EnsureEvents();
                s_onPostSimulationEvent?.RemoveEventHandler(actor, callback);
            }

            public static void UnsubscribePreSimulation(Component actor, Action<float> callback)
            {
                EnsureEvents();
                s_onPreSimulationEvent?.RemoveEventHandler(actor, callback);
            }

            private static Func<object, T> CreateGetter<T>(Type rootType, params string[] memberPath)
            {
                if (rootType == null)
                {
                    return null;
                }

                ParameterExpression rootParameter = Expression.Parameter(typeof(object), "root");
                Expression currentExpression = Expression.Convert(rootParameter, rootType);
                Type currentType = rootType;

                for (int index = 0; index < memberPath.Length; index++)
                {
                    string memberName = memberPath[index];
                    PropertyInfo property = currentType.GetProperty(
                        memberName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    if (property != null)
                    {
                        currentExpression = Expression.Property(currentExpression, property);
                        currentType = property.PropertyType;
                        continue;
                    }

                    FieldInfo field = currentType.GetField(
                        memberName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    if (field == null)
                    {
                        return null;
                    }

                    currentExpression = Expression.Field(currentExpression, field);
                    currentType = field.FieldType;
                }

                Expression body = currentType == typeof(T)
                    ? currentExpression
                    : Expression.Convert(currentExpression, typeof(T));

                return Expression.Lambda<Func<object, T>>(body, rootParameter).Compile();
            }

            private static Action<object, T> CreateSetter<T>(Type rootType, string propertyName)
            {
                if (rootType == null)
                {
                    return null;
                }

                PropertyInfo property = rootType.GetProperty(
                    propertyName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (property == null || !property.CanWrite)
                {
                    return null;
                }

                ParameterExpression rootParameter = Expression.Parameter(typeof(object), "root");
                ParameterExpression valueParameter = Expression.Parameter(typeof(T), "value");
                BinaryExpression assignExpression = Expression.Assign(
                    Expression.Property(Expression.Convert(rootParameter, rootType), property),
                    valueParameter);

                return Expression.Lambda<Action<object, T>>(
                    assignExpression,
                    rootParameter,
                    valueParameter)
                    .Compile();
            }

            private static Action<object> CreateVoidMethod(Type rootType, string methodName)
            {
                if (rootType == null)
                {
                    return null;
                }

                MethodInfo method = rootType.GetMethod(
                    methodName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    Type.EmptyTypes,
                    null);

                if (method == null)
                {
                    return null;
                }

                ParameterExpression rootParameter = Expression.Parameter(typeof(object), "root");
                MethodCallExpression callExpression = Expression.Call(
                    Expression.Convert(rootParameter, rootType),
                    method);

                return Expression.Lambda<Action<object>>(callExpression, rootParameter).Compile();
            }

            private static void EnsureEvents()
            {
                if (s_onPreSimulationEvent != null)
                {
                    return;
                }

                s_onPreSimulationEvent = CharacterActorType?.GetEvent("OnPreSimulation");
                s_onPostSimulationEvent = CharacterActorType?.GetEvent("OnPostSimulation");
                s_onAnimatorIkEvent = CharacterActorType?.GetEvent("OnAnimatorIKEvent");
            }

            private static void EnsureMembers()
            {
                if (s_getForward != null)
                {
                    return;
                }

                s_getForward = CreateGetter<Vector3>(CharacterActorType, "Forward");
                s_getRight = CreateGetter<Vector3>(CharacterActorType, "Right");
                s_getUp = CreateGetter<Vector3>(CharacterActorType, "Up");
                s_getIs2D = CreateGetter<bool>(CharacterActorType, "Is2D");
                s_getUseRootMotion = CreateGetter<bool>(CharacterActorType, "UseRootMotion");
                s_setUseRootMotion = CreateSetter<bool>(CharacterActorType, "UseRootMotion");
                s_getUpdateRootPosition = CreateGetter<bool>(CharacterActorType, "UpdateRootPosition");
                s_setUpdateRootPosition = CreateSetter<bool>(CharacterActorType, "UpdateRootPosition");
                s_getUpdateRootRotation = CreateGetter<bool>(CharacterActorType, "UpdateRootRotation");
                s_setUpdateRootRotation = CreateSetter<bool>(CharacterActorType, "UpdateRootRotation");
                s_resetIkWeights = CreateVoidMethod(CharacterActorType, "ResetIKWeights");

                PropertyInfo characterActionsProperty = CharacterBrainType?.GetProperty(
                    "CharacterActions",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                s_getCharacterActions = CreateGetter<object>(CharacterBrainType, "CharacterActions");
                s_getMovementValue = CreateGetter<Vector2>(
                    characterActionsProperty?.PropertyType,
                    "movement",
                    "value");
            }

            private static bool TryGetValue<T>(
                Component component,
                Func<object, T> getter,
                out T value)
            {
                value = default;

                if (component == null || getter == null)
                {
                    return false;
                }

                value = getter(component);
                return true;
            }

            private static bool TrySetValue<T>(
                Component component,
                Action<object, T> setter,
                T value)
            {
                if (component == null || setter == null)
                {
                    return false;
                }

                setter(component, value);
                return true;
            }
        }

        #endregion

        #region Providing The machine

        /// <summary>
        /// Casts the current instance to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to cast to.</typeparam>
        /// <returns>The instance casted to the specified type.</returns>
        public T As<T>() where T : FSMBrain
        {
            return this as T;
        }

        #endregion

        #region Machine's Logic

        /// <summary>
        /// Defines a given state as active
        /// </summary>
        /// <param name="state"> The state to be set as active </param>
        /// <param name="forceInterruption"> If an uninterruptible state should be interrupted </param>
        public virtual void RequestStateChange(IState state)
        {
            if (_status != MachineStatus.On || state == null) return;

            ChangeState(
                state,
                new StateTransitionReport(StateTransitionReason.ExternalRequest));
        }

        /// <summary>
        /// Defines a given state as active
        /// </summary>
        /// <param name="state"> The state to be set as active </param>
        /// <param name="forceInterruption"> If an uninterruptible state should be interrupted </param>
        public virtual void RequestStateChange<T>() where T : State
        {
            if (!_stateProvider.TryGet<T>(out IState state))
            {
                Debug.LogError($"A state under the Type {nameof(T)} was requested but it is not present int the state factory ", this);
                return;
            }

            RequestStateChange(state);
        }

        /// <summary>
        /// Ends the current state of the machine.
        /// </summary>
        /// <param name="target">The target state to change to. If null, the default state will be used.</param>
        public virtual void EndState(IState target = null)
        {
            CompleteState(target);
        }

        /// <summary>
        /// Completes the current state and performs a natural transition.
        /// </summary>
        /// <param name="target">The target state to change to. If null, the default state will be used.</param>
        public virtual void CompleteState(IState target = null)
        {
            // Check if the machine is turned on
            if (_status != MachineStatus.On)
            {
                Debug.LogError($"Trying to end state on a machine that is not turned on. ", this);
                return;
            }

            // Change to the target state if provided
            if (target != null)
            {
                ChangeState(
                    target,
                    new StateTransitionReport(StateTransitionReason.NaturalTransition));
                return;
            }

            // Change to the default state if available
            if (_defaultState != null)
            {
                ChangeState(
                    _defaultState,
                    new StateTransitionReport(StateTransitionReason.NaturalTransition));
                return;
            }

            // Invoke the exit action of the current state
            ExecuteStateOperation(_currentState, state => state.Exit(), "CompleteState exit");
        }

        /// <summary>
        /// Fails the current state and transitions through an error path.
        /// </summary>
        /// <param name="target">The target state to change to. If null, the default state will be used.</param>
        /// <param name="message">Optional message that should be shown in the transition history.</param>
        public virtual void FailState(IState target = null, string message = null)
        {
            if (_status != MachineStatus.On)
            {
                Debug.LogError($"Trying to fail state on a machine that is not turned on. ", this);
                return;
            }

            StateTransitionReport transitionReport = new(
                StateTransitionReason.ErrorTransition,
                message);

            if (target != null && !IsStateFaulted(target))
            {
                ChangeState(target, transitionReport);
                return;
            }

            IState fallbackState = ResolveErrorFallbackState(_currentState);

            if (fallbackState != null)
            {
                ChangeState(fallbackState, transitionReport);
                return;
            }

            AbortMachineAfterStateFailure(transitionReport);
        }

        /// <summary>
        /// Ends the specified state of type T.
        /// </summary>
        /// <typeparam name="T">The type of the state.</typeparam>
        public virtual void EndState<T>() where T : IState
        {
            // Check if the requested state of type T exists in the state factory
            if (!_stateProvider.TryGet<T>(out IState state))
            {
                Debug.LogError($"A state under the Type {nameof(T)} was requested but it is not present in the state factory ", this);
                return;
            }

            // End the specified state
            EndState(state);
        }

        /// <summary>
        /// Completes the current state and transitions naturally to the specified state type.
        /// </summary>
        /// <typeparam name="T">The type of the target state.</typeparam>
        public virtual void CompleteState<T>() where T : IState
        {
            if (!_stateProvider.TryGet<T>(out IState state))
            {
                Debug.LogError($"A state under the Type {nameof(T)} was requested but it is not present in the state factory ", this);
                return;
            }

            CompleteState(state);
        }

        /// <summary>
        /// Ends the specified state of type T.
        /// </summary>
        public virtual void EndState(string targetStateKey)
        {
            // Check if the requested state of type T exists in the state factory
            if (!_stateProvider.TryGet(targetStateKey, out IState state))
            {
                Debug.LogError($"A state under the key {targetStateKey} was requested but it is not present in the state factory ", this);
                return;
            }

            // End the specified state
            EndState(state);
        }

        /// <summary>
        /// Completes the current state and transitions naturally to the specified key.
        /// </summary>
        /// <param name="targetStateKey">The target state key.</param>
        public virtual void CompleteState(string targetStateKey)
        {
            if (!_stateProvider.TryGet(targetStateKey, out IState state))
            {
                Debug.LogError($"A state under the key {targetStateKey} was requested but it is not present in the state factory ", this);
                return;
            }

            CompleteState(state);
        }

        /// <summary>
        /// Fails the current state and transitions to the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the target state.</typeparam>
        /// <param name="message">Optional message that should be shown in the transition history.</param>
        public virtual void FailState<T>(string message = null) where T : IState
        {
            if (!_stateProvider.TryGet<T>(out IState state))
            {
                Debug.LogError($"A state under the Type {nameof(T)} was requested but it is not present in the state factory ", this);
                return;
            }

            FailState(state, message);
        }

        /// <summary>
        /// Fails the current state and transitions to the specified key.
        /// </summary>
        /// <param name="targetStateKey">The target state key.</param>
        /// <param name="message">Optional message that should be shown in the transition history.</param>
        public virtual void FailState(string targetStateKey, string message = null)
        {
            if (!_stateProvider.TryGet(targetStateKey, out IState state))
            {
                Debug.LogError($"A state under the key {targetStateKey} was requested but it is not present in the state factory ", this);
                return;
            }

            FailState(state, message);
        }

        /// <summary>
        /// Changes the state.
        /// </summary>
        /// <param name="state">The new state to change to.</param>
        /// <param name="transitionReason">The reason for the transition.</param>
        protected virtual void ChangeState(
            IState state,
            StateTransitionReport transitionReport)
        {
            // Do not change state if it is the same as the current state or null
            if (state == _currentState || state == null) return;

            if (IsStateFaulted(state))
            {
                StateTransitionReport faultedTargetReport = new(
                    StateTransitionReason.ErrorTransition,
                    BuildFaultedTargetMessage(state));

                IState fallbackState = ResolveErrorFallbackState(state);

                if (fallbackState != null && fallbackState != state)
                {
                    if (fallbackState == _currentState)
                    {
                        _lastTransitionReport = faultedTargetReport;
                        return;
                    }

                    ChangeState(fallbackState, faultedTargetReport);
                    return;
                }

                AbortMachineAfterStateFailure(faultedTargetReport);
                return;
            }

            IState previousState = _currentState;

            // Define the previous state
            _previousState = previousState;

            // Invoke the exit action of the current state
            if (!TryExecuteStateOperation(previousState, stateToExit => stateToExit.Exit(), "State exit"))
            {
                return;
            }

            // Change the current state
            _currentState = state;
            _lastTransitionReport = transitionReport;

            // Invoke the enter action of the new state
            if (!TryExecuteStateOperation(_currentState, stateToEnter => stateToEnter.Enter(), "State enter"))
            {
                return;
            }

            _firstEnteredState ??= _currentState;

            // Update the current state name
            _currentStateName = CurrentState.DisplayName;

            // Announce the new state only after the new state entered successfully.
            _stateChanged.Invoke(_currentState, _previousState);
        }

        /// <summary>
        /// Handles the tick event.
        /// </summary>
        protected virtual void EvaluateTransition()
        {
            if (_currentState == null) return;

            if (_currentState.WantsToTransition(out IState targetState))
            {
                ChangeState(
                    targetState,
                    new StateTransitionReport(StateTransitionReason.ConditionTransition));
            }
        }

        /// <summary>
        /// Invokes the optional Character Controller Pro pre-fixed-tick hook on the current state.
        /// </summary>
        /// <param name="state">The state that owns the hook.</param>
        protected void TryExecuteCharacterControllerProPreFixedTick(IState state)
        {
            TryExecuteCharacterControllerProStateAction(
                state,
                static (methods, currentState) => methods.InvokePreFixedTick(currentState),
                "PreFixedTick");
        }

        /// <summary>
        /// Invokes the optional Character Controller Pro post-fixed-tick hook on the current state.
        /// </summary>
        /// <param name="state">The state that owns the hook.</param>
        protected void TryExecuteCharacterControllerProPostFixedTick(IState state)
        {
            TryExecuteCharacterControllerProStateAction(
                state,
                static (methods, currentState) => methods.InvokePostFixedTick(currentState),
                "PostFixedTick");
        }

        /// <summary>
        /// Invokes the optional Character Controller Pro pre-simulation hook on the current state.
        /// </summary>
        /// <param name="state">The state that owns the hook.</param>
        /// <param name="dt">The simulation delta time.</param>
        protected void TryExecuteCharacterControllerProPreSimulation(IState state, float dt)
        {
            TryExecuteCharacterControllerProStateAction(
                state,
                dt,
                static (methods, currentState, deltaTime) =>
                    methods.InvokePreSimulation(currentState, deltaTime),
                "PreCharacterSimulation");
        }

        /// <summary>
        /// Invokes the optional Character Controller Pro post-simulation hook on the current state.
        /// </summary>
        /// <param name="state">The state that owns the hook.</param>
        /// <param name="dt">The simulation delta time.</param>
        protected void TryExecuteCharacterControllerProPostSimulation(IState state, float dt)
        {
            TryExecuteCharacterControllerProStateAction(
                state,
                dt,
                static (methods, currentState, deltaTime) =>
                    methods.InvokePostSimulation(currentState, deltaTime),
                "PostCharacterSimulation");
        }

        /// <summary>
        /// Invokes the optional Character Controller Pro IK hook on the current state.
        /// </summary>
        /// <param name="state">The state that owns the hook.</param>
        /// <param name="layerIndex">The animator IK layer index.</param>
        protected void TryExecuteCharacterControllerProTickIk(IState state, int layerIndex)
        {
            TryExecuteCharacterControllerProStateAction(
                state,
                layerIndex,
                static (methods, currentState, currentLayerIndex) =>
                    methods.InvokeTickIk(currentState, currentLayerIndex),
                "TickIK");
        }

        /// <summary>
        /// Dispatches Character Controller Pro actor pre-simulation callbacks.
        /// </summary>
        /// <param name="dt">The simulation delta time.</param>
        protected virtual void OnCharacterControllerProPreSimulation(float dt)
        {
            if (!UseCharacterControllerPro)
            {
                return;
            }

            TryExecuteCharacterControllerProPreSimulation(CurrentState, dt);
        }

        /// <summary>
        /// Dispatches Character Controller Pro actor post-simulation callbacks.
        /// </summary>
        /// <param name="dt">The simulation delta time.</param>
        protected virtual void OnCharacterControllerProPostSimulation(float dt)
        {
            if (!UseCharacterControllerPro)
            {
                return;
            }

            TryExecuteCharacterControllerProPostSimulation(CurrentState, dt);
        }

        /// <summary>
        /// Dispatches Character Controller Pro animator IK callbacks.
        /// </summary>
        /// <param name="layerIndex">The animator IK layer index.</param>
        protected virtual void OnCharacterControllerProAnimatorIk(int layerIndex)
        {
            if (!UseCharacterControllerPro)
            {
                return;
            }

            TryExecuteCharacterControllerProTickIk(CurrentState, layerIndex);
        }

        private bool TryExecuteCharacterControllerProStateAction(
            IState state,
            Action<CachedCharacterControllerProStateMethods, IState> operation,
            string operationName)
        {
            if (!UseCharacterControllerPro || state == null || operation == null)
            {
                return true;
            }

            try
            {
                CachedCharacterControllerProStateMethods cachedMethods =
                    GetCachedCharacterControllerProStateMethods(state.GetType());

                operation(cachedMethods, state);
                return true;
            }
            catch (StateFailureException exception)
            {
                HandleStateFailure(state, exception, operationName);
                return false;
            }
        }

        private bool TryExecuteCharacterControllerProStateAction<T>(
            IState state,
            T value,
            Action<CachedCharacterControllerProStateMethods, IState, T> operation,
            string operationName)
        {
            if (!UseCharacterControllerPro || state == null || operation == null)
            {
                return true;
            }

            try
            {
                CachedCharacterControllerProStateMethods cachedMethods =
                    GetCachedCharacterControllerProStateMethods(state.GetType());

                operation(cachedMethods, state, value);
                return true;
            }
            catch (StateFailureException exception)
            {
                HandleStateFailure(state, exception, operationName);
                return false;
            }
        }

        /// <summary>
        /// Marks a state as unavailable for the current machine session after it
        /// failed during initialization.
        /// </summary>
        /// <param name="state">The state that failed to initialize.</param>
        /// <param name="exception">The state failure exception.</param>
        internal void HandleStateInitializationFailure(
            IState state,
            StateFailureException exception)
        {
            IState failedState = exception?.FailedState ?? state;

            if (failedState == null)
            {
                return;
            }

            _faultedStates.Add(failedState);

            if (_defaultState == failedState)
            {
                _defaultState = null;
            }

            Debug.LogError(BuildStateFailureLogMessage(failedState, "Initialize", exception), this);
            Debug.LogException(exception, this);
        }

        /// <summary>
        /// Executes a brain-owned operation and converts state failure exceptions
        /// into the machine recovery path.
        /// </summary>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="relatedState">The state related to the current operation.</param>
        /// <param name="operationName">The operation label used for diagnostics.</param>
        protected void ExecuteMachineOperation(
            Action operation,
            IState relatedState,
            string operationName)
        {
            try
            {
                operation?.Invoke();
            }
            catch (StateFailureException exception)
            {
                HandleStateFailure(relatedState, exception, operationName);
            }
        }

        /// <summary>
        /// Executes a state-owned operation and converts state failure exceptions
        /// into the machine recovery path.
        /// </summary>
        /// <param name="state">The state that owns the operation.</param>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="operationName">The operation label used for diagnostics.</param>
        protected void ExecuteStateOperation(
            IState state,
            Action<IState> operation,
            string operationName)
        {
            TryExecuteStateOperation(state, operation, operationName);
        }

        /// <summary>
        /// Tries to execute a state-owned operation and returns whether the
        /// execution completed successfully.
        /// </summary>
        /// <param name="state">The state that owns the operation.</param>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="operationName">The operation label used for diagnostics.</param>
        /// <returns>True when the operation completed without a state failure.</returns>
        protected bool TryExecuteStateOperation(
            IState state,
            Action<IState> operation,
            string operationName)
        {
            if (state == null || operation == null)
            {
                return true;
            }

            try
            {
                operation(state);
                return true;
            }
            catch (StateFailureException exception)
            {
                HandleStateFailure(state, exception, operationName);
                return false;
            }
        }

        /// <summary>
        /// Handles a state failure without allowing the exception to leave the machine.
        /// </summary>
        /// <param name="failedState">The state that failed.</param>
        /// <param name="exception">The exception raised by the state.</param>
        /// <param name="operationName">The operation label used for diagnostics.</param>
        protected virtual void HandleStateFailure(
            IState failedState,
            StateFailureException exception,
            string operationName)
        {
            failedState = exception?.FailedState ?? failedState;

            if (failedState != null)
            {
                _faultedStates.Add(failedState);

                if (_defaultState == failedState)
                {
                    _defaultState = null;
                }
            }

            StateTransitionReport transitionReport = new(
                StateTransitionReason.ErrorTransition,
                BuildStateFailureHistoryMessage(failedState, operationName, exception));

            Debug.LogError(BuildStateFailureLogMessage(failedState, operationName, exception), this);
            Debug.LogException(exception, this);

            if (_status != MachineStatus.On)
            {
                _lastTransitionReport = transitionReport;
                return;
            }

            if (_isRecoveringFromStateFailure)
            {
                AbortMachineAfterStateFailure(transitionReport);
                return;
            }

            _isRecoveringFromStateFailure = true;

            try
            {
                IState fallbackState = ResolveErrorFallbackState(failedState);

                if (fallbackState != null)
                {
                    if (fallbackState == _currentState)
                    {
                        _lastTransitionReport = transitionReport;
                        return;
                    }

                    ChangeState(fallbackState, transitionReport);
                    return;
                }

                AbortMachineAfterStateFailure(transitionReport);
            }
            finally
            {
                _isRecoveringFromStateFailure = false;
            }
        }

        /// <summary>
        /// Resolves the state that should receive control after an error transition.
        /// </summary>
        /// <param name="excludedState">The failing state that must not be reused.</param>
        /// <returns>The fallback state, or null when no safe fallback exists.</returns>
        protected virtual IState ResolveErrorFallbackState(IState excludedState)
        {
            if (_defaultState != null
                && _defaultState != excludedState
                && !IsStateFaulted(_defaultState))
            {
                return _defaultState;
            }

            if (_firstEnteredState != null
                && _firstEnteredState != excludedState
                && !IsStateFaulted(_firstEnteredState))
            {
                return _firstEnteredState;
            }

            return null;
        }

        /// <summary>
        /// Gets whether the specified state is marked as faulted in the current machine session.
        /// </summary>
        /// <param name="state">The state to inspect.</param>
        /// <returns>True when the state is currently faulted.</returns>
        protected bool IsStateFaulted(IState state)
        {
            return state != null && _faultedStates.Contains(state);
        }

        /// <summary>
        /// Aborts the current machine execution after an unrecoverable state failure.
        /// </summary>
        /// <param name="transitionReport">The report that describes the unrecoverable failure.</param>
        protected virtual void AbortMachineAfterStateFailure(
            StateTransitionReport transitionReport)
        {
            _lastTransitionReport = transitionReport;
            _currentState = null;
            _previousState = null;
            _currentStateName = "None";
            ChangeStatus(MachineStatus.Off);
        }

        /// <summary>
        /// Builds a concise message for history entries generated by state failures.
        /// </summary>
        /// <param name="failedState">The state that failed.</param>
        /// <param name="operationName">The operation label used for diagnostics.</param>
        /// <param name="exception">The exception that triggered recovery.</param>
        /// <returns>The history message to store with the transition report.</returns>
        protected virtual string BuildStateFailureHistoryMessage(
            IState failedState,
            string operationName,
            StateFailureException exception)
        {
            string stateName = failedState?.DisplayName ?? "Unknown state";

            if (!string.IsNullOrWhiteSpace(exception?.Message))
            {
                return $"{stateName} failed during {operationName}: {exception.Message}";
            }

            return $"{stateName} failed during {operationName}.";
        }

        /// <summary>
        /// Builds the runtime log message emitted when a state failure is intercepted.
        /// </summary>
        /// <param name="failedState">The state that failed.</param>
        /// <param name="operationName">The operation label used for diagnostics.</param>
        /// <param name="exception">The exception that triggered recovery.</param>
        /// <returns>The formatted runtime log message.</returns>
        protected virtual string BuildStateFailureLogMessage(
            IState failedState,
            string operationName,
            StateFailureException exception)
        {
            string stateName = failedState?.DisplayName ?? "Unknown state";
            return $"State failure intercepted on '{stateName}' during {operationName}. The machine will recover through an error transition.";
        }

        /// <summary>
        /// Builds the message used when code tries to transition into a state that
        /// has already failed in the current session.
        /// </summary>
        /// <param name="state">The faulted target state.</param>
        /// <returns>The error-transition message.</returns>
        protected virtual string BuildFaultedTargetMessage(IState state)
        {
            string stateName = state?.DisplayName ?? "Unknown state";
            return $"Transition target '{stateName}' is marked as non-functional for this machine session.";
        }

        #endregion

        #region Providing States

        /// <summary>
        /// Load states from the specified state type.
        /// </summary>
        /// <param name="stateType">The type of the states to load.</param>
        public void LoadStatesOfType(Type stateType)
        {
            _stateProvider.LoadStatesFromBaseType(stateType);
        }

        /// <summary>
        /// Loads and initalizes states from a list of ScriptableState objects
        /// </summary>
        /// <param name="states">A list of ScriptableState objects to load.</param>
        public void LoadStatesFromScriptablesList(List<ScriptableState> states)
        {
            _stateProvider.LoadStatesFromScriptablesList(states, true);
        }
        /// <summary>
        /// Loads the state of the given type.
        /// </summary>
        /// <param name="stateType">The type of the state to load.</param>
        public void LoadState(Type stateType)
        {
            _stateProvider.LoadState(stateType);
        }

        /// <summary>
        /// Loads the state by passing it to the state provider.
        /// </summary>
        /// <param name="state">The state object to be loaded.</param>
        public void LoadState(IState state)
        {
            _stateProvider.LoadState(state);
        }

        /// <summary>
        /// Retrieves the state of the specified type.
        /// </summary>
        /// <param name="stateType">The type of the state to retrieve.</param>
        /// <returns>The state of the specified type.</returns>
        public IState GetState(Type stateType)
        {
            // Use the _stateProvider to get the state of the specified type.
            return _stateProvider.Get(stateType);
        }

        /// <summary>
        /// Retrieves the state associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the state to retrieve.</param>
        /// <returns>The state associated with the specified key.</returns>
        public IState GetState(string key)
        {
            return _stateProvider.Get(key);
        }

        /// <summary>
        /// Tries to get the state of the specified type.
        /// </summary>
        /// <param name="stateType">The type of the state to get.</param>
        /// <param name="state">When this method returns, contains the state associated with the specified type, if found; otherwise, null.</param>
        /// <returns><c>true</c> if the state of the specified type was found; otherwise, <c>false</c>.</returns>
        public bool TryGetState(Type stateType, out IState state)
        {
            return _stateProvider.TryGet(stateType, out state);
        }

        /// <summary>
        /// Tries to get the state associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the state to retrieve.</param>
        /// <param name="state">When this method returns, contains the state associated with the specified key, if found; otherwise, null.</param>
        /// <returns><c>true</c> if the state was found; otherwise, <c>false</c>.</returns>
        public bool TryGetState(string key, out IState state)
        {
            return _stateProvider.TryGet(key, out state);
        }

        /// <summary>
        /// Retrieves the state of type T loaded into this machine.
        /// </summary>
        /// <typeparam name="T">The type of state to retrieve.</typeparam>
        /// <returns>The state of type T.</returns>
        public T GetState<T>() where T : IState
        {
            // Use the _stateProvider to get the state of type T
            return _stateProvider.Get<T>();
        }

        /// <summary>
        /// Tries to get the state of type T loaded into this machine.
        /// </summary>
        /// <typeparam name="T">The type of the state to get.</typeparam>
        /// <param name="state">The retrieved state of type T.</param>
        /// <returns>True if the state was successfully retrieved, false otherwise.</returns>
        public bool TryGetState<T>(out IState state) where T : IState
        {
            return _stateProvider.TryGet<T>(out state);
        }

        /// <summary>
        /// Gets all states loaded into this machine. 
        /// This can have an expensive performance since
        /// it iterates all states in the state provider in order to 
        /// form a list. Never use this method in a loop.
        /// </summary>
        /// <returns>A list of IState objects.</returns>
        public List<IState> GetAllStates()
        {
            return _stateProvider.GetAllStates();
        }

        #endregion
    }

}
