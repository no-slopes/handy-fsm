using UnityEditor;
using UnityEngine;

namespace HandyFSM.Editor
{
    [FilePath("MachineStateVisualizerWindowData.asset", FilePathAttribute.Location.PreferencesFolder)]
    public class MachineStateVisualizerWindowData : ScriptableSingleton<MachineStateVisualizerWindowData>
    {
        [SerializeField]
        private StateMachine _machine;

        [SerializeField]
        GameObject _machineObj;

        public StateMachine Machine
        {
            get => _machine;
            set
            {
                _machine = value;
                if (_machine != null)
                {
                    _machineObj = _machine.gameObject;
                }
                else
                {
                    _machineObj = null;
                }
            }
        }

        public GameObject MachineObj => _machineObj;

    }
}