using System;
using UnityEngine;
using System.Collections.Generic;

public enum SkillType
{
    None,
    Active,
    Passive,
}

public enum TargetType
{
    None,
    Self,
    Ally,
    Enemy,
    All,
}

public enum RangeType
{
    None,
    Circle,
    Polygon,
}

public enum SkillActionType
{
    None,
    Damage,
    Heal,
    Shield,
    Buff,
    Debuff,
}

public enum StatType
{
    None,
    Attack,
    Armor,
    AttackSpeed,
    MoveSpeed,
    Critical,
    ProductionSpeed,
    Gold,
    Chrome,
}

[Serializable]
public class SkillInfo
{
    public int id;
    public string nameId;
    public string descId;
    public Sprite icon;
    public SkillType type;
    
    public TargetType targetType;
    
    [Header("Active Skill info")]
    public RangeData rangeData;
    public float cooldown;
    [Range(0, 1)]
    public float hitTiming;

    public List<SkillActionData> actionList;

    public SkillUpgradeData upgradeData;
}

[Serializable]
public class RangeData
{
    public RangeType type;
    public float radius;
    public List<Vector2> vertexList;
}

[Serializable]
public class SkillActionData
{
    public SkillActionType type;

    // 0 ~ 1
    // 대상의 기본 수치를 기준으로 적용되는 비율
    [Header("Active Skill Info")]
    [Range(0, 1)]
    public float value;
    // 지속 효과에만 사용
    public float duration;
    public float interval;

    [Header("Buff/Debuff Skill Info")]
    // Buff / Debuff에만 사용
    public List<StatModifierData> statModifierList;
}


[Serializable]
public class StatModifierData
{
    public StatType type;

    // 0 ~ 1
    // 현재 값의 크기에 증분을 적용
    [Range(0, 1)]
    public float value;
}


[Serializable]
public class SkillUpgradeData
{
    // 스킬 레벨업 시 증가하는 비율
    [Range(0, 1)]
    public float value;

    // 범위 증가 비율
    [Range(0, 1)]
    public float range;

    // 쿨타임 감소 비율
    [Range(0, 1)]
    public float cooldown;

    // 지속시간 증가 비율
    [Range(0, 1)]
    public float duration;
}

[CreateAssetMenu(fileName = "SkillData", menuName = "Custom/SkillData")]
public class SkillData : ScriptableObject
{
    public List<SkillInfo> infoList = new List<SkillInfo>();

    public IEnumerable<SkillInfo> GetInfoList()
    {
        return infoList;
    }
}
