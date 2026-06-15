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
    public float areaRadius;
    
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
        switch (attackAreaType)
        {
            case AttackAreaType.Area:
                resultID = PrefabID.EntityArea;
                break;
            
            case AttackAreaType.Pierce:
                resultID = PrefabID.EntityPierce;
                break;
            
            case AttackAreaType.Single:
            default:
                resultID = PrefabID.EntitySingle;
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