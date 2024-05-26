using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

namespace HandyFSM.Editor
{
    public class SignalElement : VisualElement
    {
        private static readonly string DocumentName = "SignalUI";

        private Signal _signal;
        private SerializedObject _serializedObject;

        private TemplateContainer _containerMain;
        private EnumField _fieldSignalType;
        private Toggle _fieldBoolValue;
        private IntegerField _fieldIntValue;
        private FloatField _fieldFloatValue;

        public SignalElement()
        {
            _containerMain = Resources.Load<VisualTreeAsset>($"UI Documents/{DocumentName}").Instantiate();

            Add(_containerMain);
        }

        public void SetSignal(Signal signal)
        {
            if (_signal != null && _fieldSignalType != null)
            {
                _fieldSignalType.UnregisterValueChangedCallback(OnTypeChanged);
            }

            _signal = signal;
            _serializedObject = new SerializedObject(_signal);
            _containerMain.Bind(_serializedObject);

            _fieldSignalType = _containerMain.Q<EnumField>("field-type");
            _fieldBoolValue = _containerMain.Q<Toggle>("field-bool-value");
            _fieldIntValue = _containerMain.Q<IntegerField>("field-int-value");
            _fieldFloatValue = _containerMain.Q<FloatField>("field-float-value");

            _fieldSignalType.RegisterValueChangedCallback(OnTypeChanged);
            EvaluateValueDisplay((SignalType)_fieldSignalType.value);
        }

        private void OnTypeChanged(ChangeEvent<Enum> e)
        {
            if (e.newValue == null) return;
            EvaluateValueDisplay((SignalType)e.newValue);
        }

        private void EvaluateValueDisplay(SignalType signalType)
        {
            _fieldBoolValue.style.display = signalType == SignalType.Boolean ? DisplayStyle.Flex : DisplayStyle.None;
            _fieldIntValue.style.display = signalType == SignalType.Integer ? DisplayStyle.Flex : DisplayStyle.None;
            _fieldFloatValue.style.display = signalType == SignalType.Float ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
