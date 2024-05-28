using UnityEngine;
using UnityEngine.Events;

namespace IndieGabo.HandyFSM
{
    [System.Serializable]
    public class Signal
    {
        [SerializeField]
        private string _key = string.Empty;

        [SerializeField]
        private SignalType _type = SignalType.Boolean;

        [SerializeField]
        private bool _boolValue = false;

        [SerializeField]
        private int _intValue = 0;

        [SerializeField]
        private float _floatValue = 0f;

        [SerializeField]
        private UnityEvent<SignalType, object> _valueChanged = new();

        public string Key { get => _key; set => _key = value; }
        public SignalType Type { get => _type; set => _type = value; }
        public bool BoolValue => _boolValue;
        public int IntValue => _intValue;
        public float FloatValue => _floatValue;
        public UnityEvent<SignalType, object> ValueChanged => _valueChanged;

        public void SetBool(bool value)
        {
            _boolValue = value;
            _valueChanged.Invoke(SignalType.Boolean, value);
        }

        public void SetInt(int value)
        {
            _intValue = value;
            _valueChanged.Invoke(SignalType.Integer, value);
        }

        public void SetFloat(float value)
        {
            _floatValue = value;
            _valueChanged.Invoke(SignalType.Float, value);
        }
    }

    [System.Serializable]
    public class SignalWrapper
    {
        [SerializeReference]
        public Signal signal;
    }

    public enum SignalType
    {
        [InspectorName("bool")]
        Boolean,
        [InspectorName("int")]
        Integer,
        [InspectorName("float")]
        Float,
    }
}