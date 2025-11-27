using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extend;
using ScrollDirection = RecycleScrollView.SingleDirectionScrollParam.ScrollDirection;
using ScrollbarDirection = UnityEngine.UI.Scrollbar.Direction;
using System.Drawing;
using Microsoft.Unity.VisualStudio.Editor;
using System.Runtime.InteropServices;
using System.Reflection;

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

        private const float TEMP_MIN_VALUE = 0.001f;
        private const float MIN_BAR_SIZE = 0.1f;

        [Header("ScrollBar related")]
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

        public float tempValue = 0f;
        [ContextMenu(nameof(TempSetValue))]
        public void TempSetValue()
        {
            OnScrollBarValueChanged(tempValue);
        }

        private void BindScrollBar()
        {
            if (null != _scrollBar)
            {
                _scrollBar.onValueChanged.AddListener(OnScrollBarValueChanged);
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

        private void UpdateScrollBarPosition()
        {
            if (null == _scrollBar || !HasDataSource)
            {
                return;
            }

            int dataCount = m_dataSource.DataElementCount;
            int currentShowingCount = m_currentUsingElements.Count;
            if (2 > dataCount || currentShowingCount >= dataCount)
            {
                _scrollBar.size = 1f;
                _scrollBar.SetValueWithoutNotify(0f);
                return;
            }

            float nextPos = CalculateCurrentNormalizedPosition();
            if (!Mathf.Approximately(nextPos, m_virtualNormalizedScrollBarValue) && Mathf.Abs(nextPos - m_virtualNormalizedScrollBarValue) > TEMP_MIN_VALUE)
            {
                nextPos = CalculateCurrentNormalizedPosition();
                Debug.LogError($"sync {m_virtualNormalizedScrollBarValue} -> {nextPos} to bar; Frame {Time.frameCount}");
                m_virtualNormalizedScrollBarValue = CalculateCurrentNormalizedPosition();
                _scrollBar.SetValueWithoutNotify(m_virtualNormalizedScrollBarValue);
                UpdateScrollBarVisual();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="normalizedPosition"> 1 ~ 0 (1 means at the start)</param>
        private bool TryApplyNormalizedPosition(float normalizedPosition)
        {
            if (TryGetRefElementFormScrollBarValue(normalizedPosition, out int refElementIndex, out float normalizedBaseScrollProgress, out float normalizedScrollProgressOffset))
            {
                JumpToExistElementInstant(refElementIndex, normalizedBaseScrollProgress, normalizedScrollProgressOffset);
                return true;
            }
            Debug.LogError($"eee");
            return false;
        }

        private bool TryGetRefElementFormScrollBarValue(float scrollbarValue, out int elementIndex, out float normalizedBaseScrollProgress, out float normalizedScrollProgressOffset)
        {
            if (null != m_dataSource)
            {
                scrollbarValue = Mathf.Clamp01(1f - scrollbarValue);
                int dataCount = m_dataSource.DataElementCount;
                if (Mathf.Approximately(0f, scrollbarValue))
                {
                    elementIndex = 0;
                    normalizedScrollProgressOffset = 0f;
                    normalizedBaseScrollProgress = 0f;
                }
                else if (Mathf.Approximately(0f, scrollbarValue))
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
                        if (Mathf.Approximately(temp, scrollbarValue) || temp > scrollbarValue)
                        {
                            break;
                        }
                        temp += stepSize;
                    }
                    // int stepHighBoundElementIndex = Mathf.Clamp(stepLowBoundElementIndex + 1, stepLowBoundElementIndex, dataCount - 1);
                    normalizedBaseScrollProgress = stepLowBoundElementIndex * stepSize;
                    elementIndex = stepLowBoundElementIndex;
                    normalizedScrollProgressOffset = scrollbarValue - normalizedBaseScrollProgress;
                }
                return true;
            }
            elementIndex = -1;
            normalizedBaseScrollProgress = 0f;
            normalizedScrollProgressOffset = 0f;
            return false;
        }

        private float CalculateCurrentNormalizedPosition()
        {
            if (null == m_dataSource)
            {
                return 0f;
            }

            int elementCount = m_currentUsingElements.Count;
            string debugMsg = "";
            for (int i = 0; i < elementCount; i++)
            {
                RecycleSingleDirectionScrollElement element = m_currentUsingElements[i];
                bool canCalculatValidPos = TryCalculateCurrentPositionFromElement(element, out float expectedNormalizedBasePosition, out float deltaToExpectedPosition, out float finalizedPosition);
                debugMsg += $"Element_{element.ElementIndex}_{canCalculatValidPos}; expectedNormalizedBasePosition {expectedNormalizedBasePosition}; deltaToExpectedPosition {deltaToExpectedPosition}; finalizedPosition {finalizedPosition} \n";
                if (canCalculatValidPos)
                {
                    m_tempList.Add(new TempPack(i, finalizedPosition));
                }
            }

            float tempResult = 0f;
            for (int i = 0, length = m_tempList.Count; i < length; i++)
            {
                tempResult += 1f - m_tempList[i].result;
            }
            if (0 == m_tempList.Count)
            {
                tempResult = 0f;
            }
            else
            {
                // tempResult /= m_tempList.Count;
                tempResult = 1f - m_tempList[0].result;
            }
            m_tempList.Clear();
            float result = Mathf.Clamp01(tempResult);
            debugMsg = $"Check {result} from Group:\n" + debugMsg;
            // Debug.LogError(debugMsg);
            return result;
        }

        private bool TryCalculateGapBetweenElement(int dataA, int dataB, out float gapSize)
        {
            int lowPosIndex, highPosIndex;
            bool reverseArrangement = _scrollParam.reverseArrangement;
            if (dataA < dataB)
            {
                lowPosIndex = reverseArrangement ? dataB : dataA;
                highPosIndex = reverseArrangement ? dataA : dataB;
            }
            else if (dataA < dataB)
            {
                lowPosIndex = reverseArrangement ? dataA : dataB;
                highPosIndex = reverseArrangement ? dataB : dataA;
            }
            else
            {
                gapSize = 0f;
                return false;
            }

            if (TryGetShowingElement(lowPosIndex, out RecycleSingleDirectionScrollElement lowElement) &&
                TryGetShowingElement(highPosIndex, out RecycleSingleDirectionScrollElement highElement))
            {
                float lowBoundPosition = CalculateExpectedPositionForData(lowPosIndex);
                Vector2 lowElementSize = lowElement.ElementPreferredSize;

                float hightBoundPosition = CalculateExpectedPositionForData(highPosIndex);
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

        /// <param name="dataIndex"></param>
        /// <returns> 0 ~ 1 (head ~ tail)</returns>
        private float CalculateExpectedPositionForData(int dataIndex)
        {
            // TODO Deal different direction cases
            int convertedIndex = dataIndex;
            int dataCount = m_dataSource.DataElementCount;
            float step = 1f / (dataCount - 1);
            float result = _scrollParam.reverseArrangement ?
                step * (dataCount - 1 - dataIndex) :
                step * convertedIndex;
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
        private bool TryCalculateCurrentPositionFromElement(int dataIndex, out float expectedNormalizedBasePosition, out float deltaToExpectedPosition, out float finalizedPosition)
        {
            if (TryGetShowingElement(dataIndex, out RecycleSingleDirectionScrollElement element))
            {
                return TryCalculateCurrentPositionFromElement(element, out expectedNormalizedBasePosition, out deltaToExpectedPosition, out finalizedPosition);
            }
            expectedNormalizedBasePosition = deltaToExpectedPosition = finalizedPosition = 0f;
            return false;
        }

        // TODO Deal with different direction and arrangement
        private bool TryCalculateCurrentPositionFromElement(RecycleSingleDirectionScrollElement element, out float expectedNormalizedBasePosition, out float deltaToExpectedPosition, out float finalizedNormalizedPosition)
        {
            finalizedNormalizedPosition = expectedNormalizedBasePosition = deltaToExpectedPosition = 0f;
            if (null == m_dataSource)
            {
                return false;
            }

            int dataCount = m_dataSource.DataElementCount;
            int index = element.ElementIndex;
            float stepSize = 1f / (dataCount - 1);

            if (0 == index)
            {
                finalizedNormalizedPosition = expectedNormalizedBasePosition = 0f;
            }
            else if (dataCount - 1 == index)
            {
                finalizedNormalizedPosition = expectedNormalizedBasePosition = 1f;
            }
            else
            {
                finalizedNormalizedPosition = expectedNormalizedBasePosition = stepSize * index;
            }

            Vector2 convertedNormalizedRectPosition = CalculateNormalizedRectPosition(finalizedNormalizedPosition);
            Vector2 elementTempLocalPositionInViewport = convertedNormalizedRectPosition;
            Vector3 tempV3 = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(element.ElementTransform, elementTempLocalPositionInViewport);
            RectTransform viewport = _scrollRect.viewport;
            elementTempLocalPositionInViewport = viewport.InverseTransformPoint(tempV3);

            Vector2 viewportExpectedLocalPosition = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, convertedNormalizedRectPosition);
            deltaToExpectedPosition = viewportExpectedLocalPosition.y - elementTempLocalPositionInViewport.y;

            if (Mathf.Approximately(0f, deltaToExpectedPosition) || Mathf.Abs(deltaToExpectedPosition) < TEMP_MIN_VALUE)
            {
                return true;
            }
            // Currently only calculate for UpToDown case(normal arrangement)
            else if (0f > deltaToExpectedPosition)
            {
                // Greater than pre calculated base position 
                int nextIndex = index + 1;
                if (dataCount - 1 <= nextIndex)
                {
                    if (TryCalculateGapBetweenElement(index - 1, index, out float gapSize))
                    {
                        expectedNormalizedBasePosition = (index - 1) * stepSize;
                        deltaToExpectedPosition = gapSize - deltaToExpectedPosition;
                        finalizedNormalizedPosition = expectedNormalizedBasePosition + stepSize * (deltaToExpectedPosition / gapSize);
                        return true;
                    }
                    else
                    {
                        finalizedNormalizedPosition =
                        expectedNormalizedBasePosition = 1f;
                        deltaToExpectedPosition = 0f;
                        return true;
                    }
                }
                else if (TryCalculateGapBetweenElement(index, nextIndex, out float gapSize) &&
                    Mathf.Abs(deltaToExpectedPosition) < gapSize)
                {
                    float normalizedDelta = Mathf.Abs(deltaToExpectedPosition) / gapSize;
                    finalizedNormalizedPosition = expectedNormalizedBasePosition + stepSize * normalizedDelta;
                    return true;
                }
            }
            else // 0f < deltaToExpectedPosition
            {
                // Less than pre calculated base position 
                int prevIndex = index - 1;
                if (0 > prevIndex)
                {
                    if (TryCalculateGapBetweenElement(index, index + 1, out float gapSize))
                    {
                        expectedNormalizedBasePosition = 0f;
                        finalizedNormalizedPosition = expectedNormalizedBasePosition - stepSize * (deltaToExpectedPosition / gapSize);
                        finalizedNormalizedPosition = Mathf.Clamp01(finalizedNormalizedPosition);
                    }
                    else
                    {
                        finalizedNormalizedPosition =
                        expectedNormalizedBasePosition = 0f;
                        deltaToExpectedPosition = 0f;
                    }
                    return true;
                }
                else if (TryCalculateGapBetweenElement(prevIndex, index, out float gapSize) &&
                    Mathf.Abs(deltaToExpectedPosition) < gapSize)
                {
                    expectedNormalizedBasePosition -= stepSize;
                    deltaToExpectedPosition = gapSize - deltaToExpectedPosition;
                    finalizedNormalizedPosition = expectedNormalizedBasePosition + stepSize * (deltaToExpectedPosition / gapSize);
                    return true;
                }
            }

            return false;
        }

        private bool TryGetShowingElement(int dataIndex, out RecycleSingleDirectionScrollElement element)
        {
            for (int i = 0, length = m_currentUsingElements.Count; i < length; i++)
            {
                if (m_currentUsingElements[i].ElementIndex == dataIndex)
                {
                    element = m_currentUsingElements[i];
                    return true;
                }
            }
            element = null;
            return false;
        }

        private void OnScrollBarValueChanged(float scrollbarValue)
        {
            float convertedValue = Mathf.Clamp01(scrollbarValue);
            m_hasSetScrollBarValueThisFrame = 1;
            if (Mathf.Approximately(convertedValue, m_virtualNormalizedScrollBarValue))
            {
                _scrollBar.SetValueWithoutNotify(m_virtualNormalizedScrollBarValue);
                UpdateScrollBarVisual();
                Debug.LogError($"wanna set scroll 01 {m_virtualNormalizedScrollBarValue} -> {scrollbarValue}_({convertedValue}) || FALSE; Frame {Time.frameCount}");
                return;
            }

            if (TryApplyNormalizedPosition(convertedValue))
            {
                m_hasAdjustElementsCurrentFrame = true;
                Debug.LogError($"wanna set scroll {m_virtualNormalizedScrollBarValue} -> {scrollbarValue}_({convertedValue}) || TRUE; Frame {Time.frameCount}");
                m_virtualNormalizedScrollBarValue = convertedValue;
                _scrollBar.SetValueWithoutNotify(m_virtualNormalizedScrollBarValue);
            }
            else
            {
                Debug.LogError($"wanna set scroll {m_virtualNormalizedScrollBarValue} -> {scrollbarValue}_({convertedValue}) || FALSE; Frame {Time.frameCount}");
                _scrollBar.SetValueWithoutNotify(m_virtualNormalizedScrollBarValue);
            }
            UpdateScrollBarVisual();
        }

        private void UpdateScrollBarVisual()
        {
            if (null != _scrollBar)
            {
                MethodInfo methodInfo = _scrollBar.GetType().GetMethod("UpdateVisuals", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                methodInfo.Invoke(_scrollBar, null);
            }
        }
    }
}