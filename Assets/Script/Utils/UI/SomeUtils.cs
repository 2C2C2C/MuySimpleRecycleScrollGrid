using UnityEngine;

public static class SomeUtils
{
    // anchoredPosition =localPosition -（anchorMinPos + anchorSize * pivot ）
    /// <summary>
    /// get the (actual screen)position? from a rect transfrom 
    /// </summary>
    /// <param name="rectTransform"></param>
    /// <param name="pivot"></param>
    /// <returns></returns>
    public static Vector2 GetRectTransPivotPosition(this RectTransform rectTransform, Vector2 positionPivot)
    {
        Rect rect = rectTransform.rect;
        Vector2 actualCenterPosition = rectTransform.position;
        actualCenterPosition.x += rect.center.x;
        actualCenterPosition.y += rect.center.y;

        Vector2 result = actualCenterPosition;
        result.x += (positionPivot.x - 0.5f) * rect.size.x;
        result.y += (positionPivot.y - 0.5f) * rect.size.y;

        return result;
    }

    public static bool Contains(this Rect self, Rect other)
    {
        // HACK the x,y is left top of the rect
        bool result = (self.x <= other.x && self.xMax + self.width >= other.x + other.width);
        result &= (self.y >= other.y && self.y - self.height <= other.y - other.height);
        return result;
    }
}
