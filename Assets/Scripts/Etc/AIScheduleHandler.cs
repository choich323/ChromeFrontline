using UnityEngine;

public class AIScheduleHandler
{
    private const float HOUR_TO_SECOND = 3600f;
    
    private AIScheduleInfo _aiScheduleInfo;
    private int _accumulatedTp = 0;
    private float _nextTpSupplyTimer = 0f;
    private float _nextBurstTimer = 0f;
    
    public void Init()
    {
        _aiScheduleInfo = Managers.Data.GetAIScheduleInfo();
        _nextTpSupplyTimer = _aiScheduleInfo.tpInterval;
        _nextBurstTimer = _aiScheduleInfo.burstInterval;
    }

    public void Update()
    {
        HandleTpSupply();
        HandleBurst();
    }

    void HandleTpSupply()
    {
        var playTime = Managers.Game.PlayTime;
        
        _nextTpSupplyTimer -= Time.deltaTime;
        if (_nextTpSupplyTimer <= 0f)
        {
            float tpAmount = _aiScheduleInfo.tpAmountCurve.Evaluate(playTime);
            float spendRate = _aiScheduleInfo.spendRateCurve.Evaluate(playTime);
            
            int spendAmount = (int)(tpAmount * spendRate);
            int saveAmount = (int)tpAmount - spendAmount;

            var second = playTime / HOUR_TO_SECOND;
            var nextInterval = _aiScheduleInfo.tpInterval - ((second / _aiScheduleInfo.tpIntervalDecrementInterval) * _aiScheduleInfo.tpIntervalDecrementAmount);
            _nextTpSupplyTimer = Mathf.Min(_aiScheduleInfo.minInterval, nextInterval);

            SpendTp(spendAmount, out int changeAmount);

            _accumulatedTp += saveAmount + changeAmount;
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
            
            SpendTp(spendAmount, out int _);
        }
    }

    void SpendTp(int argTp, out int outChangeAmount)
    {
        outChangeAmount = 0;
        
        
    }
}
