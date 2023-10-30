using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace HandyFSM.Editor
{
    [CustomPropertyDrawer(typeof(Configuration), true)]
    public class ConfigurationDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualTreeAsset treeAsset = Resources.Load<VisualTreeAsset>("UI Documents/ConfigurationUI");
            TemplateContainer container = treeAsset.Instantiate();
            return container;
        }
    }
}
