using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class StageSaveInfo
{
    public long tick;
    public int stage;
    public bool isCleared;
    public int starCount;

    public void SetInfo(StageSaveInfo argSaveInfo)
    {
        stage = argSaveInfo.stage;
        isCleared = argSaveInfo.isCleared;
        starCount = argSaveInfo.starCount;
    }
}

[Serializable]
public class StageRecord
{
    public const float INVALID_CLEAR_TIME = float.MaxValue;
    public const int INVALID_HQ_HP_RATIO = -1;
    
    public long tick;
    public bool isClear = false;
    public float clearTime = INVALID_CLEAR_TIME;
    public int hqhpRatio = INVALID_HQ_HP_RATIO;

    public bool IsFirstTry()
    {
        return !isClear && hqhpRatio == INVALID_HQ_HP_RATIO;
    }
}

[Serializable]
public class UserRecord
{
    // stage마다 클리어 기준 시간을 다르게 하는 것도 좋지만,
    // 유저가 별을 달성하기 위한 조건을 매번 확인하는 것은 귀찮을 것 같다.
    // 고정값으로 해야 유저가 당연히 그렇겠지 하고 기준을 맞출 듯.
    // 물론 고정하지 않으면 그건 그거대로 유저가 적응하겠지만.. 바라는 바는 아님.
    public const float CLEAR_TIME_THRESHOLD = 480f; // 8분
    private const int CLEAR_HQ_HP_RATIO = 100;
    private const int BASIC_SKILL_ID = 1001;
    private const int EMPTY_SKILL_ID = 0;
    private const int SKILL_SLOT_MAX = 4;
    
    // stage, <tick, success, bestTime, bestHqHp>>
    [JsonProperty]
    private Dictionary<int, StageRecord> _stageBestRecordDict = new Dictionary<int, StageRecord>();
    
    [JsonProperty]
    private int _maxUnlockedWorld = 1;

    [JsonProperty]
    private int _maxClearedStage = 0;

    [JsonProperty]
    private Dictionary<int, StageSaveInfo> _stageSaveInfoDict = new Dictionary<int, StageSaveInfo>();

    [JsonProperty]
    private int _chrome = 0;
    
    [JsonProperty]
    private long _lastChromeUpdateTick = 0;
    
    [JsonProperty]
    private HashSet<string> _completedTutorialIds = new HashSet<string>();

    [JsonProperty]
    private HashSet<int> _skillIds = new HashSet<int> ();

    [JsonProperty] 
    private List<int> _equipmentIds = new List<int> ();
    
    [JsonIgnore]
    public int MaxUnlockedWorld => _maxUnlockedWorld;
    
    [JsonIgnore]
    public int MaxClearedStage => _maxClearedStage;

    [JsonIgnore]
    public int Chrome => _chrome;
    
    [JsonIgnore]
    public IReadOnlyCollection<int> SkillIds => _skillIds;
    
    [JsonIgnore]
    public IReadOnlyCollection<int> EquipmentIds => _equipmentIds;

    public void CreateInitialData()
    {
        if (_skillIds.Count <= 0)
        {
            _skillIds.Add(BASIC_SKILL_ID);
        }

        if (_equipmentIds.Count <= 0)
        {
            for (int i = 0; i < SKILL_SLOT_MAX; i++)
            {
                _equipmentIds.Add(EMPTY_SKILL_ID);
            }
        }
    }
    
    public bool IsCompletedTutorialId(string argTutorialId)
    {
        return _completedTutorialIds.Contains(argTutorialId);
    }

    public void AddCompletedTutorialId(string argTutorialId)
    {
        _completedTutorialIds.Add(argTutorialId);
    }
    
    public void EarnChrome(int argAmount)
    {
        if (argAmount <= 0)
        {
            return;
        }
        _chrome += argAmount;
        _lastChromeUpdateTick = DateTime.Now.Ticks;
    }

    public bool ConsumeChrome(int argAmount)
    {
        if (argAmount <= 0 || _chrome < argAmount)
        {
            return false;
        }
        
        _chrome -= argAmount;
        _lastChromeUpdateTick = DateTime.Now.Ticks;
        return true;
    }
    
    public StageSaveInfo GetStageSaveInfo(int argStage)
    {
        return _stageSaveInfoDict.GetValueOrDefault(argStage);
    }
    
    public void SaveStageSaveInfo(int argStage, StageSaveInfo argStageSaveInfo, bool argIsLastStage = false)
    {
        if (_stageSaveInfoDict.ContainsKey(argStage) && argStageSaveInfo.tick < _stageSaveInfoDict[argStage].tick)
        {
            return;
        }
        
        _stageSaveInfoDict[argStage] = argStageSaveInfo;
        int curWorld = Managers.Data.GetWorldNumber(Managers.Game.CurWorldId);
        _maxUnlockedWorld += argIsLastStage && _maxUnlockedWorld == curWorld ? 1 : 0;
        _maxClearedStage = Math.Max(_maxClearedStage, argStage);
    }
    
    public void SaveStageBestRecord(int argStage, StageRecord argRecord)
    {
        if (_stageBestRecordDict.ContainsKey(argStage) && argRecord.tick < _stageBestRecordDict[argStage].tick)
        {
            return;
        }
        
        _stageBestRecordDict[argStage] = argRecord;
    }
    
    public StageRecord GetStageBestRecord(int argStage)
    {
        if (!_stageBestRecordDict.TryGetValue(argStage, out var result))
        {
            result = new StageRecord();
            result.tick = DateTime.Now.Ticks;
            _stageBestRecordDict.Add(argStage, result);
        }
        
        return result;
    }
    
    public void Save(UserRecord argUserRecord)
    {
        foreach (var kvp in argUserRecord._stageSaveInfoDict)
        {
            SaveStageSaveInfo(kvp.Key, kvp.Value);
        }
        
        foreach (var kvp in argUserRecord._stageBestRecordDict)
        {
            SaveStageBestRecord(kvp.Key, kvp.Value);
        }
    }
    
    public int GetStarCount()
    {
        int count = 0;
        foreach (var kvp in _stageBestRecordDict)
        {
            var stage = kvp.Key;
            if (IsClear(stage))
            {
                count++;
            }

            if (IsClearInTime(stage))
            {
                count++;
            }

            if (IsClearHqHp(stage))
            {
                count++;
            }
        }

        return count;
    }

    public bool IsClear(int argStage)
    {
        if (!_stageBestRecordDict.ContainsKey(argStage))
            return false;
        
        return _stageBestRecordDict[argStage].isClear;
    }

    public bool IsClearInTime(int argStage)
    {
        if (!_stageBestRecordDict.ContainsKey(argStage))
        {
            return false;
        }
        
        if (_stageBestRecordDict[argStage].clearTime <= CLEAR_TIME_THRESHOLD)
        {
            return true;
        }

        return false;
    }

    public bool IsClearHqHp(int argStage)
    {
        if (!_stageBestRecordDict.ContainsKey(argStage))
        {
            return false;
        }
        
        if (_stageBestRecordDict[argStage].hqhpRatio >= CLEAR_HQ_HP_RATIO)
        {
            return true;
        }
        return false;
    }

    public void AddSkillId(int argSkillId)
    {
        _skillIds.Add(argSkillId);
    }

    public void SetEquipmentSkillId(List<int> argSkillIdList)
    {
       _equipmentIds.Clear();
       foreach (var id in argSkillIdList)
       {
           _equipmentIds.Add(id);
       }
    }
}
