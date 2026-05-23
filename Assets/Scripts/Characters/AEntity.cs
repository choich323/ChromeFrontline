using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CampType
{
    None = 0,
    Pioneer,
    Revolt,
}

public enum AttackAreaType
{
    None = 0,
    Single,
    Area,
    Sweep,
    Pierce,
}

public enum Team
{
    None,
    Player,
    Enemy,
}

public enum EffectType
{
    None,
    Attack,
}

public enum EntityActionType
{
    None,
    Move,
    Combat,
}

public enum Grade
{
    Standard = 0,
    Enhanced,
    Elite,
    Vanguard,
    Ultimate,
}

[Serializable]
public struct EntityStatus
{
    public CampType camp;
    
    [Header("Team")]
    public Team team;

    public int curLevel;
    public long goldCost;
    public Grade grade;
    
    [Header("Life")]
    public int curHp;
    public float armor;

    [Header("Attack")]
    public float originAttack;
    public float attack;
    public float attackSpeed;
    public float attackRange;
    [Range(0, 1)] 
    public float criticalChance;
    
    [Header("Move")]
    public float moveSpeed;
    
    public EntityActionType curAction;
    
    public bool canAction;
}

public abstract class AEntity : MonoBehaviour
{
    private const ulong INVALID_UID = 0;
    private const float DEFAULT_CRITICAL_DAMAGE_RATIO = 2f;
    private const float EPSILON = 0.01f;
    private const float RETARGET_INTERVAL = 5f;
    private const float MIN_ATTACK_SPEED = 0.001f;
    private const float MIN_ARMOR = -99f;
    private const float REWARD_RATIO = 0.7f;
    private const float BASE_MOVE_SPEED = 0.625f;
    private const int DEFAULT_RAYCAST_COUNT = 50;
    private const string LAYER_NAME_ENTITY = "Entity";
    
    private const string ANIM_STATE_WALK = "isWalk";
    private const string ANIM_STATE_ATTACK = "tAttack";
    private const string ANIM_STATE_DIE = "tDie";
    private const string ANIM_WALK_SPEED_RATIO = "walkSpeedRatio";
    private const string ANIM_ATTACK_SPEED_RATIO = "attackSpeedRatio";
    
    private static readonly int IS_WALK = Animator.StringToHash(ANIM_STATE_WALK);
    private static readonly int TRIGGER_ATTACK = Animator.StringToHash(ANIM_STATE_ATTACK);
    private static readonly int TRIGGER_DIE = Animator.StringToHash(ANIM_STATE_DIE);
    private static readonly int WALK_SPEED_RATIO = Animator.StringToHash(ANIM_WALK_SPEED_RATIO);
    private static readonly int ATTACK_SPEED_RATIO = Animator.StringToHash(ANIM_ATTACK_SPEED_RATIO);
    
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform _visualChild;
    [SerializeField] private float _targetSearchYOffset = 5f;
    
    private EntityStatus _entityStatus;
    private int _entityLayerMask;
    private ContactFilter2D _contactFilter;
    private PrefabID _id;
    private ulong _uid;
    private Vector2 _direction;
    private Transform _targetHqCoreTransform;
    private float _attackCooldownTimer;
    private float _retargetTimer;
    private float _dieAnimDuration;
    private float _attackAnimDuration;
    private float _attackHitTiming;
    private AEntity _attackTarget;
    private RaycastHit2D[] _scanResults = new RaycastHit2D[DEFAULT_RAYCAST_COUNT];
    private Action<AEntity> _onDie;
    private Action<long> _onKill;
    private Coroutine _dieAnimCoroutine;
    private Coroutine _attackAnimCoroutine;
    private WaitForSeconds _attackWaitTime;
    private WaitForSeconds _attackRemainTime;
    private WaitForSeconds _dieWaitTime;
    
    public EntityStatus EntityStatus => _entityStatus;
    public PrefabID Id => _id;
    public ulong Uid => _uid;
    public long GoldCost => _entityStatus.goldCost;
    public bool IsDead => _entityStatus.curHp <= 0;
    public bool CanAction => _entityStatus.canAction;
    public Team Team => _entityStatus.team;
    public float CurHp => _entityStatus.curHp;
    public Grade Grade => _entityStatus.grade;
    public EntityActionType CurAction => _entityStatus.curAction;

    public virtual void Init(ulong argUid, Team argTeam, EntityInfo argEntityInfo, Grade argGrade, Transform argTargetHqCoreTransform, Action<AEntity> argOnDie, Action<long> argOnKill)
    {
        _animator.runtimeAnimatorController = argEntityInfo.animatorOverrideController;
        _entityLayerMask = LayerMask.GetMask(LAYER_NAME_ENTITY);
        
        _contactFilter.useLayerMask = true;
        _contactFilter.SetLayerMask(_entityLayerMask);
        _contactFilter.useTriggers = false;
        
        _id = argEntityInfo.GetEntityID();
        _uid = argUid;
        _entityStatus.team = argTeam;

        if (_entityStatus.team == Team.Player)
        {
            _direction = Vector2.right;
            _spriteRenderer.flipX = argEntityInfo.isOriginalSpriteFacingLeft;
        }
        else
        {
            _direction = Vector2.left;
            _spriteRenderer.flipX = !argEntityInfo.isOriginalSpriteFacingLeft;
        }
        SetEntityInfo(argEntityInfo, argGrade);
        _dieAnimDuration = argEntityInfo.dieAnimDuration;
        _attackAnimDuration = argEntityInfo.attackAnimDuration;
        _attackHitTiming = argEntityInfo.attackHitTiming;
        _targetHqCoreTransform = argTargetHqCoreTransform;
        _onDie = argOnDie;
        _onKill = argOnKill;
        _attackCooldownTimer = 0f;
        
        _attackWaitTime = new WaitForSeconds(_attackAnimDuration * _attackHitTiming / _entityStatus.attackSpeed);
        _attackRemainTime = new WaitForSeconds(_attackAnimDuration * (1f - _attackHitTiming) / _entityStatus.attackSpeed);
        _dieWaitTime = new WaitForSeconds(_dieAnimDuration);
        
        Physics2D.SyncTransforms();
    }
    
    void SetEntityInfo(EntityInfo argEntityInfo, Grade argGrade)
    {
        _entityStatus.camp = argEntityInfo.camp;
        var grade = argGrade;
        var gradeInfo = Managers.Data.GetGradeInfo(grade);
        _entityStatus.grade = grade;
        _entityStatus.curLevel = argEntityInfo.level;
        _entityStatus.curHp = (int)(argEntityInfo.hp * gradeInfo.hpRatio);
        _entityStatus.armor = argEntityInfo.armor * gradeInfo.armorRatio;
        _entityStatus.originAttack = argEntityInfo.attack;
        _entityStatus.attack = argEntityInfo.attack * gradeInfo.attackRatio;
        _entityStatus.attackSpeed = argEntityInfo.attackSpeed * gradeInfo.attackSpeedRatio;
        _entityStatus.attackRange = argEntityInfo.attackRange;
        _entityStatus.criticalChance = argEntityInfo.criticalChance;
        _entityStatus.moveSpeed = argEntityInfo.moveSpeed * gradeInfo.moveSpeedRatio;
        _entityStatus.canAction = true;
        _entityStatus.goldCost = (int)(argEntityInfo.goldCost * REWARD_RATIO);
    }
    
    protected virtual void Update()
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

    protected virtual void DoAction()
    {
        var scanOrigin = (Vector2)transform.position;
        Vector2 boxSize = new Vector2(0.1f, _targetSearchYOffset);
        int hitCount = Physics2D.BoxCast(
            scanOrigin,
            boxSize,
            0f,
            _direction,
            _contactFilter,
            _scanResults,
            _entityStatus.attackRange
        );
        
        if (hitCount > 0)
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
                target = SelectTarget(hitCount);
                _attackTarget = target;
                _retargetTimer = RETARGET_INTERVAL;
            }
            
            if (target != null)
            {
                _animator.SetBool(IS_WALK, false);
                
                if (_attackCooldownTimer <= 0)
                {
                    Attack(target);
                }
                return;
            }
        }

        _retargetTimer = 0f;
        _attackTarget = null;

        if (CheckArrival())
        {
            return;
        }
        
        Move();
    }

    protected virtual AEntity SelectTarget(int argHitCount)
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

    protected virtual void Attack(AEntity argTarget)
    {
        if (argTarget == null || argTarget.IsDead)
            return;

        if (_attackAnimCoroutine != null)
        {
            StopCoroutine(_attackAnimCoroutine);
        }
        
        _entityStatus.curAction = EntityActionType.Combat;
        _animator.SetFloat(ATTACK_SPEED_RATIO, _entityStatus.attackSpeed);
        _animator.SetTrigger(TRIGGER_ATTACK);
        
        var atkSpeed = Mathf.Max(MIN_ATTACK_SPEED, _entityStatus.attackSpeed);
        _attackCooldownTimer = 1f / atkSpeed;

        _attackAnimCoroutine = StartCoroutine(CoAttack(argTarget));
    }

    protected virtual IEnumerator CoAttack(AEntity argTarget)
    {
        _entityStatus.canAction = false;
        
        yield return _attackWaitTime;
        
        float damage = _entityStatus.attack;
        float criticalChance = _entityStatus.criticalChance;
        if (criticalChance > 0f && UnityEngine.Random.value <= criticalChance)
        {
            damage *= DEFAULT_CRITICAL_DAMAGE_RATIO;
        }
        argTarget.GetEffect(EffectType.Attack, damage, this);

        yield return _attackRemainTime;
        
        _entityStatus.canAction = true;
        _attackAnimCoroutine = null;
    }
    
    protected virtual void GetEffect(EffectType argEffectType, float argAmount, AEntity argSubject)
    {
        switch (argEffectType)
        {
            case EffectType.Attack:
                GetDamage(argAmount, argSubject);
                break;
            
            case EffectType.None:
            default:
                break;
        }
    }
    
    public virtual void GetDamage(float argDamage, AEntity argAttacker)
    {
        if (IsDead)
            return;

        float armor = Mathf.Max(_entityStatus.armor, MIN_ARMOR);
        float reducedDamage = argDamage * (100f / (100f + armor));
        
        // 체력 계산
        _entityStatus.curHp -= (int)reducedDamage;
        if (_entityStatus.curHp <= 0)
        {
            _entityStatus.curHp = 0;
            _entityStatus.canAction = false;
            _entityStatus.curAction = EntityActionType.None;
            
            if(argAttacker != null)
                argAttacker.OnKill(_entityStatus.goldCost);
            
            _attackTarget = null;

            _dieAnimCoroutine = StartCoroutine(CoDie());
        }
        else
        {
            OnDamaged();
        }
    }

    IEnumerator CoDie()
    {
        if (_attackAnimCoroutine != null)
        {
            StopCoroutine(_attackAnimCoroutine);
            _attackAnimCoroutine = null;
        }
        
        _animator.SetBool(IS_WALK, false);
        _animator.ResetTrigger(ANIM_STATE_ATTACK);
        _animator.SetTrigger(TRIGGER_DIE);

        yield return _dieWaitTime;

        Destroy();
    }
    
    protected virtual void OnKill(long argReward)
    {
        _onKill?.Invoke(argReward);
    }
    
    protected virtual void OnDamaged()
    {
        
    }

    public virtual void Destroy()
    {
        _onDie?.Invoke(this);
    }

    public virtual void ResetEntity()
    {
        _entityStatus = new EntityStatus();
        _id = PrefabID.None;
        _uid = INVALID_UID;
        _direction = Vector2.zero;
        _attackCooldownTimer = 0f;
        _retargetTimer = 0f;
        _attackTarget = null;
        _dieAnimDuration = 2f;
        _attackAnimDuration = 1f;
        _attackHitTiming = 0.8f;
        _scanResults = new RaycastHit2D[DEFAULT_RAYCAST_COUNT];
        EntityInfo emptyEntityInfo = new EntityInfo();
        SetEntityInfo(emptyEntityInfo, Grade.Standard);
        _targetHqCoreTransform = null;
        _attackCooldownTimer = 0f;
        
        if (_dieAnimCoroutine != null)
        {
            StopCoroutine(_dieAnimCoroutine);
        }
        _dieAnimCoroutine = null;

        if (_attackAnimCoroutine != null)
        {
            StopCoroutine(_attackAnimCoroutine);
        }
        _attackAnimCoroutine = null;

        _attackWaitTime = null;
        _attackRemainTime = null;
        _dieWaitTime = null;
    }

    protected virtual bool CheckArrival()
    {
        if (_targetHqCoreTransform == null)
            return false;
        
        float dist = Mathf.Abs(transform.position.x - _targetHqCoreTransform.position.x);
        
        const float errorThreshold = 0.5f;
        if (dist < errorThreshold)
        {
            Managers.Game.OnEntityArrivedAtDestination(_entityStatus.team, (int)_entityStatus.originAttack, _entityStatus.goldCost);
            
            Destroy();
            return true;
        }

        return false;
    }
    
    protected virtual void Move()
    {
        if (IsDead || !_entityStatus.canAction)
            return;

        _entityStatus.curAction = EntityActionType.Move;
        float animSpeedRatio = _entityStatus.moveSpeed * BASE_MOVE_SPEED;
        _animator.SetFloat(WALK_SPEED_RATIO, animSpeedRatio);
        _animator.SetBool(IS_WALK, true);
        transform.Translate(_direction * (_entityStatus.moveSpeed * Time.deltaTime));
    }
    
    void OnDrawGizmosSelected()
    {
        // 1. 기본 BoxCast 스캔 영역 시각화 (초록색)
        Gizmos.color = Color.green;
    
        Vector2 scanOrigin = (Vector2)transform.position;
        Vector2 boxSize = new Vector2(0.1f, 5f); // DoAction에서 사용하는 크기와 동일하게 맞춤
    
        // 에디터에서 플레이 모드가 아닐 때를 대비한 기본값 처리
        Vector2 dir = _direction != Vector2.zero ? _direction : Vector2.right;
        float range = _entityStatus.attackRange > 0 ? _entityStatus.attackRange : 2f;

        // 시작 지점 박스와 끝 지점 박스
        Vector2 endPosition = scanOrigin + (dir * range);
        Gizmos.DrawWireCube(scanOrigin, boxSize);
        Gizmos.DrawWireCube(endPosition, boxSize);

        // 박스가 이동하는 궤적 (위/아래 선)
        Vector2 topOffset = new Vector2(0, boxSize.y / 2f);
        Vector2 bottomOffset = new Vector2(0, -boxSize.y / 2f);
        Gizmos.DrawLine(scanOrigin + topOffset, endPosition + topOffset);
        Gizmos.DrawLine(scanOrigin + bottomOffset, endPosition + bottomOffset);

        // 2. 현재 타겟팅된 적이 있을 경우 시각화 (빨간색)
        if (_attackTarget != null && !_attackTarget.IsDead)
        {
            Gizmos.color = Color.red;
        
            // 내 중심점에서 타겟 중심점까지의 실제 대각선 선
            Gizmos.DrawLine(transform.position, _attackTarget.transform.position);

            // 중심점 기준 X축 거리를 확인하기 위한 포인트 (노란색)
            Gizmos.color = Color.yellow;
            Vector2 xDistancePoint = new Vector2(_attackTarget.transform.position.x, transform.position.y);
            Gizmos.DrawLine(transform.position, xDistancePoint);
            Gizmos.DrawWireSphere(xDistancePoint, 0.2f);
        }
    }
}
