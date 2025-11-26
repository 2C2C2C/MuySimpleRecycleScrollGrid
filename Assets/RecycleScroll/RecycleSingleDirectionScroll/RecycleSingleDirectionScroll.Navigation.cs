using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI.Extend;
using ScrollDirection = RecycleScrollView.SingleDirectionScrollParam.ScrollDirection;

namespace RecycleScrollView
{
    public partial class RecycleSingleDirectionScroll
    {
        [System.Serializable]
        public struct SingleScrollElementNavigationParams
        {
            public float normalizedPositionInViewPort;
            public float normalizedElementPositionAdjustment;
        }

        [SerializeField]
        private SingleScrollElementNavigationParams _defaultNavigationParams;

        public void JumpToElementInstant(int dataIndex)
        {
            JumpToElementInstant(dataIndex, _defaultNavigationParams);
        }

        public void JumpToElementInstant(int dataIndex, SingleScrollElementNavigationParams navigationParams)
        {
            if (null == m_dataSource || dataIndex < 0 || dataIndex >= m_dataSource.DataElementCount)
            {
                return;
            }

            RectTransform content = _scrollRect.content;
            RectTransform viewport = _scrollRect.viewport;
            int usingElementCount = m_currentUsingElements.Count;
            for (int i = 0; i < usingElementCount; i++)
            {
                RecycleSingleDirectionScrollElement element = m_currentUsingElements[i];
                if (element.ElementIndex == dataIndex)
                {
                    Vector2 verticalPostion = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, new Vector2(0.5f, navigationParams.normalizedPositionInViewPort));
                    Vector2 elementPosition = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(element.ElementTransform, new Vector2(0.5f, navigationParams.normalizedElementPositionAdjustment));
                    elementPosition = viewport.InverseTransformPoint(elementPosition);

                    Vector2 delta = verticalPostion - elementPosition;
                    Vector2 localPosition = content.localPosition;
                    localPosition += delta;
                    content.localPosition = localPosition;
                    Debug.LogError($"elementPosition_{elementPosition} -> verticalPostion_{verticalPostion}");
                    ForceRebuildAndStopMove();
                    return;
                }
            }

            RemoveCurrentElements();
            _scrollRect.StopMovement();

            RecycleSingleDirectionScrollElement targetElement = InternalCreateElement(dataIndex);
            targetElement.SetIndex(dataIndex);
            targetElement.CalculatePreferredSize();
            m_currentUsingElements.Add(targetElement);
            Vector2 targetElementSize = targetElement.ElementPreferredSize;

            // HACK Since the pivot of content must be fixed, we need to adjust the position of content to make the target element at the correct position
            Vector2 headCheckRectPosition = Vector2.zero;
            if (IsVertical)
            {
                Vector2 verticalPostion = RectTransformEx.CalulateRectPosition(viewport, new Vector2(0.5f, navigationParams.normalizedPositionInViewPort));
                // Content pivot is (0.5, 0) (true _scrollParam.reverseArrangement) ; Content pivot is (0.5, 1) (false _scrollParam.reverseArrangement)
                if (ScrollDirection.Vertical_UpToDown == _scrollParam.scrollDirection)
                {
                    verticalPostion.y += navigationParams.normalizedElementPositionAdjustment * targetElementSize.y;
                }
                else // ScrollDirection.Vertical_DownToUp
                {
                    verticalPostion.y -= (1f - navigationParams.normalizedElementPositionAdjustment) * targetElementSize.y;
                }
                headCheckRectPosition = verticalPostion;
                Vector3 localPosition = RectTransformEx.TransformRectPositionToLocalPosition(viewport, verticalPostion);
                content.localPosition = localPosition;
            }
            else if (IsHorizontal)
            {
                Vector2 horizontalPostion = RectTransformEx.CalulateRectPosition(viewport, new Vector2(navigationParams.normalizedPositionInViewPort, 0.5f));
                // Content pivot is (0, 0.5) (false _scrollParam.reverseArrangement) ; Content pivot is (1, 0.5) (true _scrollParam.reverseArrangement)
                if (ScrollDirection.Horizontal_LeftToRight == _scrollParam.scrollDirection)
                {
                    horizontalPostion.x -= navigationParams.normalizedElementPositionAdjustment * targetElementSize.x;
                }
                else // ScrollDirection.Horizontal_RightToLeft
                {
                    horizontalPostion.x += (1f - navigationParams.normalizedElementPositionAdjustment) * targetElementSize.x;
                }
                headCheckRectPosition = horizontalPostion;
                Vector3 localPosition = RectTransformEx.TransformRectPositionToLocalPosition(viewport, horizontalPostion);
                content.localPosition = localPosition;
            }

            // Add elements to fill the view port
            Vector2 viewportSize = viewport.rect.size;
            Vector2 headRectPosition = CalculateNormalizedRectPosition(0f);
            headRectPosition = new Vector2(viewportSize.x * headRectPosition.x, viewportSize.y * headRectPosition.y);
            int canAddIndex;
            float spacing = _scrollParam.spacing;
            while (-1 != (canAddIndex = CalculateAvailabeNextHeadElementIndex()))
            {
                AddElementToHead(canAddIndex);
                Vector2 size = m_currentUsingElements[0].ElementPreferredSize;
                bool doBreak = false;
                switch (_scrollParam.scrollDirection)
                {
                    // Vertical
                    case ScrollDirection.Vertical_UpToDown:
                        headCheckRectPosition += Vector2.up * (size.y + spacing);
                        content.localPosition += Vector3.up * (size.y + spacing);
                        doBreak = headCheckRectPosition.y > headRectPosition.y;
                        break;
                    case ScrollDirection.Vertical_DownToUp:
                        headCheckRectPosition += Vector2.down * (size.y + spacing);
                        content.localPosition += Vector3.down * (size.y + spacing);
                        doBreak = headCheckRectPosition.y < headRectPosition.y;
                        break;

                    // Horizontal
                    case ScrollDirection.Horizontal_LeftToRight:
                        headCheckRectPosition += Vector2.left * (size.x + spacing);
                        content.localPosition += Vector3.left * (size.x + spacing);
                        doBreak = headCheckRectPosition.x < headRectPosition.x;
                        break;
                    case ScrollDirection.Horizontal_RightToLeft:
                        headCheckRectPosition += Vector2.right * (size.x + spacing);
                        content.localPosition += Vector3.right * (size.x + spacing);
                        doBreak = headCheckRectPosition.x > headRectPosition.x;
                        break;
                    default:
                        break;
                }

                if (doBreak)
                {
                    break;
                }
            }

            AddElemensIfNeed();
            ForceRebuildAndStopMove();
        }

        public void JumpToElementInstant(int dataIndex, SingleScrollElementNavigationParams navigationParams, Vector2 extraOffset)
        {
            if (null == m_dataSource || dataIndex < 0 || dataIndex >= m_dataSource.DataElementCount)
            {
                return;
            }

            RectTransform content = _scrollRect.content;
            RectTransform viewport = _scrollRect.viewport;
            int usingElementCount = m_currentUsingElements.Count;
            for (int i = 0; i < usingElementCount; i++)
            {
                RecycleSingleDirectionScrollElement element = m_currentUsingElements[i];
                if (element.ElementIndex == dataIndex)
                {
                    Vector2 verticalPostion = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, new Vector2(0.5f, navigationParams.normalizedPositionInViewPort));
                    Vector2 elementPosition = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(element.ElementTransform, new Vector2(0.5f, navigationParams.normalizedElementPositionAdjustment));
                    elementPosition = viewport.InverseTransformPoint(elementPosition);

                    Vector2 delta = verticalPostion - elementPosition;
                    Vector2 localPosition = content.localPosition;
                    localPosition += delta;
                    content.localPosition = localPosition;
                    Debug.LogError($"elementPosition_{elementPosition} -> verticalPostion_{verticalPostion}");
                    ForceRebuildAndStopMove();
                    return;
                }
            }

            RemoveCurrentElements();
            _scrollRect.StopMovement();

            RecycleSingleDirectionScrollElement targetElement = InternalCreateElement(dataIndex);
            targetElement.SetIndex(dataIndex);
            targetElement.CalculatePreferredSize();
            m_currentUsingElements.Add(targetElement);
            Vector2 targetElementSize = targetElement.ElementPreferredSize;

            // HACK Since the pivot of content must be fixed, we need to adjust the position of content to make the target element at the correct position
            Vector2 headCheckRectPosition = Vector2.zero; ;
            if (IsVertical)
            {
                Vector2 verticalPostion = RectTransformEx.CalulateRectPosition(viewport, new Vector2(0.5f, navigationParams.normalizedPositionInViewPort));
                // Content pivot is (0.5, 0) (true _scrollParam.reverseArrangement) ; Content pivot is (0.5, 1) (false _scrollParam.reverseArrangement)
                if (ScrollDirection.Vertical_UpToDown == _scrollParam.scrollDirection)
                {
                    verticalPostion.y += navigationParams.normalizedElementPositionAdjustment * targetElementSize.y;
                }
                else // ScrollDirection.Vertical_DownToUp
                {
                    verticalPostion.y -= (1f - navigationParams.normalizedElementPositionAdjustment) * targetElementSize.y;
                }
                verticalPostion += extraOffset;
                headCheckRectPosition = verticalPostion;
                Vector3 localPosition = RectTransformEx.TransformRectPositionToLocalPosition(viewport, verticalPostion);
                content.localPosition = localPosition;
            }
            else if (IsHorizontal)
            {
                Vector2 horizontalPostion = RectTransformEx.CalulateRectPosition(viewport, new Vector2(navigationParams.normalizedPositionInViewPort, 0.5f));
                // Content pivot is (0, 0.5) (false _scrollParam.reverseArrangement) ; Content pivot is (1, 0.5) (true _scrollParam.reverseArrangement)
                if (ScrollDirection.Horizontal_LeftToRight == _scrollParam.scrollDirection)
                {
                    horizontalPostion.x -= navigationParams.normalizedElementPositionAdjustment * targetElementSize.x;
                }
                else // ScrollDirection.Horizontal_RightToLeft
                {
                    horizontalPostion.x += (1f - navigationParams.normalizedElementPositionAdjustment) * targetElementSize.x;
                }
                horizontalPostion += extraOffset;
                headCheckRectPosition = horizontalPostion;
                Vector3 localPosition = RectTransformEx.TransformRectPositionToLocalPosition(viewport, horizontalPostion);
                content.localPosition = localPosition;
            }

            // Add elements to fill the view port
            Vector2 viewportSize = viewport.rect.size;
            Vector2 headRectPosition = CalculateNormalizedRectPosition(0f);
            headRectPosition = new Vector2(viewportSize.x * headRectPosition.x, viewportSize.y * headRectPosition.y);
            int canAddIndex;
            float spacing = _scrollParam.spacing;
            while (-1 != (canAddIndex = CalculateAvailabeNextHeadElementIndex()))
            {
                AddElementToHead(canAddIndex);
                Vector2 size = m_currentUsingElements[0].ElementPreferredSize;
                bool doBreak = false;
                switch (_scrollParam.scrollDirection)
                {
                    // Vertical
                    case ScrollDirection.Vertical_UpToDown:
                        headCheckRectPosition += Vector2.up * (size.y + spacing);
                        content.localPosition += Vector3.up * (size.y + spacing);
                        doBreak = headCheckRectPosition.y > headRectPosition.y;
                        break;
                    case ScrollDirection.Vertical_DownToUp:
                        headCheckRectPosition += Vector2.down * (size.y + spacing);
                        content.localPosition += Vector3.down * (size.y + spacing);
                        doBreak = headCheckRectPosition.y < headRectPosition.y;
                        break;

                    // Horizontal
                    case ScrollDirection.Horizontal_LeftToRight:
                        headCheckRectPosition += Vector2.left * (size.x + spacing);
                        content.localPosition += Vector3.left * (size.x + spacing);
                        doBreak = headCheckRectPosition.x < headRectPosition.x;
                        break;
                    case ScrollDirection.Horizontal_RightToLeft:
                        headCheckRectPosition += Vector2.right * (size.x + spacing);
                        content.localPosition += Vector3.right * (size.x + spacing);
                        doBreak = headCheckRectPosition.x > headRectPosition.x;
                        break;
                    default:
                        break;
                }

                if (doBreak)
                {
                    break;
                }
            }

            AddElemensIfNeed();
            ForceRebuildAndStopMove();
        }

        private void JumpToExistElementInstant(int dataIndex, float refNormalizedRectPosition, float extraNormalizedGapOffset)
        {
            if (null == m_dataSource || dataIndex < 0 || dataIndex >= m_dataSource.DataElementCount)
            {
                return;
            }

            RectTransform content = _scrollRect.content;
            RectTransform viewport = _scrollRect.viewport;
            // if (TryGetShowingElement(dataIndex, out RecycleSingleDirectionScrollElement baseElement))
            // {
            //     return;
            // }
            // else
            {
                // TODO
                RemoveCurrentElements();
                _scrollRect.StopMovement();

                RecycleSingleDirectionScrollElement targetElement = InternalCreateElement(dataIndex);
                targetElement.SetIndex(dataIndex);
                targetElement.CalculatePreferredSize();
                m_currentUsingElements.Add(targetElement);

                // HACK Since the pivot of content must be fixed, we need to adjust the position of content to make the target element at the correct position
                Vector2 headCheckRectPosition = Vector2.zero;
                Vector3 localPosition = Vector2.zero;
                if (IsVertical)
                {
                    Vector2 verticalPostion = RectTransformEx.CalulateRectPosition(viewport, new Vector2(0.5f, refNormalizedRectPosition));
                    headCheckRectPosition = verticalPostion;
                    localPosition = RectTransformEx.TransformRectPositionToLocalPosition(viewport, verticalPostion);
                }
                else if (IsHorizontal)
                {
                    Vector2 horizontalPostion = RectTransformEx.CalulateRectPosition(viewport, new Vector2(refNormalizedRectPosition, 0.5f));
                    headCheckRectPosition = horizontalPostion;
                    localPosition = RectTransformEx.TransformRectPositionToLocalPosition(viewport, horizontalPostion);
                }

                RecycleSingleDirectionScrollElement nextElement = InternalCreateElement(dataIndex + 1);
                nextElement.SetIndex(dataIndex + 1);
                nextElement.CalculatePreferredSize();
                m_currentUsingElements.Add(nextElement);

                Vector3 offset = Vector3.zero;
                if (TryCalculateGapBetweenElement(dataIndex, dataIndex + 1, out float gapSize))
                {
                    if (IsHorizontal)
                    {
                        offset = new Vector2(gapSize * extraNormalizedGapOffset, 0f);
                    }
                    else if (IsVertical)
                    {
                        offset = new Vector2(0f, gapSize * -extraNormalizedGapOffset);
                    }
                }
                headCheckRectPosition += (Vector2)offset;
                content.localPosition = localPosition + offset;

                // Add elements to fill the view port
                Vector2 viewportSize = viewport.rect.size;
                Vector2 headRectPosition = CalculateNormalizedRectPosition(0f);
                headRectPosition = new Vector2(viewportSize.x * headRectPosition.x, viewportSize.y * headRectPosition.y);
                int canAddIndex;
                float spacing = _scrollParam.spacing;
                while (-1 != (canAddIndex = CalculateAvailabeNextHeadElementIndex()))
                {
                    AddElementToHead(canAddIndex);
                    Vector2 size = m_currentUsingElements[0].ElementPreferredSize;
                    bool doBreak = false;
                    switch (_scrollParam.scrollDirection)
                    {
                        // Vertical
                        case ScrollDirection.Vertical_UpToDown:
                            headCheckRectPosition += Vector2.up * (size.y + spacing);
                            content.localPosition += Vector3.up * (size.y + spacing);
                            doBreak = headCheckRectPosition.y > headRectPosition.y;
                            break;
                        case ScrollDirection.Vertical_DownToUp:
                            headCheckRectPosition += Vector2.down * (size.y + spacing);
                            content.localPosition += Vector3.down * (size.y + spacing);
                            doBreak = headCheckRectPosition.y < headRectPosition.y;
                            break;

                        // Horizontal
                        case ScrollDirection.Horizontal_LeftToRight:
                            headCheckRectPosition += Vector2.left * (size.x + spacing);
                            content.localPosition += Vector3.left * (size.x + spacing);
                            doBreak = headCheckRectPosition.x < headRectPosition.x;
                            break;
                        case ScrollDirection.Horizontal_RightToLeft:
                            headCheckRectPosition += Vector2.right * (size.x + spacing);
                            content.localPosition += Vector3.right * (size.x + spacing);
                            doBreak = headCheckRectPosition.x > headRectPosition.x;
                            break;
                        default:
                            break;
                    }

                    if (doBreak)
                    {
                        break;
                    }
                }

                AddElemensIfNeed();
                ForceRebuildAndStopMove();
            }
        }

        private void ForceRebuildAndStopMove()
        {
            ForceRebuildContentLayout();
            _scrollRect.CallUpdateBoundsAndPrevData();
            _scrollRect.StopMovement();
        }

#if UNITY_EDITOR

        [SerializeField]
        private bool _alwaysDrawGizmos;

        private void OnDrawGizmos()
        {
            if (_alwaysDrawGizmos)
            {
                GizmoDrawDefaultNavigationPosition();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_alwaysDrawGizmos)
            {
                GizmoDrawDefaultNavigationPosition();
            }
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