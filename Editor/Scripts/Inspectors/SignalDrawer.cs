using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;

namespace HandyFSM.Editor
{
    [CustomPropertyDrawer(typeof(Signal), true)]
    public class SignalDrawer : PropertyDrawer
    {
        private static readonly string DocumentName = "SignalUI";

        private TemplateContainer _containerMain;
        private EnumField _fieldSignalType;
        private Toggle _fieldBoolValue;
        private IntegerField _fieldIntValue;
        private FloatField _fieldFloatValue;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {

            _containerMain = Resources.Load<VisualTreeAsset>($"UI Documents/{DocumentName}").Instantiate();

            _fieldSignalType = _containerMain.Q<EnumField>("field-type");

            _fieldBoolValue = _containerMain.Q<Toggle>("field-bool-value");
            _fieldIntValue = _containerMain.Q<IntegerField>("field-int-value");
            _fieldFloatValue = _containerMain.Q<FloatField>("field-float-value");

            _fieldSignalType.RegisterValueChangedCallback((e) =>
            {
                EvaluateValueDisplay((SignalType)e.newValue);
            });
            // EditorApplication.delayCall += () =>
            // {
            // };

            EvaluateValueDisplay((SignalType)_fieldSignalType.value);
            return _containerMain;
        }

        private void EvaluateValueDisplay(SignalType signalType)
        {
            _fieldBoolValue.style.display = signalType == SignalType.Boolean ? DisplayStyle.Flex : DisplayStyle.None;
            _fieldIntValue.style.display = signalType == SignalType.Integer ? DisplayStyle.Flex : DisplayStyle.None;
            _fieldFloatValue.style.display = signalType == SignalType.Float ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
