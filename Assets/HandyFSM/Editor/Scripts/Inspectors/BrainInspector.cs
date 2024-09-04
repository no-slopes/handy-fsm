using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace IndieGabo.HandyFSM.Editor
{
    [CustomEditor(typeof(FSMBrain), true)]
    public class BrainInspector : UnityEditor.Editor
    {

        private static readonly string DocumentName = "BrainUI";

        [SerializeField]
        private MonoScript _scriptAsset;

        private FSMBrain _brain;
        private VisualElement _containerMain;
        private Label _statusText;
        private EnumField _statusField;
        private ObjectField _fieldOwner;

        public FSMBrain Brain => _brain;

        public override VisualElement CreateInspectorGUI()
        {
            _brain = target as FSMBrain;

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