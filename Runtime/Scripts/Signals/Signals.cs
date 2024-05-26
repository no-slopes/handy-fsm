using UnityEngine;
using UnityEngine.Events;

namespace HandyFSM
{
    public class Signal : ScriptableObject
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

        public string Key { get => _key; set => _key = value; }
        public SignalType Type { get => _type; set => _type = value; }
        public bool BoolValue => _boolValue;
        public int IntValue => _intValue;
        public float FloatValue => _floatValue;

        public void SetBool(bool value)
        {
            _boolValue = value;
        }

        public void SetInt(int value)
        {
            _intValue = value;
        }

        public void SetFloat(float value)
        {
            _floatValue = value;
        }
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