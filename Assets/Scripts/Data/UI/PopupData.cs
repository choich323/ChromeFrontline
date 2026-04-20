using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PopupInfo : APrefabInfo
{
    
}

[CreateAssetMenu(fileName = "PopupData", menuName = "Custom/PopupData")]
public class PopupData : APrefabData
{
    public List<PopupInfo> popupInfoList = new List<PopupInfo>();

    public override IEnumerable<APrefabInfo> GetInfoList()
    {
        return popupInfoList;
    }
}
