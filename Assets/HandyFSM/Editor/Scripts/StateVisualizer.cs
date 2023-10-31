using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HandyFSM.Editor
{
    public class StateVisualizer
    {
        #region Fields

        private VisualElement _root;

        private List<IState> _states;

        private IState _state;
        private IState _fromState;

        #endregion

        #region Getters

        public VisualElement Root => _root;

        #endregion

        #region Constructors

        public StateVisualizer()
        {
            VisualTreeAsset tree = Resources.Load<VisualTreeAsset>("UI Documents/StateVisualizerUI");
            TemplateContainer template = tree.CloneTree();
            template.style.flexGrow = 1;

            _root = template;
        }

        #endregion

        #region Flow

        public void Initialize(List<IState> states)
        {
            _states = states;
        }

        public void Dismiss()
        {
            _states = null;
        }

        public void BuildView(IState state, IState fromState)
        {
            List<Type> types = new();

            Type currentType = state.GetType();
            Type stateType = typeof(State);
            Type scriptableStateType = typeof(ScriptableState);

            while (currentType != stateType && currentType != scriptableStateType)
            {
                types.Add(currentType);
                currentType = currentType.BaseType;
            }

            foreach (Type type in types)
            {
                // Debug.Log($"{type}");
            }

        }

        #endregion
    }
}