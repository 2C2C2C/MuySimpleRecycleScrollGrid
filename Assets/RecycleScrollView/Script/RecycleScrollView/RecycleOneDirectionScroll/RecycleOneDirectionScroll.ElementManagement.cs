using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extend;

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

            RecycleOneDirectionScrollElement headElement = m_currentUsingElements[0];
            bool isBeyoudEdge = IsElementEdgeBeyoudViewportEdge(headElement, EDGE_TAIL, EDGE_HEAD);
            if (isBeyoudEdge)
            {
                if (2 <= elementCount)
                {
                    RecycleOneDirectionScrollElement head2ndElement = m_currentUsingElements[1];
                    isBeyoudEdge = IsElementEdgeBeyoudViewportEdge(head2ndElement, EDGE_HEAD, EDGE_HEAD);
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
            // return SIDE_STATUS_ENOUGH;
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

            RecycleOneDirectionScrollElement tailElement = m_currentUsingElements[elementCount - 1];
            bool isBeyoudEdge = IsElementEdgeBeyoudViewportEdge(tailElement, EDGE_HEAD, EDGE_TAIL);
            if (isBeyoudEdge)
            {
                if (2 <= elementCount)
                {
                    RecycleOneDirectionScrollElement tail2ndElement = m_currentUsingElements[elementCount - 2];
                    isBeyoudEdge = IsElementEdgeBeyoudViewportEdge(tail2ndElement, EDGE_HEAD, EDGE_TAIL);
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
            if (IsHorizontal)
            {
                result = _scrollParam.reverseArrangement ?
                    new Vector2(Mathf.Lerp(1f, 0f, normalizedValue), 0.5f) :
                    new Vector2(Mathf.Lerp(0f, 1f, normalizedValue), 0.5f);
            }
            if (IsVertical)
            {
                result = _scrollParam.reverseArrangement ?
                    new Vector2(0.5f, Mathf.Lerp(0f, 1f, normalizedValue)) :
                    new Vector2(0.5f, Mathf.Lerp(1f, 0f, normalizedValue));
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
        private bool IsElementEdgeBeyoudViewportEdge(RecycleOneDirectionScrollElement element, float normalizedElementEdgePosition, float normalizedViewportEdgePosition)
        {
            RectTransform viewport = _scrollRect.viewport;
            Vector2 viewportHeadEdgeRectPosition = CalculateNormalizedRectPosition(normalizedViewportEdgePosition);
            Vector2 viewportEdge = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, viewportHeadEdgeRectPosition);

            Vector2 headElementEdgeRectPosition = CalculateNormalizedRectPosition(normalizedElementEdgePosition);
            Vector2 headElementEdge = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(element.ElementTransform, headElementEdgeRectPosition);
            bool isBeyoudEdge = false;

            if (IsVertical)
            {
                isBeyoudEdge = _scrollParam.reverseArrangement ?
                    headElementEdge.y < viewportEdge.y :
                    headElementEdge.y > viewportEdge.y;
            }
            else if (IsHorizontal)
            {
                isBeyoudEdge = _scrollParam.reverseArrangement ?
                    headElementEdge.x > viewportEdge.x :
                    headElementEdge.x < viewportEdge.x;
            }

            return isBeyoudEdge;
        }

        private bool RemoveElementsIfNeed()
        {
            bool hasRemoveHeadElements = RemoveHeadElementsIfNeed();
            bool hasRemoveTailElements = RemoveTailElementsIfNeed();
            return hasRemoveHeadElements || hasRemoveTailElements;
        }

        private bool RemoveHeadElementsIfNeed()
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

                    Vector2 prevFrontPos = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(content, Vector2.up);
                    prevFrontPos = viewport.InverseTransformPoint(prevFrontPos);
                    float frontTotalRemoveSize = 0f;
                    while (0 < frontRemoveElementCount && 0 < m_currentUsingElements.Count)
                    {
                        RecycleOneDirectionScrollElement toRemove = m_currentUsingElements[0];
                        frontTotalRemoveSize += toRemove.ElementPreferredSize.y;
                        RemoveElementFromHead();
                        --frontRemoveElementCount;
                        hasRemoveElements = true;
                    }

                    if (0f < frontTotalRemoveSize)
                    {
                        Vector2 currentFrontPos = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(content, Vector2.up);
                        currentFrontPos = viewport.InverseTransformPoint(currentFrontPos);
                        // HACK Calculate how much movement need to apply to put the element same position
                        prevFrontPos.y -= frontTotalRemoveSize;

                        float normalizedDelta = (currentFrontPos.y - prevFrontPos.y) / (content.rect.height - viewport.rect.height);
                        _scrollRect.verticalNormalizedPosition += normalizedDelta;
                        LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
                    }
                }
            }
            return hasRemoveElements;
        }

        private bool RemoveTailElementsIfNeed()
        {
            RectTransform viewport = _scrollRect.viewport;
            RectTransform content = _scrollRect.content;
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
                    } while (-1 < removeCount);

                    Vector2 prevRearPos = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(content, Vector2.zero);
                    prevRearPos = viewport.InverseTransformPoint(prevRearPos);
                    float rearTotalRemoveSize = 0f;
                    while (0 < removeCount && 0 < m_currentUsingElements.Count)
                    {
                        RecycleOneDirectionScrollElement toRemove = m_currentUsingElements[m_currentUsingElements.Count - 1];
                        rearTotalRemoveSize += toRemove.ElementTransform.rect.height;
                        RemoveElementFromTail();
                        --removeCount;
                        hasRemoveElements = true;
                    }

                    if (0f < rearTotalRemoveSize)
                    {
                        Vector2 currentRearPos = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(content, Vector2.zero);
                        currentRearPos = viewport.InverseTransformPoint(currentRearPos);
                        // HACK Calculate how much movement need to apply to put the element same position
                        prevRearPos.y += rearTotalRemoveSize;

                        float normalizedDelta = (currentRearPos.y - prevRearPos.y) / (content.rect.height - viewport.rect.height);
                        _scrollRect.verticalNormalizedPosition -= normalizedDelta;
                        LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
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
            bool hasAddToHead = AddHeadElementsIfNeed();
            bool hasAddToTail = AddTailElementsIfNeed();
            return hasAddToHead || hasAddToTail;
        }

        private bool AddHeadElementsIfNeed()
        {
            RectTransform content = _scrollRect.content;
            RectTransform viewport = _scrollRect.viewport;
            bool hasAddElements = false;
            if (SIDE_STATUS_NEEDADD == CheckHeadSideStatus())
            {
                Vector2 prevTopPos = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(content, Vector2.up);
                prevTopPos = viewport.InverseTransformPoint(prevTopPos);
                float addSize = 0f;
                while (SIDE_STATUS_NEEDADD == CheckHeadSideStatus())
                {
                    RecycleOneDirectionScrollElement frontElement = m_currentUsingElements[0];
                    if (1 <= frontElement.ElementIndex)
                    {
                        AddElementToHead(frontElement.ElementIndex - 1);
                        addSize = m_currentUsingElements[0].ElementPreferredSize.y;
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
                if (0f < addSize)
                {
                    Vector2 currentTopPos = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(content, Vector2.up);
                    currentTopPos = viewport.InverseTransformPoint(currentTopPos);
                    prevTopPos.y += addSize;

                    float normalizedDelta = (currentTopPos.y - prevTopPos.y) / (content.rect.height - viewport.rect.height);
                    _scrollRect.verticalNormalizedPosition += normalizedDelta;
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
                    hasAddElements = true;
                }
            }
            return hasAddElements;
        }

        private bool AddTailElementsIfNeed()
        {
            RectTransform content = _scrollRect.content;
            RectTransform viewport = _scrollRect.viewport;
            bool hasAddElements = false;
            if (SIDE_STATUS_NEEDADD == CheckTailSideStatus())
            {
                float addSize = 0f;
                Vector2 prevBottomPos = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(content, Vector2.zero);
                prevBottomPos = viewport.InverseTransformPoint(prevBottomPos);
                while (SIDE_STATUS_NEEDADD == CheckTailSideStatus())
                {
                    RecycleOneDirectionScrollElement rearElement = m_currentUsingElements[m_currentUsingElements.Count - 1];
                    if (m_dataSource.DataElementCount - 1 > rearElement.ElementIndex)
                    {
                        AddElementToTail(rearElement.ElementIndex + 1);
                        addSize = m_currentUsingElements[m_currentUsingElements.Count - 1].ElementTransform.rect.height;
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
                if (0f < addSize)
                {
                    Vector2 currentBottomPos = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(content, Vector2.zero);
                    currentBottomPos = viewport.InverseTransformPoint(currentBottomPos);
                    // HACK Calculate how much movement need to apply to put the element same position
                    prevBottomPos.y -= addSize;

                    float normalizedDelta = (currentBottomPos.y - prevBottomPos.y) / (content.rect.height - viewport.rect.height);
                    _scrollRect.verticalNormalizedPosition += normalizedDelta;
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
                    hasAddElements = true;
                }
            }
            return hasAddElements;
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