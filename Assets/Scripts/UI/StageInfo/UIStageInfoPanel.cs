using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStageInfoPanel : MonoBehaviour
{
    private const float HOUR_TO_SECOND  = 3600f;
    private const float MINUTE_TO_SECOND  = 60f;
    private const float CLEAR_TIME_THRESHOLD = UserRecord.CLEAR_TIME_THRESHOLD;
    private const int CLEAR_HQ_HP_RATIO = 100;
    private const string STAGE_CLEAR_NONE = "--";
    private const string STAGE_CLEAR_TIME_NONE = "--:--";
    private const string STAGE_CLEAR_HQ_HP_NONE = "--%";
    
    [SerializeField] private TextMeshProUGUI _stageTitleText;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Skill Buttons")] 
    [SerializeField] private List<Button> _skillButtonList = new List<Button>();
    [SerializeField] private List<Image> _skillImageList = new List<Image>();
    [SerializeField] private List<GameObject> _skillStateImageList = new List<GameObject>();
    
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
    
    [Header("Clear Reward")]
    [SerializeField] private TextMeshProUGUI _clearReward;
    
    [SerializeField] private Button _btnEnter;
    
    private StageInfo _stageInfo;
    private Action<int> _onSkillBtn;
    
    public CanvasGroup CanvasGroup => _canvasGroup;

    public void Init(Action<int> argOnSkillBtn)
    {
        _btnEnter.onClick.RemoveListener(OnClickEnterStage);
        _btnEnter.onClick.AddListener(OnClickEnterStage);
        
        _onSkillBtn = argOnSkillBtn;
    }

    public void SetStageInfo(StageInfo argStageInfo)
    {
        _stageInfo = argStageInfo;
    }
    
    public async void SetUI(UserRecord argUserRecord)
    {
        for(int i = 0; i < _skillButtonList.Count; i++)
        {
            var btn = _skillButtonList[i];
            int index = i;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnSkillBtn(index));
        }
        SetSkillIcons(argUserRecord);
        
        await SetStageTitleText();
        SetMissionText(argUserRecord);
        SetRewardText();
        SetIcon(argUserRecord);
    }

    void OnSkillBtn(int argSkillIndex)
    {
        _onSkillBtn?.Invoke(argSkillIndex);
    }
    
    public void SetSkillIcons(UserRecord argUserRecord)
    {
        List<int> idList = new List<int>();
        foreach (var id in argUserRecord.EquipmentIds)
        {
            idList.Add(id);
        }

        for(int i = 0; i < idList.Count; i++)
        {
            var id = idList[i];
            var info = Managers.Data.GetSkillInfo(id);
            if (info == null)
            {
                _skillImageList[i].gameObject.SetActive(false);
                _skillStateImageList[i].SetActive(true);
                continue;
            }

            var icon = _skillImageList[i];
            icon.sprite = info.icon;
            icon.gameObject.SetActive(true);
            _skillStateImageList[i].SetActive(false);
        }
    }
    
    async Task SetStageTitleText()
    {
        var title= await Managers.String.GetStageTitle(Managers.Game.CurWorldId, _stageInfo.stage);
        _stageTitleText.SetText(title);
    }

    void SetMissionText(UserRecord argUserRecord)
    {
        var ur = argUserRecord;
        var stage = _stageInfo.stage;
        var stageRecord = ur.GetStageBestRecord(stage);
        
        var Sm = Managers.String;
        _clearMissionText.SetText(Sm.GetString(StringID.Clear));
        var clearMissionTimeThreshold = GetConvertedTimeText(CLEAR_TIME_THRESHOLD);
        _clearTimeMissionText.SetText(Sm.GetString(StringID.ClearTimeMission, clearMissionTimeThreshold));
        _hqHpMissionText.SetText(Sm.GetString(StringID.HqHpMission, CLEAR_HQ_HP_RATIO));

        if (stageRecord.IsFirstTry())
        {
            _clearBestText.SetText(STAGE_CLEAR_NONE);
            _clearTimeBestText.SetText(STAGE_CLEAR_TIME_NONE);
            _hqHpBestText.SetText(STAGE_CLEAR_HQ_HP_NONE);
        }
        else
        {
            string clearText = stageRecord.isClear ? Sm.GetString(StringID.Success) : Sm.GetString(StringID.Fail);
            _clearBestText.SetText(clearText);
        
            _clearTimeBestText.SetText(GetConvertedTimeText(stageRecord.clearTime));
        
            var ratio = stageRecord.hqhpRatio == StageRecord.INVALID_HQ_HP_RATIO ? STAGE_CLEAR_HQ_HP_NONE : $"{stageRecord.hqhpRatio:N0}%";
            _hqHpBestText.SetText(ratio);
        }
    }

    void SetRewardText()
    {
        _clearReward.SetText($"{_stageInfo.reward}");
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
    
    public void Clear()
    {        
        _btnEnter.onClick.RemoveAllListeners();
        
        _clearIcon.SetActive(false);
        _clearTimeIcon.SetActive(false);
        _hqHpIcon.SetActive(false);
    }
}
