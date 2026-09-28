using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIIngameSkillButton : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private const int INVALID_ID = 0;
    private const int INVALID_SKILL_LEVEL = 0;
    
    [SerializeField] private Image _icon;
    [SerializeField] private Button _button;
    [SerializeField] private Image _cooldown;
    [SerializeField] private GameObject _lock;
    [SerializeField] private GameObject _unlock;

    private int _id;
    private int _skillLevel;
    private SkillInfo _skillInfo;
    private SkillRange _skillRange;
    private readonly Plane _plane = new Plane(Vector3.forward, Vector3.zero);
    
    public void Init(int argSkillId, int argSkillLevel)
    {
        _id = argSkillId;
        _skillLevel = argSkillLevel;

        _skillInfo = Managers.Data.GetSkillInfo(argSkillId);
        if (_skillInfo == null)
        {
            SetLock();
            return;
        }
        
        SetUnlock();
        SetIcon(_skillInfo.icon);
    }

    public void OnDrag(PointerEventData argEventData)
    {
        UpdateSkillRangePosition(argEventData);
    }

    public void OnPointerDown(PointerEventData argEventData)
    {
        CreateSkillRange();
        
        UpdateSkillRangePosition(argEventData);
    }

    public void OnPointerUp(PointerEventData argEventData)
    {
        DestroySkillRange();
    }

    void SetIcon(Sprite argIcon)
    {
        _icon.sprite = argIcon;
    }

    void CreateSkillRange()
    {
        var obj = Managers.Pool.Instantiate(PrefabID.SkillRange);
        obj.transform.SetParent(Managers.Game.GameField.SkillRangeParent, true);
        _skillRange = obj.GetComponent<SkillRange>();
        
        var container = new SkillRangeContainer();
        container.type = _skillInfo.rangeData.type;
        container.radius = _skillInfo.rangeData.radius;
        container.vertexList = _skillInfo.rangeData.vertexList;
        _skillRange.Init(container);
    }
    
    void UpdateSkillRangePosition(PointerEventData argEventData)
    {
        var cam = Managers.CamController.Cam;
        Ray ray = cam.ScreenPointToRay(argEventData.position);
        if (!_plane.Raycast(ray, out float distance))
        {
            return;
        }
        
        var worldPos = ray.GetPoint(distance);
        _skillRange.transform.position = worldPos;
    }
    
    // temp code
    void DestroySkillRange()
    {
        _skillRange.Destroy(OnDestroySkillRange);
    }

    void OnDestroySkillRange(SkillRange argSkillRange)
    {
        Managers.Pool.Destroy(argSkillRange, PrefabID.SkillRange);
    }
    
    void SetLock()
    {
        _lock.SetActive(true);
        _unlock.SetActive(false);
        _button.interactable = false;
    }

    void SetUnlock()
    {
        _lock.SetActive(false);
        _unlock.SetActive(true);
        _button.interactable = true;
    }

    public void Clear()
    {
        _id = INVALID_ID;
        _skillLevel = INVALID_SKILL_LEVEL;
        _skillInfo = null;
        
        SetLock();
    }
}
