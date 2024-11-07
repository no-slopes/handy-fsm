using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace IndieGabo.HandyFSM
{
    public class TriggersProvider
    {
        protected FSMBrain _brain;
        protected Dictionary<string, List<UnityAction>> _datalessTriggers = new();
        protected Dictionary<string, List<UnityAction<TriggerData>>> _dataTriggers = new();

        public TriggersProvider(FSMBrain brain)
        {
            _brain = brain;
        }

        public void Squeeze(string key)
        {
            if (!_datalessTriggers.ContainsKey(key))
            {
                Debug.LogError($"Trying to squeeze trigger '{key}' but it does not exist for StateMachine Brain \"{_brain.name}\"");
                return;
            }

            foreach (UnityAction action in _datalessTriggers[key])
            {
                action?.Invoke();
            }
        }

        public void Squeeze(string key, TriggerData value)
        {
            if (!_dataTriggers.ContainsKey(key))
            {
                Debug.LogError($"Trying to squeeze trigger '{key}' but it does not exist for StateMachine Brain \"{_brain.name}\"");
                return;
            }

            foreach (UnityAction<TriggerData> action in _dataTriggers[key])
            {
                action?.Invoke(value);
            }
        }

        public void RegisterCallback(string key, UnityAction action)
        {
            if (!_datalessTriggers.ContainsKey(key))
            {
                _datalessTriggers.Add(key, new List<UnityAction>());
            }

            _datalessTriggers[key].Add(action);
        }

        public void UnregisterCallback(string key, UnityAction action)
        {
            if (!_datalessTriggers.ContainsKey(key)) return;

            var list = _datalessTriggers[key];

            list.Remove(action);

            if (list.Count == 0)
            {
                _datalessTriggers.Remove(key);
            }
        }

        public void RegisterCallback(string key, UnityAction<TriggerData> action)
        {
            if (!_dataTriggers.ContainsKey(key))
            {
                _dataTriggers.Add(key, new List<UnityAction<TriggerData>>());
            }

            _dataTriggers[key].Add(action);
        }

        public void UnregisterCallback(string key, UnityAction<TriggerData> action)
        {
            if (!_dataTriggers.ContainsKey(key)) return;

            var list = _dataTriggers[key];
            list.Remove(action);

            if (list.Count == 0)
            {
                _dataTriggers.Remove(key);
            }
        }
    }
}