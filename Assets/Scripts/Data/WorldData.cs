using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StageInfo
{
    [Header("=== Lobby UI Info ===")]
    public int stage;
    public string stageName;
    public Vector2 uiPosition;   // 에디터에서 베이킹할 맵 상의 앵커 좌표

    [Header("=== Gameplay Metadata ===")]
    public int stageIndex;       // 월드 내 정렬 순서 (해금 조건 판정용)
    public string aiScheduleId;  // 난이도 설정용

    // 임시 데이터. 최대 레벨 제한을 두는게 맞는가?
    public int maxHqLevel;
    public ulong playerAvailableEntitiesMask;
    public ulong enemyAvailableEntitiesMask;

    public ulong GetMask(Team argTeam)
    {
        if (argTeam == Team.Player)
        {
            return playerAvailableEntitiesMask;
        }
        else
        {
            return enemyAvailableEntitiesMask;
        }
    }
        
    // 에디터 툴에서 좌표를 직접 구워넣기 위한 Setter
#if UNITY_EDITOR
    public void BakePosition(Vector2 argPosition)
    {
        uiPosition = argPosition;
    }
#endif
}

[CreateAssetMenu(fileName = "WorldData", menuName = "Custom/WorldData")]
public class WorldData : ScriptableObject
{
    public string worldId;
    public Sprite bg;
    
    public List<StageInfo> stageInfoList = new List<StageInfo>();

    public IEnumerable<StageInfo> GetStageInfoList()
    {
        return stageInfoList;
    }
    
    public StageInfo GetStageInfo(int argStage)
    {
        return stageInfoList.Find(info => info.stage == argStage);
    }
}