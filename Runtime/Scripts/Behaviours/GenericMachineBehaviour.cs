namespace HandyFSM
{
    /// <summary>
    /// The state machine
    /// </summary>
    public abstract class GenericHandyFSMBrain<TBaseState, TDefaultState> : HandyFSMBrain
    {
        #region Machine Engine

        /// <summary>
        /// This method recognizes and initializes the states for the machine.
        /// </summary>
        protected virtual void BeforeInitialized()
        {
            _stateProvider.LoadStatesFromBaseType(typeof(TBaseState), false);
            _defaultState = _stateProvider.Get(typeof(TDefaultState));
        }

        #endregion
    }

    /// <summary>
    /// The state machine
    /// </summary>
    public abstract class GenericHandyFSMBrain<TBaseState> : HandyFSMBrain
    {
        #region Machine Engine

        /// <summary>
        /// This method recognizes and initializes the states for the machine.
        /// </summary>
        protected virtual void BeforeInitialized()
        {
            _stateProvider.LoadStatesFromBaseType(typeof(TBaseState), false);
        }

        #endregion
    }

}