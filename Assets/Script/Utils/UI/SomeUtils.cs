using UnityEngine;

public static class SomeUtils
{
    public enum UIOffsetType
    {
        Center = 0,

        Top = 1,
        Bottom = 2,
        Left = 3,
        Right = 4,

        TopLeft = 5,
        TopRight = 6,

        BottomLeft = 7,
        BottomRight = 8,
    }

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
        bool result = self.Contains(other.min) && self.Contains(other.max);
        return result;
    }

    public static Vector3 GetOffsetPostion(this RectTransform target, UIOffsetType offsetType)
    {
        Vector3 result = target.position;
        Vector2 targetSize = target.sizeDelta;
        Vector2 targetPivot = target.pivot;

        Vector2 pivotOffset = Vector2.one * 0.5f - targetPivot;
        pivotOffset.x *= targetSize.x;
        pivotOffset.y *= targetSize.y;

        result += (Vector3)pivotOffset;

        switch (offsetType)
        {
            case UIOffsetType.Top:
                result.y += targetSize.y * 0.5f;
                break;
            case UIOffsetType.Bottom:
                result.y -= targetSize.y * 0.5f;
                break;
            case UIOffsetType.Left:
                result.x -= targetSize.x * 0.5f;
                break;
            case UIOffsetType.Right:
                result.x += targetSize.x * 0.5f;
                break;

            case UIOffsetType.TopLeft:
                result.y += targetSize.y * 0.5f;
                result.x -= targetSize.x * 0.5f;
                break;
            case UIOffsetType.TopRight:
                result.y += targetSize.y * 0.5f;
                result.x += targetSize.x * 0.5f;
                break;

            case UIOffsetType.BottomLeft:
                result.y -= targetSize.y * 0.5f;
                result.x -= targetSize.x * 0.5f;
                break;

            case UIOffsetType.BottomRight:
                result.y -= targetSize.y * 0.5f;
                result.x += targetSize.x * 0.5f;
                break;

            default:
                break;
        }

        return result;
    }

    public static Vector3 GetOffsetLocalPosition(this RectTransform target, UIOffsetType offsetType)
    {
        Vector3 result = Vector3.zero;
        Vector2 targetSize = target.rect.size;
        Vector2 targetPivot = target.pivot;

        Vector2 pivotOffset = Vector2.one * 0.5f - targetPivot;
        pivotOffset.x *= targetSize.x;
        pivotOffset.y *= targetSize.y;

        result += (Vector3)pivotOffset;

        switch (offsetType)
        {
            case UIOffsetType.Top:
                result.y += targetSize.y * 0.5f;
                break;
            case UIOffsetType.Bottom:
                result.y -= targetSize.y * 0.5f;
                break;
            case UIOffsetType.Left:
                result.x -= targetSize.x * 0.5f;
                break;
            case UIOffsetType.Right:
                result.x += targetSize.x * 0.5f;
                break;

            case UIOffsetType.TopLeft:
                result.y += targetSize.y * 0.5f;
                result.x -= targetSize.x * 0.5f;
                break;
            case UIOffsetType.TopRight:
                result.y += targetSize.y * 0.5f;
                result.x += targetSize.x * 0.5f;
                break;

            case UIOffsetType.BottomLeft:
                result.y -= targetSize.y * 0.5f;
                result.x -= targetSize.x * 0.5f;
                break;

            case UIOffsetType.BottomRight:
                result.y -= targetSize.y * 0.5f;
                result.x += targetSize.x * 0.5f;
                break;

            default:
                break;
        }

        return result;
    }

    /// <summary>
    /// target component should inherit from ISetupable
    /// </summary>
    /// <param name="component"></param>
    /// <param name="data"></param>
    /// <typeparam name="TData"></typeparam>
    public static void ISetup<TComponent, TData>(this Component component, TData data) where TComponent : Component
    {
        if (component is ISetupable<TData> target)
        {
            target.Setup(data);
            return;
        }
        UnityEngine.Debug.LogError($"component_{component.name} is not a setupable");
    }

}
