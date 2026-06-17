using System.Collections.Generic;
using UnityEngine;

public class EntityArea : AEntity
{
    private Collider2D[] _overlapResults = new Collider2D[DEFAULT_RAYCAST_COUNT];

    protected override List<AEntity> SelectTargets(int argHitCount)
    {
        // 1. 사거리 내에서 중심점이 될 가장 가까운 적을 찾습니다.
        AEntity centerTarget = GetClosestTargetInScan(argHitCount);

        // 2. 타겟을 찾지 못했다면 빈 리스트 반환
        if (centerTarget == null)
        {
            return new List<AEntity>();
        }

        // 3. 찾은 타겟의 위치를 중심으로 광역 데미지 대상들을 수집하여 반환합니다.
        return GetTargetsInRadius(centerTarget.transform.position, _entityStatus.areaRadius);
    }
    
    // BoxCast 결과(_scanResults) 중 사거리 내에 있는 가장 가까운 유효 타겟을 반환합니다.
    AEntity GetClosestTargetInScan(int argHitCount)
    {
        AEntity closestTarget = null;
        float minDistance = float.MaxValue;
        
        for (int i = 0; i < argHitCount; i++)
        {
            var target = _scanResults[i].collider.GetComponent<AEntity>();
            
            if (!IsValidTarget(target))
                continue;

            float xDiff = target.transform.position.x - transform.position.x;
            
            // 내 전방에 있는지 방향 체크
            if (xDiff * _direction.x < 0)
                continue;

            float distance = Mathf.Abs(xDiff);
            
            // 사거리 체크
            if (distance > _entityStatus.attackRange)
                continue;

            // 최소 거리 갱신
            if (distance < minDistance - EPSILON)
            {
                minDistance = distance;
                closestTarget = target;
            }
        }

        return closestTarget;
    }
    
    // 지정된 중심점과 반지름 내에 있는 모든 유효 타겟을 리스트로 반환합니다.
    List<AEntity> GetTargetsInRadius(Vector3 argCenterPosition, float argRadius)
    {
        List<AEntity> targetsInArea = new List<AEntity>();
        int overlapCount = Physics2D.OverlapCircle(argCenterPosition, argRadius, _contactFilter, _overlapResults);

        for (int i = 0; i < overlapCount; i++)
        {
            var target = _overlapResults[i].GetComponent<AEntity>();
            
            if (IsValidTarget(target))
            {
                targetsInArea.Add(target);
            }
        }

        return targetsInArea;
    }
    
    // 타겟이 존재하고, 살아있으며, 아군이 아닌지(유효한 공격 대상인지) 검사합니다.
    bool IsValidTarget(AEntity argTarget)
    {
        if (argTarget == null || argTarget.IsDead)
            return false;
            
        if (argTarget.Team == _entityStatus.team)
            return false;

        return true;
    }

    protected override void PlayHitEffect(List<AEntity> argTargets)
    {
        if (argTargets == null || argTargets.Count == 0)
        {
            return;
        }
        
        var centerPos = argTargets[0].transform.position;
        var obj = Managers.Pool.Instantiate(PrefabID.BulletEffect);
        obj.transform.SetParent(Managers.Game.GameField.BulletParent, false);
        obj.transform.position = centerPos;
        var bullet = obj.GetComponent<BulletEffect>();
        bullet.Init(_bulletAnimatorOverrideController);
    }
}