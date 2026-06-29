using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class HeadQuarter : MonoBehaviour
{
    private const int DEFAULT_TIER = 1;
    private const float SECOND = 1f;
    
    [SerializeField] private Transform _spawnerParent;
    // 플레이어 진영이 좌우 어느 쪽인지는 변할 수 있다.
    [SerializeField] private Transform _entitySpawnerRightPos;
    [SerializeField] private Transform _entitySpawnerLeftPos;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private List<Transform> _explosionEffectPosList;
    [SerializeField] private AnimatorOverrideController _animatorOverrideController;

    private bool _useLeftSpawnerPos;
    private int _tier = DEFAULT_TIER;
    private int _maxHp;
    private int _hp;
    private long _gold;
    private Team _team;
    private EntitySpawner _spawner;
    private Func<Team, Transform> _getTargetSpawnerPos;
    private Coroutine _coroutineGoldPerSecond;
    private List<PrefabID> _usableEntityIDList = new List<PrefabID>();
    private HeadQuarterUpgradeInfo _hqUpgradeInfo;
    private event Action<long> _onGoldChanged;
    private event Action<int, int> _onHealthChanged;
    private event Action<int> _onEntityCountChanged;
    private event Action<int> _onTierChanged;

    private DataManager dm => Managers.Data;
    
    public int Tier => _tier;
    public int Hp => _hp;
    public int MaxSlotCount => _hqUpgradeInfo.maxSlotCount;
    public long Gold => _gold;
    
    public event Action<long> OnGoldChanged
    {
        add => _onGoldChanged += value;
        remove => _onGoldChanged -= value;
    }
    
    public event Action<int, int> OnHealthChanged
    {
        add => _onHealthChanged += value;
        remove => _onHealthChanged -= value;
    }
    
    public event Action<int> OnEntityCountChanged
    {
        add => _onEntityCountChanged += value;
        remove => _onEntityCountChanged -= value;
    }
    
    public event Action<int> OnTierChanged
    {
        add => _onTierChanged += value;
        remove => _onTierChanged -= value;
    }
    
    public void Init(Team argTeam, bool argUseLeftSpawnerPos, Func<Team, Transform> argGetTargetSpawnerPos)
    {
        SetTier(DEFAULT_TIER);
        _usableEntityIDList.Clear();
        AddUsableEntityIdList();
        _hqUpgradeInfo = dm.GetHeadQuarterUpgradeInfo(_tier);
        _maxHp = _hqUpgradeInfo.maxHp;
        SetHp(_hqUpgradeInfo.maxHp);
        _team = argTeam;
        EarnGold(dm.StartGold);
        
// #if UNITY_EDITOR
// 출시 전까진 테스트 편의를 위해 유지
        EarnGold(1000000);
// #endif
        
        _spriteRenderer.sprite = _hqUpgradeInfo.sprite;
        _useLeftSpawnerPos = argUseLeftSpawnerPos;
        _getTargetSpawnerPos = argGetTargetSpawnerPos;

        if (_team == Team.Player)
        {
            if (_coroutineGoldPerSecond != null)
            {
                StopCoroutine(_coroutineGoldPerSecond);
            }
            _coroutineGoldPerSecond = StartCoroutine(CoEarnGoldPerSecond());
        }
    }

    public void Run()
    {
        CreateSpawner();
    }
    
    void AddUsableEntityIdList()
    {
        var idList = dm.GetPrefabIdList(_tier);
        foreach (var id in idList)
        {
            _usableEntityIDList.Add(id);
        }
    }
    
    public IEnumerable<PrefabID> GetUsableEntityIDList()
    {
        return _usableEntityIDList;
    }

    public bool UpgradeHq()
    {
        var newInfo = dm.GetHeadQuarterUpgradeInfo(_tier + 1);

        if (_gold < newInfo.upgradeCost)
        {
            var ph = Managers.UI.PopupHandler;
            var popup = ph.OpenPopup<UINotice>(PrefabID.UINotice);
            string msg = Managers.String.GetString(StringID.NotEnoughGold);
            popup.SetData(msg, ph.ClosePopup);
            return false;
        }
        
        ConsumeGold(newInfo.upgradeCost);
        
        _hqUpgradeInfo = newInfo;
        SetTier(newInfo.tier);
        var hpRatio = newInfo.maxHp / (float)_maxHp;
        _maxHp = newInfo.maxHp;
        SetHp((int)(_hp * hpRatio));
        _spriteRenderer.sprite = _hqUpgradeInfo.sprite;
        
        AddUsableEntityIdList();
        return true;
    }

    public void ForceUpgradeHq()
    {
        var newInfo = dm.GetHeadQuarterUpgradeInfo(_tier + 1);
        if (newInfo == null)
            return;
        
        SetTier(newInfo.tier);
        var hpRatio = newInfo.maxHp / (float)_maxHp;
        _maxHp = newInfo.maxHp;
        SetHp((int)(_hp * hpRatio));
        _spriteRenderer.sprite = newInfo.sprite;
        _hqUpgradeInfo = newInfo;
    }
    
    [ContextMenu("TestEarnGold")]
    void TestEarnGold()
    {
        EarnGold(10000000);
    }
    
    IEnumerator CoEarnGoldPerSecond()
    {
        var wait = new WaitForSeconds(SECOND);
        while (true)
        {
            yield return wait;
            
            EarnGold(_hqUpgradeInfo.goldPerSecond);
        }
    }

    public void OnHqDamaged(int argDamage)
    {
        var gm = Managers.Game;
        if (gm.IsGameOver)
            return;
        
        SetHp(_hp - argDamage);

        var exObj = Managers.Pool.Instantiate(PrefabID.ExplosionEffect);
        exObj.transform.SetParent(gm.GameField.ExplosionParent);
        var exPos = _explosionEffectPosList[Random.Range(0, _explosionEffectPosList.Count)];
        exObj.transform.position = exPos.position;
        var effect = exObj.GetComponent<ExplosionEffect>();
        effect.Init(_animatorOverrideController);
        
        _onHealthChanged?.Invoke(_hp, _maxHp);
        if (_team == Team.Enemy)
        {
            gm.OnEnemyHqHpChanged(_hp, _maxHp);
        }
        if (_hp <= 0)
        {
            bool isPlayerWin = _team != Team.Player;
            gm.EndStage(isPlayerWin);
        }
    }

    void SetTier(int argTier)
    {
        _tier = argTier;
        _onTierChanged?.Invoke(_tier);
    }
    
    void SetHp(int argHp)
    {
        _hp = argHp;
        if (_hp < 0)
        {
            _hp = 0;
        }
        _onHealthChanged?.Invoke(_hp, _maxHp);
    }

    public float GetHqHpRatio()
    {
        return (float)_hp / _maxHp;
    }

    public long GetGold()
    {
        return _gold;
    }
    
    public void EarnGold(long argGold)
    {
        _gold += argGold;
        _onGoldChanged?.Invoke(_gold);
    }

    public void ConsumeGold(long argGold)
    {
        _gold -= argGold;
        _onGoldChanged?.Invoke(_gold);
    }

    float GetProductionBonus()
    {
        return _hqUpgradeInfo.productionTimeBonus;
    }
    
    public Transform GetTargetSpawnerTransform()
    {
        var pos = _useLeftSpawnerPos ? _entitySpawnerLeftPos : _entitySpawnerRightPos;
        return pos;
    }

    public EntitySpawner GetSpawner()
    {
        return _spawner;
    }
    
    void Clear()
    {
        DestroySpawners();
        _usableEntityIDList.Clear();
        _useLeftSpawnerPos = false;
        _tier = DEFAULT_TIER;
        _maxHp = 1;
        _hp = 0;
        _gold = 0;
        _team = Team.None;

        if (_coroutineGoldPerSecond != null)
        {
            StopCoroutine(_coroutineGoldPerSecond);
            _coroutineGoldPerSecond = null;
        }
    }
    
    public void Destroy()
    {
        Clear();
    }

    public EntitySpawner CreateSpawner()
    {
        var spawnerObj = Managers.Pool.Instantiate(PrefabID.EntitySpawner);
        if (spawnerObj == null)
            return null;
        
        var pos = _useLeftSpawnerPos ? _entitySpawnerLeftPos.position : _entitySpawnerRightPos.position;
        spawnerObj.transform.position = pos;
        spawnerObj.transform.SetParent(_spawnerParent);
        _spawner = spawnerObj.GetComponent<EntitySpawner>();
        var targetPos = _getTargetSpawnerPos?.Invoke(_team);
        _spawner.Init(_team, targetPos, EarnGold, ConsumeGold, GetGold, GetProductionBonus);
        _spawner.SetOnEntityCountChanged(EntityCountChanged);
        
        return _spawner;
    }

    public bool AddSlot()
    {
        var curSlotCount = _spawner.SlotCount;
        if (curSlotCount >= MaxSlotCount)
        {
            return false;
        }
        
        // gold check
        var cost = dm.GetAddSlotCost(curSlotCount);
        if (cost > _gold)
        {
            var ph = Managers.UI.PopupHandler;
            var popup = ph.OpenPopup<UINotice>(PrefabID.UINotice);
            string msg = Managers.String.GetString(StringID.NotEnoughGold);
            popup.SetData(msg, ph.ClosePopup);
            return false;
        }

        ConsumeGold(cost);
        
        _spawner.AddSlot();
        
        return true;
    }

    public bool SetSlotGrade(int argIndex, Grade argGrade)
    {
        return _spawner.SetSlotGrade(argIndex, argGrade);
    }
    
    public void ForceSpawn(SpawnRequest spawnRequest)
    {
        _spawner.ForceSpawn(spawnRequest.infoList, _hqUpgradeInfo.enemyMinGrade);
    }
    
    void DestroySpawners()
    {
        Managers.Pool.Destroy(_spawner, PrefabID.EntitySpawner);
        _spawner.Destroy();
    }
    
    public int GetSlotCount()
    {
        return _spawner.SlotCount;
    }

    public int GetEntitiesCount()
    {
        return _spawner.GetEntitiesCount();
    }

    void EntityCountChanged(int argCount)
    {
        _onEntityCountChanged?.Invoke(argCount);
    }
}
