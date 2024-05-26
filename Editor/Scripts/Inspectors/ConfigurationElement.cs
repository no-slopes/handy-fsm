using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;

namespace HandyFSM.Editor
{
    public class ConfigurationElement : VisualElement
    {
        private static readonly string DocumentName = "ConfigurationUI";

        private Configuration _configuration;
        private SerializedObject _serializedObject;
        private BrainInspector _brainInspector;
        private VisualElement _containerMain;

        private ObjectField _fieldOwner;
        private ListView _listSignals;

        public ConfigurationElement(Configuration configuration, BrainInspector brainInspector)
        {
            _configuration = configuration;
            _serializedObject = new SerializedObject(_configuration);
            _brainInspector = brainInspector;

            _containerMain = Resources.Load<VisualTreeAsset>($"UI Documents/{DocumentName}").Instantiate();
            _containerMain.Bind(_serializedObject);

            _fieldOwner = _containerMain.Q<ObjectField>("field-owner");
            _fieldOwner.objectType = typeof(Transform);


            _listSignals = _containerMain.Q<ListView>("list-signals");
            _listSignals.makeItem = () =>
            {
                SignalElement element = new();
                return element;
            };

            _listSignals.bindItem = (element, i) =>
            {
                SignalElement signalElement = element as SignalElement;
                if (_configuration.Signals[i] == null)
                {
                    _configuration.Signals[i] = ScriptableObject.CreateInstance<Signal>();
                    EditorUtility.SetDirty(_configuration.Signals[i]);
                    AssetDatabase.SaveAssetIfDirty(_configuration.Signals[i]);
                }
                signalElement.SetSignal(_configuration.Signals[i]);
            };
            _listSignals.itemsSource = _configuration.Signals;

            Add(_containerMain);
        }

        public void Persist()
        {
            EditorUtility.SetDirty(_configuration);
            AssetDatabase.SaveAssetIfDirty(_configuration);
        }
    }
}
