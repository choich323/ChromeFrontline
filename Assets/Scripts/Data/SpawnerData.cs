using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpawnerInfo : APrefabInfo
{
    
}

[CreateAssetMenu(fileName = "SpawnerData", menuName = "Custom/SpawnerData")]
public class SpawnerData : APrefabData
{
    public List<SpawnerInfo> spawnerInfoList = new List<SpawnerInfo>();
    
    public override IEnumerable<APrefabInfo> GetInfoList()
    {
        return spawnerInfoList;
    }
}
