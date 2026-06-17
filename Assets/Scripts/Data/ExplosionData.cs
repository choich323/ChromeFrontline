using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ExplosionInfo : APrefabInfo
{
    
}

[CreateAssetMenu(fileName = "ExplosionData", menuName = "Custom/ExplosionData")]
public class ExplosionData : APrefabData
{
    public List<ExplosionInfo> infoList;

    public override IEnumerable<APrefabInfo> GetInfoList()
    {
        return infoList;
    }
}
