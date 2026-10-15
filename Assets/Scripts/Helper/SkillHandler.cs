using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillHandler
{
    private SkillActionExecutor _skillActionExecutor;
    private Action<int, IEnumerator> _runCoroutine;
    private Action<int> _stopCoroutine;
    private HashSet<int> _cooltimeSkills = new HashSet<int>();
    
    public void Run(Action<int, IEnumerator> argRunCoroutine, Action<int> argStopCoroutine)
    {
        _runCoroutine = argRunCoroutine;
        _stopCoroutine = argStopCoroutine;
        RunSkillActionExecutor();
    }

    void RunSkillActionExecutor()
    {
        if (_skillActionExecutor != null)
        {
            _skillActionExecutor.Destroy();
        }
        
        _skillActionExecutor = new SkillActionExecutor();
        _skillActionExecutor.Run(_runCoroutine, _stopCoroutine);
    }

    public bool ExecuteSkill(int argId, Vector2 argPos)
    {
        if (_cooltimeSkills.Contains(argId))
        {
            return false;
        }
        
        _runCoroutine?.Invoke(argId, CoExecuteSkill(argId, argPos));
        
        return true;
    }

    IEnumerator CoExecuteSkill(int argId, Vector2 argPos)
    {
        if (!_cooltimeSkills.Add(argId))
        {
            yield break;
        }
        
        var ur = Managers.Game.UserRecord;
        var level = ur.GetSkillLevel(argId);
        var skillInfo = Managers.Data.GetSkillInfo(argId);
        if (!CheckCost(skillInfo.costList, skillInfo.GetUpgradeValue(level)))
        {
            yield break;
        }
        
        try
        {
            var cool = skillInfo.GetCooldown(level);
            _skillActionExecutor.Execute(skillInfo, level, argPos);
        
            yield return new WaitForSeconds(cool);
        }
        finally
        {
            _cooltimeSkills.Remove(argId);
        
            _stopCoroutine?.Invoke(argId);
        }
    }

    bool CheckCost(List<CostData> argCostList, float argUpgradeValue)
    {
        bool canPay = true;
        foreach (var cost in argCostList)
        {
            switch (cost.type)
            {
                case CostType.Gold:
                    if (Managers.Game.GameField.PlayerHq.Gold < (cost.value - argUpgradeValue))
                    {
                        canPay = false;
                    }
                    break;
            }
        }

        return canPay;
    }
    
    public void Destroy()
    {
        _skillActionExecutor.Destroy();

        foreach (var id in _cooltimeSkills)
        {
            _stopCoroutine?.Invoke(id);
        }
        _cooltimeSkills.Clear();
    }
}
