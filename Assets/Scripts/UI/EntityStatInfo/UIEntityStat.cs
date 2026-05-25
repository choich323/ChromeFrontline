using TMPro;
using UnityEngine;
using DG.Tweening;

public class UIEntityStat : APopup
{
    private const int MULTIPLIER = 100;
    
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private TextMeshProUGUI _armorText;
    [SerializeField] private TextMeshProUGUI _attackText;
    [SerializeField] private TextMeshProUGUI _criticalText;
    [SerializeField] private TextMeshProUGUI _attackSpeedText;
    [SerializeField] private TextMeshProUGUI _moveSpeedText;
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private Vector2 _offset = new Vector2(20f, 20f);
    [SerializeField] private float _openDuration = 0.25f;
    
    private EntityInfo _entityInfo;

    public override void Init()
    {
        base.Init();
        
        _rectTransform.DOKill();
    }
    
    public void SetData(EntityInfo argEntityInfo)
    {
        _entityInfo = argEntityInfo;
        SetText();
    }

    void SetText()
    {
        var sm = Managers.String;
        _titleText.SetText(sm.GetString(StringID.EntityStat));
        _hpText.SetText($"{_entityInfo.hp}");
        _armorText.SetText($"{_entityInfo.armor}");
        _attackText.SetText($"{_entityInfo.attack}");
        _criticalText.SetText($"{_entityInfo.criticalChance * MULTIPLIER:F0}%");
        _attackSpeedText.SetText($"{_entityInfo.attackSpeed:F2}");
        _moveSpeedText.SetText($"{_entityInfo.moveSpeed:F2}");
    }

    public void SetPos(Vector2 argMousePos)
    {
        _rectTransform.position = argMousePos + _offset;
        
        ClampToScreen();
        
        _rectTransform.localScale = Vector3.zero;
        // SetEase(Ease.OutBack)을 쓰면 튕기는 느낌이 난다.
        _rectTransform.DOScale(1f, _openDuration).SetEase(Ease.OutBack).SetUpdate(true);
    }

    void ClampToScreen()
    {
        Vector3 pos = _rectTransform.position;
        Vector3[] corners = new Vector3[4];
        _rectTransform.GetWorldCorners(corners);

        Vector3 leftBottom = corners[0], rightTop = corners[2], rightBottom = corners[3];
        if (leftBottom.x < 0)
        {
            pos.x -= leftBottom.x;
        }
        else if (rightTop.x > Screen.width)
        {
            pos.x -= rightTop.x - Screen.width;
        }

        if (rightBottom.y < 0)
        {
            pos.y -= rightBottom.y;
        }
        else if (rightTop.y > Screen.height)
        {
            pos.y -= rightTop.y - Screen.height;
        }
        
        _rectTransform.position = pos;
    }
}
