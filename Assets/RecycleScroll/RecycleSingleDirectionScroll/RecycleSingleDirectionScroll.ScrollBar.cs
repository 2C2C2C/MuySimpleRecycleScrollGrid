using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extend;
using ScrollDirection = RecycleScrollView.SingleDirectionScrollParam.ScrollDirection;
using ScrollbarDirection = UnityEngine.UI.Scrollbar.Direction;
using System.Drawing;
using Microsoft.Unity.VisualStudio.Editor;
using System.Runtime.InteropServices;

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

        [Header("ScrollBar related")]
        [SerializeField]
        private Scrollbar _scrollBar = null;

        private float m_virtualNormalizedScrollBarValue;
        private bool m_hasSetScrollBarValueThisFrame = false;

        private List<TempPack> m_tempList = new List<TempPack>(20);

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
            if (0 == dataCount || currentShowingCount >= dataCount)
            {
                _scrollBar.size = 1f;
                _scrollBar.SetValueWithoutNotify(0f);
                return;
            }

            float nextPos = CalculateCurrentNormalizedPosition();
            if (!Mathf.Approximately(nextPos, m_virtualNormalizedScrollBarValue))
            {
                m_virtualNormalizedScrollBarValue = nextPos;
                if (float.IsNaN(m_virtualNormalizedScrollBarValue))
                {
                    // HACK
                    // nextPos = CalculateCurrentNormalizedPosition();
                    // Debug.LogError($"asdsada");
                    return;
                }
                _scrollBar.SetValueWithoutNotify(m_virtualNormalizedScrollBarValue);
            }
            // Debug.LogError($"apply {m_virtualNormalizedPosition} to bar");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="normalizedPosition"> 1 ~ 0 (1 means at the start)</param>
        private bool TryApplyNormalizedPosition(float normalizedPosition)
        {
            bool result = true;
            if (TryGetRefelementFormScrollBarValue(normalizedPosition, out int refElementIndex, out float normalizedBasePosition, out float normalizedOffset))
            {
                JumpToExistElementInstant(refElementIndex, normalizedBasePosition, normalizedOffset);
                return true;
            }

            normalizedPosition = Mathf.Clamp01(1f - normalizedPosition);
            int dataCount = m_dataSource.DataElementCount;
            float stepSize = 1f / (dataCount - 1f);
            int stepLowBoundElementIndex = 0;
            float temp = 0f;
            for (int i = 0; i < dataCount - 1; i++)
            {
                if (Mathf.Approximately(temp, normalizedPosition) || temp > normalizedPosition)
                {
                    break;
                }
                // temp < normalizedPosition
                stepLowBoundElementIndex++;
                temp += stepSize;
            }
            --stepLowBoundElementIndex;
            int stepHighBoundElementIndex = Mathf.Clamp(stepLowBoundElementIndex + 1, stepLowBoundElementIndex, dataCount - 1);

            RectTransform content = _scrollRect.content;
            if (stepHighBoundElementIndex == stepLowBoundElementIndex)
            {
                if (TryGetShowingElement(stepLowBoundElementIndex, out _) && TryCalculateCurrentPositionFromElement(stepLowBoundElementIndex, out float basePostion, out float delta, out float currentFinalPosition))
                {
                    float gapSize = Mathf.Abs(delta) / Mathf.Abs(currentFinalPosition - basePostion);
                    float tempNormalizedDelta = normalizedPosition - basePostion;
                    float newDelta = tempNormalizedDelta * gapSize;
                    Debug.LogError($"old delta_{delta}; new delta_{newDelta}");
                    float move = newDelta - delta;

                    Vector3 localPos = content.localPosition;
                    localPos.y += move;
                    content.localPosition = localPos;
                }
                else
                {
                    // TODO Wrong case....
                    Debug.LogError($"eeeee");
                    result = false;
                }
            }
            else
            {
                bool lowBoundElementShowing = TryGetShowingElement(stepLowBoundElementIndex, out _);
                bool highBoundElementShowing = TryGetShowingElement(stepHighBoundElementIndex, out _);
                if (lowBoundElementShowing && highBoundElementShowing)
                {
                    if (TryCalculateGapBetweenElement(stepLowBoundElementIndex, stepHighBoundElementIndex, out float gapSize))
                    {
                        if (!TryCalculateCurrentPositionFromElement(stepLowBoundElementIndex, out float baseLowPostion, out float tempDelta, out float finalizedPosition))
                        {
                            TryCalculateCurrentPositionFromElement(stepLowBoundElementIndex, out baseLowPostion, out tempDelta, out finalizedPosition);
                        }

                        // TODO deal with inverse arrangement case
                        TryGetDeltaFromElementToExpectedPosition(stepLowBoundElementIndex, out float lowDelta, out _);
                        // TryGetDeltaFromElementToExpectedPosition(stepHighBoundElementIndex, out float highDelta, out _);
                        float newDelta = normalizedPosition - baseLowPostion;
                        newDelta *= gapSize;
                        Debug.LogError($"old delta_{lowDelta}; new delta_{newDelta}");
                        float move = newDelta - lowDelta;

                        Vector3 localPos = content.localPosition;
                        localPos.y += move;
                        content.localPosition = localPos;
                    }
                }
                else
                {
                    // TODO 
                    if (TryGetDeltaFromElementToExpectedPosition(stepLowBoundElementIndex, out float lowDelta, out float expectedRectPosInViewport))
                    {
                        SingleScrollElementNavigationParams param = new SingleScrollElementNavigationParams();
                        param.normalizedPositionInViewPort = expectedRectPosInViewport;
                        param.normalizedElementPositionAdjustment = expectedRectPosInViewport;
                        JumpToElementInstant(stepLowBoundElementIndex, param, new Vector2(0f, lowDelta));
                    }
                    else if (TryGetDeltaFromElementToExpectedPosition(stepHighBoundElementIndex, out float highDelta, out expectedRectPosInViewport))
                    {
                        SingleScrollElementNavigationParams param = new SingleScrollElementNavigationParams();
                        param.normalizedPositionInViewPort = expectedRectPosInViewport;
                        param.normalizedElementPositionAdjustment = expectedRectPosInViewport;
                        JumpToElementInstant(stepLowBoundElementIndex, param, new Vector2(0f, highDelta));
                    }

                    // TODO maybe Wrong case....
                    Debug.LogError($"EEEEEEEEEEEE");
                    result = false;
                }
            }
            ForceRebuildAndStopMove();
            return result;
        }

        private bool TryGetRefelementFormScrollBarValue(float normalized, out int dataIndex, out float normalizedBasePosition, out float normalizedGapOffset)
        {
            if (null != m_dataSource)
            {
                normalized = Mathf.Clamp01(1f - normalized);
                int dataCount = m_dataSource.DataElementCount;
                if (Mathf.Approximately(0f, normalized))
                {
                    dataIndex = 0;
                    normalizedGapOffset = 0f;
                    normalizedBasePosition = 0f;
                }
                else if (Mathf.Approximately(0f, normalized))
                {
                    dataIndex = dataCount - 1;
                    normalizedBasePosition = 1f;
                    normalizedGapOffset = 0f;
                }
                else
                {
                    float stepSize = 1f / (dataCount - 1f);
                    int stepLowBoundElementIndex = 0;
                    float temp = 0f;
                    for (int i = 0; i < dataCount - 1; i++)
                    {
                        if (Mathf.Approximately(temp, normalized) || temp > normalized)
                        {
                            break;
                        }
                        stepLowBoundElementIndex++;
                        temp += stepSize;
                    }
                    --stepLowBoundElementIndex;
                    // int stepHighBoundElementIndex = Mathf.Clamp(stepLowBoundElementIndex + 1, stepLowBoundElementIndex, dataCount - 1);
                    normalizedBasePosition = stepLowBoundElementIndex * stepSize;
                    dataIndex = stepLowBoundElementIndex;
                    normalizedGapOffset = normalized - normalizedBasePosition;
                }
                return true;
            }
            dataIndex = -1;
            normalizedBasePosition = 0f;
            normalizedGapOffset = 0f;
            return false;
        }

        // private bool TryGetBoundIndexForNormalizedPosition(float normalizedPosition, out int lowBound, out int highBound)
        // {
        //     // HACK It depends on scroll direction and the position
        //     if (null != m_dataSource)
        //     {
        //         int dataCount = m_dataSource.DataElementCount;
        //         float stepSize = 1f / (dataCount - 1);
        //         if (Mathf.Approximately(0.5f, normalizedPosition))
        //         {

        //         }
        //     }
        //     lowBound = highBound = -1;
        //     return false;
        // }

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
                tempResult /= m_tempList.Count;
            }
            m_tempList.Clear();
            float result = Mathf.Clamp01(tempResult);
            // debugMsg = $"Check {m_currentUsingElements[minDeltaIndex].ElementIndex}_{result} from Group:\n" + debugMsg;
            // Debug.LogError(msg);
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
        private bool TryCalculateCurrentPositionFromElement(RecycleSingleDirectionScrollElement element, out float expectedNormalizedBasePosition, out float deltaToExpectedPosition, out float finalizedPosition)
        {
            finalizedPosition = expectedNormalizedBasePosition = deltaToExpectedPosition = 0f;
            if (null == m_dataSource)
            {
                return false;
            }

            int dataCount = m_dataSource.DataElementCount;
            int inUsingElementCount = m_currentUsingElements.Count;
            for (int i = 0; i < inUsingElementCount; i++)
            {
                if (m_currentUsingElements[i] == element)
                {
                    // Step is between bars, bar stands for each data element
                    float stepSize = 1f / (dataCount - 1);
                    int dataIndex = element.ElementIndex;
                    finalizedPosition = expectedNormalizedBasePosition = stepSize * dataIndex;

                    // Currently only calculate for UpToDown case(normal arrangement)
                    float convertedNormalizedRectPosition = 1f - expectedNormalizedBasePosition;
                    Vector2 elementTempLocalPositionInViewport = new Vector2(0f, convertedNormalizedRectPosition);

                    RectTransform viewport = _scrollRect.viewport;
                    Vector3 tempV3 = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(element.ElementTransform, elementTempLocalPositionInViewport);
                    elementTempLocalPositionInViewport = viewport.InverseTransformPoint(tempV3);

                    Vector2 viewportExpectedLocalPosition = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, new Vector2(0f, convertedNormalizedRectPosition));
                    deltaToExpectedPosition = viewportExpectedLocalPosition.y - elementTempLocalPositionInViewport.y;

                    // Currently only calculate for UpToDown(normal arrangement) case
                    if (0f < deltaToExpectedPosition)
                    {
                        // UpToDown(1~0; normal arrangement) case, this case means actual position value is less than expected
                        if (0 == i)
                        {
                            if (-1 != CalculateAvailabeNextHeadElementIndex())
                            {
                                Debug.LogError($"Can not calculate cuz it need a prev element which is not spawned yet.");
                                return false;
                            }
                            // else case
                            finalizedPosition =
                            expectedNormalizedBasePosition =
                            deltaToExpectedPosition = 0f;
                            return true;
                        }
                        else
                        {
                            RecycleSingleDirectionScrollElement prevElement = m_currentUsingElements[i - 1];
                            int prevElementDataIndex = prevElement.ElementIndex;
                            float prevElementBasePosition = stepSize * prevElementDataIndex;

                            convertedNormalizedRectPosition = 1f - prevElementBasePosition;
                            elementTempLocalPositionInViewport = new Vector2(0f, convertedNormalizedRectPosition);
                            tempV3 = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(prevElement.ElementTransform, elementTempLocalPositionInViewport);
                            elementTempLocalPositionInViewport = viewport.InverseTransformPoint(tempV3);
                            viewportExpectedLocalPosition = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, new Vector2(0f, convertedNormalizedRectPosition));

                            // The delta of prev element should be negative
                            float prevElementDelta = viewportExpectedLocalPosition.y - elementTempLocalPositionInViewport.y;
                            if (Mathf.Approximately(0f, prevElementDelta))
                            {
                                finalizedPosition = expectedNormalizedBasePosition = prevElementBasePosition;
                                deltaToExpectedPosition = 0f;
                                return true;
                            }
                            else if (0f > prevElementDelta)
                            {
                                float gap = Mathf.Abs(prevElementDelta) + Mathf.Abs(deltaToExpectedPosition);
                                float normalizedGap = deltaToExpectedPosition / gap * stepSize;
                                finalizedPosition = expectedNormalizedBasePosition - normalizedGap;
                                return true;
                            }
                        }
                    }
                    else if (0f > deltaToExpectedPosition)
                    {
                        // UpToDown(1~0; normal arrangement) case, this case means actual position value is greater than expected
                        if (inUsingElementCount - 1 == i)
                        {
                            if (-1 != CalculateAvailabeNextTailElementIndex())
                            {
                                Debug.LogError($"Can not calculate cuz it need a next element which is not spawned yet.");
                                return false;
                            }
                            // else case
                            finalizedPosition =
                            expectedNormalizedBasePosition = 1f;
                            deltaToExpectedPosition = 0f;
                            return true;
                        }
                        else
                        {
                            RecycleSingleDirectionScrollElement nextElement = m_currentUsingElements[i + 1];
                            int nextElementDataIndex = nextElement.ElementIndex;
                            float nextElementBasePosition = stepSize * nextElementDataIndex;

                            convertedNormalizedRectPosition = 1f - nextElementBasePosition;
                            elementTempLocalPositionInViewport = new Vector2(0f, convertedNormalizedRectPosition);
                            tempV3 = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(nextElement.ElementTransform, elementTempLocalPositionInViewport);
                            elementTempLocalPositionInViewport = viewport.InverseTransformPoint(tempV3);
                            viewportExpectedLocalPosition = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, new Vector2(0f, convertedNormalizedRectPosition));

                            // The delta of prev element should be positive
                            float nextElementDelta = viewportExpectedLocalPosition.y - elementTempLocalPositionInViewport.y;
                            if (Mathf.Approximately(0f, nextElementDelta))
                            {
                                finalizedPosition = expectedNormalizedBasePosition = nextElementBasePosition;
                                deltaToExpectedPosition = 0f;
                                return true;
                            }
                            else if (0f < nextElementDelta)
                            {
                                float gap = Mathf.Abs(nextElementDelta) + Mathf.Abs(deltaToExpectedPosition);
                                float normalizedGap = (1f - nextElementDelta / gap) * stepSize;
                                finalizedPosition = expectedNormalizedBasePosition + normalizedGap;
                                return true;
                            }
                        }
                    }
                    else // 0 == deltaToExpectedPosition
                    {
                        return true;
                    }
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
            float convertedValue = scrollbarValue;
            // Debug.LogError($"wanna set scroll to {convertedValue}");
            // TempSetNormalizedPosition(convertedValue);
            if (TryApplyNormalizedPosition(convertedValue))
            {
                Debug.LogError($"wanna set scroll {m_virtualNormalizedScrollBarValue} -> {convertedValue}");
                m_virtualNormalizedScrollBarValue = convertedValue;
            }
            else
            {
                _scrollBar.SetValueWithoutNotify(m_virtualNormalizedScrollBarValue);
            }
            m_hasSetScrollBarValueThisFrame = true;
        }

        private void BindScrollBar()
        {
            if (null == _scrollBar)
            {
                return;
            }
            _scrollBar.onValueChanged.AddListener(OnScrollBarValueChanged);
        }

        private void UnBindScrollBar()
        {
            if (null == _scrollBar)
            {
                return;
            }
            _scrollBar.onValueChanged.RemoveListener(OnScrollBarValueChanged);
        }
    }
}