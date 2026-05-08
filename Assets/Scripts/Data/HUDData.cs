using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HUDInfo : APrefabInfo
{
    
}

[CreateAssetMenu(fileName = "HUDData", menuName = "Custom/HUDData")]
public class HUDData : APrefabData
{
    public List<HUDInfo> hudInfoList = new List<HUDInfo>();

    public override IEnumerable<APrefabInfo> GetInfoList()
    {
        return hudInfoList;
    }
}
