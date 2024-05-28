using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

namespace IndieGabo.HandyFSM.Editor
{
    public class SignalElement : VisualElement
    {
        private static readonly string DocumentName = "SignalUI";

        private Signal _signal;

        private TemplateContainer _containerMain;
        private TextField _fieldKey;
        private EnumField _fieldSignalType;
        private Toggle _fieldBoolValue;
        private IntegerField _fieldIntValue;
        private FloatField _fieldFloatValue;

        public SignalElement()
        {
            _containerMain = Resources.Load<VisualTreeAsset>($"UI Documents/{DocumentName}").Instantiate();

            _fieldKey = _containerMain.Q<TextField>("field-key");
            _fieldKey.RegisterValueChangedCallback(evt =>
            {
                if (_signal != null)
                    _signal.Key = evt.newValue;
            });

            _fieldSignalType = _containerMain.Q<EnumField>("field-type");
            _fieldSignalType.RegisterValueChangedCallback(OnTypeChanged);

            _fieldBoolValue = _containerMain.Q<Toggle>("field-bool-value");
            _fieldBoolValue.RegisterValueChangedCallback(evt => _signal?.SetBool(evt.newValue));

            _fieldIntValue = _containerMain.Q<IntegerField>("field-int-value");
            _fieldIntValue.RegisterValueChangedCallback(evt => _signal?.SetInt(evt.newValue));

            _fieldFloatValue = _containerMain.Q<FloatField>("field-float-value");
            _fieldFloatValue.RegisterValueChangedCallback(evt => _signal?.SetFloat(evt.newValue));

            Add(_containerMain);
        }

        public void SetSignal(Signal signal)
        {
            _signal?.ValueChanged.RemoveListener(OnValueChanged);

            _signal = signal;
            _signal.ValueChanged.AddListener(OnValueChanged);

            _fieldKey.SetValueWithoutNotify(_signal.Key);
            _fieldSignalType.SetValueWithoutNotify(_signal.Type);
            _fieldBoolValue.SetValueWithoutNotify(_signal.BoolValue);
            _fieldIntValue.SetValueWithoutNotify(_signal.IntValue);
            _fieldFloatValue.SetValueWithoutNotify(_signal.FloatValue);

            EvaluateValueDisplay((SignalType)_fieldSignalType.value);
        }

        private void OnTypeChanged(ChangeEvent<Enum> e)
        {
            if (e.newValue == null || _signal == null) return;
            EvaluateValueDisplay((SignalType)e.newValue);
            _signal.Type = (SignalType)e.newValue;
        }

        private void EvaluateValueDisplay(SignalType signalType)
        {
            _fieldBoolValue.style.display = signalType == SignalType.Boolean ? DisplayStyle.Flex : DisplayStyle.None;
            _fieldIntValue.style.display = signalType == SignalType.Integer ? DisplayStyle.Flex : DisplayStyle.None;
            _fieldFloatValue.style.display = signalType == SignalType.Float ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnValueChanged(SignalType type, object value)
        {
            switch (type)
            {
                case SignalType.Boolean:
                    _fieldBoolValue.SetValueWithoutNotify((bool)value);
                    break;
                case SignalType.Integer:
                    _fieldIntValue.SetValueWithoutNotify((int)value);
                    break;
                case SignalType.Float:
                    _fieldFloatValue.SetValueWithoutNotify((float)value);
                    break;
            }
        }
    }
}
