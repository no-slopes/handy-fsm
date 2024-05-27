using HandyFSM.Registering;
using UnityEditor;
using UnityEngine;

namespace HandyFSM.Editor
{
    [FilePath("MachineStateVisualizerWindowData.asset", FilePathAttribute.Location.PreferencesFolder)]
    public class MachineStateVisualizerWindowData : ScriptableSingleton<MachineStateVisualizerWindowData>
    {
        [SerializeField]
        private FSMBrain _machine;

        [SerializeField]
        private GameObject _machineObj;

        [SerializeField]
        private Session _session;

        public FSMBrain Machine => _machine;
        public GameObject MachineObj => _machineObj;
        public Session Session => _session;


        public void SetMachine(FSMBrain machine)
        {
            _machine = machine;
            if (_machine != null)
            {
                _machineObj = _machine.gameObject;
            }
            else
            {
                _machineObj = null;
            }
        }

        public void SetSession(Session session)
        {
            _session = session;
        }

    }
}