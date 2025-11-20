using System.Collections.Generic;

namespace UnityEngine.UI.Extend
{
    public static partial class RectTransformEx
    {
        public static void ConvertToAnchorMode(this RectTransform self)
        {
            RectTransform parent = self.parent as RectTransform;
            if (parent == null)
            {
                return;
            }

            Vector2 anchorMin = new Vector2(self.anchorMin.x + self.offsetMin.x / parent.rect.width,
                self.anchorMin.y + self.offsetMin.y / parent.rect.height);

            Vector2 anchorMax = new Vector2(self.anchorMax.x + self.offsetMax.x / parent.rect.width,
                self.anchorMax.y + self.offsetMax.y / parent.rect.height);

            self.anchorMin = anchorMin;
            self.anchorMax = anchorMax;
            self.offsetMin = self.offsetMax = Vector2.zero;
        }

        public static void SetSizeAsParent(this RectTransform rectTransform)
        {
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.localPosition = Vector3.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
        }

        public static bool TryCalculateLocalPositionInAnotherRect(this Transform self, RectTransform target, Camera uiCamera, out Vector2 localPosition)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, self.position);
            bool result = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    target,
                    screenPoint,
                    uiCamera,
                    out localPosition);
            return result;
        }

        public static Vector3 ScreenPointToUIWorldPosition(Vector2 screenPoint, Canvas rootCanvas, Camera uiCamera)
        {
            switch (rootCanvas.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    return screenPoint;
                case RenderMode.ScreenSpaceCamera:
                    RectTransformUtility.ScreenPointToWorldPointInRectangle(rootCanvas.transform as RectTransform, screenPoint, uiCamera, out Vector3 result);
                    return result;
                default:
                    break;
            }
            return Vector2.zero;
        }

        public static bool IsFullyInsideTargetRect(RectTransform rectTransform, RectTransform targetRect)
        {
            Vector3 checkWorldPoint = TransformNormalizedRectPositionToWorldPosition(rectTransform, Vector2.zero); // bottom left
            bool bottomLeftOut = !RectTransformUtility.RectangleContainsScreenPoint(targetRect, checkWorldPoint);

            checkWorldPoint = TransformNormalizedRectPositionToWorldPosition(rectTransform, Vector2.right); // bottom right
            bool bottomRightOut = !RectTransformUtility.RectangleContainsScreenPoint(targetRect, checkWorldPoint);

            checkWorldPoint = TransformNormalizedRectPositionToWorldPosition(rectTransform, Vector2.up); // top left
            bool topLeftOut = !RectTransformUtility.RectangleContainsScreenPoint(targetRect, checkWorldPoint);

            checkWorldPoint = TransformNormalizedRectPositionToWorldPosition(rectTransform, Vector2.one); // top right
            bool topRightOut = !RectTransformUtility.RectangleContainsScreenPoint(targetRect, checkWorldPoint);

            return !(bottomLeftOut || bottomRightOut || topLeftOut || topRightOut);
        }

        public static bool IsNotIntersetedWithTargetRect(RectTransform rectTransform, RectTransform targetRect)
        {
            Vector3 checkWorldPoint = TransformNormalizedRectPositionToWorldPosition(rectTransform, Vector2.zero); // bottom left
            bool bottomLeftOut = !RectTransformUtility.RectangleContainsScreenPoint(targetRect, checkWorldPoint);

            checkWorldPoint = TransformNormalizedRectPositionToWorldPosition(rectTransform, Vector2.right); // bottom right
            bool bottomRightOut = !RectTransformUtility.RectangleContainsScreenPoint(targetRect, checkWorldPoint);

            checkWorldPoint = TransformNormalizedRectPositionToWorldPosition(rectTransform, Vector2.up); // top left
            bool topLeftOut = !RectTransformUtility.RectangleContainsScreenPoint(targetRect, checkWorldPoint);

            checkWorldPoint = TransformNormalizedRectPositionToWorldPosition(rectTransform, Vector2.one); // top right
            bool topRightOut = !RectTransformUtility.RectangleContainsScreenPoint(targetRect, checkWorldPoint);

            return bottomLeftOut && bottomRightOut && topLeftOut && topRightOut;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="normalizedRectPosition"> Bottom left as (0,0) </param>
        /// <returns></returns>
        public static Vector3 TransformNormalizedRectPositionToWorldPosition(this RectTransform rectTransform, Vector2 normalizedRectPosition)
        {
            Vector2 localPos = TransformNormalizedRectPositionToLocalPosition(rectTransform, normalizedRectPosition);
            return rectTransform.TransformPoint(localPos);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="normalizedRectPosition"> Bottom left as (0,0) </param>
        /// <returns></returns>
        public static Vector2 TransformNormalizedRectPositionToLocalPosition(this RectTransform rectTransform, Vector2 normalizedRectPosition)
        {
            Vector2 rectSize = rectTransform.rect.size;
            Vector2 pivotRectPosition = rectTransform.pivot;
            pivotRectPosition.x *= rectSize.x;
            pivotRectPosition.y *= rectSize.y;

            Vector2 rectPosition = new Vector2(normalizedRectPosition.x * rectSize.x, normalizedRectPosition.y * rectSize.y);
            Vector2 result = rectPosition - pivotRectPosition;
            return result;
        }

        public static Vector2 TransformLocalPositionToNormalizedRectPosition(this RectTransform rectTransform, Vector2 localPosition)
        {
            Vector2 rectSize = rectTransform.rect.size;
            Vector2 rectPosition = rectTransform.TransformLocalPositionToRectPosition(localPosition);
            Vector2 normalizedRectPosition = new Vector2(rectPosition.x / rectSize.x, rectPosition.y / rectSize.y);
            return normalizedRectPosition;
        }

        public static Vector2 TransformLocalPositionToRectPosition(this RectTransform rectTransform, Vector2 localPosition)
        {
            Vector2 rectSize = rectTransform.rect.size;
            Vector2 pivotRectPosition = rectTransform.pivot;
            pivotRectPosition.x *= rectSize.x;
            pivotRectPosition.y *= rectSize.y;

            Vector2 rectPosition = localPosition + pivotRectPosition;
            return rectPosition;
        }

        public static Vector2 CalulateRectPosition(this RectTransform rectTransform, Vector2 normalizedRectPosition)
        {
            Vector2 rectSize = rectTransform.rect.size;
            Vector2 rectPosition = new Vector2(normalizedRectPosition.x * rectSize.x, normalizedRectPosition.y * rectSize.y);
            return rectPosition;
        }

        public static void LetRectTransformFocusSceneObject(GameObject worldObj, RectTransform rectTransform, Camera worldCamera, Camera uiCamera)
        {
            if (!worldObj.TryGetComponent<Collider>(out Collider collider))
            {
                collider = worldObj.GetComponentInChildren<Collider>();
                if (null == collider)
                {
                    // TODO well, at least we need a Renderer to grab a bounds
                    return;
                }
            }

            // Collider valid
            Bounds bounds = collider.bounds;
            Vector3 boundCenter = bounds.center;
            Vector3 extend = bounds.extents;

            // We can get List from listpool in higher version of Unity
#if UNITY_2022_3_OR_NEWER
            List<Vector3> pointList = UnityEngine.Pool.ListPool<Vector3>.Get();
#else
            List<Vector3> pointList = new List<Vector3>();
#endif
            // Get points from 6 directions
            pointList.Add(boundCenter + new Vector3(extend.x, 0, 0));
            pointList.Add(boundCenter + new Vector3(-extend.x, 0, 0));
            pointList.Add(boundCenter + new Vector3(0, extend.y, 0));
            pointList.Add(boundCenter + new Vector3(0, -extend.y, 0));
            pointList.Add(boundCenter + new Vector3(0, 0, extend.z));
            pointList.Add(boundCenter + new Vector3(0, 0, -extend.z));

            for (int i = 0, length = pointList.Count; i < length; i++)
            {
                Vector3 tempPoint = pointList[i];
                tempPoint = worldCamera.WorldToScreenPoint(tempPoint);
                pointList[i] = tempPoint;
            }

            // Calculate a big enough rectangle
            Vector2 screenMinPoint, screenMaxPoint;
            screenMinPoint = screenMaxPoint = pointList[0];
            for (int i = 1, length = pointList.Count; i < length; i++)
            {
                Vector3 tempPoint = pointList[i];
                if (tempPoint.x < screenMinPoint.x)
                {
                    screenMinPoint.x = tempPoint.x;
                }
                if (tempPoint.y < screenMinPoint.y)
                {
                    screenMinPoint.y = tempPoint.y;
                }
                if (tempPoint.x > screenMaxPoint.x)
                {
                    screenMaxPoint.x = tempPoint.x;
                }
                if (tempPoint.y > screenMaxPoint.y)
                {
                    screenMaxPoint.y = tempPoint.y;
                }
            }

#if UNITY_2022_3_OR_NEWER
            pointList.Clear();
            UnityEngine.Pool.ListPool<Vector3>.Release(pointList);
#endif
            RectTransform tempParent = rectTransform.parent as RectTransform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(tempParent, screenMinPoint, uiCamera, out Vector2 localMinPoint) &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(tempParent, screenMaxPoint, uiCamera, out Vector2 localMaxPoint))
            {
                rectTransform.pivot = Vector2.zero;
                rectTransform.sizeDelta = localMaxPoint - localMinPoint;
                rectTransform.localPosition = localMinPoint;
            }
        }

    }
}