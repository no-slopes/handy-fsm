using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HandyFSM.Editor
{
    [CustomEditor(typeof(HandyFSMBrain), true)]
    public class BrainInspector : UnityEditor.Editor
    {

        private static readonly string DocumentName = "BrainUI";

        [SerializeField]
        private MonoScript _scriptAsset;

        private HandyFSMBrain _brain;
        private VisualElement _containerMain;
        private RuntimeInfoElement _containerRuntimeInfo;
        private ConfigurationElement _containerConfiguration;

        public HandyFSMBrain Brain => _brain;

        public override VisualElement CreateInspectorGUI()
        {
            _brain = target as HandyFSMBrain;

            _containerMain = Resources.Load<VisualTreeAsset>($"UI Documents/{DocumentName}").Instantiate();

            ObjectField scriptField = _containerMain.Query<ObjectField>("script-field");
            scriptField.SetEnabled(false);
            scriptField.value = MonoScript.FromMonoBehaviour(target as MonoBehaviour);

            if (_brain.Info == null)
            {
                _brain.Info = CreateInstance<RuntimeInfo>();
                MarkAsDirty(true);
            }

            _containerRuntimeInfo = new RuntimeInfoElement(_brain.Info);
            _containerMain.Add(_containerRuntimeInfo);

            if (_brain.Config == null)
            {
                _brain.Config = CreateInstance<Configuration>();
                MarkAsDirty(true);
            }

            _containerConfiguration = new ConfigurationElement(_brain.Config, this);
            _containerMain.Add(_containerConfiguration);


            return _containerMain;
        }

        public void MarkAsDirty(bool persist = false)
        {
            EditorUtility.SetDirty(_brain);

            if (persist)
                AssetDatabase.SaveAssetIfDirty(_brain);
        }
    }
}