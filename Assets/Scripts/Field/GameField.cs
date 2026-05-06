using System;
using System.Collections.Generic;
using UnityEngine;

public class GameField : MonoBehaviour
{
    [SerializeField] private Transform _hqParent;
    [SerializeField] private Transform _playerHqPos;
    [SerializeField] private Transform _enemyHqPos;

    private HeadQuarter _playerHq;
    private HeadQuarter _enemyHq;
    
    public HeadQuarter PlayerHq => _playerHq;
    public HeadQuarter EnemyHq => _enemyHq;
    
    public void Init()
    {
        Managers.Sound.PlayIngameBgm();
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
        var hqObj = Managers.Pool.Instantiate(PrefabID.HeadQuarter);
        if (hqObj == null)
            return;
        
        hqObj.transform.SetParent(_hqParent);
        bool isPlayer = argTeam == Team.Player;
        hqObj.transform.position = isPlayer ? _playerHqPos.position : _enemyHqPos.position;
        var hq = hqObj.GetComponent<HeadQuarter>();
        hq.Init(argTeam, !isPlayer, GetTargetSpawnerPos);
        
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

    public void Restart()
    {
        ResetField();
        Init();
    }
    
    void DestroyAll()
    {
        DestroyHqs();
    }
    
    void DestroyHqs()
    {
        Managers.Pool.Destroy(_playerHq, PrefabID.HeadQuarter);
        Managers.Pool.Destroy(_enemyHq, PrefabID.HeadQuarter);
        _playerHq.Destroy();
        _enemyHq.Destroy();
        _playerHq = null;
        _enemyHq = null;
    }
}
