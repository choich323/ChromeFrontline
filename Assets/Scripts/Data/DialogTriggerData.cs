using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum DialogTriggerType
{
    HqHpBelow,
    StageFirstClear,
    StageStart,
}

[Serializable]
public class DialogTriggerInfo
{
    public string triggerId;
    public DialogTriggerType triggerType;
    public float value;
    public string dialogInfoId;
}

[CreateAssetMenu(fileName = "DialogTriggerData", menuName = "Custom/Dialog/DialogTriggerData")]
public class DialogTriggerData : ScriptableObject
{
    public string stage;
    public List<DialogTriggerInfo> triggerInfoList = new();
}