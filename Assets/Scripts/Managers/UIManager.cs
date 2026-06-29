using DG.Tweening;
using UnityEngine;


public class UIManager : MonoBehaviour
{
    [SerializeField] private RectTransform _hudParent;
    [SerializeField] private RectTransform _popupParent;
    [SerializeField] private RectTransform _pauseBtnParent;
    [SerializeField] private CanvasGroup _fadeCanvasGroup;
    [SerializeField] private GameObject _inputBlocker;

    private PopupHandler _popupHandler;
    private HUDController _topHUDController;
    private UIPauseBtn _pauseBtn;
    private UIGameSpeedBtn _gameSpeedBtn;
    private bool _isProduceIndicatorEnable;
    private bool _isHqUpgradeIndicatorEnable;
    
    public PopupHandler PopupHandler => _popupHandler;
    
    public void Init()
    {
        ActiveInputBlocker(false);
        CreatePopupHandler();
        CreateTopHUD();
    }

    void Update()
    {
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
    
    void CreateTopHUD()
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
}
