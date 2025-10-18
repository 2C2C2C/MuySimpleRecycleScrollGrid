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

        private bool m_hasAdjustCiurrentElements = false;

        public bool IsVertical => _scrollParam.IsVertical;
        public bool IsHorizontal => _scrollParam.IsHorizontal;

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
                ApplyLayoutSetting();
                while (SIDE_STATUS_NEEDADD == CheckTailSideStatus())
                {
                    if (!AddElementsToTailIfNeed())
                    {
                        break;
                    }
                }
                _scrollRect.CallUpdateBoundsAndPrevData();
            }
        }

        private void ApplyLayoutSetting()
        {
            RectTransform content = _scrollRect.content;
            _scrollRect.vertical = IsVertical;
            _scrollRect.horizontal = IsHorizontal;
            if (IsVertical)
            {
                _scrollRect.horizontal = false;
                if (_contentLayoutGroup is VerticalLayoutGroup)
                {
                    switch (_scrollParam.scrollDirection)
                    {
                        case ScrollDirection.Vertical_UpToDown:
                            content.pivot = new Vector2(0.5f, 1f);
                            _contentLayoutGroup.childAlignment = TextAnchor.UpperCenter;
                            _contentLayoutGroup.reverseArrangement = false;
                            break;
                        case ScrollDirection.Vertical_DownToUp:
                            content.pivot = new Vector2(0.5f, 0f);
                            _contentLayoutGroup.childAlignment = TextAnchor.LowerCenter;
                            _contentLayoutGroup.reverseArrangement = true;
                            break;
                    }
                }
                else
                {
                    Debug.LogError($"Vertical scroll need a VerticalLayoutGroup on content");
                }
            }
            else if (IsHorizontal)
            {
                if (_contentLayoutGroup is HorizontalLayoutGroup)
                {
                    switch (_scrollParam.scrollDirection)
                    {
                        case ScrollDirection.Horizontal_LeftToRight:
                            content.pivot = new Vector2(0f, 0.5f);
                            _contentLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
                            _contentLayoutGroup.reverseArrangement = false;
                            break;
                        case ScrollDirection.Horizontal_RightToLeft:
                            content.pivot = new Vector2(1f, 0.5f);
                            _contentLayoutGroup.childAlignment = TextAnchor.MiddleRight;
                            _contentLayoutGroup.reverseArrangement = true;
                            break;
                    }
                }
                else
                {
                    Debug.LogError($"Horizontal scroll need a HorizontalLayoutGroup on content");
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
#if UNITY_EDITOR
            newElement.name = $"{newElement.name} {dataIndex}";
#endif
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
            bool hasAdjusted = hasRemoved || hasAdded;
            if (hasAdjusted)
            {
                _scrollRect.CallUpdateBoundsAndPrevData();
            }
            return hasAdjusted;
        }

        private void InternalAdjustment()
        {
            // Debug.LogError(_scrollRect.velocity);
            // Debug.LogError(_scrollRect.normalizedPosition);
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

            if (hasAdjustedElements)
            {
                // HACK Becuz I change the anchored position of drag content, so I need to adjust the prev value here. 
                Vector2 newStartPos = content.anchoredPosition - anchorPositionDelta;
                _scrollRect.ContentStartPos = newStartPos;
            }
            m_hasAdjustCiurrentElements = hasAdjustedElements;
        }

        private void OnScrollPositionChanged(Vector2 noramlizedPosition)
        {
            // Debug.LogError("OnScrollPositionChanged");
            InternalAdjustment();
        }

        private void LateUpdate()
        {
            if (!m_hasAdjustCiurrentElements)
            {
                InternalAdjustment();
            }
            m_hasAdjustCiurrentElements = false;

            if (0 > m_nextFrameSetActive)
            {
                --m_nextFrameSetActive;
                _scrollRect.enabled = true;
                m_nextFrameSetActive = 0;
            }
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

        protected override void OnValidate()
        {
            ApplyLayoutSetting();
        }

#endif

    }
}