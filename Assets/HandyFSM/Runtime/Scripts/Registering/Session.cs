using System.Collections.Generic;
using UnityEngine;

namespace HandyFSM.Registering
{
    [System.Serializable]
    public class Session
    {
        #region Inspector

        [SerializeField]
        private StateMachine _machine;

        [SerializeField]
        private List<Record> _records;

        [SerializeField]
        private float _duration;

        #endregion

        #region Fields

        private IState _currentState;
        private int _size;

        #endregion

        #region Getters

        public StateMachine Machine => _machine;
        public List<Record> Records => _records;
        public float Duration => _duration;

        #endregion

        #region Constructors

        public Session(StateMachine machine, int size)
        {
            _machine = machine;
            _records = new List<Record>();
            _size = size;
        }

        #endregion

        #region Flow

        public void Close(float duration)
        {
            _duration = duration;
        }

        #endregion

        #region Registering

        public void Register(IState state)
        {
            IState previousState = _currentState;
            _currentState = state;
            Record newRecord = new(previousState, state, Time.time);

            if (_records.Count >= _size)
            {
                _records.RemoveAt(0);
            }

            _records.Add(newRecord);
        }

        #endregion
    }
}