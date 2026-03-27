using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EntityInfo : APrefabInfo
{
    public string stringId;
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
    public int mineralCost;

    public string typeTagStringId;
    public string combatRoleTagStringId;
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