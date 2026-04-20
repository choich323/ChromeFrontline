using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class APrefabInfo
{
    public GameObject prefab;

    public PrefabID GetPrefabID()
    {
        return Managers.Data.ConvertStringToPrefabID(prefab.name);
    }
}

public abstract class APrefabData : ScriptableObject
{
    public abstract IEnumerable<APrefabInfo> GetInfoList();
}