using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    [SerializeField] private RectTransform _hudParent;
    [SerializeField] private RectTransform _popupParent;
    [SerializeField] private RectTransform _pauseBtnParent;
    [SerializeField] private RectTransform _damageTextParent;
    [SerializeField] private CanvasGroup _fadeCanvasGroup;
    [SerializeField] private GameObject _inputBlocker;
    [SerializeField] private Button _dialogInputBtn;
    [SerializeField] private UIDialog _dialogBox;

    private PopupHandler _popupHandler;
    private HUDController _topHUDController;
    private UIPauseBtn _pauseBtn;
    private UIGameSpeedBtn _gameSpeedBtn;
    private bool _isProduceIndicatorEnable;
    private bool _isHqUpgradeIndicatorEnable;
    private bool _isShowingDialog;
    private bool _isWaitingInput;
    private List<UIDamageText> _damageTextList = new List<UIDamageText>();
    private DialogHandler _dialogHandler;
    private Dictionary<string, List<Button>> _identifierButtons = new Dictionary<string, List<Button>>();
    
    public PopupHandler PopupHandler => _popupHandler;
    public DialogHandler DialogHandler => _dialogHandler;
    public bool IsShowingDialog => _isShowingDialog;
    
    public void Init()
    {
        ActiveInputBlocker(false);
        CreatePopupHandler();
        CreateDialogHandler();

        _dialogInputBtn.onClick.RemoveAllListeners();
        _dialogInputBtn.onClick.AddListener(OnClickDialog);
        _dialogInputBtn.gameObject.SetActive(false);
        _dialogBox.gameObject.SetActive(false);
    }

    void Update()
    {
        if (_isShowingDialog)
        {
            return;
        }
        
        if (_popupHandler != null)
        {
            _popupHandler.OnUpdate();
        }
    }

    public Tween FadeOut(float argDuration = 0.5f)
    {
        _fadeCanvasGroup.gameObject.SetActive(true);
        _fadeCanvasGroup.blocksRaycasts = true;
        
        return _fadeCanvasGroup.DOFade(1f, argDuration).SetUpdate(true);
    }

    public Tween FadeIn(float argDuration = 0.5f)
    {
        return _fadeCanvasGroup.DOFade(0f, argDuration).SetUpdate(true).OnComplete(()=>
        {
            _fadeCanvasGroup.blocksRaycasts = false;
            _fadeCanvasGroup.gameObject.SetActive(false);
        });
    }

    public void ActiveInputBlocker(bool argIsActive)
    {
        if (_inputBlocker != null)
        {
            _inputBlocker.SetActive(argIsActive);
        }
    }
    
    void CreatePopupHandler()
    {
        _popupHandler = new PopupHandler();
        _popupHandler.Init();
    }

    void CreateDialogHandler()
    {
        _dialogHandler = new();
        _dialogHandler.Init();
    }
    
    public void CreateTopHUD()
    {
        var obj = InstantiateUIWithoutPool(PrefabID.UIHUDPanel);
        if (obj == null)
        {
            Debug.LogError("top HUD instantiate failed");
            return;
        }
        _topHUDController = obj.GetComponent<HUDController>();
        var hudTransform = _topHUDController.transform;
        hudTransform.SetParent(_hudParent, false);
        _topHUDController.gameObject.SetActive(false);
        
        CreatePlayBtnGroup();
    }

    public void OnEnterStage(string argStageName)
    {
        _topHUDController.gameObject.SetActive(true);
        _topHUDController.Run(argStageName);
        RefreshUI();
        
        _pauseBtn.gameObject.SetActive(true);
        _gameSpeedBtn.gameObject.SetActive(true);
    }

    public void OnExitStage()
    {
        _topHUDController.gameObject.SetActive(false);
        _pauseBtn.gameObject.SetActive(false);
        _gameSpeedBtn.gameObject.SetActive(false);

        DestroyAllDamageText();
    }

    void CreatePlayBtnGroup()
    {
        var obj = InstantiateUIWithoutPool(PrefabID.UIPlayBtnGroup);
        if (obj == null)
        {
            Debug.LogError("pause btn instantiate failed");
            return;
        }
        _pauseBtn = obj.GetComponent<UIPauseBtn>();
        var btnTransform = _pauseBtn.transform;
        btnTransform.SetParent(_pauseBtnParent, false);
        _pauseBtn.Init();
        _pauseBtn.gameObject.SetActive(false);
        
        _gameSpeedBtn = obj.GetComponent<UIGameSpeedBtn>();
        _gameSpeedBtn.Init();
        _gameSpeedBtn.gameObject.SetActive(false);
    }

    public void RefreshUI()
    {
        RefreshTopHUDText();
        _gameSpeedBtn.Reset();
    }
    
    void RefreshTopHUDText()
    {
        _topHUDController.UpdateText();
    }
    
    public GameObject InstantiateUIWithoutPool(PrefabID argPrefabID)
    {
        Managers.Data.TryGetPrefabInfo((int)argPrefabID, out var info);
        return Instantiate(info.prefab);
    }
    
    public void AttachToPopupParent(RectTransform argTarget)
    {
        argTarget.SetParent(_popupParent);
        argTarget.localScale = Vector3.one;
        argTarget.localPosition = Vector3.zero;
        argTarget.SetAsLastSibling();
    }
    
    public bool IsProduceIndicatorEnable()
    {
        var spawner = Managers.Game.GameField.PlayerHq.GetSpawner();
        var enumerator = spawner.GetSlotEnumerator();
        while(enumerator.MoveNext())
        {
            var slot = enumerator.Current;
            if (slot != null && slot.GetTargetId() == PrefabID.None)
            {
                return _isProduceIndicatorEnable = true;
            }
        }

        return _isProduceIndicatorEnable = false;
    }

    public bool IsHqUpgradeIndicatorEnable()
    {
        var playerHq = Managers.Game.GameField.PlayerHq;
        var tier = playerHq.Tier;
        var info = Managers.Data.GetHeadQuarterUpgradeInfo(tier + 1);
        if (info == null)
        {
            return _isHqUpgradeIndicatorEnable = false;
        }
        else
        {
            return _isHqUpgradeIndicatorEnable = info.upgradeCost <= playerHq.Gold;
        }
    }
    
    public bool IsEnableHUDHqIndicator()
    {
        return IsProduceIndicatorEnable() || IsHqUpgradeIndicatorEnable();
    }

    public void CreateDamageText(Vector3 argPos, float argDamage, bool argIsCritical, Team argTeam)
    {
        var obj = Managers.Pool.Instantiate(PrefabID.UIDamageText);
        obj.transform.SetParent(_damageTextParent, false);
        obj.transform.position = argPos;
        var dt = obj.GetComponent<UIDamageText>();
        _damageTextList.Add(dt);
        
        dt.PlayAnimation(argDamage, argIsCritical, argTeam, DestroyDamageText);
    }

    void DestroyDamageText(UIDamageText argDamageText)
    {
        _damageTextList.Remove(argDamageText);
        Managers.Pool.Destroy(argDamageText, PrefabID.UIDamageText);
    }

    void DestroyAllDamageText()
    {
        foreach (var dt in _damageTextList)
        {
            Managers.Pool.Destroy(dt, PrefabID.UIDamageText);
        }
        _damageTextList.Clear();
    }
    
    public void ShowDialog(string argDialogInfoId, Action argCallback = null)
    {
        var data = Managers.Data.GetDialogData();
        if (data == null)
        {
            return;
        }
        
        var info = data.GetDialogInfo(argDialogInfoId);
        if (info == null)
        {
            return;
        }

        StartCoroutine(CoShowDialog(info.dialogList, argCallback));
    }

    IEnumerator CoShowDialog(List<Dialog> argDialogList, Action argCallback = null)
    {
        while (_isShowingDialog)
        {
            yield return null;
        }
        
        Managers.Game.PauseGame();
        
        _isShowingDialog = true;
        _dialogInputBtn.gameObject.SetActive(true);
        _dialogBox.gameObject.SetActive(true);
        
        foreach (var dialog in argDialogList)
        {
            _isWaitingInput = true;
            
            var talker = Managers.Language.GetLocalizedString(dialog.talker);
            var text = Managers.Language.GetLocalizedString(dialog.text);
            
            _dialogBox.ShowText(talker, text);

            while (_isWaitingInput)
            {
                yield return null;
            }
        }
        
        _dialogInputBtn.gameObject.SetActive(false);
        _dialogBox.gameObject.SetActive(false);
        _isShowingDialog = false;
        
        Managers.Game.ResumeGame();
        
        argCallback?.Invoke();
    }

    void OnClickDialog()
    {
        _dialogBox.OnClickDialog(Callback);

        void Callback(bool argIsTyping)
        {
            if (!argIsTyping)
            {
                _isWaitingInput = false;
            }
        }
    }

    public void AddIdentifierButton(string argId, Button argButton)
    {
        _identifierButtons[argId].Add(argButton);
    }

    public void RemoveIdentifierButton(string argId, Button argButton)
    {
        if (!_identifierButtons.ContainsKey(argId))
        {
            return;
        }
        
        _identifierButtons[argId].Remove(argButton);
    }
}
