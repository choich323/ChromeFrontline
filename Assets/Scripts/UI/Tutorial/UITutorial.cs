using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITutorial : MonoBehaviour
{
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
                targetPos = new Vector2(min.x - _xPadding, (min.y + max.y) * 0.5f);
                textRect.pivot = new Vector2(1f, 0.5f);
                break;
            case TutorialTextPosType.Right:
                targetPos = new Vector2(max.x + _xPadding, (min.y + max.y) * 0.5f);
                textRect.pivot = new Vector2(0f, 0.5f);
                break;
            case TutorialTextPosType.Top:
                targetPos = new Vector2((min.x + max.x) * 0.5f, max.y + _yPadding);
                textRect.pivot = new Vector2(0.5f, 0f);
                break;
            case TutorialTextPosType.Bottom:
                targetPos = new Vector2((min.x + max.x) * 0.5f, min.y - _yPadding);
                textRect.pivot = new Vector2(0.5f, 1f);
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
