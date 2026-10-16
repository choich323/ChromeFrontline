using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIIngameSkillButton : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private const int INVALID_ID = 0;
    
    [SerializeField] private Image _icon;
    [SerializeField] private Button _button;
    [SerializeField] private Image _cooldown;
    [SerializeField] private Image _durationBorder;
    [SerializeField] private GameObject _lock;
    [SerializeField] private GameObject _unlock;

    private int _id;
    private SkillInfo _skillInfo;
    private SkillRange _skillRange;
    private readonly Plane _plane = new Plane(Vector3.forward, Vector3.zero);
    private Coroutine _cooldownCoroutine;
    private Coroutine _durationBorderCoroutine;
    
    public void Init(int argSkillId)
    {
        _id = argSkillId;

        _skillInfo = Managers.Data.GetSkillInfo(argSkillId);
        if (_skillInfo == null)
        {
            SetLock();
            return;
        }
        
        SetUnlock();
        SetIcon(_skillInfo.icon);
    }

    public void OnPointerDown(PointerEventData argEventData)
    {
        if (_skillInfo.rangeData.type == RangeType.None || _cooldownCoroutine != null)
        {
            return;
        }
        
        CreateSkillRange();
        var team = _skillInfo.targetType == TargetType.Ally ? Team.Enemy : Team.Player;
        var spawnerPos = Managers.Game.GameField.GetHq(team).Spawner.transform.position;
        spawnerPos.y = 0;
        bool isHQType = _skillInfo.rangeData.type == RangeType.HQ;
        Vector2 pos = isHQType ? spawnerPos : argEventData.position;
        UpdateSkillRangePosition(pos, !isHQType);
    }

    public void OnDrag(PointerEventData argEventData)
    {
        var rangeType = _skillInfo.rangeData.type;
        if (rangeType == RangeType.None || rangeType == RangeType.HQ || _cooldownCoroutine != null)
        {
            return;
        }
        
        UpdateSkillRangePosition(argEventData.position, true);
    }
    
    public void OnPointerUp(PointerEventData argEventData)
    {
        if (_skillInfo.type == SkillType.Passive)
        {
            return;
        }
        
        if (Managers.Game.OnIngameSkillBtn(_id, argEventData.position))
        {
            if (_skillInfo.rangeData.type != RangeType.None)
            {
                DestroySkillRange();
            }
            
            var info = Managers.Data.GetSkillInfo(_id);
            var level = Managers.Game.UserRecord.GetSkillLevel(_id);
            var cool = info.GetCooldown(level);

            if (_cooldownCoroutine != null)
            {
                StopCoroutine(_cooldownCoroutine);
            }
            _cooldownCoroutine = StartCoroutine(CoSetFillAmount(_cooldown, cool, () => _cooldownCoroutine = null));
            
            var duration = info.GetDuration(level);
            if (duration > 0)
            {
                if (_durationBorderCoroutine != null)
                {
                    StopCoroutine(_durationBorderCoroutine);
                }
                _durationBorderCoroutine = StartCoroutine(CoSetFillAmount(_durationBorder, duration, () => _durationBorderCoroutine = null)); 
            }
        }
    }

    IEnumerator CoSetFillAmount(Image argImage, float argValue, Action argCallback)
    {
        if (argValue <= 0)
        {
            argImage.fillAmount = 0f;
            argCallback?.Invoke();
            yield break;
        }
        
        var startTime = Time.time;
        var targetTime = startTime + argValue;
        argImage.fillAmount = 1f;
        
        while (Time.time < targetTime)
        {
            argImage.fillAmount = (targetTime - Time.time) / argValue;
            yield return null;
        }

        argImage.fillAmount = 0f;
        argCallback?.Invoke();
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
    
    void UpdateSkillRangePosition(Vector3 argPos, bool argIsUI)
    {
        if (argIsUI)
        {
            var cam = Managers.CamController.Cam;
            Ray ray = cam.ScreenPointToRay(argPos);
            if (!_plane.Raycast(ray, out float distance))
            {
                return;
            }
        
            var worldPos = ray.GetPoint(distance);
            _skillRange.transform.position = worldPos;
        }
        else
        {
            _skillRange.transform.position = argPos;
        }
    }
    
    void DestroySkillRange()
    {
        if (_skillRange == null)
        {
            return;
        }

        OnDestroySkillRange(_skillRange);
        _skillRange = null;
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
        _skillInfo = null;

        if (_cooldownCoroutine != null)
        {
            StopCoroutine(_cooldownCoroutine);
            _cooldownCoroutine = null;
        }

        if (_durationBorderCoroutine != null)
        {
            StopCoroutine(_durationBorderCoroutine);
            _durationBorderCoroutine = null;
        }
        
        _cooldown.fillAmount = 0;
        _durationBorder.fillAmount = 0;

        DestroySkillRange();
        SetLock();
    }
}
