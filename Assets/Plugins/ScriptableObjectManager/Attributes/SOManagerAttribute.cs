using System;

namespace MD.Editor.SOM
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SOManagerAttribute : Attribute
    {
        public string Name;
        public string DefaultSaveLocation;
        public SOManagerAttribute(string name = "", string defaultSaveLocation = "")
        {
            Name = name;
            DefaultSaveLocation = defaultSaveLocation;
        }
    }
}