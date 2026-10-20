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
    HQ,
}

public enum CostType
{
    None,
    EntityHp,
    Gold,
}

public enum SkillActionType
{
    None,
    Damage,
    Heal,
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
    Shield,
    ProductionSpeed,
    Gold,
    Chrome,
}

[Serializable]
public struct PassiveSkillContainer
{
    public float goldBonus;
    public float ChromeBonus;
    public float productionTimeBonus;
}

[Serializable]
public class SkillInfo
{
    public int id;
    public string nameId;
    public string descId;
    public string descDetailId;
    public Sprite icon;
    public SkillType type;
    
    public TargetType targetType;
    
    [Header("Active Skill info")]
    public RangeData rangeData;
    public List<CostData> costList;
    public float cooldown;
    // 지속 데미지/효과에 사용
    public float duration;
    public int count;
    [Range(0, 1)]
    public float hitTiming;

    public List<SkillActionData> actionList;

    public SkillUpgradeData upgradeData;

    public object[] GetUIParams(int argLevel)
    {
        List<object> paramList = new List<object>();
        foreach (var action in actionList)
        {
            paramList.AddRange(action.GetUIParams(argLevel, duration, count, upgradeData));
        }
        
        return paramList.ToArray();
    }

    public float GetCooldown(int argLevel)
    {
        return cooldown - upgradeData.cooldown * (argLevel - 1);
    }

    public float GetDuration(int argLevel)
    {
        return duration + upgradeData.duration * (argLevel - 1);
    }

    public int GetCount(int argLevel)
    {
        return count + upgradeData.count * (argLevel - 1);
    }
    
    public float GetUpgradeValue(int argLevel)
    {
        return upgradeData.value * (argLevel - 1);    
    }
}

[Serializable]
public class RangeData
{
    public RangeType type;
    public float radius;
    public List<Vector2> vertexList;
}

[Serializable]
public class CostData
{
    public CostType type;   
    public float value;
}

[Serializable]
public class SkillActionData
{
    // 소수 첫째자리까지 나타내기 위함.
    private const float THOUSAND = 1000f;
    private const float TEN = 10f;
    
    public SkillActionType type;

    // 대상의 기본 수치를 기준으로 적용되는 비율
    [Header("Active Skill Info")]
    public float value;

    [Header("Buff/Debuff Skill Info")]
    // Buff / Debuff에만 사용
    public List<StatModifierData> statModifierList;

    public object[] GetUIParams(int argLevel, float argDuration, int argCount, SkillUpgradeData argUpgradeData)
    {
        List<object> paramList = new List<object>();

        switch (type)
        {
            case SkillActionType.Damage:
            case SkillActionType.Heal:
                float calcValue = Mathf.Round((value + ((argLevel - 1) * argUpgradeData.value)) * THOUSAND) / TEN;
                paramList.Add(calcValue);

                if (argDuration > 0)
                {
                    float calcDuration = argDuration + ((argLevel - 1) * argUpgradeData.duration);
                    paramList.Add(calcDuration);
                }

                if (argCount > 1)
                {
                    int calcCount = argCount + (argLevel - 1) * argUpgradeData.count;
                    paramList.Add(calcCount);
                }
                
                break;
            
            case SkillActionType.Buff:
            case SkillActionType.Debuff:
                
                if (argDuration > 0)
                {
                    float calcDuration = argDuration + ((argLevel - 1)  * argUpgradeData.duration);
                    paramList.Add(calcDuration);
                }
                
                foreach (var mod in statModifierList)
                {
                    var modifier = Mathf.Round((mod.value + (argUpgradeData.value * (argLevel - 1))) * THOUSAND) / TEN;
                    if (modifier < 0)
                    {
                        modifier *= -1;
                    }
                    
                    var calcModValue = modifier;
                    paramList.Add(calcModValue);
                }
                
                break;
        }
        
        return paramList.ToArray();
    }

    public List<StatModifierData> GetStatModifierList(float argValue)
    {
        List<StatModifierData> modifierList = new List<StatModifierData>();
        modifierList.AddRange(statModifierList);
        foreach (var modify in modifierList)
        {
            modify.value += argValue;
        }
        return modifierList;
    }
}

[Serializable]
public class StatModifierData
{
    public StatType type;

    [Range(-1, 1)]
    public float value;
}

[Serializable]
public class SkillUpgradeData
{
    // 스킬 레벨업 시 증가하는 비율
    [Range(0, 1)]
    public float value;
    
    public float range;
    
    public float cooldown;
    
    public float duration;

    public int count;
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
