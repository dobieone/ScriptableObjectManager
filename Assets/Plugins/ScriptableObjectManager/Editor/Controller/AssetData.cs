using System;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace MD.Editor.SOM.Data
{
    [Serializable]
    public class AssetData
    {
        public TypeData Type;
        public string Guid;

        public AssetData(TypeData type, string guid)
        {
            Type = type;
            Guid = guid;
        }
        public string Name()
        {
            var a = AssetDatabase.GUIDToAssetPath(Guid);
            var n = Path.GetFileNameWithoutExtension(a);
            return n;
        }

        public override string ToString()
        {
            return $"Type: {Type.Name}, Name:{Name()}, Guid: {Guid}";
        }
    }
}