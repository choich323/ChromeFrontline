using System;
using UnityEngine;

public class SlotUpgradeHandler
{
    private const int MIN_RAND = 1;
    private const float MULTIPLIER = 100000f;
    
    private int _maxWeight = 0;
    
    public void Init()
    {
        var gradeInfoList = Managers.Data.GetGradeInfoList();
        foreach (var info in gradeInfoList)
        {
            _maxWeight += Mathf.RoundToInt(info.probability * MULTIPLIER);
        }
    }

    public Grade GetRandomGrade()
    {
        int seed = Guid.NewGuid().GetHashCode();
        System.Random rand = new System.Random(seed);
        int value = rand.Next(MIN_RAND, _maxWeight + 1);
        
        var gradeInfoList = Managers.Data.GetGradeInfoList();
        int cumulativeWeight = 0;
        foreach (var info in gradeInfoList)
        {
            int weight = Mathf.RoundToInt(info.probability * MULTIPLIER);
            cumulativeWeight += weight;

            if (value <= cumulativeWeight)
            {
                Debug.Log($"Random grade:{info.grade}");
                return info.grade;
            }
        }
        
        Debug.LogWarning("Random Grade Not Found");
        return Grade.Enhanced;
    }
}
