using System;
using System.Collections.Generic;
using UnityEngine;

public class GameField : MonoBehaviour
{
    [SerializeField] private Transform _hqParent;
    [SerializeField] private Transform _playerHqPos;
    [SerializeField] private Transform _enemyHqPos;

    private HeadQuater _playerHq;
    private HeadQuater _enemyHq;
    
    public HeadQuater PlayerHq => _playerHq;
    public HeadQuater EnemyHq => _enemyHq;
    
    public void Init()
    {
        CreateHqs();
        CreateSpawners();
    }

    void CreateHqs()
    {
        CreateHq(Team.Player);
        CreateHq(Team.Enemy);
    }

    void CreateSpawners()
    {
        _playerHq.CreateSpawners();
        _enemyHq.CreateSpawners();
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
        hq.Init(hqInfo, argTeam, !isPlayer, GetTargetSpawnerPos);
        
        if (isPlayer)
            _playerHq = hq;
        else
            _enemyHq = hq;
    }

    Transform GetTargetSpawnerPos(Team argTeam, int argIndex)
    {
        if (argTeam == Team.Enemy)
        {
            return _playerHq.GetTargetSpawnerTransform(argIndex);
        }
        else
        {
            return _enemyHq.GetTargetSpawnerTransform(argIndex);
        }
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
        DestroyHqs();
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
}
