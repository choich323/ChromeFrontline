using UnityEngine;
using UnityEngine.UI;

public static class ScrollRectExtension
{
    public static void Focus(this ScrollRect argScrollRect, RectTransform argTarget)
    {
        RectTransform content = argScrollRect.content;
        RectTransform viewport = argScrollRect.viewport;

        // 타겟의 중앙 위치와 뷰포트의 중앙을 계산
        float y = -argTarget.anchoredPosition.y + (argTarget.rect.height * 0.5f) - (viewport.rect.height * 0.5f) ;

        // 최상단, 최하단인 경우 처리
        float maxY = Mathf.Max(0f, content.rect.height - viewport.rect.height);
        y = Mathf.Clamp(y, 0f, maxY);

        // 적용
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, y);
    }
}
