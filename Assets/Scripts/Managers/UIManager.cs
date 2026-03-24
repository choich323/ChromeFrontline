using UnityEngine;


public class UIManager : MonoBehaviour
{
    [SerializeField] private RectTransform _hudParent;
    [SerializeField] private RectTransform _popupParent;

    private PopupHandler _popupHandler;
    private HUDController _topHUDController;
    
    public PopupHandler PopupHandler => _popupHandler;
    
    public void Init()
    {
        CreatePopupHandler();
        CreateHUD();
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
    
    void CreateHUD()
    {
        var obj = InstantiateUIWithoutPool(PrefabID.UITopHUDPanel);
        if (obj == null)
        {
            Debug.LogError("instantiate failed");
            return;
        }
        _topHUDController = obj.GetComponent<HUDController>();
        var hudTransform = _topHUDController.transform;
        hudTransform.SetParent(_hudParent);
        hudTransform.localPosition = Vector3.zero;
        hudTransform.localScale = Vector3.one;
        var rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        _topHUDController.Init();
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
