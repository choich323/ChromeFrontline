using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EntityTypeInfo : APrefabInfo
{
    
}

[CreateAssetMenu(fileName = "EntityTypeData", menuName = "Custom/EntityTypeData")]
public class EntityTypeData : APrefabData
{
    public List<EntityTypeInfo> infoList = new List<EntityTypeInfo>();

    public override IEnumerable<APrefabInfo> GetInfoList()
    {
        return infoList;
    }
}
