using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HandyFSM.Editor
{
    [CustomEditor(typeof(HandyFSMBrain), true)]
    public class BrainInspector : UnityEditor.Editor
    {

        private static readonly string DocumentName = "BrainUI";

        [SerializeField]
        private MonoScript _scriptAsset;

        private HandyFSMBrain _brain;
        private VisualElement _containerMain;
        private Label _statusText;
        private EnumField _statusField;
        private ObjectField _fieldOwner;
        private ListView _listSignals;

        public HandyFSMBrain Brain => _brain;

        public override VisualElement CreateInspectorGUI()
        {
            _brain = target as HandyFSMBrain;

            _containerMain = Resources.Load<VisualTreeAsset>($"UI Documents/{DocumentName}").Instantiate();

            ObjectField scriptField = _containerMain.Query<ObjectField>("script-field");
            scriptField.SetEnabled(false);
            scriptField.value = MonoScript.FromMonoBehaviour(target as MonoBehaviour);

            _statusField = _containerMain.Q<EnumField>("status-field");
            _statusField.RegisterValueChangedCallback(OnStatusChanged);

            _statusText = _containerMain.Q<Label>("status-text");
            SetStatusText(_brain.Status);


            _fieldOwner = _containerMain.Q<ObjectField>("field-owner");
            _fieldOwner.objectType = typeof(Transform);


            _listSignals = _containerMain.Q<ListView>("list-signals");
            _listSignals.makeItem = () =>
            {
                SignalElement element = new();
                return element;
            };

            _listSignals.bindItem = (element, i) =>
            {
                SignalElement signalElement = element as SignalElement;
                if (_brain.SignalsList[i] == null)
                {
                    _brain.SignalsList[i] = new();
                }
                signalElement.SetSignal(_brain.SignalsList[i]);
            };
            _listSignals.itemsSource = _brain.SignalsList;

            return _containerMain;
        }

        public void MarkAsDirty(bool persist = false)
        {
            EditorUtility.SetDirty(_brain);

            if (persist)
                AssetDatabase.SaveAssetIfDirty(_brain);
        }

        #region Status

        private void SetStatusText(MachineStatus status)
        {
            _statusText.text = status.ToString().ToUpper();

            _statusText.RemoveFromClassList("off");
            _statusText.RemoveFromClassList("on");
            _statusText.RemoveFromClassList("paused");

            switch (status)
            {
                case MachineStatus.On:
                    _statusText.AddToClassList("on");
                    break;
                case MachineStatus.Paused:
                    _statusText.AddToClassList("paused");
                    break;
                case MachineStatus.Off:
                    _statusText.AddToClassList("off");
                    break;
                default:
                    _statusText.AddToClassList("off");
                    break;
            }
        }

        private void OnStatusChanged(ChangeEvent<System.Enum> evt)
        {
            if (evt == null) return;
            SetStatusText((MachineStatus)evt.newValue);
        }

        #endregion
    }
}