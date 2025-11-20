using UnityEngine;
using UnityEngine.UI.Extend;
using ScrollDirection = RecycleScrollView.SingleDirectionScrollParam.ScrollDirection;

namespace RecycleScrollView
{
    public partial class RecycleSingleDirectionScroll
    {
        private const int SIDE_STATUS_ENOUGH = 0;
        private const int SIDE_STATUS_NEEDADD = -1;
        private const int SIDE_STATUS_NEEDREMOVE = 1;

        private const float EDGE_HEAD = 0F;
        private const float EDGE_TAIL = 1F;

        public void AddElementToHead(int dataIndex, bool forceRebuildlayout)
        {
            RecycleSingleDirectionScrollElement newElement = InternalCreateElement(dataIndex);
            m_currentUsingElements.Insert(0, newElement);
            newElement.CalculatePreferredSize();
            newElement.transform.SetAsFirstSibling();
            newElement.SetIndex(dataIndex);
            if (forceRebuildlayout)
            {
                ForceRebuildContentLayout();
            }
            // Debug.LogError($"Add on top data{dataIndex} Time {Time.time}");
        }

        public void AddElementToTail(int dataIndex, bool forceRebuildlayout)
        {
            RecycleSingleDirectionScrollElement newElement = InternalCreateElement(dataIndex);
            m_currentUsingElements.Add(newElement);
            newElement.CalculatePreferredSize();
            newElement.transform.SetAsLastSibling();
            newElement.SetIndex(dataIndex);
            if (forceRebuildlayout)
            {
                ForceRebuildContentLayout();
            }
            // Debug.LogError($"Add on bottom data{dataIndex} Time {Time.time}");
        }

        public void InsertElement(int dataIndex, bool forceRebuildlayout)
        {
            int indexUpperBound = GetCurrentShowingElementIndexUpperBound();
            if (dataIndex > indexUpperBound)
            {
                return;
            }

            bool hasAdded = false;
            if (_scrollParam.reverseArrangement)
            {
                for (int i = m_currentUsingElements.Count - 1; i >= 0; i--)
                {
                    RecycleSingleDirectionScrollElement element = m_currentUsingElements[i];
                    int elementIndex = element.ElementIndex;
                    if (dataIndex == elementIndex && !hasAdded)
                    {
                        RecycleSingleDirectionScrollElement newElement = InternalCreateElement(dataIndex);
                        newElement.ElementTransform.SetSiblingIndex(element.ElementTransform.GetSiblingIndex() + 1);
                        newElement.SetIndex(dataIndex);
                        m_currentUsingElements.Insert(i, newElement);
                        hasAdded = true;
                    }
                    else if (dataIndex < elementIndex && hasAdded)
                    {
                        InternalChangeElementIndex(element, elementIndex + 1, false);
                    }
                }
            }
            else
            {
                for (int i = 0, length = m_currentUsingElements.Count; i < length; i++)
                {
                    RecycleSingleDirectionScrollElement element = m_currentUsingElements[i];
                    int elementIndex = element.ElementIndex;
                    if (dataIndex == elementIndex && !hasAdded)
                    {
                        RecycleSingleDirectionScrollElement newElement = InternalCreateElement(dataIndex);
                        newElement.ElementTransform.SetSiblingIndex(element.ElementTransform.GetSiblingIndex());
                        newElement.SetIndex(dataIndex);
                        m_currentUsingElements.Insert(i, newElement);
                        length++;
                        hasAdded = true;
                    }
                    else if (dataIndex <= elementIndex && hasAdded)
                    {
                        InternalChangeElementIndex(element, elementIndex + 1, false);
                    }
                }
            }
            if (hasAdded)
            {
                if (forceRebuildlayout)
                {
                    ForceRebuildContentLayout();
                }
            }
        }

        public void RemoveElement(int dataIndex)
        {
            int indexUpperBound = GetCurrentShowingElementIndexUpperBound();
            if (dataIndex > indexUpperBound)
            {
                return;
            }

            bool hasRemoved = false;
            if (_scrollParam.reverseArrangement)
            {
                for (int i = m_currentUsingElements.Count - 1; i >= 0; i--)
                {
                    RecycleSingleDirectionScrollElement element = m_currentUsingElements[i];
                    int elementIndex = element.ElementIndex;
                    if (dataIndex == elementIndex && !hasRemoved)
                    {
                        m_currentUsingElements.RemoveAt(i);
                        InternalRemoveElement(element);
                        hasRemoved = true;
                        break;
                    }
                    else if (dataIndex < elementIndex && !hasRemoved)
                    {
                        InternalChangeElementIndex(element, elementIndex - 1, false);
                    }
                }
            }
            else
            {
                for (int i = 0, length = m_currentUsingElements.Count; i < length; i++)
                {
                    RecycleSingleDirectionScrollElement element = m_currentUsingElements[i];
                    int elementIndex = element.ElementIndex;
                    if (dataIndex == elementIndex && !hasRemoved)
                    {
                        m_currentUsingElements.RemoveAt(i);
                        length--;
                        i--;
                        InternalRemoveElement(element);
                        hasRemoved = true;
                    }
                    else if (dataIndex < elementIndex && hasRemoved)
                    {
                        InternalChangeElementIndex(element, elementIndex - 1, false);
                    }
                }
            }

            if (hasRemoved)
            {
                ForceRebuildContentLayout();
                ForceAdjustElements();
            }
        }

        /// <returns> -1 Need add, 0 Enough, 1 Need remove</returns>
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

            ScrollDirection checkDirection = _scrollParam.scrollDirection;
            switch (checkDirection)
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
                    break;
            }
            bool isBeyoudEdge = IsElementEdgeBeyoudViewportEdge(0, normalizedViewportEdgePosition: EDGE_HEAD, normalizedElementEdgePosition: EDGE_TAIL, checkDirection);
            if (isBeyoudEdge)
            {
                if (2 <= elementCount)
                {
                    isBeyoudEdge = IsElementEdgeBeyoudViewportEdge(1, normalizedViewportEdgePosition: EDGE_HEAD, normalizedElementEdgePosition: EDGE_TAIL, checkDirection);
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
            bool isBeyoudEdge = IsElementEdgeBeyoudViewportEdge(elementCount - 1, normalizedViewportEdgePosition: EDGE_TAIL, normalizedElementEdgePosition: EDGE_HEAD, checkDirection);
            if (isBeyoudEdge)
            {
                if (2 <= elementCount)
                {
                    isBeyoudEdge = IsElementEdgeBeyoudViewportEdge(elementCount - 2, normalizedViewportEdgePosition: EDGE_TAIL, normalizedElementEdgePosition: EDGE_HEAD, checkDirection);
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

        /// <param name="elementIndex"> Index in the list of current in using elements </param>
        /// <param name="normalizedElementEdgePosition"> Head(0) ~ Tail(1) </param>
        /// <param name="normalizedViewportEdgePosition"> Head(0) ~ Tail(1) </param>
        /// <returns></returns>
        private bool IsElementEdgeBeyoudViewportEdge(int elementIndex, float normalizedViewportEdgePosition, float normalizedElementEdgePosition, ScrollDirection checkDirection)
        {
            if (0 > elementIndex || elementIndex >= m_currentUsingElements.Count)
            {
                return false;
            }

            RectTransform viewport = _scrollRect.viewport;
            RectTransform content = _scrollRect.content;
            Vector2 viewportSize = viewport.rect.size;
            Vector2 viewportEdgeRectPosition = CalculateNormalizedRectPosition(normalizedViewportEdgePosition);
            viewportEdgeRectPosition = new Vector2(viewportSize.x * viewportEdgeRectPosition.x, viewportSize.y * viewportEdgeRectPosition.y);
            // ContentPivotRectPositionInViewport
            Vector2 baseRectPosition = RectTransformEx.TransformLocalPositionToRectPosition(viewport, content.localPosition);

            float tempSize = CalculateCurrentContentTotalPreferredSize(elementIndex);
            Vector2 elementEdgeRectPosition = CalculateNormalizedRectPosition(normalizedElementEdgePosition);
            RecycleSingleDirectionScrollElement element = m_currentUsingElements[elementIndex];
            float elementEdgePositionExtra = IsHorizontal ? element.ElementPreferredSize.x * elementEdgeRectPosition.x : element.ElementPreferredSize.y * elementEdgeRectPosition.y;
            tempSize += elementEdgePositionExtra;

            bool isBeyoudEdge = false;
            switch (_scrollParam.scrollDirection)
            {
                case ScrollDirection.Vertical_UpToDown:
                    baseRectPosition.y -= tempSize;
                    break;
                case ScrollDirection.Vertical_DownToUp:
                    baseRectPosition.y += tempSize;
                    break;

                case ScrollDirection.Horizontal_LeftToRight:
                    baseRectPosition.x += tempSize;
                    break;
                case ScrollDirection.Horizontal_RightToLeft:
                    baseRectPosition.x -= tempSize;
                    break;
                default:
                    break;
            }

            switch (checkDirection)
            {
                case ScrollDirection.Vertical_UpToDown:
                    isBeyoudEdge = baseRectPosition.y < viewportEdgeRectPosition.y;
                    break;
                case ScrollDirection.Vertical_DownToUp:
                    isBeyoudEdge = baseRectPosition.y > viewportEdgeRectPosition.y;
                    break;

                case ScrollDirection.Horizontal_LeftToRight:
                    isBeyoudEdge = baseRectPosition.x > viewportEdgeRectPosition.x;
                    break;
                case ScrollDirection.Horizontal_RightToLeft:
                    isBeyoudEdge = baseRectPosition.x < viewportEdgeRectPosition.x;
                    break;
                default:
                    break;
            }

            return isBeyoudEdge;
        }

        private bool RemoveElementsFromHeadIfNeed()
        {
            int prevElementCount = m_currentUsingElements.Count;
            if (0 < prevElementCount)
            {
                float removeSize = 0f;
                RectTransform content = _scrollRect.content;
                while (SIDE_STATUS_NEEDREMOVE == CheckHeadSideStatus() && -1 != CalculateAvailabeNextTailElementIndex())
                {
                    RecycleSingleDirectionScrollElement toRemove = m_currentUsingElements[0];
                    if (IsVertical)
                    {
                        removeSize = toRemove.ElementPreferredSize.y + _scrollParam.spacing;
                    }
                    else if (IsHorizontal)
                    {
                        removeSize = toRemove.ElementPreferredSize.x + _scrollParam.spacing;
                    }
                    RemoveElementFromHead();

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
                }
                return 0f < removeSize;
            }
            return false;
        }

        private bool RemoveElementsFromTailIfNeed()
        {
            bool hasRemoveElements = false;
            int prevElementCount = m_currentUsingElements.Count;
            if (0 < prevElementCount)
            {
                while (SIDE_STATUS_NEEDREMOVE == CheckTailSideStatus() && -1 != CalculateAvaialbeNextHeadElementIndex())
                {
                    RemoveElementFromTail();
                    hasRemoveElements = true;
                }
                // HACK Since I force the pivot of content, no need to adjust position at this case
            }
            return hasRemoveElements;
        }

        private bool AddElementsToHeadIfNeed()
        {
            RectTransform content = _scrollRect.content;
            float addSize = 0f;
            int canAddIndex;
            while (SIDE_STATUS_NEEDADD == CheckHeadSideStatus() && -1 != (canAddIndex = CalculateAvaialbeNextHeadElementIndex()))
            {
                AddElementToHead(canAddIndex, false);
                if (IsVertical)
                {
                    addSize += m_currentUsingElements[0].ElementPreferredSize.y + _scrollParam.spacing;
                }
                else if (IsHorizontal)
                {
                    addSize += m_currentUsingElements[0].ElementPreferredSize.x + _scrollParam.spacing;
                }

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
            }

            return 0f < addSize;
        }

        private bool AddElementsToTailIfNeed()
        {
            int addCount = 0;
            while (SIDE_STATUS_NEEDADD == CheckTailSideStatus())
            {
                int canAddIndex = CalculateAvailabeNextTailElementIndex();
                if (-1 != canAddIndex)
                {
                    AddElementToTail(canAddIndex, false);
                    addCount++;
                }
                else
                {
                    break;
                }
                // HACK Since I force the pivot of content, no need to adjust position at this case
            }
            return 0 < addCount;
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

        /// <summary> The data index of the element for adding head </summary>
        /// <returns> -1 Means it can not find valid index </returns>
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
        /// <returns> -1 Means it can not find valid index </returns>
        private int CalculateAvailabeNextTailElementIndex()
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

        private float CalculateCurrentContentTotalPreferredSize(int exceptIndex = -1)
        {
            float totalSize = 0f;
            int length = m_currentUsingElements.Count;
            for (int i = 0; i < length; i++)
            {
                if (-1 != exceptIndex && i == exceptIndex)
                {
                    break;
                }

                RecycleSingleDirectionScrollElement element = m_currentUsingElements[i];
                if (IsVertical)
                {
                    totalSize += element.ElementPreferredSize.y;
                }
                else if (IsHorizontal)
                {
                    totalSize += element.ElementPreferredSize.x;
                }

                if (i > 0 && i < length - 1)
                {
                    totalSize += _scrollParam.spacing;
                }
            }
            return totalSize;
        }

    }
}