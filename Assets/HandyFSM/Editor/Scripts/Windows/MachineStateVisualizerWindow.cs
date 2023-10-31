using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HandyFSM.Editor
{
    public class MachineStateVisualizerWindow : EditorWindow
    {
        #region Static

        public static MachineStateVisualizerWindow OpenEditorWindow()
        {
            var window = GetWindow<MachineStateVisualizerWindow>();
            window.titleContent = new GUIContent("State Visualizer");
            window.minSize = new Vector2(300, 150);
            window.Show();
            return window;
        }

        #endregion

        #region Fields

        private MachineStateVisualizerWindowData _data;
        private StateMachine _machine;
        private StateVisualizer _stateVisualizer;
        private VisualElement _stateVisualizerRoot;

        private TemplateContainer _root;
        private VisualElement _body;
        private VisualElement _selectStateMachineContainer;
        private VisualElement _enterPlayModeContainer;

        private ObjectField _machineSelectorField;
        private Button _fromSelectionButton;

        #endregion


        #region Getters

        public ObjectField MachineSelectorField
        {
            get
            {
                _machineSelectorField ??= rootVisualElement.Q<ObjectField>("machine-selector-field");

                return _machineSelectorField;
            }
        }

        public MachineStateVisualizerWindowData Data => MachineStateVisualizerWindowData.instance;

        #endregion

        #region Behaviour

        private void OnEnable()
        {
            VisualTreeAsset tree = Resources.Load<VisualTreeAsset>("UI Documents/MachineStateVisualizerWindowUI");
            _root = tree.CloneTree();

            _root.style.flexGrow = 1;

            _body = _root.Q<VisualElement>("body");

            _selectStateMachineContainer = _root.Q("select-machine-container");
            _enterPlayModeContainer = _root.Q("enter-play-mode-container");

            _stateVisualizer = new StateVisualizer();
            _stateVisualizerRoot = _stateVisualizer.Root;
            _body.Add(_stateVisualizerRoot);

            _machineSelectorField = _root.Q<ObjectField>("machine-selector-field");

            _machineSelectorField.RegisterValueChangedCallback((e) =>
            {
                StateMachine machine = e.newValue as StateMachine;
                EvaluateMachine(machine);
            });

            _fromSelectionButton = _root.Q<Button>("from-selection-button");
            _fromSelectionButton.clicked += () =>
            {
                GameObject selectedObject = Selection.activeObject as GameObject;
                if (selectedObject == null) return;
                StateMachine machine = selectedObject.GetComponent<StateMachine>();
                if (machine == null) return;
                _machineSelectorField.value = machine;
                EvaluateMachine(machine);
            };

            EditorApplication.playModeStateChanged += OnPlayModeChange;

            LoadMachineFromData();

            rootVisualElement.Add(_root);
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChange;
        }

        private void OnDestroy()
        {
            Data.Machine = null;
        }

        private void OnPlayModeChange(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange.Equals(PlayModeStateChange.EnteredPlayMode))
            {
                Initialize();
                EvaluateDisplay();
            }
            else if (playModeStateChange.Equals(PlayModeStateChange.EnteredEditMode))
            {
                LoadMachineFromData();
                Dismiss();
            }
        }

        #endregion

        #region Flow

        private void Initialize()
        {
            if (Data.Machine == null) return;

            _stateVisualizer.Initialize(Data.Machine.GetAllStates());
            if (Data.Machine.CurrentState != null)
            {
                BuildStateView(Data.Machine.CurrentState, Data.Machine.PreviousState);
            }

            Data.Machine.StateChanged.AddListener(OnStateChanged);
        }

        private void Dismiss()
        {
            _stateVisualizer.Dismiss();

            if (Data.Machine != null)
            {
                Data.Machine.StateChanged.RemoveListener(OnStateChanged);
            }
        }

        private void EvaluateDisplay()
        {
            if (Data.Machine != null && EditorApplication.isPlaying)
            {
                _enterPlayModeContainer.style.display = DisplayStyle.None;
                _selectStateMachineContainer.style.display = DisplayStyle.None;
                _stateVisualizerRoot.style.display = DisplayStyle.Flex;
                return;
            }
            else if (Data.Machine != null && !EditorApplication.isPlaying)
            {
                _enterPlayModeContainer.style.display = DisplayStyle.Flex;
                _selectStateMachineContainer.style.display = DisplayStyle.None;
                _stateVisualizerRoot.style.display = DisplayStyle.None;
                return;
            }
            else if (Data.Machine == null)
            {
                _enterPlayModeContainer.style.display = DisplayStyle.None;
                _selectStateMachineContainer.style.display = DisplayStyle.Flex;
                _stateVisualizerRoot.style.display = DisplayStyle.None;
                return;
            }
        }

        #endregion

        #region Machine

        public void EvaluateMachine(StateMachine machine)
        {
            Data.Machine = machine;
            EvaluateDisplay();
        }

        private void LoadMachineFromData()
        {
            if (Data.Machine == null)
            {
                if (Data.MachineObj != null)
                {
                    StateMachine machine = Data.MachineObj.GetComponent<StateMachine>();
                    Data.Machine = machine.gameObject.GetComponent<StateMachine>();
                    _machineSelectorField.value = null;
                    _machineSelectorField.value = machine;
                }
            }
            else
            {
                StateMachine machine = Data.Machine;
                _machineSelectorField.value = null;
                _machineSelectorField.value = machine;
            }

            EvaluateDisplay();
        }

        #endregion

        #region States

        private void BuildStateView(IState state, IState previous)
        {
            _stateVisualizer.BuildView(state, previous);
        }

        private void OnStateChanged(IState state, IState previous)
        {
            BuildStateView(state, previous);
        }

        #endregion
    }
}