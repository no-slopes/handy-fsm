using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace HandyFSM.Editor
{
    public class StatesGraphView : GraphView
    {
        #region Getters

        public new class UxmlFactory : UxmlFactory<StatesGraphView, GraphView.UxmlTraits> { }

        #endregion

        #region Constructors

        public StatesGraphView()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            StyleSheet styleSheet = Resources.Load<StyleSheet>("Styles/handy-fsm-state-visualizer-styles");
            styleSheets.Add(styleSheet);
        }

        #endregion
    }
}