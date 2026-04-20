using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HeadQuarterInfo : APrefabInfo
{
    public int hp;
    public int shield;
}

[CreateAssetMenu(fileName = "HeadQuarterData", menuName = "Custom/HeadQuarterData")]
public class HeadQuarterData : APrefabData
{
    public List<HeadQuarterInfo> hqInfoList = new List<HeadQuarterInfo>();

    public override IEnumerable<APrefabInfo> GetInfoList()
    {
        return hqInfoList;
    }
}
