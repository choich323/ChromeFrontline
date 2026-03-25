using System;
using System.Collections.Generic;
using UnityEngine;

public class GameField : MonoBehaviour
{
    private const int DEFAULT_INDEX = 0;
    private const int DEFAULT_SPAWNER_COUNT = 3;
    
    [SerializeField] private List<Transform> _playerEntitySpawnerPosList;
    [SerializeField] private List<Transform> _enemyEntitySpawnerPosList;
    [SerializeField] private List<Transform> _playerHqCorePosList;
    [SerializeField] private List<Transform> _enemyHqCorePosList;
    [SerializeField] private Transform _hqParent;
    [SerializeField] private Transform _spawnerParent;
    [SerializeField] private Transform _playerHqPos;
    [SerializeField] private Transform _enemyHqPos;
    
    private Dictionary<Team, List<EntitySpawner>> _spawnerDict = new Dictionary<Team, List<EntitySpawner>>();
    private Dictionary<Team, Dictionary<Type, HashSet<AEntity>>> _entityDict = new Dictionary<Team, Dictionary<Type, HashSet<AEntity>>>();
    private HeadQuater _playerHq;
    private HeadQuater _enemyHq;
    
    public HeadQuater PlayerHq => _playerHq;
    public HeadQuater EnemyHq => _enemyHq;
    
    public void Init()
    {
        InitDict();
        
        CreateHqs();
        CreateSpawners();
    }

    void InitDict()
    {
        _spawnerDict[Team.Player] = new List<EntitySpawner>();
        _spawnerDict[Team.Enemy] = new List<EntitySpawner>();
        
        _entityDict[Team.Player] = new Dictionary<Type, HashSet<AEntity>>();
        _entityDict[Team.Enemy] = new Dictionary<Type, HashSet<AEntity>>();
    }

    void CreateHqs()
    {
        CreateHq(Team.Player);
        CreateHq(Team.Enemy);
    }
    
    void CreateHq(Team argTeam)
    {
        var hqObj = Managers.Pool.Instantiate(PrefabID.HeadQuater);
        if (hqObj == null)
            return;
        
        hqObj.transform.SetParent(_hqParent);
        bool isPlayer = argTeam == Team.Player;
        hqObj.transform.position = isPlayer ? _playerHqPos.position : _enemyHqPos.position;
        Managers.Data.TryGetPrefabInfo((int)PrefabID.HeadQuater, out var info);
        var hqInfo = info as HeadQuaterInfo;
        var hq = hqObj.GetComponent<HeadQuater>();
        hq.Init(hqInfo, argTeam);
        
        if (isPlayer)
            _playerHq = hq;
        else
            _enemyHq = hq;
    }
    
    void CreateSpawners()
    {
        for(int i = 0; i < DEFAULT_SPAWNER_COUNT; i++)
        {
            CreateSpawner(Team.Player, i);
            CreateSpawner(Team.Enemy, i);
        }
    }

    EntitySpawner CreateSpawner(Team argTeam, int argSpawnerIndex)
    {
        var spawnerObj = Managers.Pool.Instantiate(PrefabID.EntitySpawner);
        if (spawnerObj == null)
            return null;
        
        bool isPlayer = argTeam == Team.Player;
        var posList = isPlayer ? _playerEntitySpawnerPosList : _enemyEntitySpawnerPosList;
        var pos = posList[argSpawnerIndex].position;
        spawnerObj.transform.position = pos;
        spawnerObj.transform.SetParent(_spawnerParent);
        var spawner = spawnerObj.GetComponent<EntitySpawner>();
        var targetHqCoreTransform = isPlayer ? _enemyHqCorePosList[argSpawnerIndex] : _playerHqCorePosList[argSpawnerIndex];
        spawner.Init(argTeam, targetHqCoreTransform, DestroyEntity);
        _spawnerDict[argTeam].Add(spawner);
        
        return spawner;
    }
    
    public void AddEntity(AEntity argEntity)
    {
        var es = argEntity.EntityStatus;
        Team team = es.team;
        Type type = argEntity.GetType();
        
        var teamDict = _entityDict[team];

        if (!teamDict.ContainsKey(type))
        {
            teamDict[type] = new HashSet<AEntity>();
        }

        teamDict[type].Add(argEntity);
    }

    public void RemoveEntity(AEntity argEntity)
    {
        var es = argEntity.EntityStatus;
        Team team = es.team;
        Type type = argEntity.GetType();

        if (_entityDict.ContainsKey(team) && _entityDict[team].TryGetValue(type, out var entitySet))
        {
            entitySet.Remove(argEntity);
        }
    }

    public int GetSlotCount(Team argTeam)
    {
        if (!_spawnerDict.TryGetValue(argTeam, out var spawnerList))
        {
            return 0;
        }
        
        return spawnerList[0].SlotCount;
    }

    public int GetEntityCount(Team argTeam)
    {
        int count = 0;
        var innerDict = _entityDict[argTeam];
        foreach (var kvp in innerDict)
        {
            count += kvp.Value.Count;
        }

        return count;
    }
    
    public bool IsGameOver()
    {
        return _playerHq.Hp <= 0 || _enemyHq.Hp <= 0;
    }
    
    public void ResetField()
    {
        DestroyAll();
    }
    
    void DestroyAll()
    {
        DestroySpawners();
        DestroyHqs();
        DestroyEntities();
    }
    
    void DestroySpawners()
    {
        foreach (var kvp in _spawnerDict)
        {
            var spawnerList = kvp.Value;
            foreach (var spawner in spawnerList)
            {
                Managers.Pool.Destroy(spawner, PrefabID.EntitySpawner);
                spawner.Destroy();
            }
            spawnerList.Clear();
        }
        _spawnerDict.Clear();
    }

    void DestroyHqs()
    {
        Managers.Pool.Destroy(_playerHq, PrefabID.HeadQuater);
        Managers.Pool.Destroy(_enemyHq, PrefabID.HeadQuater);
        _playerHq.Destroy();
        _enemyHq.Destroy();
        _playerHq = null;
        _enemyHq = null;
    }
    
    void DestroyEntities()
    {
        List<AEntity> entityList = new List<AEntity>();
        foreach(var entityDict2 in _entityDict)
        {
            foreach (var entitySet in entityDict2.Value)
            {
                foreach (var entity in entitySet.Value)
                {
                    entityList.Add(entity);
                }
            }
        }
        _entityDict.Clear();

        foreach (var entity in entityList)
        {
            DestroyEntity(entity);
        }
    }

    void DestroyEntity(AEntity argEntity)
    {
        Managers.Pool.Destroy(argEntity, argEntity.Id);
        argEntity.ResetEntity();
    }
}
