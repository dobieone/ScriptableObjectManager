using System;

namespace MD.Editor.SOM
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SOManagerAttribute : Attribute
    {
        public string Name;
        public SOManagerAttribute(string name = "")
        {
            Name = name;
        }
    }
}