using UnityEngine;
using UnityEngine.EventSystems;
using MovementType = UnityEngine.UI.ScrollRect.MovementType;
using ScrollRectEvent = UnityEngine.UI.ScrollRect.ScrollRectEvent;

namespace RecycleScrollView
{
    public partial class Scroller : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler
    {
        [SerializeField]
        private RectTransform _viewport;
        [SerializeField]
        private bool _horizontal = true;
        [SerializeField]
        private bool _vertical = true;

        [SerializeField]
        private MovementType _movementType = MovementType.Elastic;

        [SerializeField]
        private float _elasticity = 0.1f;
        [SerializeField]
        private bool _inertia = true;

        [SerializeField]
        private float _decelerationRate = 0.135f; // Only used when inertia is enabled

        [SerializeField]
        private float _scrollSensitivity = 1.0f;

        [SerializeField]
        private ScrollRectEvent _onValueChanged = new ScrollRectEvent();
        [SerializeField]
        private ScrollerStateEvent _onStateChanged = new ScrollerStateEvent();

        private Bounds m_viewportBounds;
        private Bounds m_contentBounds;

        private Vector2 m_contentSize;
        /// <summary> The local position in viewport </summary>
        private Vector2 m_prevContentPosition;

        private ScrollerState m_scrollerState;
        private Vector2 m_velocity;

        private bool m_isScrolling;

        // Drag related stuff
        private bool m_isDragging;
        private int m_dragPointerId = int.MinValue;
        private Vector2 m_pointerStartLocalCursor = Vector2.zero;
        private Vector2 m_contentDragStartPosition = Vector2.zero;

        public ScrollerState ScrollerState => m_scrollerState;

        public void SetVirtualContentSize(Vector2 size)
        {
            if (0f > size.x)
            {
                size.x = 0f;
            }
            if (0f > size.y)
            {
                size.y = 0f;
            }
            m_contentSize = size;
        }

        public void StopMovement()
        {
            m_velocity = Vector2.zero;
        }

        public void SetNormalizedPosition(Vector2 normalizedPosition)
        {

        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (PointerEventData.InputButton.Left != eventData.button || !IsActive())
            {
                return;
            }

            if (m_isDragging)
            {
                if (eventData.pointerId == m_dragPointerId)
                {
                    // TODO
                }
                return;
            }

            m_velocity = Vector2.zero;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (PointerEventData.InputButton.Left != eventData.button || !IsActive())
            {
                return;
            }

            if (m_isDragging)
            {
                if (eventData.pointerId == m_dragPointerId)
                {
                    // TODO
                }
                return;
            }

            m_contentDragStartPosition = m_contentBounds.center;
            m_pointerStartLocalCursor = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_viewport, eventData.position, eventData.pressEventCamera, out m_pointerStartLocalCursor);
            // TODO Calculate content pos
            m_dragPointerId = eventData.pointerId;
            m_isDragging = true;
            Debug.LogError($"[RecycleScroll] Scroller DragBegin at viewport local pos {m_pointerStartLocalCursor}");
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!m_isDragging || PointerEventData.InputButton.Left != eventData.button || !IsActive())
            {
                return;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_viewport, eventData.position, eventData.pressEventCamera, out Vector2 localCursor))
            {
                // TODO update bounds?
                Vector2 pointerDelta = localCursor - m_pointerStartLocalCursor;
                Vector2 position = m_contentDragStartPosition + pointerDelta;
                // Debug.LogError($"[RecycleScroll] Scroller OnDrag at viewport local pos {localCursor}; delta pos {pointerDelta}; current pos {m_contentBounds.center}");
                // Offset to get content into place in the view.
                Vector2 offset = CalculateOffset((Vector3)position - m_contentBounds.center);
                position += offset;
                if (MovementType.Elastic == _movementType)
                {
                    if (!Mathf.Approximately(0f, offset.x))
                    {
                        position.x -= RubberDelta(offset.x, m_viewportBounds.size.x);
                    }
                    if (!Mathf.Approximately(0f, offset.y))
                    {
                        position.y -= RubberDelta(offset.y, m_viewportBounds.size.y);
                    }
                }

                SetContentPosition(position);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!m_isDragging || PointerEventData.InputButton.Left != eventData.button || !IsActive())
            {
                return;
            }

            // TODO
            if (eventData.pointerId == m_dragPointerId)
            {
                m_dragPointerId = int.MinValue;
                m_isDragging = false;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_viewport, eventData.position, eventData.pressEventCamera, out Vector2 localCursor);
                Debug.LogError($"[RecycleScroll] Scroller OnEndDrag at viewport local pos {localCursor}; current pos {m_contentBounds.center}");
            }
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (!IsActive())
            {
                return;
            }

            // TODO update bounds

            Vector2 delta = eventData.scrollDelta;
            // Down is positive for scroll events, while in UI system up is positive.
            delta.y *= -1;
            if (_vertical && !_horizontal)
            {
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                {
                    delta.y = delta.x;
                }
                delta.x = 0;
            }
            if (_horizontal && !_vertical)
            {
                if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                {
                    delta.x = delta.y;
                }
                delta.y = 0;
            }

            if (eventData.IsScrolling())
            {
                m_isScrolling = true;
            }

            Debug.LogError($"OnScroll");

            Vector3 position = m_contentBounds.center;
            position += (Vector3)delta * _scrollSensitivity;
            if (MovementType.Clamped == _movementType)
            {
                Vector2 offset = CalculateOffset(position - m_contentBounds.center);
                position += (Vector3)offset;

            }

            // TODO update bounds
            // CalculateOffset(Vector2 delta)
            SetContentPosition(position);
            // UpdateBounds();
        }

        protected override void Start()
        {
            InitDefaultBounds();
        }

        private void LateUpdate()
        {
            // TODO Ppdate self bounds and viewport bounds
            // UpdateBounds();

            float deltaTime = Time.unscaledDeltaTime;
            Vector2 offset = CalculateOffset(Vector2.zero);
            if (!m_isDragging && (offset != Vector2.zero || m_velocity != Vector2.zero))
            {
                Vector3 contentPosition = m_contentBounds.center;
                Vector2 position = contentPosition;
                for (int axis = 0; axis < 2; axis++)
                {
                    // Apply spring physics if movement is elastic and content has an offset from the view.
                    if (MovementType.Elastic == _movementType && offset[axis] != 0)
                    {
                        float speed = m_velocity[axis];
                        float smoothTime = _elasticity;
                        if (m_isScrolling)
                        {
                            smoothTime *= 3.0f;
                        }
                        position[axis] = Mathf.SmoothDamp(contentPosition[axis], contentPosition[axis] + offset[axis], ref speed, smoothTime, Mathf.Infinity, deltaTime);
                        if (1 > Mathf.Abs(speed))
                        {
                            speed = 0;
                        }
                        m_velocity[axis] = speed;
                    }
                    // Else move content according to velocity with deceleration applied.
                    else if (_inertia)
                    {
                        m_velocity[axis] *= Mathf.Pow(_decelerationRate, deltaTime);
                        if (1 > Mathf.Abs(m_velocity[axis]))
                        {
                            m_velocity[axis] = 0;
                        }
                        position[axis] += m_velocity[axis] * deltaTime;
                    }
                    // If we have neither elaticity or friction, there shouldn't be any velocity.
                    else
                    {
                        m_velocity[axis] = 0;
                    }
                }

                if (_movementType == MovementType.Clamped)
                {
                    offset = CalculateOffset((Vector3)position - m_contentBounds.center);
                    position += offset;
                }

                // Actually move content
                SetContentPosition(position);
            }

            if (m_isDragging && _inertia)
            {
                Vector3 newVelocity = ((Vector2)m_contentBounds.center - m_prevContentPosition) / deltaTime;
                m_velocity = Vector3.Lerp(m_velocity, newVelocity, deltaTime * 10);
            }

            // if (m_ViewBounds != m_PrevViewBounds || m_ContentBounds != m_PrevContentBounds || m_Content.anchoredPosition != m_PrevPosition)
            {
                // UpdateScrollbars(offset);
                // UISystemProfilerApi.AddMarker("ScrollRect.value", this);
                // m_OnValueChanged.Invoke(normalizedPosition);
            }
            // UpdatePrevData();
            m_prevContentPosition = m_contentBounds.center;
            // UpdateScrollbarVisibility();
            // norma
            // m_onValueChanged.Invoke(normalizedPosition);
            m_isScrolling = false;
        }

    }
}