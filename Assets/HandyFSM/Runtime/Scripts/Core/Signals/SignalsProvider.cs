using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace IndieGabo.HandyFSM
{
    public class SignalsProvider
    {
        protected FSMBrain _brain;
        protected Dictionary<string, Signal> _signals = new();
        protected List<Signal> _signalsList;

        public SignalsProvider(FSMBrain brain, List<Signal> signals)
        {
            _brain = brain;
            _signalsList = signals;
            _signalsList.ForEach(signal => _signals.Add(signal.Key, signal));
        }

        public void CreateSignal(string key, SignalType type, object startingValue)
        {
            if (_signals.ContainsKey(key))
            {
                Debug.LogError($"Trying to create signal '{key}' but it already exists for StateMachine Brain \"{_brain.name}\"", _brain);
                return;
            }

            Signal signal = new()
            {
                Type = type
            };

            switch (type)
            {
                case SignalType.Boolean:
                    signal.SetBool((bool)startingValue);
                    break;
                case SignalType.Integer:
                    signal.SetInt((int)startingValue);
                    break;
                case SignalType.Float:
                    signal.SetFloat((float)startingValue);
                    break;
                case SignalType.Vector2:
                    signal.SetVector2((Vector2)startingValue);
                    break;
            }

            _signals.Add(key, signal);
            _signalsList.Add(signal);
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

        public bool ValidateSignal(string key, IState state)
        {
            bool valid = _signals.ContainsKey(key);

            if (!valid)
            {
                Debug.LogError($"State {state.Name} needs a signal named '{key}' but it does not exist for StateMachine Brain \"{_brain.name}\"", _brain);
            }

            return valid;
        }
    }
}
