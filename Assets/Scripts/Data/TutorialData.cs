using System;
using System.Collections.Generic;
using UnityEngine;

public enum TutorialTriggerType
{
    LobbyEnter,
    StageStart,
    StageEnd,
    StageStartDialogEnd,
    StageEndDialogEnd,
}

public enum TutorialTextPosType
{
    None = 0,
    Top,
    Bottom,
    Right,
    Left
}

[Serializable]
public class TutorialRequirements
{
    // 이 스테이지까지 클리어하는 것이 요구사항
    public int stage;
}

[Serializable]
public class TutorialInfo
{
    public string targetId;
    public TutorialTextPosType textPos;
    public LocalizationText text;
}

[CreateAssetMenu(fileName = "TutorialData", menuName = "Custom/Tutorial/TutorialData")]
public class TutorialData : ScriptableObject
{
    public string id;

    public TutorialRequirements requirements;
    public TutorialTriggerType triggerType;

    public List<TutorialInfo> infoList = new List<TutorialInfo>();
}
