using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStageInfo : APopup
{
    private const float HOUR_TO_SECOND  = 3600f;
    private const float MINUTE_TO_SECOND  = 60f;
    private const float CLEAR_TIME_THRESHOLD = 720f; // 12분
    private const int CLEAR_HQ_HP_RATIO = 100;
    private const string STAGE_CLEAR_NONE = "--";
    private const string STAGE_CLEAR_TIME_NONE = "--:--";
    private const string STAGE_CLEAR_HQ_HP_NONE = "--%";
    
    [SerializeField] private TextMeshProUGUI _stageNumberText;
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

    public void SetData(StageInfo argStageInfo, UserRecord argUserRecord)
    {
        _stageInfo = argStageInfo;
        
        SetUI(argUserRecord);
    }

    void SetUI(UserRecord argUserRecord)
    {
        //SetStageNumberText(argUserRecord);
        SetStageTitleText(argUserRecord);
        SetStageDescText(argUserRecord);
        SetMissionText(argUserRecord);
        SetIcon(argUserRecord);
    }

    void SetStageNumberText(UserRecord argUserRecord)
    {
        var worldNum = Managers.Data.GetWorldIndex(argUserRecord.CurrentWorldId) + 1;
        _stageNumberText.SetText($"{worldNum}-{_stageInfo.stage}");
    }

    void SetStageTitleText(UserRecord argUserRecord)
    {
        _stageTitleText.SetText(Managers.String.GetStageTitle(argUserRecord.CurrentWorldId, _stageInfo.stage));
    }
    
    void SetStageDescText(UserRecord argUserRecord)
    {
        _stageDescriptionText.SetText(Managers.String.GetStageDesc(argUserRecord.CurrentWorldId, _stageInfo.stage));
    }

    void SetMissionText(UserRecord argUserRecord)
    {
        var ur = argUserRecord;
        var stage = _stageInfo.stage;
        var stageRecord = ur.GetStageBestRecord(stage);
        
        var Sm = Managers.String;
        _clearMissionText.SetText(Sm.GetString(StringID.Clear));
        var clearMissionTimeThreshold = GetConvertedTimeText(CLEAR_TIME_THRESHOLD);
        _hqHpMissionText.SetText(Sm.GetString(StringID.HqHpMission, CLEAR_HQ_HP_RATIO));

        if (stageRecord == null)
        {
            _clearBestText.SetText(STAGE_CLEAR_NONE);
            _clearTimeBestText.SetText(STAGE_CLEAR_TIME_NONE);
            _hqHpBestText.SetText(STAGE_CLEAR_HQ_HP_NONE);
        }
        else
        {
            string clearText = stageRecord.isClear ? Sm.GetString(StringID.Success) : Sm.GetString(StringID.Fail);
            _clearBestText.SetText(clearText);
        
            _clearTimeMissionText.SetText(Sm.GetString(StringID.ClearTimeMission, clearMissionTimeThreshold));
            _clearTimeBestText.SetText(GetConvertedTimeText(stageRecord.clearTime));
        
            var ratio = stageRecord.hqhpRatio == StageRecord.INVALID_HQ_HP_RATIO ? STAGE_CLEAR_HQ_HP_NONE : $"{stageRecord.hqhpRatio:N0}%";
            _hqHpBestText.SetText(ratio);
        }
    }

    string GetConvertedTimeText(float argTime)
    {
        var playTime = argTime;
        if (playTime >= float.MaxValue)
        {
            return STAGE_CLEAR_TIME_NONE;
        }
        
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

    void SetIcon(UserRecord argUserRecord)
    {
        var stage = _stageInfo.stage;
        
        if (argUserRecord.IsClear(stage))
        {
            _clearIcon.SetActive(true);
        }
        else
        {
            _clearIcon.SetActive(false);
        }

        if (argUserRecord.IsClearInTime(stage))
        {
            _clearTimeIcon.SetActive(true);
        }
        else
        {
            _clearTimeIcon.SetActive(false);
        }

        if (argUserRecord.IsClearHqHp(stage))
        {
            _hqHpIcon.SetActive(true);
        }
        else
        {
            _hqHpIcon.SetActive(false);
        }
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
        base.Clear();
        
        _btnEnter.onClick.RemoveAllListeners();
        
        _clearIcon.SetActive(false);
        _clearTimeIcon.SetActive(false);
        _hqHpIcon.SetActive(false);
    }
}
