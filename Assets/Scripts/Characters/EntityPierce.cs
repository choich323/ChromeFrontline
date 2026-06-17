using System.Collections.Generic;
using UnityEngine;

public class EntityPierce : AEntity
{
    // 부모(AEntity)의 DoAction에서 이미 전방 X축 라인으로 BoxCast를 쏘고 있으므로,
    // SelectTargets만 오버라이드하여 사거리 내의 모든 적을 반환하면 됩니다.
    protected override List<AEntity> SelectTargets(int argHitCount)
    {
        List<AEntity> validTargets = new List<AEntity>();
        
        for (int i = 0; i < argHitCount; i++)
        {
            var scanResult = _scanResults[i];
            var target = scanResult.collider.GetComponent<AEntity>();
            
            // 1. 타겟 유효성 및 사망 체크
            if (target == null || target.IsDead)
                continue;
                
            // 2. 아군 제외 (팀 체크)
            if (target.Team == _entityStatus.team)
                continue;

            // 3. 내 전방에 있는지 방향 체크
            float xDiff = target.transform.position.x - transform.position.x;
            if (xDiff * _direction.x < 0)
            {
                continue;
            }

            // 4. 사거리 내에 있는지 최종 거리 체크
            float distance = Mathf.Abs(xDiff);
            if (distance > _entityStatus.attackRange)
            {
                continue;
            }
            
            validTargets.Add(target);
        }
        
        return validTargets;
    }
}
