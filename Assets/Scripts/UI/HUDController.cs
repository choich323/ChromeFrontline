using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class HUDController : MonoBehaviour
{
    private const float HUNDRED_PERCENT  = 100f;
    private const float HP_MEDIUM = 0.6f;
    private const float HP_LOW = 0.3f;
    private const float HOUR_TO_SECOND  = 3600f;
    private const float MINUTE_TO_SECOND  = 60f;
    
    [Header("Player HQ UI")] 
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private Slider _shieldSlider;
    [SerializeField] private Image _hpBarFillImage; // 색상 변경용
    [SerializeField] private TextMeshProUGUI _hpText;

    [Header("Capital UI")] 
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _mineralText;
    
    [Header("Info UI")]
    [SerializeField] private TextMeshProUGUI _timerText;
    
    [Header("Hp Color Set")]
    [SerializeField] private Color _highHpColor = Color.green;
    [SerializeField] private Color _mediumHpColor = Color.yellow;
    [SerializeField] private Color _lowHpColor = Color.red;

    [Header("Menu Button")]
    [SerializeField] private Button _hqBtn;
    [SerializeField] private GameObject _subBtnGroup;
    [SerializeField] private List<CanvasGroup> _subBtnCanvasGroupList;
    [SerializeField] private float _subMenuAnimPlayTime = 0.1f;
    [SerializeField] private Button _optionBtn;
    [SerializeField] private Button _restartBtn;
    [SerializeField] private Button _exitBtn;
    
    private bool _isSubMenuOpen = false;
    private bool _isRestarting = false;
    private Coroutine _menuAnimCoroutine;

    bool IsPaused => Managers.Game.IsPaused;

    public void Init()
    {
        Clear();
        
        _hqBtn.onClick.AddListener(OnBtnHq);
        _optionBtn.onClick.AddListener(OnBtnOption);
        _restartBtn.onClick.AddListener(OnBtnReStart);
        _exitBtn.onClick.AddListener(OnBtnExit);
    }

    public void Clear()
    {
        _isSubMenuOpen = false;
        if (_menuAnimCoroutine != null)
        {
            StopCoroutine(_menuAnimCoroutine);
            _menuAnimCoroutine = null;
        }
        _subBtnGroup.SetActive(false);
        foreach (var cg in _subBtnCanvasGroupList)
        {
            cg.alpha = 0;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
        
        _hqBtn.onClick.RemoveAllListeners();
        _optionBtn.onClick.RemoveAllListeners();
        _restartBtn.onClick.RemoveAllListeners();
        _exitBtn.onClick.RemoveAllListeners();
    }
    
    private void Update()
    {
        if (_isRestarting || IsPaused)
            return;
        
        UpdatePlayerStatus();
        UpdateTimer();
    }

    void UpdatePlayerStatus()
    {
        var hq = Managers.Game.GameField.PlayerHq;
        float hpRatio = hq.GetHqHpRatio();
        float shieldRatio = hq.GetShieldRatio();
        long curGold = hq.Gold;
        int curMineral = hq.Mineral;
        
        _hpSlider.value = hpRatio;
        _shieldSlider.value = shieldRatio;
        var shieldText = shieldRatio <= 0 ? "" : $"+{shieldRatio * HUNDRED_PERCENT:N0}%";
        _hpText.text = $"{(hpRatio * HUNDRED_PERCENT):N0}%" + shieldText;
        _goldText.text = $"{curGold}";
        _mineralText.text = $"{curMineral}";
        
        UpdateHpBarColor(hpRatio);
    }

    void UpdateHpBarColor(float argRatio)
    {
        if (argRatio > HP_MEDIUM)
        {
            _hpBarFillImage.color = _highHpColor;
        }
        else if (argRatio > HP_LOW)
        {
            _hpBarFillImage.color = _mediumHpColor;
        }
        else
        {
            _hpBarFillImage.color = _lowHpColor;
        }
    }

    void UpdateTimer()
    {
        var playTime = Managers.Game.PlayTime;
        
        int hours = Mathf.FloorToInt(playTime / HOUR_TO_SECOND);
        int minutes = Mathf.FloorToInt((playTime % HOUR_TO_SECOND) / MINUTE_TO_SECOND);
        int seconds = Mathf.FloorToInt(playTime % MINUTE_TO_SECOND);

        if (hours > 0)
        {
            _timerText.text = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }
        else
        {
            _timerText.text = $"{minutes:D2}:{seconds:D2}";
        }
    }

    void OnBtnHq()
    {
        var popup = Managers.UI.PopupHandler.OpenPopup<UIHqManagement>(PrefabID.UIHqManagement);
        popup.SetOnClose(OnBtnPopupClose);
    }

    void OnBtnMenu()
    {
        if (_menuAnimCoroutine != null)
        {
            StopCoroutine(_menuAnimCoroutine);
        }

        _isSubMenuOpen = !_isSubMenuOpen;
        _menuAnimCoroutine = StartCoroutine(CoAnimateSubMenu(_isSubMenuOpen));
    }

    IEnumerator CoAnimateSubMenu(bool argOpen) {
        if (argOpen)
        {
            _subBtnGroup.SetActive(true);
        }

        float duration = _subMenuAnimPlayTime;

        int count = _subBtnCanvasGroupList.Count;
        for (int i = 0; i < count; i++)
        {
            int index = argOpen ? i : count - 1 - i;
            var cg = _subBtnCanvasGroupList[index];

            float elapsed = 0;
            float startAlpha = cg.alpha;
            float endAlpha = argOpen ? 1f : 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                yield return null;
            }
            
            cg.alpha = endAlpha;
            cg.interactable = argOpen;
            cg.blocksRaycasts = argOpen;
        }

        if (!argOpen)
        {
            _subBtnGroup.SetActive(false);
        }
    }

    void OnBtnOption()
    {
        var popup = Managers.UI.PopupHandler.OpenPopup<UIOption>(PrefabID.UIOption);
        popup.SetOnClose(OnBtnPopupClose);
    }
    
    void OnBtnReStart()
    {
        var popup = Managers.UI.PopupHandler.OpenPopup<UIConfirm>(PrefabID.UIConfirm);
        var sm = Managers.String;
        string msg = sm.GetString(StringID.ConfirmRestartStage);
        string confirm = sm.GetString(StringID.Yes);
        string cancel = sm.GetString(StringID.No);
        popup.SetData(msg, OnConfirm, OnBtnPopupClose, confirm, cancel);

        void OnConfirm()
        {
            _isRestarting = true;
            Managers.Game.RestartStage();
            Init();
            _isRestarting = false;
        }
    }

    void OnBtnPopupClose()
    {
        Managers.UI.PopupHandler.ClosePopup();
    }
    
    void OnBtnExit()
    {
        var popup = Managers.UI.PopupHandler.OpenPopup<UIConfirm>(PrefabID.UIConfirm);
        var sm = Managers.String;
        string msg = sm.GetString(StringID.ConfirmExitStage);
        string confirm = sm.GetString(StringID.Yes);
        string cancel = sm.GetString(StringID.No);
        popup.SetData(msg, OnConfirm, OnBtnPopupClose, confirm, cancel);

        void OnConfirm()
        {
            // TODO: 실제 씬 이름을 가져오도록 수정 필요
            // UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby Scene");
        }
    }
}
