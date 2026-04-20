using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UIInfo : APrefabInfo
{
    
}

[CreateAssetMenu(fileName = "UIData", menuName = "Custom/UIData")]
public class UIData : APrefabData
{
    public List<UIInfo> uiInfoList = new List<UIInfo>();

    public override IEnumerable<APrefabInfo> GetInfoList()
    {
        return uiInfoList;
    }
}
