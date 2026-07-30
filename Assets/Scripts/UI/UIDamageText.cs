using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class UIDamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private float _criticalFontSizeRatio = 1.3f;
    [SerializeField] private float _originalFontSize;
    [SerializeField] private float _damageTextXOffset = 30f;
    [SerializeField] private float _damageTextYOffset = 75f;
    [SerializeField] private Color _playerDamagedColor;
    [SerializeField] private Color _playerCriticalColor;
    [SerializeField] private Color _enemyDamagedColor;
    [SerializeField] private Color _enemyCriticalColor;
    
    [Header("Animation Settings")]
    [SerializeField] private float _floatHeight = 25f;
    [SerializeField] private float _durationMin = 0.5f;
    [SerializeField] private float _durationMax = 1f;
    
    public void PlayAnimation(float argDamage, bool argIsCritical, Team argTeam, Action<UIDamageText> argOnComplete)
    {
        Clear();
        
        float offsetX = UnityEngine.Random.Range(-_damageTextXOffset, _damageTextXOffset);
        float offsetY = UnityEngine.Random.Range(0, _damageTextYOffset);
        transform.position += new Vector3(offsetX, offsetY, 0f);
        
        _text.text = Mathf.RoundToInt(argDamage).ToString();
        
        Color targetColor = GetDamageColor(argIsCritical, argTeam);
        targetColor.a = 1f;
        _text.color = targetColor;
        if (argIsCritical)
        {
            _text.fontSize *= _criticalFontSizeRatio;
        }

        var duration = UnityEngine.Random.Range(_durationMin, _durationMax);
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMoveY(transform.position.y + _floatHeight, duration).SetEase(Ease.OutCubic));
        seq.Join(_text.DOFade(0f, duration).SetEase(Ease.InExpo));
        
        seq.OnComplete(() => OnComplete(argOnComplete));
    }

    Color GetDamageColor(bool argIsCritical, Team argTeam)
    {
        if (argTeam == Team.Player)
        {
            return argIsCritical ? _playerCriticalColor : _playerDamagedColor;
        }

        return argIsCritical ? _enemyCriticalColor : _enemyDamagedColor;
    }

    void OnComplete(Action<UIDamageText> argOnComplete)
    {
        Clear();
        
        argOnComplete?.Invoke(this);
    }

    void Clear()
    {
        _text.text = string.Empty;
        _text.fontSize = _originalFontSize;
    }
}
