using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BulletInfo : APrefabInfo
{
    
}

[CreateAssetMenu(fileName = "BulletData", menuName = "Custom/BulletData")]
public class BulletData : APrefabData
{
    public List<BulletInfo> infoList;

    public override IEnumerable<APrefabInfo> GetInfoList()
    {
        return infoList;
    }
}
