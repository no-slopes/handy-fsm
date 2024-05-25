using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;

namespace HandyFSM.Editor
{
    [CustomPropertyDrawer(typeof(Configuration), true)]
    public class ConfigurationDrawer : PropertyDrawer
    {
        private VisualElement _containerMain;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualTreeAsset treeAsset = Resources.Load<VisualTreeAsset>("UI Documents/ConfigurationUI");
            TemplateContainer container = treeAsset.Instantiate();
            _containerMain = container.Q<VisualElement>("container-main");

            Type targetType = property.serializedObject.targetObject.GetType();
            if (targetType != typeof(HandyFSMBrain))
            {
                container.Remove(_containerMain);

                Foldout foldout = new()
                {
                    text = "Machine Configuration",
                    value = false,
                };

                foldout.Add(_containerMain);
                container.Add(foldout);
            }

            return container;
        }
    }
}
