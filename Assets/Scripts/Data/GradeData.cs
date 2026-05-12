using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GradeInfo
{
    public Grade grade;
    public float probability;
    public int gold;
    public Color color;
    public float attackRatio;
    public float hpRatio;
    public float armorRatio;
    public float attackSpeedRatio;
    public float moveSpeedRatio;
}

[CreateAssetMenu(fileName = "GradeData", menuName = "Custom/GradeData")]
public class GradeData : ScriptableObject
{
    public List<GradeInfo> gradeInfoList;

    public IEnumerable<GradeInfo> GetInfoList()
    {
        return gradeInfoList;
    }
}
