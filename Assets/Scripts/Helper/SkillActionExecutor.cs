using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillActionExecutor
{
    private const string LAYER_ENTITY = "Entity";
    private const int TARGET_SIZE = 400;
    private const int EXECUTION_ID_MIN = 100001;

    private int _executionId = EXECUTION_ID_MIN;
    private ContactFilter2D _targetFilter;
    private readonly Collider2D[] _targetColliders = new Collider2D[TARGET_SIZE];
    private Action<int, IEnumerator> _runCoroutine;
    private Action<int> _stopCoroutine;
    private HashSet<int> _executeIds = new HashSet<int>();
    
    public void Run(Action<int, IEnumerator> argRunCoroutine, Action<int> argStopCoroutine)
    {
        _targetFilter = new ContactFilter2D();
        _targetFilter.SetLayerMask(LayerMask.GetMask(LAYER_ENTITY));
        
        _runCoroutine = argRunCoroutine;
        _stopCoroutine = argStopCoroutine;
    }

    int GetExecutionId()
    {
        return _executionId++;
    }

    void AddExecutionId(int argExecutionId)
    {
        _executeIds.Add(argExecutionId);
    }
    
    void RemoveExecutionId(int argExecutionId)
    {
        _executeIds.Remove(argExecutionId);
        _stopCoroutine?.Invoke(argExecutionId);
    }

    public void Execute(SkillInfo argInfo, int argLevel, Vector2 argPos)
    {
        List<AEntity> targets = FindTarget(argInfo, argPos);
        if (!TryPayCost(argInfo.costList, targets, argInfo.GetUpgradeValue(argLevel)))
        {
            return;
        }
        
        foreach (var action in argInfo.actionList)
        {
            int id = GetExecutionId();
            AddExecutionId(id);
            _runCoroutine?.Invoke(id, CoExecuteAction(id, argInfo, action, argLevel, targets));
        }
    }

    IEnumerator CoExecuteAction(int argExecutionId, SkillInfo argInfo, SkillActionData argAction, int argLevel, List<AEntity> argTargets)
    {
        try
        {
            yield return null;

            float value = argAction.value + argInfo.upgradeData.value * (argLevel - 1);

            if (argInfo.duration > 0)
            {
                int count = argInfo.GetCount(argLevel);
                float term = argInfo.GetDuration(argLevel) / count;
                for (int i = 0; i < count; i++)
                {
                    ApplyAction(argInfo, argAction, argLevel, argTargets, value);

                    yield return new WaitForSeconds(term);
                }
            }
            else
            {
                ApplyAction(argInfo, argAction, argLevel, argTargets, value);
            }
        }
        finally
        {
            RemoveAction(argInfo);
            
            RemoveExecutionId(argExecutionId);
        }
    }

    void ApplyAction(SkillInfo argInfo, SkillActionData argAction, int argLevel, List<AEntity> argTargets, float argValue)
    {
        switch (argAction.type)
        {
            case SkillActionType.Damage:
                ApplyDamage(argTargets, argValue);
                break;
            
            case SkillActionType.Heal:
                ApplyHeal(argTargets, argValue);
                break;
            
            case SkillActionType.Buff:
            case SkillActionType.Debuff:
                ApplyModifier(argInfo, argAction, argLevel, argTargets);
                break;
        }
    }

    bool TryPayCost(List<CostData> argCostList, List<AEntity> argTargets, float argUpgradeValue)
    {
        foreach (var cost in argCostList)
        {
            switch (cost.type)
            {
                case CostType.EntityHp:
                    foreach (var entity in argTargets)
                    {
                        if (entity.GetHpRatio() <= (cost.value - argUpgradeValue))
                        {
                            return false;
                        }
                    }
                    break;
            }
        }

        foreach (var cost in argCostList)
        {
            switch (cost.type)
            {
                case CostType.EntityHp:
                    foreach (var entity in argTargets)
                    {
                        entity.PayHpCost(cost.value - argUpgradeValue);
                    }
                    break;
            }
        }

        return true;
    }
    
    void ApplyDamage(List<AEntity> argTargets, float argValue)
    {
        foreach (var entity in argTargets)
        {
            entity.GetDamageByMaxHp(argValue);
        }
    }

    void ApplyHeal(List<AEntity> argTargets, float argValue)
    {
        foreach (var entity in argTargets)
        {
            entity.GetHealByMaxHp(argValue);
        }
    }
    
    void ApplyModifier(SkillInfo argInfo, SkillActionData argAction, int argLevel, List<AEntity> argTargets)
    {
        var modifier = GetStatModifier(argAction, (argLevel - 1) * argInfo.upgradeData.value);
        bool isBuff = argAction.type == SkillActionType.Buff;
        foreach (var entity in argTargets)
        {
            entity.ApplyStatModify(argInfo.id, isBuff, modifier);
        }
    }

    EntityStatModifier GetStatModifier(SkillActionData argAction, float argValue)
    {
        EntityStatModifier modifier = new EntityStatModifier();
        foreach (var modify in argAction.GetStatModifierList(argValue))
        {
            modifier.SetStats(modify);
        }
        return modifier;
    }

    void RemoveAction(SkillInfo argInfo)
    {
        if (!Managers.Game.IsInStage)
        {
            return;
        }
        
        if (argInfo.duration <= 0)
        {
            return;
        }
        
        var targetTeam = Team.Player;
        if (argInfo.targetType == TargetType.Enemy)
        {
            targetTeam = Team.Enemy;
        }
        var targets = Managers.Game.GameField.GetHq(targetTeam).GetEntityList();
        foreach (var entity in targets)
        {
            entity.RemoveStatModify(argInfo.id);
        }
    }

    List<AEntity> FindTarget(SkillInfo argInfo, Vector2 argPos)
    {
        var targetTeam = Team.Player;
        if (argInfo.targetType == TargetType.Enemy)
        {
            targetTeam = Team.Enemy;
        }

        List<AEntity> targets = new List<AEntity>();
        if (argInfo.rangeData != null && argInfo.rangeData.type != RangeType.None)
        {
            targets = FindTarget(argInfo.rangeData, argPos, targetTeam);
        }
        else
        {
            targets = Managers.Game.GameField.GetHq(targetTeam).GetEntityList();
        }
        
        return targets;
    }
    
    List<AEntity> FindTarget(RangeData argRangeData, Vector2 argPos, Team argTargetTeam)
    {
        List<AEntity> targetList = new List<AEntity>();
        
        if (argRangeData.type == RangeType.None)
        {
            return targetList;
        }
        
        if (argRangeData.type == RangeType.Circle)
        {
            var count = Physics2D.OverlapCircle(argPos, argRangeData.radius, _targetFilter, _targetColliders);
            for (int i = 0; i < count; i++)
            {
                var target = _targetColliders[i];
                var entity = target.GetComponent<AEntity>();

                var offset = (Vector2)entity.transform.position - argPos;
                if (offset.sqrMagnitude > argRangeData.radius * argRangeData.radius)
                {
                    continue;
                }
                
                if (entity == null || entity.Team != argTargetTeam)
                {
                    continue;
                }

                if (!targetList.Contains(entity))
                {
                    targetList.Add(entity);
                }
            }
        }
        else
        {
            var vertexList = argRangeData.vertexList;
            if (vertexList == null || vertexList.Count < 3)
            {
                return targetList;
            }
            
            var min = vertexList[0];
            var max = vertexList[0];
            foreach (var vertex in vertexList)
            {
                min = Vector2.Min(min, vertex);
                max = Vector2.Max(max, vertex);
            }
            
            var boxSize = max - min;
            var boxCenter = argPos + (max + min) * 0.5f;

            int count = Physics2D.OverlapBox(boxCenter, boxSize, 0f, _targetFilter, _targetColliders);
            for (int i = 0; i < count; i++)
            {
                var entity = _targetColliders[i].GetComponent<AEntity>();
                if (entity == null)
                {
                    continue;
                }

                var localPos = (Vector2)entity.transform.position - argPos;

                if (!IsPointInPolygon(vertexList, localPos))
                {
                    continue;
                }

                if (!targetList.Contains(entity))
                {
                    targetList.Add(entity);
                }
            }
        }

        return targetList;
    }

    bool IsPointInPolygon(List<Vector2> argPoints, Vector2 argPos)
    {
        bool isInside = false;

        for (int i = 0, j = argPoints.Count - 1; i < argPoints.Count; j = i++)
        {
            var a = argPoints[i];
            var b = argPoints[j];
            
            if((a.y > argPos.y) != (b.y > argPos.y) &&
                argPos.x < (b.x - a.x) * (argPos.y - a.y) / (b.y - a.y) + a.x)
            {
                isInside = !isInside;
            }
        }

        return isInside;
    }
    
    public void Destroy()
    {
        foreach (var id in _executeIds)
        {
            _stopCoroutine?.Invoke(id);
        }
        _executeIds.Clear();
        
        _executionId = EXECUTION_ID_MIN;
    }
}
