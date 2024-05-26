using UnityEngine;

namespace HandyFSM
{
    public class RuntimeInfo : ScriptableObject
    {
        #region Inspector

        /// <summary>
        /// The current machine's status of the MachineStatus enum type. 
        /// </summary>
        [SerializeField]
        private MachineStatus _status = MachineStatus.Off;

        /// <summary>
        /// The current state name
        /// </summary>
        [SerializeField]
        private string _currentStateName = "None";

        #endregion

        #region Properties

        public MachineStatus Status { get => _status; set => _status = value; }
        public string CurrentStateName { get => _currentStateName; set => _currentStateName = value; }

        #endregion

    }

}