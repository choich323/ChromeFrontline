using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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
    private const string DEFAULT_WORLD_ID = "world1";
    
    // stage, <tick, success, bestTime, bestHqHp>>
    [JsonProperty]
    private Dictionary<int, StageRecord> _stageBestRecordDict = new Dictionary<int, StageRecord>();

    private string _currentWorldId = DEFAULT_WORLD_ID;

    [JsonProperty]
    private Dictionary<int, StageSaveInfo> _stageSaveInfoDict = new Dictionary<int, StageSaveInfo>();

    public string CurrentWorldId => _currentWorldId;

    public void SetCurrentWorldId(string argWorldId)
    {
        _currentWorldId = argWorldId;
    }

    public StageSaveInfo GetStageSaveInfo(int argStage)
    {
        if (!_stageSaveInfoDict.TryGetValue(argStage, out var result))
        {
            result = new StageSaveInfo();
            result.stage = argStage;
            result.tick = DateTime.Now.Ticks;
            _stageSaveInfoDict.Add(argStage, result);
        }

        return result;
    }
    
    public void SaveStageSaveInfo(int argKey, StageSaveInfo argStageSaveInfo)
    {
        _stageSaveInfoDict[argKey] = argStageSaveInfo;
    }
    
    public void SaveStageBestRecord(int argKey, StageRecord argRecord)
    {
        _stageBestRecordDict[argKey] = argRecord;
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
        foreach (var record in argUserRecord._stageBestRecordDict)
        {
            var key = record.Key;
            if (!_stageBestRecordDict.ContainsKey(key) || record.Value.tick > _stageBestRecordDict[key].tick)
            {
                _stageBestRecordDict[key] = record.Value;
            }
        }

        foreach (var info in argUserRecord._stageSaveInfoDict)
        {
            var key = info.Key;
            if (!_stageSaveInfoDict.ContainsKey(key) || info.Value.tick > _stageSaveInfoDict[key].tick)
            {
                _stageSaveInfoDict[key] = info.Value;
            }
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
}
