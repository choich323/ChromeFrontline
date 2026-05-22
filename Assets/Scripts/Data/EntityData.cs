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
    public Sprite iconImage;
    
    [Header("Stat")]
    public AttackAreaType attackAreaType = AttackAreaType.Single; 
    public CampType camp;
    public int tier;
    public Grade grade = Grade.Standard;
    public int level;
    public int hp;
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
        
        PrefabID resultID;
        var isPioneer = camp.Equals(CampType.Pioneer);
        switch (attackAreaType)
        {
            case AttackAreaType.Area:
                resultID = isPioneer ? PrefabID.PioneerArea : PrefabID.RevoltArea;
                break;
            
            case AttackAreaType.Sweep:
                resultID = isPioneer ? PrefabID.PioneerSweep : PrefabID.RevoltSweep;
                break;
            
            case AttackAreaType.Pierce:
                resultID = isPioneer ? PrefabID.PioneerPierce : PrefabID.RevoltPierce;
                break;
            
            case AttackAreaType.Single:
            default:
                resultID = isPioneer ? PrefabID.PioneerSingle : PrefabID.RevoltSingle;
                break;
        }

        return resultID;
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