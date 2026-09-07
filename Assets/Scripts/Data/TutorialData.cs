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
}

[CreateAssetMenu(fileName = "TutorialData", menuName = "Custom/Tutorial/TutorialData")]
public class TutorialData : ScriptableObject
{
    public string id;

    public TutorialRequirements requirements;
    public TutorialTriggerType triggerType;

    public List<TutorialInfo> infoList = new List<TutorialInfo>();
}
