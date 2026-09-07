using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public void Init()
    {
        SetTutorialData();
    }

    void SetTutorialData()
    {
        Managers.Data.GetNextTutorial(SetTutorialTrigger);
    }
    
    void SetTutorialTrigger(TutorialData argTutorialData)
    {
        // load fail or all tutorial complete
        if (argTutorialData == null)
        {
            return;
        }

        var gm = Managers.Game;
        var ui = Managers.UI;
        switch (argTutorialData.triggerType)
        {
            case TutorialTriggerType.LobbyEnter:
                gm.OnEnterLobby += OnTriggered;
                break;
            case TutorialTriggerType.StageStart:
                gm.OnStartStage += OnTriggered;
                break;
            case TutorialTriggerType.StageEnd:
                gm.OnEndStage += OnTriggered;
                break;
            case TutorialTriggerType.StageStartDialogEnd:
                ui.DialogHandler.OnStageStartEnd += OnTriggered;
                break;
            case TutorialTriggerType.StageEndDialogEnd:
                ui.DialogHandler.OnStageFirstClearEnd += OnTriggered;
                break;
        }
        
        void OnTriggered()
        {
            int size = argTutorialData.infoList.Count;
            int index = 0;
            string id = argTutorialData.infoList[index++].targetId;
            Managers.UI.ActivateHighlight(id, OnHighlight);
            
            void OnHighlight(){
                if (index >= size)
                {
                    OnEnd();
                    return;
                }
                
                id = argTutorialData.infoList[index++].targetId;
                Managers.UI.ActivateHighlight(id, OnHighlight);
            }
        }

        void OnEnd()
        {
            // 완전히 다 끝나면 소거. 그 전에 해버리면 튜토리얼 끝내지 않았는데 없앨수도.
            ClearEvents();
            
            Managers.Game.UserRecord.AddCompletedTutorialId(argTutorialData.id);
            Managers.Game.SaveUserRecord();
            
            SetTutorialData();
        }

        void ClearEvents()
        {
            gm.OnEnterLobby -= OnTriggered;
            gm.OnStartStage -= OnTriggered;
            gm.OnEndStage -= OnTriggered;
            ui.DialogHandler.OnStageStartEnd -= OnTriggered;
            ui.DialogHandler.OnStageFirstClearEnd -= OnTriggered;
        }
    }
}
