using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace HandyFSM.Editor
{
    public class RuntimeInfoElement : VisualElement
    {
        #region Fields

        private RuntimeInfo _runtimeInfo;
        private SerializedObject _serializedObject;

        private VisualElement _infoContainer;
        private Label _statusText;
        private EnumField _statusField;

        #endregion

        #region GUI

        public RuntimeInfoElement(RuntimeInfo runtimeInfo)
        {
            _runtimeInfo = runtimeInfo;
            _serializedObject = new SerializedObject(_runtimeInfo);

            VisualTreeAsset treeAsset = Resources.Load<VisualTreeAsset>("UI Documents/StateMachineRuntimeInfoUI");
            TemplateContainer container = treeAsset.Instantiate();
            container.Bind(_serializedObject);

            _infoContainer = container.Q<VisualElement>("runtime-info-container");

            _statusField = container.Q<EnumField>("status-field");
            _statusField.RegisterValueChangedCallback(OnStatusChanged);

            _statusText = container.Q<Label>("status-text");
            MachineStatus currentStatus = _runtimeInfo.Status;
            SetStatusText(currentStatus);

            // EvaluateVisibility();
            // EditorApplication.playModeStateChanged += PlayModeStateChange => EvaluateVisibility();

            Add(container);
        }

        #endregion

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
            SetStatusText((MachineStatus)evt.newValue);
        }

        #endregion

        #region Displaying

        private void EvaluateVisibility()
        {
            if (EditorApplication.isPlaying)
            {
                _infoContainer.style.display = DisplayStyle.Flex;
            }
            else
            {
                _infoContainer.style.display = DisplayStyle.None;
            }
        }

        #endregion

    }
}
