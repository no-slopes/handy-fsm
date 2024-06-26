using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace IndieGabo.HandyFSM
{
    public abstract class ScriptableState : ScriptableObject, IState
    {
        #region Inspector

        [SerializeField]
        protected string _name;

        [SerializeField]
        protected string _key;

        #endregion

        #region Fields

        protected FSMBrain _brain;
        protected List<StateTransition> _transitions = new();
        protected List<IState> _transitionTargets = new();

        #endregion

        #region  Getters

        public string DisplayName => string.IsNullOrEmpty(_name) ? name : _name;
        public FSMBrain Brain => _brain;
        public string Key => _key;

        #endregion

        #region Cycle Methods

        public virtual void Initialize(FSMBrain brain)
        {
            _brain = brain;
            SortTransitions();
            Type type = GetType();
            LoadActions(type);
            OnInitAction?.Invoke();
        }

        public virtual bool CanEnter(IState from) => true;
        public virtual void Enter() { OnEnterAction?.Invoke(); }
        public virtual void Exit() { OnExitAction?.Invoke(); }
        public virtual void Tick() { OnTickAction?.Invoke(); }
        public virtual void FixedTick() { OnFixedTickAction?.Invoke(); }
        public virtual void LateTick() { OnLateTickAction?.Invoke(); }

        protected UnityAction OnInitAction { get; private set; }
        protected UnityAction OnEnterAction { get; private set; }
        protected UnityAction OnExitAction { get; private set; }
        protected UnityAction OnInitializedAction { get; private set; }
        protected UnityAction OnTickAction { get; private set; }
        protected UnityAction OnLateTickAction { get; private set; }
        protected UnityAction OnFixedTickAction { get; private set; }

        #endregion

        #region Transitions

        /// <summary>
        /// Adds a transition into the available transitions of this state
        /// </summary>
        /// <param name="Condition"> A bool returning callback wich evaluates if the state should become active or not </param>
        /// <param name="targetState"> The state wich should become active based on condition </param>
        /// <param name="priority"> Priority level </param>
        protected virtual void AddTransition(Func<bool> Condition, IState targetState, int priority = 0)
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
        /// Checks if there are valid transitions and sets the output parameter with the target states list.
        /// </summary>
        /// <param name="targets">A list of target states this state wants to transition into.</param>
        /// <returns>True if a valid transition is found, otherwise false.</returns>
        public bool WantsToTransition(out List<IState> targets)
        {
            _transitionTargets.Clear();

            // Iterate through each transition
            for (int i = 0; i < _transitions.Count; i++)
            {
                // Get the current transition
                StateTransition transition = _transitions[i];

                // Check if the condition for the transition is met
                if (!transition.ConditionMet()) continue;

                // Set the output parameter to the target state
                _transitionTargets.Add(transition.TargetState);
            }

            // Default to null if no valid transition was found
            targets = _transitionTargets;

            // No transition condition was met, return false
            return _transitionTargets.Count > 0;
        }


        /// <summary>
        /// Loads methods as actions to be called during the state's lifecycle.
        /// </summary>
        protected virtual void LoadActions(Type type)
        {
            OnInitAction = GetDelegate<UnityAction>(type, "OnInit");
            OnEnterAction = GetDelegate<UnityAction>(type, "OnEnter");
            OnExitAction = GetDelegate<UnityAction>(type, "OnExit");
            OnTickAction = GetDelegate<UnityAction>(type, "OnTick");
            OnLateTickAction = GetDelegate<UnityAction>(type, "OnLateTick");
            OnFixedTickAction = GetDelegate<UnityAction>(type, "OnFixedTick");
        }

        protected virtual TDelegate GetDelegate<TDelegate>(Type type, string methodName) where TDelegate : class
        {
            MethodInfo method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (method != null)
            {
                return Delegate.CreateDelegate(typeof(TDelegate), this, method) as TDelegate;
            }
            return null;
        }

        #endregion
    }
}