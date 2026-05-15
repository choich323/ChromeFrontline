using System.Diagnostics;
using UnityEngine;

public class OpenPersistentPath
{
    [UnityEditor.MenuItem("Tools/Open Persistent Data Path")]
    static void Open()
    {
        Process.Start(Application.persistentDataPath);
    }
}
