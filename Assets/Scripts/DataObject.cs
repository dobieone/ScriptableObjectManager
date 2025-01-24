using MD.Editor.SOM;
using UnityEngine;

[SOManager()]
[CreateAssetMenu]
public class DataObject : ScriptableObject
{
    public string Name;

    public override string ToString()
    {
        return Name;
    }
}
