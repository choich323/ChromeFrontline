using System;
using System.Collections.Generic;
using UnityEngine;

public enum TutorialTriggerType
{
    StageStart,
    StageClear,
    LobbyReturn,
    WorldClear,
    HqHpBelow, 
    SlotUpgrade,
    HqUpgrade,
}

[Serializable]
public class TutorialRequirements
{
    public int stage;
}

[Serializable]
public class TutorialInfo
{
    public TutorialTriggerType triggerType;
    public float value;
    // dialogInfoId, UITargetId 에 해당. 트리거 타입에 따라 활용
    public string id;
}

[CreateAssetMenu(fileName = "TutorialData", menuName = "Custom/Tutorial/TutorialData")]
public class TutorialData : ScriptableObject
{
    public string tutorialId;

    public TutorialRequirements requirements;
    public List<TutorialInfo> infoList = new List<TutorialInfo>();
}
