using UnityEngine;

namespace IndieGabo.HandyFSM.CCPro
{
    public abstract class StatsProvider<TStats> : MonoBehaviour
    {

        #region Inspector

        [SerializeField]
        protected TStats _defaultStats;

        #endregion

        #region Fields

        protected TStats _currentStats;

        #endregion

        #region Getters

        public TStats CurrentStats => _currentStats != null ? _currentStats : _defaultStats;

        #endregion

        #region Behaviour

        protected virtual void Awake()
        {
            _currentStats = _defaultStats;
        }

        #endregion

        #region Setting stats

        public void SetStats(TStats stats)
        {
            _currentStats = stats;
        }

        public void SetDefaultStats()
        {
            _currentStats = _defaultStats;
        }

        #endregion
    }
}