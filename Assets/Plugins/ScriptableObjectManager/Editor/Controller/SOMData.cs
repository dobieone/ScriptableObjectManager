using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace MD.Editor.SOM.Data
{
    public class SOMData
    {
        public Dictionary<TypeData, List<AssetData>> Data = new Dictionary<TypeData, List<AssetData>>();
        public List<TypeData> Types = new List<TypeData>();
        public List<AssetData> Assets = new List<AssetData>();

        public void Init()
        {
            UpdateTypes();
            UpdateLists();
        }

        public void ApplyFilterToTypes(string filter)
        {
            var t = new List<TypeData>();
            foreach (var item in Types)
            {
                if (item.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
                    t.Add(item);
            }

            if (t.Count > 0)
            {
                Types.Clear();
                Types.AddRange(t);
            }
            else
            {
                Types.Clear();
            }
        }

        public void ApplyFilterToAssets(string filter)
        {
            var a = new List<AssetData>();
            foreach (var item in Assets)
            {
                if (item.Name().Contains(filter, StringComparison.InvariantCultureIgnoreCase))
                    a.Add(item);
            }

            if (a.Count > 0)
            {
                Assets.Clear();
                Assets.AddRange(a);
            }
            else
            {
                Assets.Clear();
            }
        }

        public void RemoveFilterFromTypes()
        {
            UpdateLists();
        }

        public void RemoveFilterFromAssets()
        {
            UpdateLists();
        }

        public void UpdateLists()
        {
            Types.Clear();
            foreach (var item in Data.Keys)
            {
                Types.Add(item);
            }
            Assets.Clear();
        }

        public void UpdateAssetList(TypeData type)
        {
            Assets.Clear();

            if (type == null)
                return;

            Assets.AddRange(Data[type]);
        }

        public void UpdateAssetList(string type)
        {
            var t = Types.Where(t => t.Name == type).FirstOrDefault();
            UpdateAssetList(t);
        }

        public void DeleteAsset(AssetData data)
        {
            var i = -1;
            i = Data[data.Type].IndexOf(data);
            if (i >= 0)
                Data[data.Type].RemoveAt(i);

            UpdateAssetList(data.Type);
        }

        public void OutputLists()
        {
            foreach (var item in Data)
            {
                foreach (var data in item.Value)
                {
                    Debug.Log(data.ToString());
                }
            }

            foreach (var item in Types)
                Debug.Log(item.ToString());

            foreach (var item in Assets)
                Debug.Log(item.ToString());
        }

        public void UpdateTypes()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    var attrs = type.GetCustomAttributes(typeof(SOManagerAttribute), false);

                    foreach (Attribute attr in attrs)
                    {
                        if (attr is SOManagerAttribute a)
                        {
                            AddItemToList(type);
                        }
                    }
                }
            }
        }

        public void UpdateAssets(TypeData type)
        {
            Data[type].Clear();

            string[] guids = AssetDatabase.FindAssets($"t:{type.Type}", null);

            foreach (string guid in guids)
            {
                var a = AssetDatabase.GUIDToAssetPath(guid);
                var n = Path.GetFileNameWithoutExtension(a);

                AddAssetToListWithoutUpdate(new AssetData(type, guid));
            }

        }

        public void AddAssetToList(AssetData asset)
        {
            Data[asset.Type].Add(asset);
            UpdateAssetList(asset.Type);
        }

        public void AddAssetToListWithoutUpdate(AssetData asset)
        {
            Data[asset.Type].Add(asset);
        }

        private void AddItemToList(Type type)
        {
            var td = new TypeData(type);

            if (!Data.ContainsKey(td))
            {
                Data.Add(td, new List<AssetData>());
            }
            UpdateAssets(td);
        }

    }
}