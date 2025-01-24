using MD.Editor.SOM.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MD.Editor.SOM
{
    public class ScriptableObjectManager : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset _visualTreeAsset = default;

        private SOMData _data = new SOMData();

        [MenuItem("Tools/Scriptable Object Manager")]
        public static void ShowExample()
        {
            ScriptableObjectManager wnd = GetWindow<ScriptableObjectManager>();
            wnd.titleContent = new GUIContent("Scriptable Object Manager");
        }

        public void CreateGUI()
        {
            _data.Init();

            VisualElement root = rootVisualElement;
            VisualElement uxml = _visualTreeAsset.Instantiate();
            root.Add(uxml);

            var somc = new SOMController(root, _data);
        }
    }
}