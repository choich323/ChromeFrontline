using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public struct SpawnRequest
{
    public List<EntityInfo> infoList;
}

public class AIScheduleHandler
{
    private AIScheduleInfo _aiScheduleInfo;
    private int _accumulatedTp = 0;
    private float _nextUpgradeTimer = 0f;
    private float _nextTpSupplyTimer = 0f;
    private float _nextBurstTimer = 0f;
    
    public AIScheduleInfo ScheduleInfo => _aiScheduleInfo;
    
    public void Init()
    {
        _aiScheduleInfo = Managers.Data.GetAIScheduleInfo();
        _nextUpgradeTimer = _aiScheduleInfo.upgradeInterval;
        _nextTpSupplyTimer = _aiScheduleInfo.tpInterval;
        _nextBurstTimer = _aiScheduleInfo.burstInterval;
    }

    public void Update()
    {
        HandleUpgrade();
        HandleTpSupply();
        HandleBurst();
    }

    void HandleUpgrade()
    {
        _nextUpgradeTimer -= Time.deltaTime;
        var gm = Managers.Game;
        if (_nextUpgradeTimer <= 0)
        {
            gm.GameField.EnemyHq.ForceUpgradeHq();
            _nextUpgradeTimer = _aiScheduleInfo.upgradeInterval;
        }
    }
    
    void HandleTpSupply()
    {
        _nextTpSupplyTimer -= Time.deltaTime;
        
        if (_nextTpSupplyTimer <= 0f)
        {
            var playTime = Managers.Game.PlayTime;

            float tpAmount = _aiScheduleInfo.tpAmountCurve.Evaluate(playTime);
            float spendRate = _aiScheduleInfo.spendRateCurve.Evaluate(playTime);
            
            int spendAmount = (int)(tpAmount * spendRate);
            int saveAmount = (int)tpAmount - spendAmount;

            var decrementCount = Mathf.FloorToInt(playTime / _aiScheduleInfo.tpIntervalDecrementInterval);
            var nextInterval = _aiScheduleInfo.tpInterval - decrementCount * _aiScheduleInfo.tpIntervalDecrementAmount;
            _nextTpSupplyTimer = Mathf.Max(_aiScheduleInfo.minInterval, nextInterval);

            SpendTp(spendAmount, out int _);

            _accumulatedTp += saveAmount;
        }
    }

    void HandleBurst()
    {
        _nextBurstTimer -= Time.deltaTime;

        if (_nextBurstTimer <= 0f)
        {
            int spendAmount = _accumulatedTp;
            _accumulatedTp = 0;
            _nextBurstTimer = _aiScheduleInfo.burstInterval;
            
            SpendTp(spendAmount, out int changeAmount);
            
            _accumulatedTp += changeAmount;
        }
    }

    void SpendTp(int argTp, out int outChangeAmount)
    {
        outChangeAmount = 0;
        var dm = Managers.Data;

        var spendTp = argTp;
        SpawnRequest req;
        List<EntityInfo> infoList = new List<EntityInfo>();
        while (true)
        {
            var spawnableEntityInfoList = dm.GetRevoltInfoList(spendTp);
            if (spawnableEntityInfoList.Count <= 0)
            {
                break;
            }
            int randIndex = Random.Range(0, spawnableEntityInfoList.Count);
            var selectedEntityInfo = spawnableEntityInfoList[randIndex];
            var cost = selectedEntityInfo.goldCost;
            infoList.Add(selectedEntityInfo);
            
            spendTp -= cost;
        }

        req.infoList = infoList;
        outChangeAmount += spendTp;
        
        Managers.Game.ForceSpawn(req);
    }

    public void Emergency()
    {
        var playTime = Managers.Game.PlayTime;
        var tpAmount = _aiScheduleInfo.tpAmountCurve.Evaluate(playTime);
        int emergencyTp = (int)(tpAmount * _aiScheduleInfo.emergencyTpMultiplier);
        SpendTp(emergencyTp, out int _);
    }

    public void Destroy()
    {
        _aiScheduleInfo = null;
        _accumulatedTp = 0;
        _nextTpSupplyTimer = 0f;
        _nextBurstTimer = 0f;
    }
}
