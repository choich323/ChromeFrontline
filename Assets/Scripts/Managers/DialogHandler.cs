using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogHandler
{
    private const float HUNDRED = 100f;
    
    private List<Action> _triggerReleaseList = new List<Action>();
    
    public void Init()
    {

    }

    public void ReleaseTrigger()
    {
        foreach (var release in _triggerReleaseList)
        {
            release.Invoke();
        }
        _triggerReleaseList.Clear();
    }
    
    public void SetTrigger()
    {
        var data = Managers.Data.GetDialogTriggerData();
        if (data == null)
        {
            return;
        }

        foreach (var info in data.triggerInfoList)
        {
            switch (info.triggerType)
            {
                case DialogTriggerType.StageStart:
                    SetStageStart(info);
                    break;

                case DialogTriggerType.HqHpBelow:
                    SetHqHpBelow(info);
                    break;

                case DialogTriggerType.StageFirstClear:
                    SetStageFirstClear(info);
                    break;
            }
        }
    }

    void SetStageStart(DialogTriggerInfo argInfo)
    {
        Managers.Game.OnStartStage += OnStartStage;
        _triggerReleaseList.Add(() => Managers.Game.OnStartStage -= OnStartStage);
        
        void OnStartStage()
        {
            Managers.UI.ShowDialog(argInfo.dialogInfoId);
            Managers.Game.OnStartStage -= OnStartStage;
        } 
    }

    void SetHqHpBelow(DialogTriggerInfo argInfo)
    {
        var hq = Managers.Game.GameField.PlayerHq;

        hq.OnHealthChanged += OnHqHpBelow;
        _triggerReleaseList.Add(() => hq.OnHealthChanged -= OnHqHpBelow);
        
        void OnHqHpBelow(int argHp, int argMaxHp)
        {
            var hp = (float)argHp / argMaxHp;
            hp *= HUNDRED;
            if (hp <= argInfo.value)
            {
                Managers.UI.ShowDialog(argInfo.dialogInfoId);
                hq.OnHealthChanged -= OnHqHpBelow;
            }
        }
    }
    
    void SetStageFirstClear(DialogTriggerInfo argInfo)
    {
        Managers.Game.OnEndStage += OnEndStage;
        _triggerReleaseList.Add(() => Managers.Game.OnEndStage -= OnEndStage);

        void OnEndStage()
        {
            Managers.UI.ShowDialog(argInfo.dialogInfoId, OnEnd);
            Managers.Game.OnEndStage -= OnEndStage;

            void OnEnd()
            {
                Managers.Game.PauseGame();
            }
        }
    }
}
