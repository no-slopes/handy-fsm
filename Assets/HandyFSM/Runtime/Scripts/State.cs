using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace HandyFSM
{
    /// <summary>
    /// Represents a State controlled by the StateMachine class.
    /// </summary>
    [Serializable]
    public abstract class State : IState
    {
        #region Fields

        [SerializeField]
        private string _name;

        [SerializeField]
        private bool _interruptible;

        [SerializeField]
        private StateMachineBehaviour _machine;

        [SerializeField]
        private List<StateTransition> _transitions = new();

        #endregion

        #region Getters

        /// <summary>
        /// Either the state name defined on Inspector or the type of the state class
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// If the state can be interrupted.
        /// </summary>
        public bool Interruptible => _interruptible;

        public StateMachineBehaviour Machine => _machine;

        #endregion

        #region Cycle Methods

        /// <summary>
        /// This will be called before the  method.
        /// </summary>
        public virtual void Initialize(StateMachineBehaviour machine)
        {
            _machine = machine;
            _name = GetType().Name;
            SortTransitions();
            LoadActions();
            OnInitAction?.Invoke();
        }

        public void Enter() { OnEnterAction?.Invoke(); }
        public void Exit() { OnExitAction?.Invoke(); }
        public void Tick() { OnTickAction?.Invoke(); }
        public void FixedTick() { OnFixedTickAction?.Invoke(); }
        public void LateTick() { OnLateTickAction?.Invoke(); }

        public void OnCollisionEnter2D(Collision2D collision) { OnCollisionEnter2DAction?.Invoke(collision); }
        public void OnCollisionStay2D(Collision2D collision) { OnCollisionStay2DAction?.Invoke(collision); }
        public void OnCollisionExit2D(Collision2D collision) { OnCollisionExit2DAction?.Invoke(collision); }
        public void OnTriggerEnter2D(Collider2D other) { OnTriggerEnter2DAction?.Invoke(other); }
        public void OnTriggerStay2D(Collider2D other) { OnTriggerStay2DAction?.Invoke(other); }
        public void OnTriggerExit2D(Collider2D other) { OnTriggerExit2DAction?.Invoke(other); }

        protected UnityAction OnInitAction { get; private set; }
        protected UnityAction OnEnterAction { get; private set; }
        protected UnityAction OnExitAction { get; private set; }
        protected UnityAction OnInitializedAction { get; private set; }
        protected UnityAction OnTickAction { get; private set; }
        protected UnityAction OnLateTickAction { get; private set; }
        protected UnityAction OnFixedTickAction { get; private set; }
        protected UnityAction<Collision2D> OnCollisionEnter2DAction { get; private set; }
        protected UnityAction<Collision2D> OnCollisionStay2DAction { get; private set; }
        protected UnityAction<Collision2D> OnCollisionExit2DAction { get; private set; }
        protected UnityAction<Collider2D> OnTriggerEnter2DAction { get; private set; }
        protected UnityAction<Collider2D> OnTriggerStay2DAction { get; private set; }
        protected UnityAction<Collider2D> OnTriggerExit2DAction { get; private set; }

        #endregion       

        #region Transitions

        /// <summary>
        /// Adds a transition into the available transitions of this state
        /// </summary>
        /// <param name="Condition"> A bool returning callback wich evaluates if the state should become active or not </param>
        /// <param name="targetState"> The state wich should become active based on condition </param>
        /// <param name="priority"> Priority level </param>
        protected virtual void AddTransition(Func<bool> Condition, State targetState, int priority = 0)
        {
            StateTransition transition = new(Condition, targetState, priority);
            AddTransition(transition);
        }

        /// <summary>
        /// Adds a transition into the available transitions of this state
        /// </summary>
        /// <param name="transition"> The State Trasitions to be add </param>
        protected virtual void AddTransition(StateTransition transition)
        {
            _transitions.Add(transition);
        }

        /// <summary>
        /// Sorts transitions based on priority. Descending
        /// </summary>
        public virtual void SortTransitions()
        {
            _transitions.OrderByDescending(transition => transition.Priority).ToList();
        }

        /// <summary>
        /// Checks if there is a valid transition and sets the output parameter with the target state.
        /// </summary>
        /// <param name="state">The target state if a valid transition is found.</param>
        /// <returns>True if a valid transition is found, otherwise false.</returns>
        public bool ShouldTransition(out IState state)
        {
            // Initialize the output parameter
            state = null;

            // Iterate through each transition
            for (int i = 0; i < _transitions.Count; i++)
            {
                // Get the current transition
                StateTransition transition = _transitions[i];

                // Check if the condition for the transition is met
                if (!transition.ConditionMet()) continue;

                // Set the output parameter to the target state
                state = transition.TargetState;

                // Return true to indicate a successful transition
                return true;
            }

            // No transition condition was met, return false
            return false;
        }

        /// <summary>
        /// Loads methods as actions to be called during the state's lifecycle.
        /// </summary>
        protected virtual void LoadActions()
        {
            Type stateType = GetType();
            OnInitAction = GetDelegate<UnityAction>(stateType, "OnInit");
            OnEnterAction = GetDelegate<UnityAction>(stateType, "OnEnter");
            OnExitAction = GetDelegate<UnityAction>(stateType, "OnExit");
            OnTickAction = GetDelegate<UnityAction>(stateType, "OnTick");
            OnLateTickAction = GetDelegate<UnityAction>(stateType, "OnLateTick");
            OnFixedTickAction = GetDelegate<UnityAction>(stateType, "OnFixedTick");
            OnCollisionEnter2DAction = GetDelegate<UnityAction<Collision2D>>(stateType, "OnCollisionEnter2D");
            OnCollisionStay2DAction = GetDelegate<UnityAction<Collision2D>>(stateType, "OnCollisionStay2D");
            OnCollisionExit2DAction = GetDelegate<UnityAction<Collision2D>>(stateType, "OnCollisionExit2D");
            OnTriggerEnter2DAction = GetDelegate<UnityAction<Collider2D>>(stateType, "OnTriggerEnter2D");
            OnTriggerStay2DAction = GetDelegate<UnityAction<Collider2D>>(stateType, "OnTriggerStay2D");
            OnTriggerExit2DAction = GetDelegate<UnityAction<Collider2D>>(stateType, "OnTriggerExit2D");
        }

        private TDelegate GetDelegate<TDelegate>(Type type, string methodName) where TDelegate : class
        {
            MethodInfo method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (method != null)
            {
                return Delegate.CreateDelegate(typeof(TDelegate), this, method) as TDelegate;
            }
            return null;
        }

        #endregion

        #region Configurations

        protected void SetName(string name)
        {
            _name = name;
        }

        protected void SetInterruptible(bool interruptible)
        {
            _interruptible = interruptible;
        }

        #endregion
    }
}