using System;
using System.Collections.Generic;
using UnityEngine;

public class HeadQuater : MonoBehaviour
{
    private const int DEFAULT_SPAWNER_COUNT = 3;
    
    [SerializeField] private Transform _spawnerParent;
    // 플레이어 진영이 좌우 어느 쪽인지는 변할 수 있다.
    [SerializeField] private List<Transform> _entitySpawnerRightPosList;
    [SerializeField] private List<Transform> _entitySpawnerLeftPosList;

    private bool _useLeftSpawnerPos;
    private int _maxHp;
    private int _hp;
    private int _shield;
    private long _gold;
    private int _mineral;
    private Team _team;
    private List<EntitySpawner> _spawnerList = new List<EntitySpawner>();
    private Func<Team, int, Transform> _getTargetSpawnerPos;
    
    public int Hp => _hp;
    public int Shield => _shield;
    public long Gold => _gold;
    public int Mineral => _mineral;
    
    public void Init(HeadQuaterInfo argInfo, Team argTeam, bool argUseLeftSpawnerPos, Func<Team, int, Transform> argGetTargetSpawnerPos)
    {
        _maxHp = argInfo.hp;
        _hp = argInfo.hp;
        _shield = argInfo.shield;
        _team = argTeam;
        _useLeftSpawnerPos = argUseLeftSpawnerPos;
        _getTargetSpawnerPos = argGetTargetSpawnerPos;
    }

    public void OnHqDamaged(int argDamage)
    {
        var gm = Managers.Game;
        if (gm.IsGameOver)
            return;

        if (_shield > 0)
        {
            if (_shield > argDamage)
            {
                _shield -= argDamage;
                argDamage = 0;
            }
            else
            {
                argDamage -= _shield;
                _shield = 0;
            }
        }
        
        _hp -= argDamage;
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

    public float GetShieldRatio()
    {
        return (float)_shield / _maxHp;
    }

    public void EarnGold(long argGold)
    {
        _gold += argGold;
    }

    public void ConsumeGold(long argGold)
    {
        _gold -= argGold;
    }

    public void EarnMineral(int argMineral)
    {
        _mineral += argMineral;
    }

    public void ConsumeMineral(int argMineral)
    {
        _mineral -= argMineral;
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

        _useLeftSpawnerPos = false;
        _maxHp = 0;
        _hp = 0;
        _shield = 0;
        _gold = 0;
        _mineral = 0;
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
        spawner.Init(_team, (Lane)argSpawnerIndex, targetPos);
        _spawnerList.Add(spawner);
        
        return spawner;
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
