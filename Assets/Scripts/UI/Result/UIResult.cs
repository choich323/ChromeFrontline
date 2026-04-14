using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct ResultData
{
    public bool isVictory;
    public string timerText;
    public string hpText;
}

public class UIResult : APopup
{
    [SerializeField] private Image _bgImage;
    [SerializeField] private Color _bgVictoryColor;
    [SerializeField] private Color _bgDefeatColor;
    
    [Header("Title")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private Color _victoryTextColor;
    [SerializeField] private Color _defeatTextColor;

    [Header("Clear Mission")] 
    [SerializeField] private GameObject _clearIcon;
    [SerializeField] private TextMeshProUGUI _clearMissionText;
    [SerializeField] private TextMeshProUGUI _clearText;
    [SerializeField] private TextMeshProUGUI _clearBestText;
    [SerializeField] private GameObject _clearNewTextObject;
    
    [Header("ClearTime Mission")]
    [SerializeField] private GameObject _clearTimeIcon;
    [SerializeField] private TextMeshProUGUI _clearTimeMissionText;
    [SerializeField] private TextMeshProUGUI _clearTimeText;
    [SerializeField] private TextMeshProUGUI _clearTimeBestText;
    [SerializeField] private GameObject _clearTimeNewText;
    
    [Header("Hq Hp Mission")]
    [SerializeField] private GameObject _hqHpIcon;
    [SerializeField] private TextMeshProUGUI _hqHpMissionText;
    [SerializeField] private TextMeshProUGUI _hqHpText;
    [SerializeField] private TextMeshProUGUI _hqHpBestText;
    [SerializeField] private GameObject _hqHpNewText;

    [Header("Buttons")]
    [SerializeField] private Button _retryBtn;
    [SerializeField] private TextMeshProUGUI _retryBtnText;
    [SerializeField] private Button _exitBtn;
    [SerializeField] private TextMeshProUGUI _exitBtnText;
    
    private StringManager Sm => Managers.String;

    private ResultData _resultData;
    
    public void SetData(ResultData argResultData)
    {
        _resultData = argResultData;
        SetButton();
        SetColor();
        SetText();
    }

    void SetButton()
    {
        _retryBtn.onClick.RemoveAllListeners();
        _exitBtn.onClick.RemoveAllListeners();
        
        _retryBtn.onClick.AddListener(Managers.Game.RestartStage);
        _exitBtn.onClick.AddListener(Managers.Game.ExitStage);
    }
    
    void SetText()
    {
        SetTitleText();
        SetMissionText();
        SetButtonText();
        SetClearIcon();
    }

    void SetTitleText()
    {
        _titleText.text = _resultData.isVictory ? Sm.GetString(StringID.Victory) : Sm.GetString(StringID.Defeat);
    }

    void SetMissionText()
    {
        // Clear Mission
        _clearMissionText.text = Sm.GetString(StringID.Clear);
        var clearText = _resultData.isVictory ? Sm.GetString(StringID.Success) : Sm.GetString(StringID.Fail);
        _clearText.text = clearText;
        


        _clearTimeMissionText.text = Sm.GetString(StringID.ClearTimeMission);
        _clearTimeText.text = _resultData.timerText;
        
        
        
        _hqHpMissionText.text = Sm.GetString(StringID.HqHpMission);
        _hqHpText.text = _resultData.hpText;
        
        
    }

    void SetButtonText()
    {
        _retryBtnText.text = Sm.GetString(StringID.Retry);
        _exitBtnText.text = Sm.GetString(StringID.Exit);
    }

    void SetClearIcon()
    {
        
    }
    
    void SetColor()
    {
        if (_resultData.isVictory)
        {
            _bgImage.color = _bgVictoryColor;
            _titleText.color = _victoryTextColor;
        }
        else
        {
            _bgImage.color = _bgDefeatColor;
            _titleText.color = _defeatTextColor;
        }
    }
}
