using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class HeadQuarterUpgradeInfo
{
    public int tier;
    public int upgradeCost;
    public int maxHp;
    public int goldPerSecond;
    public int maxSlotCount;
    public float productionTimeBonus;
    public Grade minGrade;
}

[CreateAssetMenu(fileName = "HeadQuarterUpgradeData", menuName = "Custom/HeadQuarterUpgradeData")]
public class HeadQuarterUpgradeData : ScriptableObject
{
    public List<HeadQuarterUpgradeInfo> upgradeInfoList = new List<HeadQuarterUpgradeInfo>();
    
    public IEnumerable<HeadQuarterUpgradeInfo> GetInfoList()
    {
        return upgradeInfoList;
    }
}
