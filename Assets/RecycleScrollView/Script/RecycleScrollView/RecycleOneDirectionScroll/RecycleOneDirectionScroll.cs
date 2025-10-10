using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ScrollDirection = RecycleScrollView.SingleDirectionScrollParam.ScrollDirection;

namespace RecycleScrollView
{
    [RequireComponent(typeof(UnityScrollRectExtended))]
    public partial class RecycleOneDirectionScroll : UIBehaviour
    {
        [SerializeField]
        private UnityScrollRectExtended _scrollRect;
        [SerializeField]
        private HorizontalOrVerticalLayoutGroup _contentLayoutGroup;
        [SerializeField]
        private RectTransform _fallbackElementPrefab;

        // Simple layout param
        [SerializeField]
        private SingleDirectionScrollParam _scrollParam;

        [SerializeField]
        private float _velocityStopThreshold = 7f;
        [SerializeField]
        private float _velocityMaxClamp = 1000f;

        private bool m_hasAdjustmentCurrentFrame = false;

        public bool IsVertical => ScrollDirection.vertical == _scrollParam.scrollDirection;
        public bool IsHorizontal => ScrollDirection.Horizontal == _scrollParam.scrollDirection;

        private List<RecycleOneDirectionScrollElement> m_currentUsingElements = new List<RecycleOneDirectionScrollElement>();
        private IOneDirectionScrollDataSource m_dataSource;

        private UnityAction<Vector2> m_onScrollPositionChanged;

        public void UnInit()
        {
            if (null != m_dataSource)
            {
                for (int i = 0, length = m_currentUsingElements.Count; i < length; i++)
                {
                    m_dataSource.ReturnElement(m_currentUsingElements[i].ElementTransform);
                }
                m_currentUsingElements.Clear();
                m_dataSource = null;
            }
        }

        public void Init(IOneDirectionScrollDataSource dataSource)
        {
            if (null == m_dataSource)
            {
                m_dataSource = dataSource;
                RectTransform content = _scrollRect.content;
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                _contentLayoutGroup.reverseArrangement = _scrollParam.reverseArrangement;
                if (IsVertical)
                {
                    if (_contentLayoutGroup is VerticalLayoutGroup)
                    {
                        content.pivot = _scrollParam.reverseArrangement ?
                            new Vector2(0.5f, 0f) :
                            new Vector2(0.5f, 1f);
                        _contentLayoutGroup.childAlignment = _scrollParam.reverseArrangement ?
                            TextAnchor.LowerCenter :
                            TextAnchor.UpperCenter;
                    }
                    else
                    {
                        Debug.LogError($"Vertical scroll need a VerticalLayoutGroup on content");
                    }
                }
                if (IsHorizontal)
                {
                    if (_contentLayoutGroup is HorizontalLayoutGroup)
                    {
                        content.pivot = _scrollParam.reverseArrangement ?
                            new Vector2(1f, 0.5f) :
                            new Vector2(0f, 0.5f);
                    }
                    else
                    {
                        Debug.LogError($"Horizontal scroll need a VerticalLayoutGroup on content");
                    }
                }

                int dataCount = dataSource.DataElementCount;
                int elementCount = 0;
                if (0 < dataCount)
                {
                    do
                    {
                        if (SIDE_STATUS_NEEDADD == CheckTailSideStatus())
                        {
                            AddElementToTail(elementCount++);
                            continue;
                        }
                        break;
                    } while (elementCount < dataCount);
                }
            }
        }

        private RecycleOneDirectionScrollElement InternalCreateElement(int dataIndex)
        {
            RectTransform content = _scrollRect.content;
            RecycleOneDirectionScrollElement newElement;
            if (null == m_dataSource)
            {
                RectTransform spawned = Instantiate(_fallbackElementPrefab, content);
                if (!spawned.TryGetComponent<RecycleOneDirectionScrollElement>(out newElement))
                {
                    Debug.LogError($"[RecycleScrollView] receive wrong element");
                }
            }
            else
            {
                RectTransform requestedElement = m_dataSource.RequestElement(content, dataIndex);
                if (!requestedElement.TryGetComponent<RecycleOneDirectionScrollElement>(out newElement))
                {
                    Debug.LogError($"[RecycleScrollView] receive wrong element");
                }
            }
            return newElement;
        }

        private void InternalRemoveElement(RecycleOneDirectionScrollElement element)
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

        private bool AdjustElementsIfNeed()
        {
            bool hasRemoved = RemoveElementsIfNeed();
            bool hasAdded = AddElemensIfNeed();

            return hasRemoved || hasAdded;
        }

        private void InternalAdjustment()
        {
            // Debug.LogError(noramlizedPosition);
            RectTransform content = _scrollRect.content;
            Vector2 prevContentStartPos = _scrollRect.ContentStartPos;
            Vector2 anchorPositionDelta = content.anchoredPosition - prevContentStartPos;
            Vector2 velocity = _scrollRect.velocity;

            bool isOutOfBounds = 0f >= _scrollRect.verticalNormalizedPosition || 1f <= _scrollRect.verticalNormalizedPosition;
            bool hasAdjustedElements = AdjustElementsIfNeed();
            if (_velocityStopThreshold * _velocityStopThreshold > velocity.sqrMagnitude)
            {
                _scrollRect.velocity = Vector2.zero;
            }
            else if (_velocityMaxClamp * _velocityMaxClamp < velocity.sqrMagnitude)
            {
                velocity = _velocityMaxClamp * velocity.normalized;
                _scrollRect.velocity = velocity;
            }
            else if (hasAdjustedElements || isOutOfBounds)
            {
                _scrollRect.velocity = velocity;
            }

            if (hasAdjustedElements || !isOutOfBounds)
            {
                // HACK Becuz I change the anchored position of drag content, so I need to adjust the prev value here. 
                Vector2 newStartPos = content.anchoredPosition - anchorPositionDelta;
                _scrollRect.ContentStartPos = newStartPos;
            }
        }

        private void OnScrollPositionChanged(Vector2 noramlizedPosition)
        {
            InternalAdjustment();
            m_hasAdjustmentCurrentFrame = true;
        }

        private void LateUpdate()
        {
            if (!m_hasAdjustmentCurrentFrame)
            {
                InternalAdjustment();
            }
            m_hasAdjustmentCurrentFrame = false;
        }

        protected override void OnEnable()
        {
            if (null == m_onScrollPositionChanged)
            {
                m_onScrollPositionChanged = new UnityAction<Vector2>(OnScrollPositionChanged);
            }
            _scrollRect.onValueChanged.AddListener(m_onScrollPositionChanged);
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
            TryGetComponent<UnityScrollRectExtended>(out _scrollRect);
        }

#endif

    }
}