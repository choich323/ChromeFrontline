using System.Collections.Generic;
using UnityEngine;

public class EntitySingle : AEntity
{
    private const float RETARGET_INTERVAL = 5f;
    
    private AEntity _attackTarget;
    private float _retargetTimer;
    
    protected override void Update()
    {
        if (IsDead)
            return;
        
        if(_attackCooldownTimer > 0)
            _attackCooldownTimer -= Time.deltaTime;

        if(_retargetTimer > 0)
            _retargetTimer -= Time.deltaTime;
        
        if (!_entityStatus.canAction)
            return;
        
        DoAction();
    }
    
    protected override bool CheckAttackSequence(int argHitCount)
    {
        if (argHitCount > 0)
        {
            AEntity target = _attackTarget;
            bool isTargetInvalid = true;
            if (target != null)
            {
                float distance = Mathf.Abs(transform.position.x - target.transform.position.x);
                bool outOfRange = distance > _entityStatus.attackRange;
                isTargetInvalid = target == null || target.IsDead || outOfRange;
            }
            
            if (isTargetInvalid || _retargetTimer <= 0f)
            {
                target = SelectTarget(argHitCount);
                _attackTarget = target;
                _retargetTimer = RETARGET_INTERVAL;
            }
            
            if (target != null)
            {
                _animator.SetBool(IS_WALK, false);
                
                if (_attackCooldownTimer <= 0)
                {
                    Attack(new List<AEntity>() { target });
                }

                return true;
            }
        }

        _attackTarget = null;
        _retargetTimer = 0f;
        
        return false;
    }

    AEntity SelectTarget(int argHitCount)
    {
        AEntity bestTarget = null;
        float minDistance = float.MaxValue;
        float minHp = float.MaxValue;
        
        for (int i = 0; i < argHitCount; i++)
        {
            var scanResult = _scanResults[i];
            var target = scanResult.collider.GetComponent<AEntity>();
            // 타겟 없음 또는 죽은 상태
            if (target == null || target.IsDead)
                continue;
            // 아군
            if (target.Team == _entityStatus.team)
                continue;

            float xDiff = target.transform.position.x - transform.position.x;
            if (xDiff * _direction.x < 0)
            {
                continue;
            }
            float distance = Mathf.Abs(xDiff);
            if (distance > _entityStatus.attackRange)
            {
                continue;
            }
            
            float hp = target.CurHp;

            bool isCloserTarget = distance < minDistance - EPSILON;
            bool isSimilarDistance = Mathf.Abs(distance - minDistance) <= EPSILON;
            bool hasLowerHp = hp < minHp;
            if (isCloserTarget || (isSimilarDistance && hasLowerHp))
            {
                minDistance = distance;
                minHp = hp;
                bestTarget = target;
            }
        }
        
        return bestTarget;
    }
    
    public override void ResetEntity()
    {
        base.ResetEntity();

        _attackTarget = null;
        _retargetTimer = 0f;
    }
}
