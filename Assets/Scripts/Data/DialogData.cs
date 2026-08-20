using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class DialogInfo
{
    public string infoId;
    public LocalizationText talker;
    public LocalizationText text;
}

[CreateAssetMenu(fileName = "DialogData", menuName = "Custom/DialogData")]
public class DialogData : ScriptableObject
{
    public string dialogDataId;
    public List<DialogInfo> dialogInfoList = new List<DialogInfo>();

    public IEnumerable<DialogInfo> GetInfoList()
    {
        return dialogInfoList;
    }
}
