using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AIScheduleInfo
{
    public string id;
    
    public float tpInterval = 10f;
    public float tpIntervalDecrementAmount = 0.4f; // tp 충전 주기 감소시 얼마나 감소할지
    public float tpIntervalDecrementInterval = 60f; // 감소가 발생하는 주기
    public float minInterval = 4f;
    // x:시간, y:수급량
    public AnimationCurve tpAmountCurve;

    // x:시간, y:소비 비율 - 0.3이면 0.3지출, 0.7은 저축
    public AnimationCurve spendRateCurve;
    // 빅 웨이브 주기
    public float burstInterval = 60f;

    // x:시간, y:엔티티 레벨
    public AnimationCurve levelCurve;

    [Range(0f, 1f)]
    public float emergencyHpThreshold = 0.3f; // 발악 패턴 체력 기준
    public float tpBonusMultiplier = 10f; // 초당 수급량의 x배 만큼 지급
}

[CreateAssetMenu(fileName = "AIScheduleData", menuName = "Custom/AI/AIScheduleData")]
public class AIScheduleData : ScriptableObject
{
    public List<AIScheduleInfo> scheduleInfoList = new List<AIScheduleInfo>();

    public IEnumerable<AIScheduleInfo> GetScheduleInfoList()
    {
        return scheduleInfoList;
    }
    
    public AIScheduleInfo GetScheduleInfo(string argId)
    {
        return scheduleInfoList.Find(x => x.id == argId);
    }
    
    public AIScheduleInfo GetScheduleInfo(int argIndex)
    {
        return scheduleInfoList[argIndex];
    } 
}
