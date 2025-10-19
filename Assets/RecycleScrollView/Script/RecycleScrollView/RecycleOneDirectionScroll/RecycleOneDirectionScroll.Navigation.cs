using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extend;

namespace RecycleScrollView
{
    public partial class RecycleOneDirectionScroll
    {
        /// <summary> HACK IDK why but do jumpto for horizontal scroll will result a weird offset in next/current frame, so I need to skil 2 frames </summary>
        const int JUMPTO_SKIP_FRAME_COUNT = 1;

        [System.Serializable]
        public struct ScrollViewNavigationParams
        {
            public float normalizedPositionInViewPort;
            public float normalizedElementPositionAdjustment;
        }

        [SerializeField]
        private ScrollViewNavigationParams _defaultNavigationParams;

        /// <summary> HACK IDK why but do jumpto for horizontal scroll will result a weird offset in next/current frame, so I need to skil 2 frames </summary>
        private int m_nextFrameSetActive = 0;

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
            RecycleOneDirectionScrollElement targetElement = InternalCreateElement(dataIndex);
            targetElement.SetIndex(dataIndex);
            targetElement.CalculatePreferredSize();
            m_currentUsingElements.Add(targetElement);

            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            // HACK Since the pivot of content must be fixed, we need to adjust the position of content to make the target element at the correct position
            if (IsVertical)
            {
                Vector2 verticalPostion = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, new Vector2(0.5f, _defaultNavigationParams.normalizedPositionInViewPort));
                // Content pivot is (0.5, 0) (true _scrollParam.reverseArrangement) ; Content pivot is (0.5, 1) (false _scrollParam.reverseArrangement)
                Vector3 elementPosition = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(targetElement.ElementTransform, new Vector2(0.5f, 1f - _defaultNavigationParams.normalizedElementPositionAdjustment));
                elementPosition = viewport.InverseTransformPoint(elementPosition);
                float delta = verticalPostion.y - elementPosition.y;
                Vector3 localPosition = content.localPosition;
                localPosition.y += delta;
                content.localPosition = localPosition;
            }
            else if (IsHorizontal)
            {
                Vector2 horizontalPostion = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, new Vector2(_defaultNavigationParams.normalizedPositionInViewPort, 0.5f));
                // Content pivot is (0, 0.5) (false _scrollParam.reverseArrangement) ; Content pivot is (1, 0.5) (true _scrollParam.reverseArrangement)
                Vector3 elementPosition = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(targetElement.ElementTransform, new Vector2(_defaultNavigationParams.normalizedElementPositionAdjustment, 0.5f));
                elementPosition = viewport.InverseTransformPoint(elementPosition);
                float delta = horizontalPostion.x - elementPosition.x;
                Vector3 localPosition = content.localPosition;
                localPosition.x += delta;
                content.localPosition = localPosition;

            }

            AddElemensIfNeed();
            _scrollRect.CallUpdateBoundsAndPrevData();
            if (IsVertical)
            {
                _scrollRect.enabled = true;
            }
            else if (IsHorizontal)
            {
                /// <summary> HACK IDK why but do jumpto for horizontal scroll will result a weird offset in next/current frame, so I need to skil 2 frames </summary>
                m_nextFrameSetActive = JUMPTO_SKIP_FRAME_COUNT;
            }
            _scrollRect.StopMovement();
        }


        public float CalculateCurrentNormalizedPosition()
        {
            // TODO
            return 0f;
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
                elementLocalPos.y += refElementSize.y * (_defaultNavigationParams.normalizedElementPositionAdjustment - 1f);
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
                elementLocalPos.y -= 0.5f * refElementSize.y;
                elementLocalPos.x -= refElementSize.x * _defaultNavigationParams.normalizedElementPositionAdjustment;
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