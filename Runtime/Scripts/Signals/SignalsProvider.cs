using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandyFSM
{
    public class SignalsProvider
    {
        protected HandyFSMBrain _brain;
        protected Dictionary<string, Signal> _signals = new();

        public SignalsProvider(HandyFSMBrain brain, List<Signal> signals)
        {
            _brain = brain;
            signals.ForEach(signal => _signals.Add(signal.Key, signal));
        }

        public void Set(string key, bool value)
        {
            if (!_signals.ContainsKey(key))
            {
                Debug.LogError($"Signal '{key}' does not exist for StateMachine Brain \"{_brain.name}\"", _brain);
                return;
            }

            _signals[key].SetBool(value);
        }

        public void Set(string key, int value)
        {
            if (!_signals.ContainsKey(key))
            {
                Debug.LogError($"Signal '{key}' does not exist for StateMachine Brain \"{_brain.name}\"", _brain);
                return;
            }

            _signals[key].SetInt(value);
        }

        public void Set(string key, float value)
        {
            if (!_signals.ContainsKey(key))
            {
                Debug.LogError($"Signal '{key}' does not exist for StateMachine Brain \"{_brain.name}\"", _brain);
                return;
            }

            _signals[key].SetFloat(value);
        }

        public bool ReadBool(string key)
        {
            if (!_signals.ContainsKey(key))
            {
                Debug.LogError($"Signal '{key}' does not exist for StateMachine Brain \"{_brain.name}\"", _brain);
                return false;
            }

            return _signals[key].BoolValue;
        }

        public int ReadInt(string key)
        {
            if (!_signals.ContainsKey(key))
            {
                Debug.LogError($"Signal '{key}' does not exist for StateMachine Brain \"{_brain.name}\"", _brain);
                return default;
            }

            return _signals[key].IntValue;
        }

        public float ReadFloat(string key)
        {
            if (!_signals.ContainsKey(key))
            {
                Debug.LogError($"Signal '{key}' does not exist for StateMachine Brain \"{_brain.name}\"", _brain);
                return default;
            }

            return _signals[key].FloatValue;
        }
    }
}
