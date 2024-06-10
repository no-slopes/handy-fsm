using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace IndieGabo.HandyFSM
{
    public class TriggersProvider
    {
        protected FSMBrain _brain;
        protected Dictionary<string, UnityEvent<TriggerData>> _triggers = new();

        public TriggersProvider(FSMBrain brain)
        {
            _brain = brain;
        }

        public void Squeeze(string key, TriggerData data = null)
        {
            if (!_triggers.ContainsKey(key))
            {
                Debug.LogError($"Trying to squeeze trigger '{key}' but it does not exist for StateMachine Brain \"{_brain.name}\"");
                return;
            }

            _triggers[key].Invoke(data);
        }

        public void RegisterCallback(string key, UnityAction<TriggerData> action)
        {
            if (!_triggers.ContainsKey(key))
            {
                _triggers.Add(key, new UnityEvent<TriggerData>());
            }

            _triggers[key].AddListener(action);
        }

        public void UnregisterCallback(string key, UnityAction<TriggerData> action)
        {
            if (!_triggers.ContainsKey(key))
            {
                Debug.LogError($"Trying to unregister callback for trigger '{key}' but it does not exist for StateMachine Brain \"{_brain.name}\"");
                return;
            }

            _triggers[key].RemoveListener(action);
        }
    }
}