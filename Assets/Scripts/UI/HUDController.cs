using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class HUDController : MonoBehaviour
{
    private const float HUNDRED_PERCENT  = 100f;
    private const float HOUR_TO_SECOND  = 3600f;
    private const float MINUTE_TO_SECOND  = 60f;
    
    [Header("Player HQ UI")] 
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private TextMeshProUGUI _hpText;

    [Header("Enemy HQ UI")] 
    [SerializeField] private Slider _enemyHpSlider;
    [SerializeField] private TextMeshProUGUI _enemyHpText;

    [Header("Capital UI")] 
    [SerializeField] private Image _hqImage;
    [SerializeField] private GameObject _hqIndicator;
    [SerializeField] private TextMeshProUGUI _goldText;
    
    [Header("Info UI")]
    [SerializeField] private TextMeshProUGUI _timerText;

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
    private int _lastTotalSeconds = -1;
    private Coroutine _menuAnimCoroutine;

    private bool IsPaused => Managers.Game.IsPaused;
    private SoundManager Sm => Managers.Sound;

    public void Init()
    {
        Clear();
        
        _hqBtn.onClick.AddListener(OnBtnHq);
        _optionBtn.onClick.AddListener(OnBtnOption);
        _restartBtn.onClick.AddListener(OnBtnReStart);
        _exitBtn.onClick.AddListener(OnBtnExit);

        var gf = Managers.Game.GameField;
        var playerHq = gf.PlayerHq;
        playerHq.OnGoldChanged -= UpdateGold;
        playerHq.OnGoldChanged += UpdateGold;
        playerHq.OnHealthChanged -= UpdatePlayerHp;
        playerHq.OnHealthChanged += UpdatePlayerHp;
        playerHq.OnTierChanged -= UpdateHqBtnImage;
        playerHq.OnTierChanged += UpdateHqBtnImage;
        gf.EnemyHq.OnHealthChanged -= UpdateEnemyHp;
        gf.EnemyHq.OnHealthChanged += UpdateEnemyHp;
        UpdateText();
        UpdateHqBtnImage(playerHq.Tier);
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
        
        UpdateTimer();
    }

    public void UpdateText()
    {
        UpdateTimer();
        UpdateGold();
        UpdatePlayerHp();
        UpdateEnemyHp();
    }

    void UpdatePlayerHp()
    {
        var hq = Managers.Game.GameField.PlayerHq;
        float hpRatio = hq.GetHqHpRatio();
        
        _hpSlider.value = hpRatio;
        _hpText.SetText("{0:0}%", hpRatio * HUNDRED_PERCENT);
    }
    
    void UpdatePlayerHp(int argHp, int argMaxHp)
    {
        var hpRatio = (float)argHp / argMaxHp;
        _hpSlider.value = hpRatio;
        _hpText.SetText("{0:0}%", hpRatio * HUNDRED_PERCENT);
    }

    void UpdateEnemyHp()
    {
        var hq = Managers.Game.GameField.EnemyHq;
        var hpRatio = hq.GetHqHpRatio();
        _enemyHpSlider.value = hpRatio;
        _enemyHpText.SetText("{0:0}%", hpRatio * HUNDRED_PERCENT);
    }
    
    void UpdateEnemyHp(int argHp, int argMaxHp)
    {
        var hpRatio = (float)argHp / argMaxHp;
        _enemyHpSlider.value = hpRatio;
        _enemyHpText.SetText("{0:0}%", hpRatio * HUNDRED_PERCENT);
    }
    
    void UpdateGold()
    {
        var curGold = Managers.Game.GameField.PlayerHq.Gold;
        UpdateGold(curGold);
    }

    void UpdateHqBtnImage(int argTier)
    {
        var info = Managers.Data.GetHeadQuarterUpgradeInfo(argTier);
        _hqImage.sprite = info.sprite;
    }
    
    void UpdateGold(long argGold)
    {
        _goldText.SetText("{0}", argGold);
        OnUpdateGold();
    }

    void OnUpdateGold()
    {
        if (Managers.UI.IsEnableHUDHqIndicator())
        {
            _hqIndicator.SetActive(true);
        }
        else
        {
            _hqIndicator.SetActive(false);
        }
    }

    void UpdateTimer()
    {
        var totalSeconds = Mathf.FloorToInt(Managers.Game.PlayTime);
        if (totalSeconds == _lastTotalSeconds)
        {
            return;
        }

        _lastTotalSeconds = totalSeconds;
        
        int hours = Mathf.FloorToInt(totalSeconds / HOUR_TO_SECOND);
        int minutes = Mathf.FloorToInt((totalSeconds % HOUR_TO_SECOND) / MINUTE_TO_SECOND);
        int seconds = Mathf.FloorToInt(totalSeconds % MINUTE_TO_SECOND);

        if (hours > 0)
        {
            _timerText.SetText("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }
        else
        {
            _timerText.SetText("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void OnBtnHq()
    {
        Sm.PlaySelectSfx();
        
        var popup = Managers.UI.PopupHandler.OpenPopup<UIHqManagement>(PrefabID.UIHqManagement);
        popup.SetOnClose(OnBtnPopupClose);
    }

    void OnBtnMenu()
    {
        if (_menuAnimCoroutine != null)
        {
            StopCoroutine(_menuAnimCoroutine);
        }

        Managers.Sound.PlaySelectSfx();
        
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
        Sm.PlaySelectSfx();
        
        var popup = Managers.UI.PopupHandler.OpenPopup<UIOption>(PrefabID.UIOption);
        popup.SetOnClose(OnBtnPopupClose);
    }
    
    void OnBtnReStart()
    {
        Sm.PlaySelectSfx();
        
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
        Sm.PlaySelectSfx();
        Managers.UI.PopupHandler.ClosePopup();
    }
    
    void OnBtnExit()
    {
        Sm.PlaySelectSfx();
        
        var popup = Managers.UI.PopupHandler.OpenPopup<UIConfirm>(PrefabID.UIConfirm);
        var sm = Managers.String;
        string msg = sm.GetString(StringID.ConfirmExitStage);
        string confirm = sm.GetString(StringID.Yes);
        string cancel = sm.GetString(StringID.No);
        popup.SetData(msg, OnConfirm, OnBtnPopupClose, confirm, cancel);

        void OnConfirm()
        {
            Managers.Game.Exit();
        }
    }
}
