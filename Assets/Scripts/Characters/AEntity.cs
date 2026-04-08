using System;
using UnityEngine;

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

[Serializable]
public struct EntityStatus
{
    [Header("Team")]
    public Team team;

    public int curLevel;
    public long reward;
    
    [Header("Life")]
    public int curHp;
    public int curShield;
    public float armor;

    [Header("Attack")]
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
    private const float REWARD_RATIO = 0.25f;
    private const int DEFAULT_RAYCAST_COUNT = 50;
    private const int INVALID_SPAWNER_INDEX = -1;
    private const string LAYER_NAME_ENTITY = "Entity";
    
    private EntityStatus _entityStatus;
    private int _entityLayerMask;
    private int _homeSpawnerIndex;
    private PrefabID _id;
    private ulong _uid;
    private Vector2 _direction;
    private Transform _targetHqCoreTransform;
    private float _attackCooldownTimer;
    private float _retargetTimer;
    private AEntity _attackTarget;
    private RaycastHit2D[] _scanResults = new RaycastHit2D[DEFAULT_RAYCAST_COUNT];
    private Action<AEntity> _onDie;
    private Action<long> _onKill;
    
    public EntityStatus EntityStatus => _entityStatus;
    public PrefabID Id => _id;
    public ulong Uid => _uid;
    public long Reward => _entityStatus.reward;
    public bool IsDead => _entityStatus.curHp <= 0;
    public bool CanAction => _entityStatus.canAction;
    public Team Team => _entityStatus.team;
    public float CurHp => _entityStatus.curHp;
    public float CurShield => _entityStatus.curShield;
    public EntityActionType CurAction => _entityStatus.curAction;

    public virtual void Init(PrefabID argId, ulong argUid, Team argTeam, EntityInfo argEntityInfo, int argHomeSpawnerIndex, Transform argTargetHqCoreTransform, Action<AEntity> argOnDie, Action<long> argOnKill)
    {
        _entityLayerMask = LayerMask.GetMask(LAYER_NAME_ENTITY);
        
        _id = argId;
        _uid = argUid;
        _entityStatus.team = argTeam;

        if (_entityStatus.team == Team.Player)
        {
            _direction = Vector2.right;
        }
        else
        {
            _direction = Vector2.left;
        }
        SetEntityInfo(argEntityInfo);
        _homeSpawnerIndex = argHomeSpawnerIndex;
        _targetHqCoreTransform = argTargetHqCoreTransform;
        _onDie = argOnDie;
        _onKill = argOnKill;
        _attackCooldownTimer = 0f;
    }
    
    void SetEntityInfo(EntityInfo argEntityInfo)
    {
        _entityStatus.curLevel = argEntityInfo.level;
        _entityStatus.curHp = argEntityInfo.hp;
        _entityStatus.curShield = argEntityInfo.shield;
        _entityStatus.armor = argEntityInfo.armor;
        _entityStatus.attack = argEntityInfo.attack;
        _entityStatus.attackSpeed = argEntityInfo.attackSpeed;
        _entityStatus.attackRange = argEntityInfo.attackRange;
        _entityStatus.criticalChance = argEntityInfo.criticalChance;
        _entityStatus.moveSpeed = argEntityInfo.moveSpeed;
        _entityStatus.canAction = true;
        _entityStatus.reward = (int)(argEntityInfo.goldCost * REWARD_RATIO);
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
        
        int hitCount = Physics2D.RaycastNonAlloc(
            scanOrigin,
            _direction,
            _scanResults,
            _entityStatus.attackRange,
            _entityLayerMask
        );

        //Debug.DrawRay(scanOrigin, _direction * _entityStatus.attackRange, hitCount > 0 ? Color.red : Color.green);
        
        if (hitCount > 0)
        {
            AEntity target = _attackTarget;
            bool isTargetInvalid = true;
            if (target != null)
            {
                float distance = Vector2.Distance(transform.position, target.transform.position);
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
                if (_attackCooldownTimer <= 0)
                {
                    Attack(target);
                }
                return;
            }
        }

        _retargetTimer = 0f;
        _attackTarget = null;

        CheckArrival();
        
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

            float distance = scanResult.distance;
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
        _entityStatus.curAction = EntityActionType.Combat;
        
        var atkSpeed = Mathf.Max(MIN_ATTACK_SPEED, _entityStatus.attackSpeed);
        _attackCooldownTimer = 1f / atkSpeed;

        float damage = _entityStatus.attack;
        float criticalChance = _entityStatus.criticalChance;
        if (criticalChance > 0f && UnityEngine.Random.value <= criticalChance)
        {
            damage *= DEFAULT_CRITICAL_DAMAGE_RATIO;
            Debug.Log("Critical!");
        }
        argTarget.GetEffect(EffectType.Attack, damage, this);
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

        // shield 계산
        if (_entityStatus.curShield > 0)
        {
            if (_entityStatus.curShield >= reducedDamage)
            {
                _entityStatus.curShield -= (int)reducedDamage;
                reducedDamage = 0;
            }
            else
            {
                reducedDamage -= _entityStatus.curShield;
                _entityStatus.curShield = 0;
            }
        }
        
        // 체력 계산
        _entityStatus.curHp -= (int)reducedDamage;
        if (_entityStatus.curHp <= 0)
        {
            _entityStatus.curHp = 0;
            argAttacker.OnKill(_entityStatus.reward);
            Destroy();
        }
        else
        {
            OnDamaged();
        }
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
        _scanResults = new RaycastHit2D[DEFAULT_RAYCAST_COUNT];
        EntityInfo emptyEntityInfo = new EntityInfo();
        SetEntityInfo(emptyEntityInfo);
        _targetHqCoreTransform = null;
        _homeSpawnerIndex = INVALID_SPAWNER_INDEX;
        _attackCooldownTimer = 0f;
    }

    protected virtual void CheckArrival()
    {
        if (_targetHqCoreTransform == null) return;
        
        float dist = Mathf.Abs(transform.position.x - _targetHqCoreTransform.position.x);
        
        const float errorThreshold = 0.5f;
        if (dist < errorThreshold)
        {
            var field = Managers.Game.GameField;
            var hq = _entityStatus.team == Team.Player ? field.EnemyHq : field.PlayerHq;
            hq.OnHqDamaged((int)_entityStatus.attack);
            
            Destroy();
        }
    }
    
    protected virtual void Move()
    {
        _entityStatus.curAction = EntityActionType.Move;
        transform.Translate(_direction * (_entityStatus.moveSpeed * Time.deltaTime));
    }
}
