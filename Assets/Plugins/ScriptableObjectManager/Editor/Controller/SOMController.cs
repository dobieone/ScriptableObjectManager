using MD.Editor.SOM.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace MD.Editor.SOM
{
    public class SOMController
    {
        private SOMData _data;
        private VisualElement _root;

        private ToolbarButton _typeRefreshButton;
        private ToolbarButton _assetRefreshButton;
        private ToolbarButton _assetAddButton;
        private ToolbarButton _duplicateAssetButton;
        private ToolbarButton _deleteAssetButton;

        private ListView _typeListView;
        private ListView _assetListView;
        private VisualElement _inspectorContainer;
        private Button _inspectorPath;

        private TypeData _selectedType;
        private AssetData _selectedAsset;

        private ToolbarSearchField _searchFieldType;
        private ToolbarSearchField _searchFieldAsset;

        private Label _message;

        public SOMController(VisualElement root, SOMData data)
        {
            _data = data;
            _root = root;
            SetToolbarButtons();
            SetToolbarSearch();

            _typeListView = root.Q<ListView>(name: "TypeListView");
            _assetListView = root.Q<ListView>(name: "AssetListView");
            _inspectorContainer = root.Q<VisualElement>(name: "Inspector");
            _inspectorPath = root.Q<Button>(name: "InspectorPath");
            _inspectorPath.clicked += SelectAsset;
            _message = root.Q<Label>(name: "Message");

            _typeListView.makeItem = MakeItemType;
            _typeListView.bindItem = (e, i) => BindItemType(e, i, _data.Types[i], AddAsset);
            _typeListView.itemsSource = _data.Types;
            _typeListView.dataSource = _data.Types;
            _typeListView.bindingPath = "_data.Types";
            _typeListView.fixedItemHeight = 28;
            _typeListView.selectionChanged += item => ViewAssetList(item);
            _typeListView.selectionType = SelectionType.Single;

            _assetListView.makeItem = MakeItemAsset;
            _assetListView.bindItem = (e, i) => BindItemAsset(e, i, _data.Assets[i]);
            _assetListView.itemsSource = _data.Assets;
            _assetListView.dataSource = _data.Assets;
            _assetListView.bindingPath = "_data.Assets";
            _assetListView.fixedItemHeight = 28;
            _assetListView.selectionChanged += item => ViewAsset(item);
            _assetListView.selectionType = SelectionType.Single;

            VisualElement MakeItemType()
            {
                var container = new Box();
                container.AddToClassList("soe-listview-item");

                var lbl = new Label();
                lbl.AddToClassList("soe-listview-label");

                var btn = new Button();
                btn.text = "+";
                btn.AddToClassList("soe-listview-button");

                container.Add(lbl);
                container.Add(btn);

                return container;
            }

            VisualElement MakeItemAsset()
            {
                var container = new Box();
                container.AddToClassList("soe-listview-item");

                var lbl = new Label();
                lbl.AddToClassList("soe-listview-label");

                container.Add(lbl);

                return container;
            }

            static void BindItemType(VisualElement element, int index, TypeData data, Action<EventBase> addAction)
            {
                var name = element.Q<Label>();
                var button = element.Q<Button>();
                button.userData = data;

                name.text = data.Name;
                button.clickable.clickedWithEventInfo += addAction;
            }

            static void BindItemAsset(VisualElement element, int index, AssetData data)
            {
                var name = element.Q<Label>();
                name.text = data.Name();
            }

        }

        private void SetMessage()
        {
            var type = _selectedType?.Name;
            var asset = _selectedAsset?.Name();

            var message = string.Empty;
            if (type != null && asset == null)
            {
                message = type;
            }
            else if (asset != null)
            {
                message = string.Join(" > ", type, asset);
            }

            _message.text = message;
        }

        private void ViewAssetList(IEnumerable<object> data)
        {
            if (data == null || data.Count() == 0)
                return;

            var t = data.First() as TypeData;

            _selectedType = t;
            _selectedAsset = null;

            _data.UpdateAssetList(t);
            _assetListView.Rebuild();
            _assetListView.ClearSelection();

            _searchFieldAsset.value = string.Empty;

            ClearInspector();
        }

        public void AddAsset()
        {
            if (_selectedType == null)
                return;
            AddAsset(_selectedType);
        }

        private void AddAsset(EventBase e)
        {
            e.StopImmediatePropagation();

            var btn = e.target as Button;
            var td = btn.userData as TypeData;

            AddAsset(td);
        }


        private void AddAsset(TypeData data)
        {
            var asset = ScriptableObject.CreateInstance(data.Type);
            string directory = EditorUtility.SaveFilePanelInProject("Select Directory", "New " + data.Name, "asset", "", "Assets");

            if (asset != null && !string.IsNullOrEmpty(directory))
            {
                AssetDatabase.CreateAsset(asset, directory);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                var guid = AssetDatabase.GUIDFromAssetPath(directory);
                var n = Path.GetFileNameWithoutExtension(directory);
                var a = new AssetData(data, guid.ToString());

                _data.AddAssetToList(a);
                _assetListView.Rebuild();

                SelectAssetInListView(a);
            }
        }

        private void ViewAsset(IEnumerable<object> data)
        {
            if (data == null || data.Count() == 0)
                return;

            var a = data.First() as AssetData;
            ViewAsset(a);
        }

        private void ViewAsset(AssetData data)
        {
            _selectedAsset = data;

            var path = AssetDatabase.GUIDToAssetPath(data.Guid);
            var a = AssetDatabase.LoadAssetAtPath(path, data.Type.Type);

            var so = new SerializedObject(a);

            ClearInspector();
            InspectorElement.FillDefaultInspector(_inspectorContainer, so, null);
            _inspectorContainer.Bind(so);
            _inspectorPath.text = path;
        }

        private void SelectAsset()
        {
            if (_selectedAsset == null)
                return;

            var path = AssetDatabase.GUIDToAssetPath(_selectedAsset.Guid);
            EditorUtility.FocusProjectWindow();
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            Selection.activeObject = obj;
        }

        private void ClearInspector()
        {
            _inspectorContainer.Clear();
            _inspectorPath.text = "";
            SetMessage();
        }

        public void DeleteAsset()
        {
            if (_selectedAsset == null)
                return;

            var currentPath = AssetDatabase.GUIDToAssetPath(_selectedAsset.Guid);
            FileUtil.DeleteFileOrDirectory(currentPath);
            FileUtil.DeleteFileOrDirectory(currentPath + ".meta");
            AssetDatabase.Refresh();

            _data.DeleteAsset(_selectedAsset);
            _assetListView.Rebuild();

        }

        public void DuplicateAsset()
        {
            if (_selectedAsset == null)
                return;

            char separator = Path.DirectorySeparatorChar;

            var currentPath = AssetDatabase.GUIDToAssetPath(_selectedAsset.Guid);
            var filename = Path.GetFileNameWithoutExtension(currentPath);
            var path = Path.GetDirectoryName(currentPath);
            var extension = Path.GetExtension(currentPath);

            if (filename.IndexOf(" - Copy") > -1)
                filename = filename.Substring(0, filename.IndexOf(" - Copy"));

            for (int i = 1; i <= 100; i++)
            {
                var newFile = $"{path}{separator}{filename} - Copy ({i}){extension}";
                var check = File.Exists(newFile);
                if (!check)
                {
                    if (AssetDatabase.CopyAsset(currentPath, newFile))
                    {
                        AssetDatabase.Refresh();
                        var guid = AssetDatabase.GUIDFromAssetPath(newFile);
                        var n = Path.GetFileNameWithoutExtension(newFile);
                        var a = new AssetData(_selectedAsset.Type, guid.ToString());

                        _data.AddAssetToList(a);
                        _assetListView.Rebuild();

                        SelectAssetInListView(a);

                        break;
                    }
                }
            }
        }

        private void SelectAssetInListView(AssetData data)
        {
            var idx = _data.Assets.IndexOf(data);
            _assetListView.SetSelection(idx);

            ViewAsset(data);
        }

        private void SetToolbarButtons()
        {
            _typeRefreshButton = _root.Q<ToolbarButton>(name: "TypeRefreshButton");
            _assetRefreshButton = _root.Q<ToolbarButton>(name: "AssetRefreshButton");
            _assetAddButton = _root.Q<ToolbarButton>(name: "AssetAddButton");
            _duplicateAssetButton = _root.Q<ToolbarButton>(name: "DuplicateAssetButton");
            _deleteAssetButton = _root.Q<ToolbarButton>(name: "DeleteAssetButton");

            var refresh = EditorGUIUtility.IconContent("Refresh").image;
            _typeRefreshButton.Add(new Image { image = refresh });
            _assetRefreshButton.Add(new Image { image = refresh });

            var plus = EditorGUIUtility.IconContent("Toolbar Plus").image;
            _assetAddButton.Add(new Image { image = plus });

            _typeRefreshButton.clicked += RefreshTypeList;
            _assetRefreshButton.clicked += RefreshAssetList;

            _assetAddButton.clicked += AddAsset;
            _duplicateAssetButton.clicked += DuplicateAsset;
            _deleteAssetButton.clicked += DeleteAsset;
        }

        private void RefreshTypeList()
        {
            ClearInspector();
            _selectedAsset = null;
            _selectedType = null;

            _data.UpdateLists();

            _typeListView.Rebuild();
            _assetListView.Rebuild();
        }

        private void RefreshAssetList()
        {
            ClearInspector();
            _selectedAsset = null;

            if (_selectedType == null)
                return;

            _data.UpdateAssetList(_selectedType);

            _assetListView.Rebuild();
        }

        private void SetToolbarSearch()
        {
            _searchFieldType = _root.Q<ToolbarSearchField>(name: "SearchType");
            _searchFieldAsset = _root.Q<ToolbarSearchField>(name: "SearchAsset");

            _searchFieldType.RegisterValueChangedCallback(FilterTypes);
            _searchFieldAsset.RegisterValueChangedCallback(FilterAssets);
        }

        private void FilterTypes(ChangeEvent<string> e)
        {
            var v = e.newValue;

            if (v == string.Empty)
            {
                _data.RemoveFilterFromTypes();
                _typeListView.Rebuild();
                _typeListView.ClearSelection();

                RefreshAssetList();
                ClearInspector();

                return;
            }

            if (v.Length > 2)
            {
                _selectedAsset = null;
                _selectedType = null;

                _data.ApplyFilterToTypes(v);

                _typeListView.Rebuild();
                _typeListView.ClearSelection();

                _data.UpdateAssetList(_selectedType);
                _assetListView.RefreshItems();
                _assetListView.Rebuild();
                ClearInspector();
            }
        }

        private void FilterAssets(ChangeEvent<string> e)
        {
            var v = e.newValue;

            if (v == string.Empty)
            {
                _selectedAsset = null;

                _data.RemoveFilterFromAssets();
                RefreshAssetList();
                _assetListView.ClearSelection();

                return;
            }

            if (v.Length > 2)
            {
                _selectedAsset = null;

                _data.ApplyFilterToAssets(v);

                _assetListView.Rebuild();

                ClearInspector();
            }
        }
    }
}