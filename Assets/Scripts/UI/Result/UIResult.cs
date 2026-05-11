using System;
using TMPro;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UI;

public struct ResultData
{
    public int stage;
    public bool isVictory;
}

public class UIResult : APopup
{
    private const float HOUR_TO_SECOND  = 3600f;
    private const float MINUTE_TO_SECOND  = 60f;
    private const float HUNDRED_PERCENT  = 100f;
    private const float CLEAR_TIME_THRESHOLD = 720f; // 12분
    private const int CLEAR_HQ_HP_RATIO = 100;
    
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
    [SerializeField] private GameObject _clearTimeNewTextObject;
    
    [Header("Hq Hp Mission")]
    [SerializeField] private GameObject _hqHpIcon;
    [SerializeField] private TextMeshProUGUI _hqHpMissionText;
    [SerializeField] private TextMeshProUGUI _hqHpText;
    [SerializeField] private TextMeshProUGUI _hqHpBestText;
    [SerializeField] private GameObject _hqHpNewTextObject;

    [Header("Buttons")]
    [SerializeField] private Button _retryBtn;
    [SerializeField] private TextMeshProUGUI _retryBtnText;
    [SerializeField] private Button _exitBtn;
    [SerializeField] private TextMeshProUGUI _exitBtnText;
    
    private StringManager Sm => Managers.String;
    private GameManager Gm => Managers.Game;
    
    private ResultData _resultData;
    private float _playTime;
    private bool _isClearChanged = false;
    private bool _isBestClearTimeChanged = false;
    private bool _isBestHqHpChanged = false;
    
    public void SetData(ResultData argResultData)
    {
        Clear();
        
        _resultData = argResultData;
        _playTime = Gm.PlayTime;
        
        CheckSave();
        
        SetButton();
        SetColor();
        SetText();
    }

    public override void Clear()
    {
        base.Clear();
        
        _isClearChanged = false;
        _isBestClearTimeChanged = false;
        _isBestHqHpChanged = false;
        _clearNewTextObject.SetActive(false);
        _clearTimeNewTextObject.SetActive(false);
        _hqHpNewTextObject.SetActive(false);
        _clearIcon.SetActive(false);
        _clearTimeIcon.SetActive(false);
        _hqHpIcon.SetActive(false);
    }
    
    void CheckSave()
    {
        var record = new UserRecord();
        var tick = DateTime.Now.Ticks;
        var oldUserRecord = Gm.UserRecord;

        var stageBestRecord = oldUserRecord.GetStageBestRecord(_resultData.stage);
        if (!stageBestRecord.clear && _resultData.isVictory)
        {
            _isClearChanged = true;
            stageBestRecord.clear = true;
        }

        if (_resultData.isVictory && _playTime < stageBestRecord.clearTime)
        {
            _isBestClearTimeChanged = true;
            stageBestRecord.clearTime = _playTime;
        }

        var hpRatio = Gm.GameField.PlayerHq.GetHqHpRatio() * HUNDRED_PERCENT;
        if (stageBestRecord.hqhpRatio < HUNDRED_PERCENT && hpRatio >= HUNDRED_PERCENT)
        {
            _isBestHqHpChanged = true;
            stageBestRecord.hqhpRatio = (int)hpRatio;
        }

        if (_isClearChanged || _isBestClearTimeChanged || _isBestHqHpChanged)
        {
            stageBestRecord.tick = tick;
            record.SaveStageBestRecord(_resultData.stage, stageBestRecord);
            Gm.SaveUserRecord(record);
        }
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
        SetIcon();
    }

    void SetTitleText()
    {
        _titleText.text = _resultData.isVictory ? Sm.GetString(StringID.Victory) : Sm.GetString(StringID.Defeat);
    }

    void SetMissionText()
    {
        var ur = Gm.UserRecord;
        var stage = _resultData.stage;
        var stageRecord = ur.GetStageBestRecord(stage);
        
        // Clear Mission
        _clearMissionText.text = Sm.GetString(StringID.Clear);
        var clearText = _resultData.isVictory ? Sm.GetString(StringID.Success) : Sm.GetString(StringID.Fail);
        _clearText.text = clearText;
        var bestClear = stageRecord.clear ? Sm.GetString(StringID.Success) : Sm.GetString(StringID.Fail);
        _clearBestText.text = Sm.GetString(StringID.Best, bestClear);
        if (_isClearChanged)
        {
            _clearNewTextObject.SetActive(true);
        }

        // Clear Time Mission
        var clearMissionTimeThreshold = GetConvertedTimeText(CLEAR_TIME_THRESHOLD);
        _clearTimeMissionText.text = Sm.GetString(StringID.ClearTimeMission, clearMissionTimeThreshold);
        _clearTimeText.text = GetConvertedTimeText(Gm.PlayTime);
        string bestClearTimeText = _isBestClearTimeChanged ? _clearTimeText.text : GetConvertedTimeText(stageRecord.clearTime);
        _clearTimeBestText.text = Sm.GetString(StringID.Best, bestClearTimeText);
        
        if (_isBestClearTimeChanged)
        {
            _clearTimeNewTextObject.SetActive(true);
        }
        
        // HqHp Mission
        _hqHpMissionText.text = Sm.GetString(StringID.HqHpMission, CLEAR_HQ_HP_RATIO);
        var hpRatio = Gm.GameField.PlayerHq.GetHqHpRatio() * HUNDRED_PERCENT;
        if (hpRatio < 0)
        {
            hpRatio = 0f;
        }
        _hqHpText.text = $"{hpRatio:N0}%";
        var bestHpText = _isBestHqHpChanged ? _hqHpText.text : stageRecord.hqhpRatio + "%";
        _hqHpBestText.text = Sm.GetString(StringID.Best, bestHpText);
        if (_isBestHqHpChanged)
        {
            _hqHpNewTextObject.SetActive(true);
        }
    }

    string GetConvertedTimeText(float argTime)
    {
        var playTime = argTime;
        
        int hours = Mathf.FloorToInt(playTime / HOUR_TO_SECOND);
        int minutes = Mathf.FloorToInt((playTime % HOUR_TO_SECOND) / MINUTE_TO_SECOND);
        int seconds = Mathf.FloorToInt(playTime % MINUTE_TO_SECOND);

        var result = string.Empty;
        if (hours > 0)
        {
            // 이 경우는 없을 것 같지만 일단 추가 해놓음
            result = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }
        else
        {
            result = $"{minutes:D2}:{seconds:D2}";
        }
        return result;
    }

    void SetButtonText()
    {
        _retryBtnText.text = Sm.GetString(StringID.Retry);
        _exitBtnText.text = Sm.GetString(StringID.Exit);
    }

    void SetIcon()
    {
        var ur = Gm.UserRecord;
        var stage = _resultData.stage;
        if (ur.IsClear(stage))
        {
            _clearIcon.SetActive(true);
        }

        if (ur.IsClearInTime(stage))
        {
            _clearTimeIcon.SetActive(true);
        }

        if (ur.IsClearHqHp(stage))
        {
            _hqHpIcon.SetActive(true);
        }
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
