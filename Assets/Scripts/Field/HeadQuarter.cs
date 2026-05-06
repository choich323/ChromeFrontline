using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadQuarter : MonoBehaviour
{
    private const int DEFAULT_SPAWNER_COUNT = 3;
    private const int DEFAULT_LEVEL = 1;
    private const float SECOND = 1f;
    
    [SerializeField] private Transform _spawnerParent;
    // 플레이어 진영이 좌우 어느 쪽인지는 변할 수 있다.
    [SerializeField] private List<Transform> _entitySpawnerRightPosList;
    [SerializeField] private List<Transform> _entitySpawnerLeftPosList;

    private bool _useLeftSpawnerPos;
    private int _level = DEFAULT_LEVEL;
    private int _maxHp;
    private int _hp;
    private long _gold;
    private Team _team;
    private List<EntitySpawner> _spawnerList = new List<EntitySpawner>();
    private Func<Team, int, Transform> _getTargetSpawnerPos;
    private Coroutine _coroutineGoldPerSecond;
    private List<PrefabID> _usableEntityIDList = new List<PrefabID>();
    private HeadQuarterUpgradeInfo _hqUpgradeInfo;

    private DataManager dm => Managers.Data;
    
    public int Level => _level;
    public int Hp => _hp;
    public long Gold => _gold;
    
    public void Init(Team argTeam, bool argUseLeftSpawnerPos, Func<Team, int, Transform> argGetTargetSpawnerPos)
    {
        _level = DEFAULT_LEVEL;
        _usableEntityIDList.Clear();
        SetUsableEntityIdList();
        _hqUpgradeInfo = dm.GetHeadQuarterUpgradeInfo(_level);
        _maxHp = _hqUpgradeInfo.maxHp;
        _hp = _hqUpgradeInfo.maxHp;
        _team = argTeam;
        _gold = dm.StartGold;
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

    void SetUsableEntityIdList()
    {
        // 새롭게 추가만 해도 좋지만, id 리스트 누락이 없도록 매번 재편성
        _usableEntityIDList.Clear();
        for (int i = _level; i > 0; i--)
        {
            var idList = dm.GetPrefabIdList(i);
            foreach (var id in idList)
            {
                _usableEntityIDList.Add(id);
            }
        }
    }
    
    public IEnumerable<PrefabID> GetUsableEntityIDList()
    {
        return _usableEntityIDList;
    }

    public void UpgradeHq()
    {
        var newInfo = dm.GetHeadQuarterUpgradeInfo(_level);
        _level = newInfo.level;
        var hpRatio = _maxHp / (float)_hp;
        _maxHp = newInfo.maxHp;
        _hp = (int)(_hp * hpRatio);
        _hqUpgradeInfo = newInfo;
        SetUsableEntityIdList();
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
        
        _hp -= argDamage;
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
    }

    public void ConsumeGold(long argGold)
    {
        _gold -= argGold;
    }

    float GetProductionBonus()
    {
        return _hqUpgradeInfo.productionTimeBonus;
    }
    
    public Transform GetTargetSpawnerTransform(int argSpawnerIndex)
    {
        var posList = _useLeftSpawnerPos ? _entitySpawnerLeftPosList : _entitySpawnerRightPosList;
        if (argSpawnerIndex < 0 || argSpawnerIndex >= posList.Count)
        {
            return null;
        }
        return posList[argSpawnerIndex];
    }

    public EntitySpawner GetSpawner(int argSpawnerIndex)
    {
        if (argSpawnerIndex < 0 || argSpawnerIndex >= _spawnerList.Count)
        {
            return null;
        }
        return _spawnerList[argSpawnerIndex];
    }
    
    void Clear()
    {
        DestroySpawners();
        _usableEntityIDList.Clear();
        _useLeftSpawnerPos = false;
        _level = DEFAULT_LEVEL;
        _maxHp = 0;
        _hp = 0;
        _gold = 0;
        _team = Team.None;
    }
    
    public void Destroy()
    {
        Clear();
    }
    
    public void CreateSpawners()
    {
        for(int i = 0; i < DEFAULT_SPAWNER_COUNT; i++)
        {
            CreateSpawner(i);
        }
    }

    EntitySpawner CreateSpawner(int argSpawnerIndex)
    {
        var spawnerObj = Managers.Pool.Instantiate(PrefabID.EntitySpawner);
        if (spawnerObj == null)
            return null;
        
        var pos = _useLeftSpawnerPos ? _entitySpawnerLeftPosList[argSpawnerIndex].position : _entitySpawnerRightPosList[argSpawnerIndex].position;
        spawnerObj.transform.position = pos;
        spawnerObj.transform.SetParent(_spawnerParent);
        var spawner = spawnerObj.GetComponent<EntitySpawner>();
        var targetPos = _getTargetSpawnerPos?.Invoke(_team, argSpawnerIndex);
        spawner.Init(_team, (Lane)argSpawnerIndex, targetPos, EarnGold, ConsumeGold, GetGold, GetProductionBonus);
        _spawnerList.Add(spawner);
        
        return spawner;
    }

    public void ForceSpawn(List<SpawnRequest> spawnRequestList)
    {
        foreach (var req in spawnRequestList)
        {
            var spawner = _spawnerList[req.laneIndex];
            spawner.ForceSpawn(req.infoList);
        }
    }
    
    void DestroySpawners()
    {
        foreach (var spawner in _spawnerList)
        {
            Managers.Pool.Destroy(spawner, PrefabID.EntitySpawner);
            spawner.Destroy();
        }
        _spawnerList.Clear();
    }
    
    public int GetSlotCount()
    {
        if (_spawnerList.Count == 0)
            return 0;
        
        return _spawnerList[0].SlotCount;
    }

    public int GetEntitiesCount()
    {
        int count = 0;
        foreach (var spawner in _spawnerList)
        {
            count += spawner.GetEntitiesCount();
        }

        return count;
    }
}
