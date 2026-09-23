using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStageInfo : APopup
{
    [SerializeField] private UIStageInfoPanel _stageInfoPanel;
    [SerializeField] private UISkillSelectPanel _skillSelectPanel;
    [SerializeField] private float _transitionDuration = 0.2f;
    [SerializeField] private float _slideDistance = 100f;
    
    public override void Init()
    {
        base.Init();
        
        _stageInfoPanel.gameObject.SetActive(true);
        _stageInfoPanel.CanvasGroup.alpha = 1f;
        _stageInfoPanel.CanvasGroup.interactable = _stageInfoPanel.CanvasGroup.blocksRaycasts = true;
        
        _skillSelectPanel.gameObject.SetActive(false);
        _skillSelectPanel.CanvasGroup.alpha = 0f;
        _skillSelectPanel.CanvasGroup.interactable = _skillSelectPanel.CanvasGroup.blocksRaycasts = false;
        
        _stageInfoPanel.Init(OnSkillBtn);
        _skillSelectPanel.Init(OnSkillSelect);
    }

    public void SetUI(StageInfo argStageInfo, UserRecord argUserRecord)
    {
        _stageInfoPanel.SetStageInfo(argStageInfo);
        _stageInfoPanel.SetUI(argUserRecord);
        _skillSelectPanel.SetUI(argUserRecord);
    }

    void Transition(CanvasGroup argCurCanvas, CanvasGroup argTargetCanvas, bool argIsGoBack)
    {
        argCurCanvas.interactable = argCurCanvas.blocksRaycasts = false;

        argTargetCanvas.alpha = 0;
        argTargetCanvas.gameObject.SetActive(true);
        
        float startX = argIsGoBack ? -_slideDistance : _slideDistance;
        var rectTransform = argTargetCanvas.transform as RectTransform;
        rectTransform.anchoredPosition = new Vector2(startX, 0);
        
        argCurCanvas.DOFade(0, _transitionDuration).SetUpdate(true);
        argTargetCanvas.DOFade(1, _transitionDuration).SetUpdate(true);
        rectTransform.DOAnchorPos(Vector2.zero, _transitionDuration).SetEase(Ease.OutCubic).SetUpdate(true)
            .OnComplete(() =>
            {
               argTargetCanvas.interactable = argTargetCanvas.blocksRaycasts = true;
               argCurCanvas.gameObject.SetActive(false);
            });
    }

    void OnSkillBtn(int argSkillSlotIndex)
    {
        Managers.Sound.PlaySelectSfx();
        
        _skillSelectPanel.SetCurSkillSlot(argSkillSlotIndex);
        _skillSelectPanel.SetUI(Managers.Game.UserRecord);
        Transition(_stageInfoPanel.CanvasGroup, _skillSelectPanel.CanvasGroup, false);
    }
    
    void OnSkillSelect(CanvasGroup argCanvasGroup)
    {
        Managers.Sound.PlaySelectSfx();
        
        _stageInfoPanel.SetUI(Managers.Game.UserRecord);
        Transition(argCanvasGroup, _stageInfoPanel.CanvasGroup, true);
    }
    
    public override void Clear()
    {
        base.Clear();
        
        _stageInfoPanel.Clear();
        _skillSelectPanel.Clear();
    }
}
