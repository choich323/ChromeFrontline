using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AddSlotCostInfo
{
    // 1~2번째 slot은 기본 제공이지만, 개발 편의를 위해 cost 0으로 저장
    public int cost;
}

[CreateAssetMenu(fileName = "AddSlotCostData", menuName = "Custom/AddSlotCostData")]
public class AddSlotCostData : ScriptableObject
{
    public List<AddSlotCostInfo> slotCostInfoList;

    public IEnumerable<AddSlotCostInfo> GetInfoList()
    {
        return slotCostInfoList;
    } 
}
