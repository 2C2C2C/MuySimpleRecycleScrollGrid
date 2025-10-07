using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extend;

namespace RecycleScrollView
{
    // [DefaultExecutionOrder(11)]
    [RequireComponent(typeof(ScrollRect))]
    public class RecycleSingleDirectionScrollController : UIBehaviour
    {
        private const int SIDE_STATUS_ENOUGH = 0;
        private const int SIDE_STATUS_NEEDADD = -1;
        private const int SIDE_STATUS_NEEDREMOVE = 1;

        private static MethodInfo s_updatePrevDataMethodHandle = null;
        private static MethodInfo s_updateBoundsMethodHandle = null;

        [SerializeField]
        private ScrollRect _scrollRect;
        [SerializeField]
        private RectTransform _fallbackElementPrefab;

        // Simple layout param
        [SerializeField]
        private float _spacing;
        [SerializeField]
        private float _frontPadding;
        [SerializeField]
        private float _rearPadding;

        [SerializeField]
        private float _velocityStopThreshold = 7f;
        [SerializeField]
        private float _velocityMaxClamp = 1000f;

        private List<RecycleSingleDirectionScrollElement> m_currentUsingElements = new List<RecycleSingleDirectionScrollElement>();

        private ISingleDirectionListView m_dataSource;

        private UnityAction<Vector2> m_onScrollPositionChanged;

        public void UnInit()
        {
            if (null != m_dataSource)
            {

            }
        }

        public void Init(ISingleDirectionListView dataSource)
        {
            if (null == m_dataSource)
            {
                m_dataSource = dataSource;
                RectTransform content = _scrollRect.content;
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                if (content.TryGetComponent<VerticalLayoutGroup>(out VerticalLayoutGroup verticalLayoutGroup))
                {
                    int dataCount = dataSource.DataElementCount;
                    int elementCount = 0;
                    if (0 < dataCount)
                    {
                        do
                        {
                            if (SIDE_STATUS_NEEDADD == CheckBottomSideStatus())
                            {
                                AddElementOnBottom(elementCount++);
                                continue;
                            }
                            break;
                        } while (elementCount < dataCount);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>-1 Need add, 0 Enough, 1 Need remove</returns>
        private int CheckTopSideStatus()
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

            RecycleSingleDirectionScrollElement headElement = m_currentUsingElements[0];
            RectTransform viewport = _scrollRect.viewport;
            Vector2 viewportTop = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, Vector2.up);
            Vector2 headElementBottom = viewport.InverseTransformPoint(RectTransformEx.TransformNormalizedRectPositionToWorldPosition(headElement.ElementTransform, Vector2.zero));
            if (headElementBottom.y > viewportTop.y) // Head element already above viewport top
            {
                if (2 <= elementCount)
                {
                    RecycleSingleDirectionScrollElement head2ndElement = m_currentUsingElements[1];
                    headElementBottom = viewport.InverseTransformPoint(RectTransformEx.TransformNormalizedRectPositionToWorldPosition(head2ndElement.ElementTransform, Vector2.zero));
                    if (headElementBottom.y > viewportTop.y)
                    {
                        return SIDE_STATUS_NEEDREMOVE;
                    }
                }
            }
            else
            {
                // Vector2 headElementTop = viewport.InverseTransformPoint(RectTransformEx.TransformNormalizedRectPositionToWorldPosition(headElement.ElementTransform, Vector2.up));
                // if (headElementTop.y <= viewportTop.y)
                // {
                //     return SIDE_STATUS_NEEDADD;
                // }
                return SIDE_STATUS_NEEDADD;
            }

            return SIDE_STATUS_ENOUGH;
        }

        private int CheckBottomSideStatus()
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

            RecycleSingleDirectionScrollElement tailElement = m_currentUsingElements[elementCount - 1];
            RectTransform viewport = _scrollRect.viewport;
            Vector2 viewportBottom = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, Vector2.zero);
            Vector2 tailElementTop = viewport.InverseTransformPoint(RectTransformEx.TransformNormalizedRectPositionToWorldPosition(tailElement.ElementTransform, Vector2.up));
            if (tailElementTop.y < viewportBottom.y) // Tail element already under viewport top
            {
                if (2 <= elementCount)
                {
                    RecycleSingleDirectionScrollElement tail2ndElement = m_currentUsingElements[elementCount - 2];
                    tailElementTop = viewport.InverseTransformPoint(RectTransformEx.TransformNormalizedRectPositionToWorldPosition(tail2ndElement.ElementTransform, Vector2.up));
                    if (tailElementTop.y < viewportBottom.y)
                    {
                        return SIDE_STATUS_NEEDREMOVE;
                    }
                }
            }
            else
            {
                return SIDE_STATUS_NEEDADD;
                // Vector2 tailElementBottom = viewport.InverseTransformPoint(RectTransformEx.TransformNormalizedRectPositionToWorldPosition(tailElement.ElementTransform, Vector2.zero));
                // if (tailElementBottom.y >= viewportBottom.y)
                // {
                //     return SIDE_STATUS_NEEDADD;
                // }
            }
            return SIDE_STATUS_ENOUGH;
        }

        private void AddElementOnFront(int dataIndex)
        {
            RecycleSingleDirectionScrollElement newElement = InternalCreateElement(dataIndex);
            m_currentUsingElements.Insert(0, newElement);
            newElement.CalculatePreferredSize();
            newElement.transform.SetAsFirstSibling();
            newElement.SetIndex(dataIndex);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
            Debug.LogError($"Add on top data{dataIndex}");
        }

        private void AddElementOnBottom(int dataIndex)
        {
            RecycleSingleDirectionScrollElement newElement = InternalCreateElement(dataIndex);
            m_currentUsingElements.Add(newElement);
            newElement.CalculatePreferredSize();
            newElement.transform.SetAsLastSibling();
            newElement.SetIndex(dataIndex);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
            Debug.LogError($"Add on bottom data{dataIndex}");
        }

        private void RemoveElementOnTop()
        {
            RecycleSingleDirectionScrollElement element = m_currentUsingElements[0];
            int dataIndex = element.ElementIndex;
            m_currentUsingElements.RemoveAt(0);
            InternalRemoveElement(element);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
            Debug.LogError($"Remove on top data{dataIndex}");
        }

        private void RemoveElementOnBottom()
        {
            int elementIndex = m_currentUsingElements.Count - 1;
            RecycleSingleDirectionScrollElement element = m_currentUsingElements[elementIndex];
            int dataIndex = element.ElementIndex;
            m_currentUsingElements.RemoveAt(elementIndex);
            InternalRemoveElement(element);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
            Debug.LogError($"Remove on bottom data{dataIndex}");
        }

        private RecycleSingleDirectionScrollElement InternalCreateElement(int dataIndex)
        {
            RectTransform content = _scrollRect.content;
            RecycleSingleDirectionScrollElement newElement;
            if (null == m_dataSource)
            {
                RectTransform spawned = Instantiate(_fallbackElementPrefab, content);
                if (!spawned.TryGetComponent<RecycleSingleDirectionScrollElement>(out newElement))
                {
                    Debug.LogError($"");
                }
            }
            else
            {
                RectTransform requestedElement = m_dataSource.RequestElement(content, dataIndex);
                if (!requestedElement.TryGetComponent<RecycleSingleDirectionScrollElement>(out newElement))
                {
                    Debug.LogError($"");
                }
            }
            return newElement;
        }

        private void InternalRemoveElement(RecycleSingleDirectionScrollElement element)
        {
            if (null == m_dataSource)
            {
                GameObject.Destroy(element.gameObject);
            }
            else
            {
                m_dataSource.ReturnElement(element.transform as RectTransform);
            }
        }

        private void AdjustElementsIfNeed()
        {
            RemoveElementsIfNeed();
            AddElemensIfNeed();
        }

        private void RemoveElementsIfNeed()
        {
            // Check if more elements out of viewport
            RectTransform viewport = _scrollRect.viewport;
            RectTransform content = _scrollRect.content;

            int prevElementCount = m_currentUsingElements.Count;
            if (0 < prevElementCount)
            {
                // Top side check
                if (SIDE_STATUS_NEEDREMOVE == CheckTopSideStatus())
                {
                    int frontRemoveElementCount = -1;
                    do
                    {
                        RecycleSingleDirectionScrollElement toRemove = m_currentUsingElements[frontRemoveElementCount + 1];
                        if ((0 > frontRemoveElementCount && RectTransformEx.IsNotIntersetedWithTargetRect(toRemove.ElementTransform, viewport)) ||
                           (0 <= frontRemoveElementCount && RectTransformEx.IsNotIntersetedWithTargetRect(toRemove.ElementTransform, viewport)))
                        {
                            ++frontRemoveElementCount;
                            if (0 < frontRemoveElementCount)
                            {
                                break;
                            }
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
                        RecycleSingleDirectionScrollElement toRemove = m_currentUsingElements[0];
                        frontTotalRemoveSize += toRemove.ElementTransform.rect.height;
                        RemoveElementOnTop();
                        --frontRemoveElementCount;
                    }

                    if (0f < frontTotalRemoveSize)
                    {
                        Vector2 currentFrontPos = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(content, Vector2.up);
                        currentFrontPos = viewport.InverseTransformPoint(currentFrontPos);
                        // HACK Calculate how much movement need to apply to put the element same position
                        prevFrontPos.y -= frontTotalRemoveSize;

                        float normalizedDelta = (currentFrontPos.y - prevFrontPos.y) / content.rect.height;
                        _scrollRect.verticalNormalizedPosition -= normalizedDelta;
                        LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
                        NotifyUpdatePrevData();
                    }
                }

                // Bottom side check
                if (SIDE_STATUS_NEEDREMOVE == CheckBottomSideStatus())
                {
                    prevElementCount = m_currentUsingElements.Count;
                    int removeCount = -1;
                    do
                    {
                        int index = prevElementCount - 1 - (removeCount + 1);
                        if (0 < index)
                        {
                            RecycleSingleDirectionScrollElement toRemove = m_currentUsingElements[index];
                            if ((0 > removeCount && RectTransformEx.IsNotIntersetedWithTargetRect(toRemove.ElementTransform, viewport)) ||
                               (0 <= removeCount && RectTransformEx.IsNotIntersetedWithTargetRect(toRemove.ElementTransform, viewport)))
                            {
                                ++removeCount;
                                if (0 < removeCount)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        break;
                    } while (-1 < removeCount);

                    Vector2 prevRearPos = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(content, Vector2.zero);
                    prevRearPos = viewport.InverseTransformPoint(prevRearPos);
                    float rearTotalRemoveSize = 0f;
                    while (0 < removeCount && 0 < m_currentUsingElements.Count)
                    {
                        RecycleSingleDirectionScrollElement toRemove = m_currentUsingElements[m_currentUsingElements.Count - 1];
                        rearTotalRemoveSize += toRemove.ElementTransform.rect.height;
                        RemoveElementOnBottom();
                        --removeCount;
                    }

                    if (0f < rearTotalRemoveSize)
                    {
                        Vector2 currentRearPos = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(content, Vector2.up);
                        currentRearPos = viewport.InverseTransformPoint(currentRearPos);
                        // HACK Calculate how much movement need to apply to put the element same position
                        prevRearPos.y += rearTotalRemoveSize;

                        float normalizedDelta = (currentRearPos.y - prevRearPos.y) / content.rect.height;
                        _scrollRect.verticalNormalizedPosition += normalizedDelta;
                        LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
                        NotifyUpdatePrevData();
                    }
                }
            }
        }

        private void AddElemensIfNeed()
        {
            RectTransform content = _scrollRect.content;
            RectTransform viewport = _scrollRect.viewport;

            // Check top side
            if (SIDE_STATUS_NEEDADD == CheckTopSideStatus())
            {
                Vector2 prevFrontPos = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(content, Vector2.up);
                prevFrontPos = viewport.InverseTransformPoint(prevFrontPos);
                float addSize = 0f;
                while (SIDE_STATUS_NEEDADD == CheckTopSideStatus())
                {
                    RecycleSingleDirectionScrollElement frontElement = m_currentUsingElements[0];
                    if (1 <= frontElement.ElementIndex)
                    {
                        AddElementOnFront(frontElement.ElementIndex - 1);
                        addSize = m_currentUsingElements[0].currentSize.y;
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
                if (0f < addSize)
                {
                    Vector2 currentFrontPos = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(content, Vector2.up);
                    currentFrontPos = viewport.InverseTransformPoint(currentFrontPos);
                    prevFrontPos.y += addSize;

                    float normalizedDelta = (currentFrontPos.y - prevFrontPos.y) / content.rect.height;
                    _scrollRect.verticalNormalizedPosition += normalizedDelta;
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
                    NotifyUpdatePrevData();
                }
            }

            // Check bottom side
            if (SIDE_STATUS_NEEDADD == CheckBottomSideStatus())
            {
                float addSize = 0f;
                Vector2 prevRearPos = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(content, Vector2.zero);
                prevRearPos = viewport.InverseTransformPoint(prevRearPos);
                while (SIDE_STATUS_NEEDADD == CheckBottomSideStatus())
                {
                    RecycleSingleDirectionScrollElement rearElement = m_currentUsingElements[m_currentUsingElements.Count - 1];
                    if (m_dataSource.DataElementCount - 1 > rearElement.ElementIndex)
                    {
                        AddElementOnBottom(rearElement.ElementIndex + 1);
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
                    Vector2 currentRearPos = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(content, Vector2.zero);
                    currentRearPos = viewport.InverseTransformPoint(currentRearPos);
                    // HACK Calculate how much movement need to apply to put the element same position
                    prevRearPos.y -= addSize;

                    float normalizedDelta = (currentRearPos.y - prevRearPos.y) / content.rect.height;
                    _scrollRect.verticalNormalizedPosition -= normalizedDelta;
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
                    NotifyUpdatePrevData();
                }
            }
        }

        private bool isInLah = false;
        private void OnScrollPositionChanged(Vector2 positionDelta)
        {
            if (isInLah)
            {
                return;
            }
            isInLah = true;
            // Debug.LogError($"normalizedPosition {_scrollRect.normalizedPosition}");
            Vector2 velocity = _scrollRect.velocity;
            AdjustElementsIfNeed();
            if (_velocityStopThreshold * _velocityStopThreshold > velocity.sqrMagnitude)
            {
                _scrollRect.velocity = Vector2.zero;
            }
            else if (_velocityMaxClamp * _velocityMaxClamp < velocity.sqrMagnitude)
            {
                velocity = _velocityMaxClamp * velocity.normalized;
                _scrollRect.velocity = velocity;
            }
            isInLah = false;
        }

        private void NotifyUpdatePrevData()
        {
            if (null != s_updateBoundsMethodHandle)
            {
                s_updateBoundsMethodHandle.Invoke(_scrollRect, null);
                if (null != s_updatePrevDataMethodHandle)
                {
                    s_updatePrevDataMethodHandle.Invoke(_scrollRect, null);
                }
            }
        }

        protected override void OnEnable()
        {
            if (null == m_onScrollPositionChanged)
            {
                m_onScrollPositionChanged = new UnityAction<Vector2>(OnScrollPositionChanged);
            }
            _scrollRect.onValueChanged.AddListener(m_onScrollPositionChanged);

            if (null == s_updatePrevDataMethodHandle)
            {
                MethodInfo methodInfo = typeof(ScrollRect).GetMethod("UpdatePrevData", BindingFlags.NonPublic | BindingFlags.Instance);
                if (null != methodInfo)
                {
                    s_updatePrevDataMethodHandle = methodInfo;
                }

                methodInfo = typeof(ScrollRect).GetMethod("UpdateBounds", BindingFlags.NonPublic | BindingFlags.Instance);
                if (null != methodInfo)
                {
                    s_updateBoundsMethodHandle = methodInfo;
                }
            }
        }

        protected override void OnDisable()
        {
            if (null != m_onScrollPositionChanged)
            {
                _scrollRect.onValueChanged.RemoveListener(m_onScrollPositionChanged);
            }
        }

#if UNITY_EDITOR

        protected override void Reset()
        {
            TryGetComponent<ScrollRect>(out _scrollRect);
        }

#endif

    }
}