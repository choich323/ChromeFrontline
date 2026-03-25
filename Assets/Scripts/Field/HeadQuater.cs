using System;
using UnityEngine;

public class HeadQuater : MonoBehaviour
{
    private int _maxHp;
    private int _hp;
    private int _shield;
    private long _gold;
    private int _mineral;
    private Team _team;
    
    public int Hp => _hp;
    public int Shield => _shield;
    public long Gold => _gold;
    public int Mineral => _mineral;
    
    public void Init(HeadQuaterInfo argInfo, Team argTeam)
    {
        _maxHp = argInfo.hp;
        _hp = argInfo.hp;
        _shield = argInfo.shield;
        _team = argTeam;
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
    
    public void Reset()
    {
        _maxHp = 0;
        _hp = 0;
        _shield = 0;
        _gold = 0;
        _mineral = 0;
        _team = Team.None;
    }
    
    public void Destroy()
    {
        Reset();
    }

    [ContextMenu("ShowStatus")]
    public void TestShowStatus()
    {
        Debug.Log($"Hp: {_hp}, Shield: {_shield}");
    }
}
