using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extend;

namespace RecycleScrollView
{
    public partial class RecycleOneDirectionScroll
    {
        [System.Serializable]
        public struct ScrollViewNavigationParams
        {
            public float normalizedPositionInViewPort;
            public float normalizedElementPositionAdjustment;
        }

        [SerializeField]
        private ScrollViewNavigationParams _defaultNavigationParams;

        public void JumpToElementInstant(int dataIndex)
        {
            if (null == m_dataSource || dataIndex < 0 || dataIndex >= m_dataSource.DataElementCount)
            {
                return;
            }

            for (int i = 0, length = m_currentUsingElements.Count; i < length; i++)
            {
                InternalRemoveElement(m_currentUsingElements[i]);
            }
            m_currentUsingElements.Clear();

            _scrollRect.StopMovement();
            _scrollRect.enabled = false;
            RectTransform viewport = _scrollRect.viewport;
            RectTransform content = _scrollRect.content;
            RecycleOneDirectionScrollElement add = InternalCreateElement(dataIndex);
            add.SetIndex(dataIndex);
            add.CalculatePreferredSize();
            m_currentUsingElements.Add(add);

            // HACK Since the pivot of content must be fixed, we need to adjust the position of content to make the target element at the correct position
            if (IsVertical)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                Vector2 verticalPostion = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, new Vector2(0.5f, _defaultNavigationParams.normalizedPositionInViewPort));
                if (_scrollParam.reverseArrangement)
                {
                    Vector3 elementPosition = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(add.ElementTransform, new Vector2(0.5f, _defaultNavigationParams.normalizedElementPositionAdjustment));
                    elementPosition = viewport.InverseTransformPoint(elementPosition);
                    // Content pivot is (0.5, 0)
                    float delta = verticalPostion.y - elementPosition.y;
                    Vector3 localPosition = content.localPosition;
                    localPosition.y += delta;
                    content.localPosition = localPosition;
                }
                else
                {
                    Vector3 elementPosition = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(add.ElementTransform, new Vector2(0.5f, 1f - _defaultNavigationParams.normalizedElementPositionAdjustment));
                    elementPosition = viewport.InverseTransformPoint(elementPosition);
                    // Content pivot is (0.5, 1)
                    float delta = verticalPostion.y - elementPosition.y;
                    Vector3 localPosition = content.localPosition;
                    localPosition.y += delta;
                    content.localPosition = localPosition;
                }
            }
            else if (IsHorizontal)
            {
                // TODO
                // Vector2 horizontalPostion = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, new Vector2(_defaultNavigationParams.normalizedPositionInViewPort, 0.5f));
                // horizontalPostion.x += targetElementSize.x * _defaultNavigationParams.normalizedElementPositionAdjustment;
                // content.anchoredPosition = new Vector2(horizontalPostion.x, content.anchoredPosition.y);
            }
            AddElemensIfNeed();
            _scrollRect.enabled = true;
            _scrollRect.StopMovement();
        }


        public void EETMP(int dataIndex)
        {
            for (int i = 0, length = m_currentUsingElements.Count; i < length; i++)
            {
                var element = m_currentUsingElements[i];
                if (element.ElementIndex == dataIndex)
                {
                    RectTransform viewport = _scrollRect.viewport;
                    Vector2 verticalPostion = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, new Vector2(0.5f, _defaultNavigationParams.normalizedPositionInViewPort));
                    Vector3 elementPosition = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(element.ElementTransform, new Vector2(0.5f, _defaultNavigationParams.normalizedElementPositionAdjustment));
                    elementPosition = viewport.InverseTransformPoint(elementPosition);

                    RectTransform content = _scrollRect.content;
                    float delta = verticalPostion.y - elementPosition.y;
                    Vector3 prevAnchorPosition = content.anchoredPosition;
                    prevAnchorPosition.y += delta;
                    content.anchoredPosition = prevAnchorPosition;

                    return;
                }
            }
        }

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            GizmoDrawDefaultNavigationPosition();
        }

        private void GizmoDrawDefaultNavigationPosition()
        {
            if (null == _scrollRect || null == _scrollRect.viewport)
            {
                return;
            }

            Vector2 refElementSize = new Vector2(100, 100);
            Color prevColor = Gizmos.color;
            Gizmos.color = Color.green;
            RectTransform viewport = _scrollRect.viewport;
            if (IsVertical)
            {
                // Draw ref line
                Vector2 viewPortLocalLeft = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, new Vector2(-0.1f, _defaultNavigationParams.normalizedPositionInViewPort));
                Vector2 viewPortLocalRight = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, new Vector2(1.1f, _defaultNavigationParams.normalizedPositionInViewPort));
                Vector3 viewPortLocalLeftWorld = viewport.TransformPoint(viewPortLocalLeft);
                Vector3 viewPortLocalRightWorld = viewport.TransformPoint(viewPortLocalRight);
                Gizmos.DrawLine(viewPortLocalLeftWorld, viewPortLocalRightWorld);
                // Draw ref element position
                Vector2 elementLocalPos = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, new Vector2(0.5f, _defaultNavigationParams.normalizedPositionInViewPort));
                elementLocalPos.x -= refElementSize.x * 0.5f;
                elementLocalPos.y -= refElementSize.y;
                elementLocalPos.y += refElementSize.y * _defaultNavigationParams.normalizedElementPositionAdjustment;
                GizmoDrawRect(elementLocalPos, refElementSize, viewport.localToWorldMatrix, Color.yellow);
            }
            else if (IsHorizontal)
            {
                // Draw ref line
                Vector2 viewPortLocalTop = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, new Vector2(_defaultNavigationParams.normalizedPositionInViewPort, 1.1f));
                Vector2 viewPortLocalBottom = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, new Vector2(_defaultNavigationParams.normalizedPositionInViewPort, -0.1f));
                Vector3 viewPortLocalTopWorld = viewport.TransformPoint(viewPortLocalTop);
                Vector3 viewPortLocalBottomWorld = viewport.TransformPoint(viewPortLocalBottom);
                Gizmos.DrawLine(viewPortLocalBottomWorld, viewPortLocalTopWorld);
                // Draw ref element position
                Vector2 elementLocalPos = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, new Vector2(_defaultNavigationParams.normalizedPositionInViewPort, 0.5f));
                elementLocalPos.x -= 0.5f * refElementSize.x;
                elementLocalPos.y -= 0.5f * refElementSize.y;
                elementLocalPos.x += refElementSize.x * _defaultNavigationParams.normalizedElementPositionAdjustment;
                GizmoDrawRect(elementLocalPos, refElementSize, viewport.localToWorldMatrix, Color.yellow);
            }

            Gizmos.color = prevColor;
        }

        private void GizmoDrawRect(Vector2 localBottomLeftPosition, Vector2 size, Matrix4x4 localToWorldMatrix, Color color)
        {
            Vector2 bottomLeft = new Vector3(localBottomLeftPosition.x, localBottomLeftPosition.y);
            Vector2 bottomRight = new Vector3(localBottomLeftPosition.x + size.x, localBottomLeftPosition.y);
            Vector2 topRight = new Vector3(localBottomLeftPosition.x + size.x, localBottomLeftPosition.y + size.y);
            Vector2 topLeft = new Vector3(localBottomLeftPosition.x, localBottomLeftPosition.y + size.y);
            bottomLeft = localToWorldMatrix.MultiplyPoint(bottomLeft);
            bottomRight = localToWorldMatrix.MultiplyPoint(bottomRight);
            topLeft = localToWorldMatrix.MultiplyPoint(topLeft);
            topRight = localToWorldMatrix.MultiplyPoint(topRight);

            Color prevColor = Gizmos.color;
            Gizmos.color = color;
            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);
            Gizmos.color = prevColor;
        }

#endif
    }
}