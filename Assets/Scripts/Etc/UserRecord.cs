using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class StageSaveInfo
{
    public string stageId;       // StageData의 stageId와 동일하게 맞춤
    public bool isCleared;
    public int starCount;
}

[Serializable]
public class UserRecord
{
    private const float CLEAR_TIME_THRESHOLD = 720f; // 12분
    private const int CLEAR_HQ_HP_RATIO = 100;
    private const int DEFAULT_STAGE = 1;
    private const string DEFAULT_WORLD_ID = "World_1";
    
    // stage, <tick, success, bestTime, bestHqHp>>
    [JsonProperty]
    private Dictionary<int, (long tick, bool clear, float clearTime, int hqhpRatio)> _stageBestRecordDict = new Dictionary<int, (long, bool, float, int)>();

    private string _currentWorldId = DEFAULT_WORLD_ID;
    private List<StageSaveInfo> _stageSaveInfoList = new List<StageSaveInfo>();

    public void Init()
    {
        InitStageBestRecord();
        _stageSaveInfoList = new List<StageSaveInfo>();
    }

    void InitStageBestRecord()
    {
        var tick = DateTime.Now.Ticks;
        var record = (tick, false, float.MaxValue, 0);
        _stageBestRecordDict[DEFAULT_STAGE] = record;
    }

    public StageSaveInfo GetStageSaveInfo(string argStageId)
    {
        return _stageSaveInfoList.Find(info => info.stageId == argStageId);
    }
    
    public void SaveStageBestRecord(int argKey, (long, bool, float, int) argRecord)
    {
        _stageBestRecordDict[argKey] = argRecord;
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
    }

    public (long tick, bool clear, float clearTime, int hqhpRatio) GetStageBestRecord(int argStage)
    {
        _stageBestRecordDict.TryGetValue(argStage, out var result);
        return result;
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
        
        return _stageBestRecordDict[argStage].clear;
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
