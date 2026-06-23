using System;
using System.Collections.Generic;
using UnityEngine;

public class GameField : MonoBehaviour
{
    [SerializeField] private Transform _hqParent;
    [SerializeField] private Transform _playerHqPos;
    [SerializeField] private Transform _enemyHqPos;
    [SerializeField] private Transform _explosionParent;

    private HeadQuarter _playerHq;
    private HeadQuarter _enemyHq;
    
    public HeadQuarter PlayerHq => _playerHq;
    public HeadQuarter EnemyHq => _enemyHq;
    public Transform ExplosionParent => _explosionParent;
    
    public void Run()
    {
        ResetField();
        CreateHqs();
        RunHqs();
    }

    void CreateHqs()
    {
        CreateHq(Team.Player);
        CreateHq(Team.Enemy);
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
        
        if (isPlayer)
            _playerHq = hq;
        else
            _enemyHq = hq;
        
        hq.Init(argTeam, !isPlayer, GetTargetSpawnerPos);
    }

    void RunHqs()
    {
        _playerHq.Run();
        _enemyHq.Run();
    }

    Transform GetTargetSpawnerPos(Team argTeam)
    {
        if (argTeam == Team.Enemy)
        {
            return _playerHq.GetTargetSpawnerTransform();
        }
        else
        {
            return _enemyHq.GetTargetSpawnerTransform();
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
        if (_playerHq != null)
        {
            Managers.Pool.Destroy(_playerHq, PrefabID.HeadQuarter);
            _playerHq.Destroy();
            _playerHq = null;
        }

        if (_enemyHq != null)
        {
            Managers.Pool.Destroy(_enemyHq, PrefabID.HeadQuarter);
            _enemyHq.Destroy();
            _enemyHq = null;
        }
    }
}
