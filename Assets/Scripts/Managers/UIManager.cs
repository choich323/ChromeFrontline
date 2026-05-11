using UnityEngine;


public class UIManager : MonoBehaviour
{
    [SerializeField] private RectTransform _hudParent;
    [SerializeField] private RectTransform _popupParent;
    [SerializeField] private RectTransform _pauseBtnParent;

    private PopupHandler _popupHandler;
    private HUDController _topHUDController;
    private UIPauseBtn _pauseBtn;
    
    public PopupHandler PopupHandler => _popupHandler;
    
    public void Init()
    {
        CreatePopupHandler();
        CreateTopHUD();
    }

    void Update()
    {
        _popupHandler.OnUpdate();
    }
    
    void CreatePopupHandler()
    {
        _popupHandler = new PopupHandler();
        _popupHandler.Init();
    }
    
    void CreateTopHUD()
    {
        CreatTopHUD();
        CreatePauseBtn();
    }

    void CreatTopHUD()
    {
        var obj = InstantiateUIWithoutPool(PrefabID.UITopHUDPanel);
        if (obj == null)
        {
            Debug.LogError("top HUD instantiate failed");
            return;
        }
        _topHUDController = obj.GetComponent<HUDController>();
        var hudTransform = _topHUDController.transform;
        hudTransform.SetParent(_hudParent);
        var rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        _topHUDController.Init();
    }

    void CreatePauseBtn()
    {
        var obj = InstantiateUIWithoutPool(PrefabID.UIPauseBtn);
        if (obj == null)
        {
            Debug.LogError("pause btn instantiate failed");
            return;
        }
        _pauseBtn = obj.GetComponent<UIPauseBtn>();
        var btnTransform = _pauseBtn.transform;
        btnTransform.SetParent(_pauseBtnParent);
        var rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        _pauseBtn.Init();
    }

    public void RefreshUI()
    {
        UpdateTopHUDText();
    }
    
    void UpdateTopHUDText()
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
    }
}
