using UnityEngine;
using ScrollDirection = RecycleScrollView.SingleDirectionScrollParam.ScrollDirection;

namespace RecycleScrollView
{
    public partial class RecycleSingleDirectionScroll
    {
        /// <param name="elementIndex"> Index from scroll progress </param>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool TryGetShowingElement(int elementIndex, out RecycleSingleDirectionScrollElement element)
        {
            for (int i = 0, length = m_currentUsingElements.Count; i < length; i++)
            {
                if (m_currentUsingElements[i].ElementIndex == elementIndex)
                {
                    element = m_currentUsingElements[i];
                    return true;
                }
            }
            element = null;
            return false;
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

        private bool TryCalculateGapBetweenElement(int lowElementIndex, int highElementIndex, out float gapSize)
        {
            if (lowElementIndex >= highElementIndex)
            {
                gapSize = 0f;
                return false;
            }

            if (TryGetShowingElement(lowElementIndex, out RecycleSingleDirectionScrollElement lowElement) &&
                TryGetShowingElement(highElementIndex, out RecycleSingleDirectionScrollElement highElement))
            {
                float lowBoundPosition = CalculateExpectedPositionForData(lowElementIndex);
                Vector2 lowElementSize = lowElement.ElementPreferredSize;

                float hightBoundPosition = CalculateExpectedPositionForData(highElementIndex);
                Vector2 highElementBSize = highElement.ElementPreferredSize;

                // From low element to high element
                if (IsHorizontal)
                {
                    gapSize = (lowElementSize.x * (1f - lowBoundPosition)) + (highElementBSize.x * hightBoundPosition);
                }
                else if (IsVertical)
                {
                    gapSize = (lowElementSize.y * (1f - lowBoundPosition)) + (highElementBSize.y * hightBoundPosition);
                }
                else
                {
                    gapSize = 0f;
                    return false;
                }
                gapSize += _scrollParam.spacing;
                return true;
            }
            gapSize = 0f;
            return false;
        }

        // To solve reverse arrangement issues
        private int ElementIndexDataIndex2WayConvert(int index)
        {
            if (null == m_dataSource)
            {
                return -1;
            }
            int dataCount = m_dataSource.DataElementCount;
            int result = _scrollParam.reverseArrangement ?
                dataCount - index - 1 :
                index;
            return result;
        }

        /// <summary> The element index of the element for adding head </summary>
        /// <returns> -1 Means it can not find valid index </returns>
        private int CalculateAvailabeNextHeadElementIndex()
        {
            if (null == m_dataSource || 0 == m_dataSource.DataElementCount)
            {
                return -1;
            }
            if (0 == m_currentUsingElements.Count)
            {
                return 0;
            }

            int index = m_currentUsingElements[0].ElementIndex;
            if (0 == index)
            {
                return -1;
            }
            return index - 1;
        }

        /// <summary> The element index of the element for adding tail </summary>
        /// <returns> -1 Means it can not find valid index </returns>
        private int CalculateAvailabeNextTailElementIndex()
        {
            if (null == m_dataSource || 0 == m_dataSource.DataElementCount)
            {
                return -1;
            }
            if (0 == m_currentUsingElements.Count)
            {
                return 0;
            }

            int dataCount = m_dataSource.DataElementCount;
            int index = m_currentUsingElements[m_currentUsingElements.Count - 1].ElementIndex;
            if (dataCount - 1 == index)
            {
                return -1;
            }
            return index + 1;
        }

        public int GetCurrentShowingElementIndexLowerBound()
        {
            int elementCount = m_currentUsingElements.Count;
            return (0 < elementCount) ? m_currentUsingElements[elementCount - 1].ElementIndex : -1;
        }

        public int GetCurrentShowingElementIndexUpperBound()
        {
            int elementCount = m_currentUsingElements.Count;
            return (0 < elementCount) ? m_currentUsingElements[0].ElementIndex : -1;
        }

        private Vector2 GetScrollDirectionVector(ScrollDirection scrollDirection)
        {
            Vector2 result = scrollDirection switch
            {
                ScrollDirection.Vertical_UpToDown => Vector2.down,
                ScrollDirection.Vertical_DownToUp => Vector2.up,
                ScrollDirection.Horizontal_LeftToRight => Vector2.right,
                ScrollDirection.Horizontal_RightToLeft => Vector2.left,
                _ => Vector2.zero,
            };
            return result;
        }

    }
}