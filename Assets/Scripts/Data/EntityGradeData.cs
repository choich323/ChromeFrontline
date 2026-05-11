using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EntityGradeInfo
{
    public Grade grade;
    public Color color;
    public float attackRatio;
    public float hpRatio;
    public float armorRatio;
    public float attackSpeedRatio;
    public float moveSpeedRatio;
}

[CreateAssetMenu(fileName = "EntityGradeData", menuName = "Custom/EntityGradeData")]
public class EntityGradeData : ScriptableObject
{
    public List<EntityGradeInfo> entityGradeInfoList;

    public IEnumerable<EntityGradeInfo> GetInfoList()
    {
        return entityGradeInfoList;
    }
}
