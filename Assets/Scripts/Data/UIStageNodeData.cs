using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UIStageNodeInfo : APrefabInfo
{
    
}

[CreateAssetMenu(fileName = "UIStageNodeData", menuName = "Custom/UI/UIStageNodeData")]
public class UIStageNodeData : APrefabData
{
    public List<UIStageNodeInfo> infoList = new List<UIStageNodeInfo>();

    public override IEnumerable<APrefabInfo> GetInfoList()
    {
        return infoList;
    }
}
