using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Dialogue
{
    public LocalizationText talker;
    public LocalizationText text;
}

[Serializable]
public class DialogInfo
{
    public string infoId;
    public List<Dialogue> dialogueList = new();
}

[CreateAssetMenu(fileName = "DialogData", menuName = "Custom/Dialog/DialogData")]
public class DialogData : ScriptableObject
{
    public string stage;
    public List<DialogInfo> dialogInfoList = new();

    public DialogInfo GetDialogInfo(string argInfoId)
    {
        return dialogInfoList.Find(info => info.infoId == argInfoId);
    }
}