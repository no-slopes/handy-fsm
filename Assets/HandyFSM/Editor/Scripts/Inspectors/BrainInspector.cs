using System;
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
        private Label _thirdPartyLabel;
        private EnumField _statusField;
        private ObjectField _fieldOwner;
        private Label _transitionsLabel;
        private Toggle _toggleUseSimpleBlackboard;
        private ObjectField _fieldBlackboard;
        private VisualElement _thirdPartySimpleBlackboardContainer;
        private VisualElement _thirdPartyCCProContainer;
        private VisualElement _thirdPartyCCProFieldsContainer;
        private VisualElement _thirdPartyCCProOptionsContainer;
        private Button _openVisualizerButton;
        private Toggle _toggleUseCharacterControllerPro;

        private bool _showSimpleBlackboardSection;
        private bool _showCharacterControllerProSection;

        public FSMBrain Brain => _brain;

        /// <summary>
        /// Creates the custom inspector UI for an FSM brain.
        /// </summary>
        /// <returns>The root visual element for the inspector.</returns>
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

            _thirdPartyLabel = _containerMain.Q<Label>("label-third-party");


            _fieldOwner = _containerMain.Q<ObjectField>("field-owner");
            _fieldOwner.objectType = typeof(Transform);

            _transitionsLabel = _containerMain.Q<Label>("label-transitions");

            if (_transitionsLabel != null)
            {
                // Animator is a general runtime dependency that can be shared by
                // multiple integration paths, so it lives alongside the core
                // brain configuration instead of inside the CCPro subsection.
                _containerMain.Insert(
                    _containerMain.IndexOf(_transitionsLabel),
                    CreateObjectField("Animator", "_animator", typeof(Animator)));
            }

            _toggleUseSimpleBlackboard =
                _containerMain.Q<Toggle>("field-use-simple-blackboard");

            _fieldBlackboard = _containerMain.Q<ObjectField>("field-blackboard");
            _thirdPartySimpleBlackboardContainer =
                _containerMain.Q<VisualElement>("third-party-simple-blackboard");

            _thirdPartyCCProContainer =
                _containerMain.Q<VisualElement>("third-party-ccpro");

            _thirdPartyCCProFieldsContainer =
                _containerMain.Q<VisualElement>("third-party-ccpro-fields");

            ConfigureBlackboardField();
            ConfigureCharacterControllerProFields();
            UpdateThirdPartySectionVisibility();

            _openVisualizerButton = new Button(OpenVisualizer)
            {
                text = "Open State Visualizer"
            };

            _openVisualizerButton.AddToClassList("primary");
            _openVisualizerButton.style.marginTop = 8f;
            _openVisualizerButton.style.paddingLeft = 10f;
            _openVisualizerButton.style.paddingRight = 10f;
            _openVisualizerButton.style.paddingTop = 4f;
            _openVisualizerButton.style.paddingBottom = 4f;

            _containerMain.Add(_openVisualizerButton);

            return _containerMain;
        }

        /// <summary>
        /// Configures the optional Simple Blackboard object field.
        /// </summary>
        private void ConfigureBlackboardField()
        {
            if (_fieldBlackboard == null
                || _toggleUseSimpleBlackboard == null
                || _thirdPartySimpleBlackboardContainer == null)
                return;

            Type containerType = FSMBrain.SimpleBlackboardContainerType;

            if (containerType == null)
            {
                _showSimpleBlackboardSection = false;
                _thirdPartySimpleBlackboardContainer.style.display = DisplayStyle.None;
                UpdateThirdPartySectionVisibility();
                return;
            }

            _showSimpleBlackboardSection = true;
            _thirdPartySimpleBlackboardContainer.style.display = DisplayStyle.Flex;
            _toggleUseSimpleBlackboard.style.display = DisplayStyle.Flex;
            _toggleUseSimpleBlackboard.RegisterValueChangedCallback(
                OnUseSimpleBlackboardChanged);

            _fieldBlackboard.objectType = containerType;
            UpdateSimpleBlackboardVisibility(
                serializedObject.FindProperty("_useSimpleBlackboard")?.boolValue ?? false);

            UpdateThirdPartySectionVisibility();
        }

        /// <summary>
        /// Configures the optional Character Controller Pro subsection when the
        /// package is available in the current project.
        /// </summary>
        private void ConfigureCharacterControllerProFields()
        {
            if (_thirdPartyCCProContainer == null || _thirdPartyCCProFieldsContainer == null)
            {
                return;
            }

            _thirdPartyCCProFieldsContainer.Clear();

            if (!FSMBrain.IsCharacterControllerProAvailable)
            {
                _showCharacterControllerProSection = false;
                _thirdPartyCCProContainer.style.display = DisplayStyle.None;
                UpdateThirdPartySectionVisibility();
                return;
            }

            _showCharacterControllerProSection = true;
            _thirdPartyCCProContainer.style.display = DisplayStyle.Flex;

            _toggleUseCharacterControllerPro = CreateToggleField(
                "Use Character Controller Pro?",
                "_useCharacterControllerPro");

            _toggleUseCharacterControllerPro.RegisterValueChangedCallback(
                OnUseCharacterControllerProChanged);

            _thirdPartyCCProFieldsContainer.Add(_toggleUseCharacterControllerPro);

            _thirdPartyCCProOptionsContainer = new VisualElement();
            _thirdPartyCCProOptionsContainer.style.marginLeft = 12f;

            _thirdPartyCCProOptionsContainer.Add(
                CreateObjectField(
                    "Character Actor",
                    "_characterActor",
                    FSMBrain.CharacterActorType));

            _thirdPartyCCProOptionsContainer.Add(
                CreateObjectField(
                    "Material Controller",
                    "_materialController",
                    FSMBrain.MaterialControllerType));

            _thirdPartyCCProOptionsContainer.Add(
                CreateObjectField(
                    "Character Brain",
                    "_characterBrain",
                    FSMBrain.CharacterBrainType));

            _thirdPartyCCProOptionsContainer.Add(
                CreatePropertyField(
                    "_movementReferenceMode",
                    "Movement Reference"));

            _thirdPartyCCProOptionsContainer.Add(
                CreateObjectField(
                    "External Reference",
                    "_externalReference",
                    typeof(Transform)));

            _thirdPartyCCProFieldsContainer.Add(_thirdPartyCCProOptionsContainer);

            UpdateCharacterControllerProVisibility(
                serializedObject.FindProperty("_useCharacterControllerPro")?.boolValue ?? false);

            UpdateThirdPartySectionVisibility();
        }

        /// <summary>
        /// Creates a bound property field for the custom inspector.
        /// </summary>
        /// <param name="bindingPath">The serialized property binding path.</param>
        /// <param name="label">The field label to show in the inspector.</param>
        /// <returns>The configured property field.</returns>
        private PropertyField CreatePropertyField(string bindingPath, string label)
        {
            SerializedProperty property = serializedObject.FindProperty(bindingPath);

            PropertyField field = new(property, label);

            if (property != null)
            {
                field.BindProperty(property);
            }

            return field;
        }

        /// <summary>
        /// Creates a bound object field for the custom inspector.
        /// </summary>
        /// <param name="label">The field label to show in the inspector.</param>
        /// <param name="bindingPath">The serialized property binding path.</param>
        /// <param name="objectType">The allowed object type.</param>
        /// <returns>The configured object field.</returns>
        private ObjectField CreateObjectField(
            string label,
            string bindingPath,
            Type objectType)
        {
            ObjectField field = new(label)
            {
                objectType = objectType ?? typeof(UnityEngine.Object)
            };

            SerializedProperty property = serializedObject.FindProperty(bindingPath);

            if (property != null)
            {
                field.BindProperty(property);
            }

            return field;
        }

        /// <summary>
        /// Creates a bound toggle field for the custom inspector.
        /// </summary>
        /// <param name="label">The field label to show in the inspector.</param>
        /// <param name="bindingPath">The serialized property binding path.</param>
        /// <returns>The configured toggle field.</returns>
        private Toggle CreateToggleField(string label, string bindingPath)
        {
            Toggle field = new(label);
            SerializedProperty property = serializedObject.FindProperty(bindingPath);

            if (property != null)
            {
                field.BindProperty(property);
            }

            return field;
        }

        /// <summary>
        /// Updates the root third-party label visibility based on the available integrations.
        /// </summary>
        private void UpdateThirdPartySectionVisibility()
        {
            if (_thirdPartyLabel == null)
            {
                return;
            }

            _thirdPartyLabel.style.display =
                _showSimpleBlackboardSection || _showCharacterControllerProSection
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
        }

        /// <summary>
        /// Updates the optional Simple Blackboard fields when the toggle changes.
        /// </summary>
        /// <param name="evt">The toggle change event.</param>
        private void OnUseSimpleBlackboardChanged(ChangeEvent<bool> evt)
        {
            if (evt == null)
            {
                return;
            }

            UpdateSimpleBlackboardVisibility(evt.newValue);
        }

        /// <summary>
        /// Updates the Character Controller Pro fields when the toggle changes.
        /// </summary>
        /// <param name="evt">The toggle change event.</param>
        private void OnUseCharacterControllerProChanged(ChangeEvent<bool> evt)
        {
            if (evt == null)
            {
                return;
            }

            UpdateCharacterControllerProVisibility(evt.newValue);
        }

        /// <summary>
        /// Shows the container field only when the user enabled the optional integration.
        /// </summary>
        /// <param name="isEnabled">Whether Simple Blackboard usage is enabled.</param>
        private void UpdateSimpleBlackboardVisibility(bool isEnabled)
        {
            if (_fieldBlackboard == null)
            {
                return;
            }

            if (!_showSimpleBlackboardSection)
            {
                _fieldBlackboard.style.display = DisplayStyle.None;
                return;
            }

            _fieldBlackboard.style.display = isEnabled
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }

        /// <summary>
        /// Shows the Character Controller Pro configuration fields only when the
        /// optional integration is enabled.
        /// </summary>
        /// <param name="isEnabled">Whether Character Controller Pro usage is enabled.</param>
        private void UpdateCharacterControllerProVisibility(bool isEnabled)
        {
            if (_thirdPartyCCProOptionsContainer == null)
            {
                return;
            }

            if (!_showCharacterControllerProSection)
            {
                _thirdPartyCCProOptionsContainer.style.display = DisplayStyle.None;
                return;
            }

            _thirdPartyCCProOptionsContainer.style.display = isEnabled
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }

        /// <summary>
        /// Marks the inspected brain as dirty inside the editor.
        /// </summary>
        /// <param name="persist">Whether the change should be saved immediately.</param>
        public void MarkAsDirty(bool persist = false)
        {
            EditorUtility.SetDirty(_brain);

            if (persist)
                AssetDatabase.SaveAssetIfDirty(_brain);
        }

        /// <summary>
        /// Opens the state visualizer window focused on the current brain.
        /// </summary>
        private void OpenVisualizer()
        {
            MachineStateVisualizerWindow window =
                MachineStateVisualizerWindow.OpenEditorWindow();

            EditorApplication.delayCall += () =>
            {
                if (window == null || _brain == null)
                    return;

                window.SetMachine(_brain);
                window.MachineSelectorField.value = _brain;
            };
        }

        #region Status

        /// <summary>
        /// Updates the runtime status badge styling.
        /// </summary>
        /// <param name="status">The status currently displayed by the brain.</param>
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

        /// <summary>
        /// Reacts to status value changes coming from the hidden enum field.
        /// </summary>
        /// <param name="evt">The enum field change event.</param>
        private void OnStatusChanged(ChangeEvent<System.Enum> evt)
        {
            if (evt == null) return;
            SetStatusText((MachineStatus)evt.newValue);
        }

        #endregion
    }
}