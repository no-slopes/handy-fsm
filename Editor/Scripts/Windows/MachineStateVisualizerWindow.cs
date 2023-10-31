using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HandyFSM.Editor
{
    public class MachineStateVisualizerWindow : EditorWindow
    {
        #region Static

        public static MachineStateVisualizerWindow OpenEditorWindow(StateMachine machine)
        {
            var window = GetWindow<MachineStateVisualizerWindow>();
            window.titleContent = new GUIContent("State Visualizer");
            window.minSize = new Vector2(300, 150);
            window.Show();

            return window;
        }

        #endregion

        #region Fields

        private StateMachine _machine;
        private StateVisualizer _stateVisualizer;

        #endregion

        #region Behaviour

        private void OnEnable()
        {
            VisualTreeAsset tree = Resources.Load<VisualTreeAsset>("UI Documents/MachineStateVisualizerWindowUI");
            TemplateContainer template = tree.CloneTree();

            template.style.flexGrow = 1;

            _stateVisualizer = new StateVisualizer();
            template.Add(_stateVisualizer.Root);

            EditorApplication.playModeStateChanged += OnPlayModeChange;

            rootVisualElement.Add(template);
        }

        private void OnDisable()
        {
            _machine?.StatusChanged.RemoveListener(OnStatusChange);
            Dismiss();
        }

        private void OnPlayModeChange(PlayModeStateChange obj)
        {
            _machine?.StatusChanged.AddListener(OnStatusChange);
        }

        #endregion

        #region Flow

        private void Initialize()
        {
            _stateVisualizer.Initialize(_machine.GetAllStates());
            if (_machine.CurrentState != null)
            {
                BuildStateView(_machine.CurrentState, _machine.PreviousState);
            }
            _machine.StateChanged.AddListener(OnStateChanged);
        }

        private void Dismiss()
        {
            _stateVisualizer.Dismiss();
            _machine.StateChanged.RemoveListener(OnStateChanged);
        }

        #endregion

        #region Machine

        public void SetMachine(StateMachine machine)
        {
            _machine = machine;

            if (_machine.IsOn)
            {
                Initialize();
            }

            _machine.StatusChanged.AddListener(OnStatusChange);
        }

        private void OnStatusChange(MachineStatus status)
        {
            if (status.Equals(MachineStatus.Off))
            {
                Dismiss();
            }
            else if (status.Equals(MachineStatus.On))
            {
                Initialize();
            }
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