using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStageInfo : APopup
{
    [SerializeField] private TextMeshProUGUI _stageTitleText;
    [SerializeField] private TextMeshProUGUI _stageDescriptionText;
    
    [Header("Clear Mission")] 
    [SerializeField] private GameObject _clearIcon;
    [SerializeField] private TextMeshProUGUI _clearMissionText;
    [SerializeField] private TextMeshProUGUI _clearBestText;
    
    [Header("ClearTime Mission")]
    [SerializeField] private GameObject _clearTimeIcon;
    [SerializeField] private TextMeshProUGUI _clearTimeMissionText;
    [SerializeField] private TextMeshProUGUI _clearTimeBestText;
    
    [Header("Hq Hp Mission")]
    [SerializeField] private GameObject _hqHpIcon;
    [SerializeField] private TextMeshProUGUI _hqHpMissionText;
    [SerializeField] private TextMeshProUGUI _hqHpBestText;
    
    [SerializeField] private Button _btnEnter;
    
    private StageInfo _stageInfo;
    
    public override void Init()
    {
        base.Init();
        
        _btnEnter.onClick.AddListener(OnClickEnterStage);
    }

    public void SetData(StageInfo argStageInfo)
    {
        _stageInfo = argStageInfo;
        
        SetText();
    }

    void SetText()
    {
        SetTitleText();
        SetMissionText();
        SetButtonText();
        SetIcon();
    }

    void SetTitleText()
    {
        
    }

    void SetMissionText()
    {
        
    }

    void SetButtonText()
    {
        
    }

    void SetIcon()
    {
        
    }
    
    void OnClickEnterStage()
    {
        Managers.Sound.PlaySelectSfx();

        Managers.UI.PopupHandler.ClosePopup();

        if (_stageInfo == null)
        {
            return;
        }
        Managers.Game.EnterStage(_stageInfo);
    }

    public override void Clear()
    {
        _btnEnter.onClick.RemoveAllListeners();
        
        base.Clear();
    }
}
