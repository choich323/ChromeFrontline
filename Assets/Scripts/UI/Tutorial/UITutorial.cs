using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITutorial : MonoBehaviour
{
    private const float HALF = 0.5f;
    private const float ZERO = 0f;
    private const float ONE = 1f;
    
    [SerializeField] private UITutorialDimmer _dimmer;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private float _xPadding = 50f;
    [SerializeField] private float _yPadding = 50f;

    public void SetHole(List<RectTransform> argHoleList, string argText, TutorialTextPosType argPosType)
    {
        _dimmer.SetTargets(argHoleList);
        _text.SetText(argText);
        SetTextPos(argHoleList, argPosType);
    }

    void SetTextPos(List<RectTransform> argHoleList, TutorialTextPosType argPosType)
    {
        _text.gameObject.SetActive(true);
        
        var min = new Vector2(float.MaxValue, float.MaxValue);
        var max = new Vector2(float.MinValue, float.MinValue);

        foreach (var hole in argHoleList)
        {
            if (hole == null)
                continue;

            var corners = new Vector3[4];
            hole.GetWorldCorners(corners);

            for (int i = 0; i < corners.Length; i++)
            {
                var point = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }
        }

        Vector2 targetPos;
        var textRect = _text.rectTransform;

        switch (argPosType)
        {
            case TutorialTextPosType.Left:
                targetPos = new Vector2(min.x - _xPadding, (min.y + max.y) * HALF);
                textRect.pivot = new Vector2(ONE, HALF);
                break;
            case TutorialTextPosType.Right:
                targetPos = new Vector2(max.x + _xPadding, (min.y + max.y) * HALF);
                textRect.pivot = new Vector2(ZERO, HALF);
                break;
            case TutorialTextPosType.Top:
                targetPos = new Vector2((min.x + max.x) * HALF, max.y + _yPadding);
                textRect.pivot = new Vector2(HALF, ZERO);
                break;
            case TutorialTextPosType.Bottom:
                targetPos = new Vector2((min.x + max.x) * HALF, min.y - _yPadding);
                textRect.pivot = new Vector2(HALF, ONE);
                break;
            default:
                _text.gameObject.SetActive(false);
                return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            textRect.parent as RectTransform,
            targetPos,
            null,
            out var localPos);
        
        textRect.anchoredPosition = localPos;
    }
}
