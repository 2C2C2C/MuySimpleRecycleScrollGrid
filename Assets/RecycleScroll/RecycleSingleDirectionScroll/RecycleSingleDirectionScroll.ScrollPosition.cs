using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extend;
using ScrollDirection = RecycleScrollView.SingleDirectionScrollParam.ScrollDirection;
using ScrollbarDirection = UnityEngine.UI.Scrollbar.Direction;
using System;

namespace RecycleScrollView
{
    public partial class RecycleSingleDirectionScroll
    {
        internal struct TempPack
        {
            public int elementIndex;
            public float result;
            public TempPack(int i, float r)
            {
                elementIndex = i;
                result = r;
            }
        }

        private const float MIN_BAR_SIZE = 0.1f;

        [Header("ScrollBar params")]
        [SerializeField]
        private Scrollbar _scrollBar = null;

        [SerializeField]
        /// <summary> It is from 0 to 1 (As Scroll direction's head to tail) </summary>
        private float m_scrollProgress; // TODO Should be non-serialized but show in inspector
        [SerializeField]
        /// <summary> Damn, it is from 1 to 0 (As Scroll direction's head to tail) </summary>
        private float m_virtualNormalizedScrollBarValue; // TODO Should be non-serialized but show in inspector

        private int m_hasSetScrollBarValueThisFrame = 0;
        private List<TempPack> m_tempList = new List<TempPack>(20);
        private Comparison<TempPack> m_packSort = null;

        private void BindScrollBar()
        {
            if (null != _scrollBar)
            {
                _scrollBar.onValueChanged.AddListener(OnScrollBarValueChanged);
                _scrollBar.SetValueWithoutNotify(m_virtualNormalizedScrollBarValue = 1f - Mathf.Clamp01(m_scrollProgress));
            }
        }

        private void UnBindScrollBar()
        {
            if (null != _scrollBar)
            {
                _scrollBar.onValueChanged.RemoveListener(OnScrollBarValueChanged);
            }
        }

        private void ApplyLayoutSettingToScrollBar()
        {
            if (null != _scrollBar)
            {
                ScrollbarDirection barDirection = _scrollParam.scrollDirection switch
                {
                    ScrollDirection.Vertical_UpToDown => ScrollbarDirection.BottomToTop,
                    ScrollDirection.Vertical_DownToUp => ScrollbarDirection.TopToBottom,
                    ScrollDirection.Horizontal_LeftToRight => ScrollbarDirection.RightToLeft,
                    ScrollDirection.Horizontal_RightToLeft => ScrollbarDirection.LeftToRight,
                    _ => ScrollbarDirection.BottomToTop
                };
                _scrollBar.SetDirection(barDirection, false);
            }
        }

        private void AdjustScrollBarSize()
        {
            if (null == _scrollBar || !HasDataSource)
            {
                return;
            }

            // Adjust scroll bar size
            int dataCount = m_dataSource.DataElementCount;
            int currentShowingCount = m_currentUsingElements.Count;
            if (currentShowingCount >= dataCount)
            {
                _scrollBar.size = 1f;
            }
            else
            {
                float barSize = currentShowingCount / (float)dataCount;
                if (barSize < MIN_BAR_SIZE)
                {
                    barSize = MIN_BAR_SIZE;
                }
                _scrollBar.size = barSize;
            }
        }

        private void UpdateScrollProgress()
        {
            if (null == _scrollBar || !HasDataSource)
            {
                return;
            }

            int dataCount = m_dataSource.DataElementCount;
            int currentShowingCount = m_currentUsingElements.Count;
            if (2 > dataCount || currentShowingCount >= dataCount)
            {
                m_scrollProgress = 0f;
                _scrollBar.size = 1f;
                _scrollBar.SetValueWithoutNotify(m_virtualNormalizedScrollBarValue = 1f);
                return;
            }

            if (CalculateCurrentScrollProgress(out float scrollPogress) &&
                !Mathf.Approximately(scrollPogress, m_scrollProgress))
            {
                m_scrollProgress = scrollPogress;
                float scrollBarValue = 1f - m_scrollProgress;
                _scrollBar.SetValueWithoutNotify(m_virtualNormalizedScrollBarValue = scrollBarValue);
            }
        }

        /// <param name="scrollProgress"> 0 ~ 1 (head to tail)</param>
        private bool TryGetRefElementFormScrollProgress(float scrollProgress, out int elementIndex, out float normalizedBaseScrollProgress, out float normalizedScrollProgressOffset)
        {
            if (null != m_dataSource)
            {
                int dataCount = m_dataSource.DataElementCount;
                if (Mathf.Approximately(0f, scrollProgress))
                {
                    elementIndex = 0;
                    normalizedScrollProgressOffset = 0f;
                    normalizedBaseScrollProgress = 0f;
                }
                else if (Mathf.Approximately(0f, scrollProgress))
                {
                    elementIndex = dataCount - 1;
                    normalizedBaseScrollProgress = 1f;
                    normalizedScrollProgressOffset = 0f;
                }
                else
                {
                    float stepSize = 1f / (dataCount - 1);
                    int stepLowBoundElementIndex = -1;
                    float temp = 0f;
                    for (int i = 0; i < dataCount - 1; i++)
                    {
                        stepLowBoundElementIndex++;
                        if (Mathf.Approximately(temp, scrollProgress) || temp > scrollProgress)
                        {
                            break;
                        }
                        temp += stepSize;
                    }
                    // int stepHighBoundElementIndex = Mathf.Clamp(stepLowBoundElementIndex + 1, stepLowBoundElementIndex, dataCount - 1);
                    normalizedBaseScrollProgress = stepLowBoundElementIndex * stepSize;
                    elementIndex = stepLowBoundElementIndex;
                    normalizedScrollProgressOffset = scrollProgress - normalizedBaseScrollProgress;
                }
                return true;
            }
            elementIndex = -1;
            normalizedBaseScrollProgress = 0f;
            normalizedScrollProgressOffset = 0f;
            return false;
        }

        /// <returns> Nomralized value (0~1) </returns>
        private bool CalculateCurrentScrollProgress(out float result)
        {
            if (null == m_dataSource)
            {
                result = 0f;
                return false;
            }

            int elementCount = m_currentUsingElements.Count;
            bool canCalculatValidPos = false;
            // string debugMsg = "";
            for (int i = 0; i < elementCount; i++)
            {
                RecycleSingleDirectionScrollElement element = m_currentUsingElements[i];
                canCalculatValidPos = TryCalculateScrollProgressFromElement(element, out float expectedNormalizedBasePosition, out float finalizedPosition);
                // debugMsg += $"Element_{element.ElementIndex}_{canCalculatValidPos}; expectedNormalizedBasePosition {expectedNormalizedBasePosition}; deltaToExpectedPosition {deltaToExpectedPosition}; finalizedPosition {finalizedPosition} \n";
                if (canCalculatValidPos)
                {
                    m_tempList.Add(new TempPack(element.ElementIndex, finalizedPosition));
                }
            }

            if (0 == m_tempList.Count)
            {
                Debug.LogError($"Can not calculate var value");
                result = 0f;
                for (int i = 0; i < elementCount; i++)
                {
                    RecycleSingleDirectionScrollElement element = m_currentUsingElements[i];
                    canCalculatValidPos = TryCalculateScrollProgressFromElement(element, out float expectedNormalizedBasePosition, out float finalizedPosition);
                    // debugMsg += $"Element_{element.ElementIndex}_{canCalculatValidPos}; expectedNormalizedBasePosition {expectedNormalizedBasePosition}; deltaToExpectedPosition {deltaToExpectedPosition}; finalizedPosition {finalizedPosition} \n";
                    if (canCalculatValidPos)
                    {
                        m_tempList.Add(new TempPack(element.ElementIndex, finalizedPosition));
                    }
                }
                return false;
            }

            if (null == m_packSort)
            {
                // Should use elements the nearer to the head/ tail edge
                m_packSort = (x, y) =>
                {
                    int dataCount = m_dataSource.DataElementCount;
                    float half = (0 == dataCount % 2) ? dataCount / 2 - 0.5f : dataCount / 2;
                    float deltaX = Mathf.Abs(x.elementIndex - half);
                    float deltaY = Mathf.Abs(y.elementIndex - half);
                    return deltaY.CompareTo(deltaX);
                };
            }

            m_tempList.Sort(m_packSort);
            result = m_tempList[0].result;
            m_tempList.Clear();
            result = Mathf.Clamp01(result);
            return true;
        }

        /// <param name="expectedRectPosInViewport"> expectedRectPosInViewport </param>
        /// <returns></returns>
        private bool TryGetDeltaFromElementToExpectedPosition(int dataIndex, out float delta, out float expectedRectPosInViewport)
        {
            if (TryGetShowingElement(dataIndex, out RecycleSingleDirectionScrollElement element))
            {
                delta = GetDeltaFromElementToExpectedPosition(element, out expectedRectPosInViewport);
                return true;
            }
            delta = expectedRectPosInViewport = 0f;
            return false;
        }

        /// <param name="elementIndex"></param>
        /// <returns> 0 ~ 1 (head ~ tail)</returns>
        private float CalculateExpectedPositionForData(int elementIndex)
        {
            int dataCount = m_dataSource.DataElementCount;
            float step = 1f / (dataCount - 1);
            float result = step * elementIndex;
            return result;
        }

        /// <param name="expectedRectPosInViewport"> expectedRectPosInViewport </param>
        /// <returns></returns>
        private float GetDeltaFromElementToExpectedPosition(RecycleSingleDirectionScrollElement element, out float expectedRectPosInViewport)
        {
            int dataCount = m_dataSource.DataElementCount;
            int gapCount = dataCount - 1;
            int index = element.ElementIndex;
            RectTransform viewport = _scrollRect.viewport;
            float step = 1f / gapCount;
            float tempPos = 1f - step * index;

            Vector2 elementTempLocalPositionInViewport = new Vector2(0f, tempPos);
            Vector3 tempV3 = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(element.ElementTransform, elementTempLocalPositionInViewport);
            elementTempLocalPositionInViewport = viewport.InverseTransformPoint(tempV3);
            Vector2 viewportExpectedLocalPosition = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, new Vector2(0f, tempPos));

            float delta = elementTempLocalPositionInViewport.y - viewportExpectedLocalPosition.y;
            expectedRectPosInViewport = tempPos;
            return delta;
        }

        // TODO Deal with different direction and arrangement
        private bool TryCalculateCurrentPositionFromElement(int elementIndex, out float expectedNormalizedBasePosition, out float finalizedPosition)
        {
            if (TryGetShowingElement(elementIndex, out RecycleSingleDirectionScrollElement element))
            {
                return TryCalculateScrollProgressFromElement(element, out expectedNormalizedBasePosition, out finalizedPosition);
            }
            expectedNormalizedBasePosition = finalizedPosition = 0f;
            return false;
        }

        // TODO Deal with different direction and arrangement
        private bool TryCalculateScrollProgressFromElement(RecycleSingleDirectionScrollElement element, out float expectedNormalizedBasePosition, out float finalizedNormalizedPosition)
        {
            finalizedNormalizedPosition = expectedNormalizedBasePosition = 0f;
            if (null == m_dataSource)
            {
                return false;
            }

            int elementCount = m_dataSource.DataElementCount;
            int elementIndex = element.ElementIndex;
            float stepSize = 1f / (elementCount - 1);

            if (0 == elementIndex)
            {
                finalizedNormalizedPosition = expectedNormalizedBasePosition = 0f;
            }
            else if (elementCount - 1 == elementIndex)
            {
                finalizedNormalizedPosition = expectedNormalizedBasePosition = 1f;
            }
            else
            {
                finalizedNormalizedPosition = expectedNormalizedBasePosition = stepSize * elementIndex;
            }

            Vector2 convertedNormalizedRectPosition = CalculateNormalizedRectPosition(finalizedNormalizedPosition);
            Vector2 elementTempLocalPositionInViewport = convertedNormalizedRectPosition;
            Vector3 tempV3 = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(element.ElementTransform, elementTempLocalPositionInViewport);
            RectTransform viewport = _scrollRect.viewport;
            elementTempLocalPositionInViewport = viewport.InverseTransformPoint(tempV3);

            float deltaToExpectedPosition = 0f;
            Vector2 viewportExpectedLocalPosition = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, convertedNormalizedRectPosition);
            if (IsVertical)
            {
                deltaToExpectedPosition = viewportExpectedLocalPosition.y - elementTempLocalPositionInViewport.y;
                if (Mathf.Approximately(0f, deltaToExpectedPosition))
                {
                    return true;
                }

                // In DownToUp a positive delta indicates "greater than base"; in UpToDown a negative delta indicates "greater than base".
                bool greaterIfPositive = ScrollDirection.Vertical_DownToUp == _scrollParam.scrollDirection;

                // Greater than pre-calculated base position -> try gap to next element
                if ((greaterIfPositive && deltaToExpectedPosition > 0f) || (!greaterIfPositive && deltaToExpectedPosition < 0f))
                {
                    if (TryCalculateGapBetweenElement(elementIndex, elementIndex + 1, out float gapSize) &&
                        Mathf.Abs(deltaToExpectedPosition) < gapSize)
                    {
                        float normalizedDelta = Mathf.Clamp01(Mathf.Abs(deltaToExpectedPosition) / gapSize);
                        finalizedNormalizedPosition = expectedNormalizedBasePosition + stepSize * normalizedDelta;
                        return true;
                    }
                    else if (elementCount - 1 == elementIndex)
                    {
                        finalizedNormalizedPosition = expectedNormalizedBasePosition = 1f;
                        return true;
                    }
                    else
                    {
                        // Debug.LogError($"Check {elementIndex} {expectedNormalizedBasePosition} {deltaToExpectedPosition}");
                        int tempNextIndex = elementIndex + 1;
                        if (TryCalculateGapBetweenElement(tempNextIndex, tempNextIndex + 1, out float nextGap))
                        {
                            float tempDelta = Mathf.Abs(deltaToExpectedPosition) - gapSize;
                            tempDelta = stepSize * (tempDelta / nextGap);
                            expectedNormalizedBasePosition += stepSize;
                            finalizedNormalizedPosition = expectedNormalizedBasePosition + tempDelta;
                            return true;
                        }
                    }
                }
                // Less than pre-calculated base position -> try gap to previous element
                else if ((greaterIfPositive && deltaToExpectedPosition < 0f) || (!greaterIfPositive && deltaToExpectedPosition > 0f))
                {
                    if (TryCalculateGapBetweenElement(elementIndex - 1, elementIndex, out float gapSize) &&
                        Mathf.Abs(deltaToExpectedPosition) < gapSize)
                    {
                        expectedNormalizedBasePosition -= stepSize;
                        deltaToExpectedPosition = gapSize - deltaToExpectedPosition;
                        finalizedNormalizedPosition = expectedNormalizedBasePosition + stepSize * Mathf.Clamp01(deltaToExpectedPosition / gapSize);
                        return true;
                    }
                    else if (0 == elementIndex)
                    {
                        finalizedNormalizedPosition = expectedNormalizedBasePosition = 0f;
                        return true;
                    }
                    else
                    {
                        // Debug.LogError($"Check {elementIndex} {expectedNormalizedBasePosition} {deltaToExpectedPosition}");
                        int tempPrevIndex = elementIndex - 1;
                        if (TryCalculateGapBetweenElement(tempPrevIndex - 1, tempPrevIndex, out float nextGap))
                        {
                            float tempDelta = Mathf.Abs(deltaToExpectedPosition) - gapSize;
                            tempDelta = stepSize * (tempDelta / nextGap);
                            expectedNormalizedBasePosition -= stepSize;
                            finalizedNormalizedPosition = expectedNormalizedBasePosition - tempDelta;
                            return true;
                        }
                    }
                }
            }
            else if (IsHorizontal)
            {
                // Use X axis for horizontal directions
                deltaToExpectedPosition = viewportExpectedLocalPosition.x - elementTempLocalPositionInViewport.x;

                if (Mathf.Approximately(0f, deltaToExpectedPosition))
                {
                    return true;
                }

                // For RightToLeft a positive delta indicates "greater than base";
                bool greaterIfPositive = ScrollDirection.Horizontal_LeftToRight == _scrollParam.scrollDirection;

                // Greater than pre-calculated base position -> try gap to next element
                if ((greaterIfPositive && deltaToExpectedPosition > 0f) || (!greaterIfPositive && deltaToExpectedPosition < 0f))
                {
                    if (TryCalculateGapBetweenElement(elementIndex, elementIndex + 1, out float gapSize) &&
                        Mathf.Abs(deltaToExpectedPosition) < gapSize)
                    {
                        float normalizedDelta = Mathf.Abs(deltaToExpectedPosition) / gapSize;
                        finalizedNormalizedPosition = expectedNormalizedBasePosition + stepSize * normalizedDelta;
                        return true;
                    }
                    else if (elementCount - 1 == elementIndex)
                    {
                        finalizedNormalizedPosition = expectedNormalizedBasePosition = 1f;
                        return true;
                    }
                }
                // Less than pre-calculated base position -> try gap to previous element
                else if ((greaterIfPositive && deltaToExpectedPosition < 0f) || (!greaterIfPositive && deltaToExpectedPosition > 0f))
                {
                    if (TryCalculateGapBetweenElement(elementIndex - 1, elementIndex, out float gapSize) &&
                        Mathf.Abs(deltaToExpectedPosition) < gapSize)
                    {
                        expectedNormalizedBasePosition -= stepSize;
                        deltaToExpectedPosition = gapSize - deltaToExpectedPosition;
                        finalizedNormalizedPosition = expectedNormalizedBasePosition + stepSize * Mathf.Clamp01(deltaToExpectedPosition / gapSize);
                        return true;
                    }
                    else if (0 == elementIndex)
                    {
                        finalizedNormalizedPosition = expectedNormalizedBasePosition = 0f;
                        return true;
                    }
                }
            }

            return false;
        }

        private void OnScrollBarValueChanged(float scrollbarValue)
        {
            _scrollBar.SetValueWithoutNotify(m_virtualNormalizedScrollBarValue = 1f - Mathf.Clamp01(m_scrollProgress));
            // TODO
            // float convertedValue = Mathf.Clamp01(scrollbarValue);
            // m_hasSetScrollBarValueThisFrame = 1;
            // if (Mathf.Approximately(convertedValue, m_virtualNormalizedScrollBarValue))
            // {
            //     _scrollBar.SetValueWithoutNotify(m_virtualNormalizedScrollBarValue);
            //     // Debug.LogError($"wanna set scroll 01 {m_virtualNormalizedScrollBarValue} -> {scrollbarValue}_({convertedValue}) || FALSE; Frame {Time.frameCount}");
            //     return;
            // }

            // if (TryApplyNormalizedPosition(convertedValue))
            // {
            //     m_hasAdjustElementsCurrentFrame = true;
            //     // Debug.LogError($"wanna set scroll {m_virtualNormalizedScrollBarValue} -> {scrollbarValue}_({convertedValue}) || TRUE; Frame {Time.frameCount}");
            //     m_virtualNormalizedScrollBarValue = convertedValue;
            //     _scrollBar.SetValueWithoutNotify(m_virtualNormalizedScrollBarValue);
            // }
            // else
            // {
            //     Debug.LogError($"wanna set scroll {m_virtualNormalizedScrollBarValue} -> {scrollbarValue}_({convertedValue}) || FALSE; Frame {Time.frameCount}");
            //     _scrollBar.SetValueWithoutNotify(m_virtualNormalizedScrollBarValue);
            // }
        }

    }
}