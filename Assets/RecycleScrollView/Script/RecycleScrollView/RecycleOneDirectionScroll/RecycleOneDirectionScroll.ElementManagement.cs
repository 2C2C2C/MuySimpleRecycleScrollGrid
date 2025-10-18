using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extend;
using ScrollDirection = RecycleScrollView.SingleDirectionScrollParam.ScrollDirection;

namespace RecycleScrollView
{
    public partial class RecycleOneDirectionScroll
    {
        private const int SIDE_STATUS_ENOUGH = 0;
        private const int SIDE_STATUS_NEEDADD = -1;
        private const int SIDE_STATUS_NEEDREMOVE = 1;

        private const float EDGE_HEAD = 0F;
        private const float EDGE_TAIL = 1F;

        /// <summary>
        /// 
        /// </summary>
        /// <returns>-1 Need add, 0 Enough, 1 Need remove</returns>
        private int CheckHeadSideStatus()
        {
            if (null == m_dataSource)
            {
                return SIDE_STATUS_ENOUGH;
            }

            int elementCount = m_currentUsingElements.Count;
            if (0 == elementCount)
            {
                return SIDE_STATUS_ENOUGH; // HACK
            }

            ScrollDirection checkDirection;
            switch (_scrollParam.scrollDirection)
            {
                case ScrollDirection.Horizontal_LeftToRight:
                    checkDirection = ScrollDirection.Horizontal_RightToLeft;
                    break;
                case ScrollDirection.Horizontal_RightToLeft:
                    checkDirection = ScrollDirection.Horizontal_LeftToRight;
                    break;

                case ScrollDirection.Vertical_UpToDown:
                    checkDirection = ScrollDirection.Vertical_DownToUp;
                    break;
                case ScrollDirection.Vertical_DownToUp:
                    checkDirection = ScrollDirection.Vertical_UpToDown;
                    break;
                default:
                    checkDirection = ScrollDirection.None;
                    break;
            }
            RecycleOneDirectionScrollElement headElement = m_currentUsingElements[0];
            bool isBeyoudEdge = IsElementEdgeBeyoudViewportEdge(headElement, normalizedViewportEdgePosition: EDGE_HEAD, normalizedElementEdgePosition: EDGE_TAIL, checkDirection);
            if (isBeyoudEdge)
            {
                if (2 <= elementCount)
                {
                    RecycleOneDirectionScrollElement head2ndElement = m_currentUsingElements[1];
                    isBeyoudEdge = IsElementEdgeBeyoudViewportEdge(head2ndElement, normalizedViewportEdgePosition: EDGE_HEAD, normalizedElementEdgePosition: EDGE_HEAD, checkDirection);
                    if (isBeyoudEdge)
                    {
                        return SIDE_STATUS_NEEDREMOVE;
                    }
                }
            }
            else
            {
                return SIDE_STATUS_NEEDADD;
            }
            return SIDE_STATUS_ENOUGH;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>-1 Need add, 0 Enough, 1 Need remove</returns>
        private int CheckTailSideStatus()
        {
            if (null == m_dataSource)
            {
                return SIDE_STATUS_ENOUGH;
            }

            int elementCount = m_currentUsingElements.Count;
            if (0 == elementCount)
            {
                if (m_dataSource.DataElementCount > 0)
                {
                    return SIDE_STATUS_NEEDADD; // HACK
                }
                else
                {
                    return SIDE_STATUS_ENOUGH; // HACK
                }
            }

            ScrollDirection checkDirection = _scrollParam.scrollDirection;
            RecycleOneDirectionScrollElement tailElement = m_currentUsingElements[elementCount - 1];
            bool isBeyoudEdge = IsElementEdgeBeyoudViewportEdge(tailElement, normalizedViewportEdgePosition: EDGE_TAIL, normalizedElementEdgePosition: EDGE_HEAD, checkDirection);
            if (isBeyoudEdge)
            {
                if (2 <= elementCount)
                {
                    RecycleOneDirectionScrollElement tail2ndElement = m_currentUsingElements[elementCount - 2];
                    isBeyoudEdge = IsElementEdgeBeyoudViewportEdge(tail2ndElement, normalizedViewportEdgePosition: EDGE_TAIL, normalizedElementEdgePosition: EDGE_HEAD, checkDirection);
                    if (isBeyoudEdge)
                    {
                        return SIDE_STATUS_NEEDREMOVE;
                    }
                }
            }
            else
            {
                return SIDE_STATUS_NEEDADD;
            }
            return SIDE_STATUS_ENOUGH;
        }

        /// <param name="normalizedValue"> Head(0) ~ Tail(1) </param>
        /// <returns></returns>
        private Vector2 CalculateNormalizedRectPosition(float normalizedValue)
        {
            Vector2 result = Vector2.zero;
            switch (_scrollParam.scrollDirection)
            {
                case ScrollDirection.Horizontal_LeftToRight:
                    result = new Vector2(Mathf.Lerp(0f, 1f, normalizedValue), 0.5f);
                    break;
                case ScrollDirection.Horizontal_RightToLeft:
                    result = new Vector2(Mathf.Lerp(1f, 0f, normalizedValue), 0.5f);
                    break;
                case ScrollDirection.Vertical_UpToDown:
                    result = new Vector2(0.5f, Mathf.Lerp(1f, 0f, normalizedValue));
                    break;
                case ScrollDirection.Vertical_DownToUp:
                    result = new Vector2(0.5f, Mathf.Lerp(0f, 1f, normalizedValue));
                    break;
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="normalizedElementEdgePosition"> Head(0) ~ Tail(1) </param>
        /// <param name="normalizedViewportEdgePosition"> Head(0) ~ Tail(1) </param>
        /// <returns></returns>
        private bool IsElementEdgeBeyoudViewportEdge(RecycleOneDirectionScrollElement element, float normalizedViewportEdgePosition, float normalizedElementEdgePosition, ScrollDirection checkDirection)
        {
            RectTransform viewport = _scrollRect.viewport;
            Vector2 viewportEdgeRectPosition = CalculateNormalizedRectPosition(normalizedViewportEdgePosition);
            Vector2 viewportEdge = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, viewportEdgeRectPosition);

            Vector2 elementEdgeRectPosition = CalculateNormalizedRectPosition(normalizedElementEdgePosition);
            Vector2 elementEdge = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(element.ElementTransform, elementEdgeRectPosition);
            elementEdge = viewport.InverseTransformPoint(elementEdge);

            bool isBeyoudEdge = false;
            switch (checkDirection)
            {
                // Horizontal
                case ScrollDirection.Horizontal_LeftToRight:
                    isBeyoudEdge = elementEdge.x > viewportEdge.x;
                    break;
                case ScrollDirection.Horizontal_RightToLeft:
                    isBeyoudEdge = elementEdge.x < viewportEdge.x;
                    break;

                // Vertical
                case ScrollDirection.Vertical_UpToDown:
                    isBeyoudEdge = elementEdge.y < viewportEdge.y;
                    break;
                case ScrollDirection.Vertical_DownToUp:
                    isBeyoudEdge = elementEdge.y > viewportEdge.y;
                    break;
                default:
                    break;
            }
            return isBeyoudEdge;
        }

        private bool RemoveElementsIfNeed()
        {
            bool hasRemoveHeadElements = RemoveElementsFromHeadIfNeed();
            bool hasRemoveTailElements = RemoveElementsFromTailIfNeed();
            return hasRemoveHeadElements || hasRemoveTailElements;
        }

        private bool RemoveElementsFromHeadIfNeed()
        {
            RectTransform viewport = _scrollRect.viewport;
            RectTransform content = _scrollRect.content;
            bool hasRemoveElements = false;
            int prevElementCount = m_currentUsingElements.Count;
            if (0 < prevElementCount)
            {
                if (SIDE_STATUS_NEEDREMOVE == CheckHeadSideStatus())
                {
                    int frontRemoveElementCount = -1;
                    do
                    {
                        RecycleOneDirectionScrollElement toRemove = m_currentUsingElements[frontRemoveElementCount + 1];
                        // TODO This check is different with CheckHeadSideStatus(), need unify
                        if ((0 > frontRemoveElementCount && RectTransformEx.IsNotIntersetedWithTargetRect(toRemove.ElementTransform, viewport)) ||
                           (0 <= frontRemoveElementCount && RectTransformEx.IsNotIntersetedWithTargetRect(toRemove.ElementTransform, viewport)))
                        {
                            ++frontRemoveElementCount;
                        }
                        else
                        {
                            break;
                        }
                    } while (-1 < frontRemoveElementCount);

                    float removeSize = 0f;
                    while (0 < frontRemoveElementCount && 0 < m_currentUsingElements.Count)
                    {
                        RecycleOneDirectionScrollElement toRemove = m_currentUsingElements[0];
                        if (IsVertical)
                        {
                            removeSize += toRemove.ElementPreferredSize.y;
                        }
                        else if (IsHorizontal)
                        {
                            removeSize += toRemove.ElementPreferredSize.x;
                        }
                        RemoveElementFromHead();
                        --frontRemoveElementCount;
                        hasRemoveElements = true;
                    }

                    if (0f < removeSize) // HACK Becuz I use a fixed pivot for content, so I can directly adjust local position
                    {
                        switch (_scrollParam.scrollDirection)
                        {
                            // Vertical
                            case ScrollDirection.Vertical_UpToDown:
                                content.localPosition += Vector3.down * removeSize;
                                break;
                            case ScrollDirection.Vertical_DownToUp:
                                content.localPosition += Vector3.up * removeSize;
                                break;

                            // Horizontal
                            case ScrollDirection.Horizontal_LeftToRight:
                                content.localPosition += Vector3.right * removeSize;
                                break;
                            case ScrollDirection.Horizontal_RightToLeft:
                                content.localPosition += Vector3.left * removeSize;
                                break;
                            default:
                                break;
                        }
                        LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
                    }
                }
            }
            return hasRemoveElements;
        }

        private bool RemoveElementsFromTailIfNeed()
        {
            RectTransform viewport = _scrollRect.viewport;
            bool hasRemoveElements = false;
            int prevElementCount = m_currentUsingElements.Count;
            if (0 < prevElementCount)
            {
                if (SIDE_STATUS_NEEDREMOVE == CheckTailSideStatus())
                {
                    prevElementCount = m_currentUsingElements.Count;
                    int removeCount = -1;
                    do
                    {
                        int index = prevElementCount - 1 - (removeCount + 1);
                        if (0 < index)
                        {
                            RecycleOneDirectionScrollElement toRemove = m_currentUsingElements[index];
                            // TODO This check is different with CheckTailSideStatus(), need unify
                            if ((0 > removeCount && RectTransformEx.IsNotIntersetedWithTargetRect(toRemove.ElementTransform, viewport)) ||
                               (0 <= removeCount && RectTransformEx.IsNotIntersetedWithTargetRect(toRemove.ElementTransform, viewport)))
                            {
                                ++removeCount;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    } while (-1 < removeCount);

                    float rearTotalRemoveSize = 0f;
                    while (0 < removeCount && 0 < m_currentUsingElements.Count)
                    {
                        RecycleOneDirectionScrollElement toRemove = m_currentUsingElements[m_currentUsingElements.Count - 1];
                        if (IsVertical)
                        {
                            rearTotalRemoveSize += toRemove.ElementPreferredSize.y;
                        }
                        else if (IsHorizontal)
                        {
                            rearTotalRemoveSize += toRemove.ElementPreferredSize.x;
                        }
                        RemoveElementFromTail();
                        --removeCount;
                        hasRemoveElements = true;
                    }

                    if (0f < rearTotalRemoveSize)
                    {
                        // HACK Since I force the pivot of content, no need to adjust position at this case
                    }
                }
            }
            return hasRemoveElements;
        }

        private void RemoveElementFromHead()
        {
            RecycleOneDirectionScrollElement element = m_currentUsingElements[0];
            int dataIndex = element.ElementIndex;
            m_currentUsingElements.RemoveAt(0);
            InternalRemoveElement(element);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
            // Debug.LogError($"Remove on top data{dataIndex}");
        }

        private void RemoveElementFromTail()
        {
            int elementIndex = m_currentUsingElements.Count - 1;
            RecycleOneDirectionScrollElement element = m_currentUsingElements[elementIndex];
            int dataIndex = element.ElementIndex;
            m_currentUsingElements.RemoveAt(elementIndex);
            InternalRemoveElement(element);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
            // Debug.LogError($"Remove on bottom data{dataIndex}");
        }

        private bool AddElemensIfNeed()
        {
            bool hasAddToHead = AddElementsToHeadIfNeed();
            bool hasAddToTail = AddElementsToTailIfNeed();
            return hasAddToHead || hasAddToTail;
        }

        private bool AddElementsToHeadIfNeed()
        {
            RectTransform content = _scrollRect.content;
            RectTransform viewport = _scrollRect.viewport;
            bool hasAddElements = false;
            if (SIDE_STATUS_NEEDADD == CheckHeadSideStatus())
            {
                Vector2 headRectPos = CalculateNormalizedRectPosition(EDGE_HEAD);
                Vector2 prevHeadPos = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(content, headRectPos);
                prevHeadPos = viewport.InverseTransformPoint(prevHeadPos);
                Vector2 viewportHeadPos = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, headRectPos);
                Vector2 currentDelta = default;
                currentDelta.y = Mathf.Abs(viewportHeadPos.y - prevHeadPos.y);
                currentDelta.x = Mathf.Abs(currentDelta.x);
                float addSize = 0f;
                while (SIDE_STATUS_NEEDADD == CheckHeadSideStatus())
                {
                    int canAddIndex = CalculateAvaialbeNextHeadElementIndex();
                    if (-1 != canAddIndex)
                    {
                        AddElementToHead(canAddIndex);
                        if (IsVertical)
                        {
                            addSize += m_currentUsingElements[0].ElementPreferredSize.y;
                        }
                        else if (IsHorizontal)
                        {
                            addSize += m_currentUsingElements[0].ElementPreferredSize.x;
                        }
                    }
                    else
                    {
                        break;
                    }

                    if ((IsVertical && addSize > currentDelta.y) ||
                             (IsHorizontal && addSize > currentDelta.x))
                    {
                        break;
                    }
                }

                if (0f < addSize)
                {
                    // HACK Becuz I use a fixed pivot for content, so I can directly adjust local position
                    switch (_scrollParam.scrollDirection)
                    {
                        // Vertical
                        case ScrollDirection.Vertical_UpToDown:
                            content.localPosition += Vector3.up * addSize;
                            break;
                        case ScrollDirection.Vertical_DownToUp:
                            content.localPosition += Vector3.down * addSize;
                            break;

                        // Horizontal
                        case ScrollDirection.Horizontal_LeftToRight:
                            content.localPosition += Vector3.left * addSize;
                            break;
                        case ScrollDirection.Horizontal_RightToLeft:
                            content.localPosition += Vector3.right * addSize;
                            break;
                        default:
                            break;
                    }
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
                    hasAddElements = true;
                }
            }
            return hasAddElements;
        }

        private bool AddElementsToTailIfNeed()
        {
            RectTransform content = _scrollRect.content;
            RectTransform viewport = _scrollRect.viewport;
            bool hasAddElements = false;
            if (SIDE_STATUS_NEEDADD == CheckTailSideStatus())
            {
                Vector2 tailRectPos = CalculateNormalizedRectPosition(EDGE_TAIL);
                Vector2 prevTailPos = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(content, tailRectPos);
                prevTailPos = viewport.InverseTransformPoint(prevTailPos);
                Vector2 viewportTailPos = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, tailRectPos);
                Vector2 currentDelta = new Vector2(Mathf.Abs(viewportTailPos.x - prevTailPos.x), Mathf.Abs(viewportTailPos.y - prevTailPos.y));
                currentDelta.y = Mathf.Abs(viewportTailPos.y - prevTailPos.y);
                float totalAddSize = 0f;
                while (SIDE_STATUS_NEEDADD == CheckTailSideStatus())
                {
                    int canAddIndex = CalculateAvaialbeNextTailElementIndex();
                    if (-1 != canAddIndex)
                    {
                        AddElementToTail(canAddIndex);
                        if (IsVertical)
                        {
                            totalAddSize += m_currentUsingElements[m_currentUsingElements.Count - 1].ElementPreferredSize.y;
                        }
                        else if (IsHorizontal)
                        {
                            totalAddSize += m_currentUsingElements[m_currentUsingElements.Count - 1].ElementPreferredSize.x;
                        }

                        if ((IsVertical && totalAddSize > currentDelta.y) ||
                                 (IsHorizontal && totalAddSize > currentDelta.x))
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (0f < totalAddSize)
                {
                    // HACK Since I force the pivot of content, no need to adjust position at this case
                    hasAddElements = true;
                }
            }
            return hasAddElements;
        }

        /// <summary> The data index of the element for adding head </summary>
        /// <returns> -1 Means can not find valid index </returns>
        private int CalculateAvaialbeNextHeadElementIndex()
        {
            if (null == m_dataSource)
            {
                return -1;
            }

            if (0 == m_currentUsingElements.Count)
            {
                return _scrollParam.reverseArrangement ? m_dataSource.DataElementCount - 1 : 0;
            }

            int index = m_currentUsingElements[0].ElementIndex;
            bool isReachedLimit = _scrollParam.reverseArrangement ? index >= m_dataSource.DataElementCount - 1 : index <= 0;
            if (isReachedLimit)
            {
                return -1;
            }
            int dataIndex = _scrollParam.reverseArrangement ? (index + 1) : (index - 1);
            return dataIndex;
        }

        /// <summary> The data index of the element for adding tail </summary>
        /// <returns> -1 Means can not find valid index </returns>
        private int CalculateAvaialbeNextTailElementIndex()
        {
            if (null == m_dataSource)
            {
                return -1;
            }

            if (0 == m_currentUsingElements.Count)
            {
                return _scrollParam.reverseArrangement ? m_dataSource.DataElementCount - 1 : 0;
            }

            int index = m_currentUsingElements[m_currentUsingElements.Count - 1].ElementIndex;
            bool isReachedLimit = _scrollParam.reverseArrangement ? index <= 0 : index >= m_dataSource.DataElementCount - 1;
            if (isReachedLimit)
            {
                return -1;
            }
            int dataIndex = _scrollParam.reverseArrangement ? (index - 1) : (index + 1);
            return dataIndex;
        }

        private void AddElementToHead(int dataIndex)
        {
            RecycleOneDirectionScrollElement newElement = InternalCreateElement(dataIndex);
            m_currentUsingElements.Insert(0, newElement);
            newElement.CalculatePreferredSize();
            newElement.transform.SetAsFirstSibling();
            newElement.SetIndex(dataIndex);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
            // Debug.LogError($"Add on top data{dataIndex}");
        }

        private void AddElementToTail(int dataIndex)
        {
            RecycleOneDirectionScrollElement newElement = InternalCreateElement(dataIndex);
            m_currentUsingElements.Add(newElement);
            newElement.CalculatePreferredSize();
            newElement.transform.SetAsLastSibling();
            newElement.SetIndex(dataIndex);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
            // Debug.LogError($"Add on bottom data{dataIndex}");
        }

    }
}