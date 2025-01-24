using System;
using UnityEditor;
using UnityEngine;


namespace MD.Editor.SOM.Data
{
    [Serializable]
    public class TypeData
    {
        public Type Type { get; private set; }
        public string Name { get; private set; }

        public TypeData(Type type)
        {
            Type = type;
            Name = ObjectNames.NicifyVariableName(type.Name);
        }

        public override string ToString()
        {
            return $"Type: {Type.ToString()}, Name: {Name}";
        }

    }
}