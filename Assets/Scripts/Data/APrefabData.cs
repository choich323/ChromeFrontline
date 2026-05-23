using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class APrefabInfo
{
    public string id;
    public GameObject prefab;

    public virtual PrefabID GetPrefabID()
    {
        return Managers.Data.ConvertStringToPrefabID(id);
    }
}

public abstract class APrefabData : ScriptableObject
{
    public abstract IEnumerable<APrefabInfo> GetInfoList();
}