using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EntityInfo : APrefabInfo
{
    [Header("Animation")]
    public AnimatorOverrideController animatorOverrideController;

    [Header("Visual Settings")]
    public bool isOriginalSpriteFacingLeft;
    public float dieAnimDuration = 2f;
    public float attackAnimDuration = 1f;
    [Range(0, 1)] public float attackHitTiming = 0.8f;
    
    [Header("Stat")]
    public AttackAreaType attackAreaType = AttackAreaType.Single; 
    public CampType camp;
    public int tier;
    public int level;
    public int hp;
    public int shield;
    public float armor;

    public float attack;
    public float attackSpeed;
    public float attackRange;
    [Range(0, 1)]
    public float criticalChance;
    
    public float moveSpeed;

    public float productionTime;
    public int goldCost;

    public override PrefabID GetPrefabID()
    {
        if (prefab != null)
        {
            return base.GetPrefabID();
        }
        
        var isSingle = attackAreaType.Equals(AttackAreaType.Single);
        var isPioneer = camp.Equals(CampType.Pioneer);
        if (isSingle)
        {
            return isPioneer ? PrefabID.PioneerSingle : PrefabID.RevoltSingle;
        }
        else
        {
            return isPioneer ? PrefabID.PioneerArea : PrefabID.RevoltArea;
        }
    }

    public PrefabID GetEntityID()
    {
        return Managers.Data.ConvertStringToPrefabID(id);
    }
}

[CreateAssetMenu(fileName = "EntityData", menuName = "Custom/EntityData")]
public class EntityData : APrefabData
{
    public List<EntityInfo> entityInfoList = new List<EntityInfo>();

    public override IEnumerable<APrefabInfo> GetInfoList()
    {
        return entityInfoList;
    }
}