using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extend;

namespace RecycleScrollGrid
{
    [ExecuteAlways]
    [RequireComponent(typeof(ScrollRect))]
    public partial class RecycleScrollGridController : UIBehaviour
    {
        private static Comparison<RecycleScrollGridElement> s_gridElementCompare;

        public static Comparison<RecycleScrollGridElement> GridElementCompare
        {
            get
            {
                if (null == s_gridElementCompare)
                {
                    s_gridElementCompare = new Comparison<RecycleScrollGridElement>((x, y) =>
                    {
                        int xIndex = x.ElementIndex, yIndex = y.ElementIndex;
                        if (xIndex == yIndex)
                        {
                            return 0;
                        }

                        // Minus value need to be on the back
                        if (0 > xIndex && 0 <= yIndex)
                        {
                            return 1;
                        }
                        else if (0 <= xIndex && 0 > yIndex)
                        {
                            return -1;
                        }

                        return xIndex.CompareTo(yIndex);
                    });
                }
                return s_gridElementCompare;
            }
        }

        [SerializeField]
        private ScrollRect _scrollRect = null;
        [SerializeField]
        private RectTransform _gridContainer = null;
        [SerializeField]
        private RecycleScrollGridElement _fallbackElementPrefab = null;

        [SerializeField]
        private bool _drawActualUIGridElements = true;

        [Space, Header("Grid Layout Setting"), SerializeField]
        private ScrollGridLayoutData _gridLayoutData = new ScrollGridLayoutData();

        [SerializeField] // This value should be NonSerialized but better to show it in inspector
        /// <summary> The value should greater than 0 </summary>
        private int m_simulatedDataCount = 0;

        // <summary>
        // The actual element count may show in the viewport
        // </summary>
        private int m_viewElementCount = -1;

        private int m_viewElementCountInRow = 0;
        private int m_viewElementCountInColumn = 0;

        private IListView m_listView = null;
        private List<RecycleScrollGridElement> m_gridElements;

        private UnityAction<Vector2> m_onScrollRectValueChanged;
        public event Action BeforedGridElementsListResized;
        public event Action AfterGridElementListResized;

        public int ViewItemCount => m_viewElementCount;
        public int ViewItemCountInRow => m_viewElementCountInRow;
        public int ViewItemCountInColumn => m_viewElementCountInColumn;

        public IReadOnlyList<RecycleScrollGridElement> ElementList => m_gridElements ??= new List<RecycleScrollGridElement>();
        public ScrollGridLayoutData GridLayoutData => _gridLayoutData;
        public int SimulatedDataCount => m_simulatedDataCount;

        public void Init(IListView listView)
        {
            if (null == m_listView)
            {
                if (null == listView)
                {
                    Debug.LogError("RecycleScrollGridController Init failed, the listview is null", this.gameObject);
                    return;
                }
                m_listView = listView;
                RefreshLayoutChanges();
            }
        }

        public void Uninit()
        {
            if (null != m_listView)
            {
                for (int i = 0, length = m_gridElements.Count; i < length; i++)
                {
                    RectTransform gridRectTransform = m_gridElements[i].ElementRectTransform;
                    m_listView.UnInitElement(gridRectTransform);
                    m_listView.RemoveElement(gridRectTransform);
                }
                m_gridElements.Clear();
                m_listView = null;
            }
        }

        public void UpdateConstraintWithAutoFit()
        {
            float viewportHeight, viewportWidth;
            RectTransform viewport = _scrollRect.viewport;
            Vector2 spacing = _gridLayoutData.Spacing;
            viewportHeight = viewport.rect.height;
            viewportWidth = viewport.rect.width;
            Vector2 itemSize = new Vector2(_gridLayoutData.gridSize.x, _gridLayoutData.gridSize.y);

            int constraintCount;
            if (ScrollGridLayoutData.Constraint.FixedColumnCount == _gridLayoutData.constraint)
            {
                constraintCount = Mathf.FloorToInt(viewportWidth / (itemSize.x + spacing.x));
            }
            else
            {
                constraintCount = Mathf.FloorToInt(viewportHeight / (itemSize.y + spacing.y));
            }

            constraintCount = Mathf.Clamp(constraintCount, 1, int.MaxValue);
            _gridLayoutData.constraintCount = constraintCount;
        }

        public void RefreshLayoutChanges()
        {
            //UpdateConstraintWithAutoFit();
            ApplySizeToScrollContent();
            AdjustCachedItems();
            ApplySizeOnElements();
            OnScrollRectValueChanged(Vector2.zero);
        }

        [ContextMenu("Adjust Cached Items")]
        private int CalculateCurrentViewportShowCount()
        {
            m_viewElementCountInRow = 0;
            m_viewElementCountInColumn = 0;
            Vector2 gridSize = new Vector2(_gridLayoutData.gridSize.x, _gridLayoutData.gridSize.y);

            Vector2 spacing = _gridLayoutData.Spacing;
            RectTransform viewport = _scrollRect.viewport;
            float viewportHeight = Mathf.Abs(viewport.rect.height);
            float viewportWidth = Mathf.Abs(viewport.rect.width);
            m_viewElementCountInColumn = Mathf.FloorToInt(viewportHeight / (gridSize.y + spacing.y));
            m_viewElementCountInRow = Mathf.FloorToInt(viewportWidth / (gridSize.x + spacing.x));

            m_viewElementCountInColumn += (0 < viewportHeight % (gridSize.y + spacing.y)) ? 2 : 1;
            m_viewElementCountInRow += (0 > viewportWidth % (gridSize.x + spacing.x)) ? 2 : 1;

            if (ScrollGridLayoutData.Constraint.FixedColumnCount == _gridLayoutData.constraint)
            {
                m_viewElementCountInRow = Mathf.Clamp(m_viewElementCountInRow, 1, _gridLayoutData.constraintCount);
            }
            else
            {
                m_viewElementCountInColumn = Mathf.Clamp(m_viewElementCountInColumn, 1, _gridLayoutData.constraintCount);
            }

            int result = m_viewElementCountInRow * m_viewElementCountInColumn;
            return result;
        }

        private void AdjustCachedItems()
        {
            BeforedGridElementsListResized?.Invoke();
            m_viewElementCount = CalculateCurrentViewportShowCount();
            AdjustElementArray(m_viewElementCount);
            ApplySizeOnElements();
            AfterGridElementListResized?.Invoke();
        }

        private void ApplySizeToScrollContent()
        {
            if (null != m_listView)
            {
                m_simulatedDataCount = m_listView.DataElementCount;
            }
            int dataCount = m_simulatedDataCount;
            // m_simulatedDataCount
            Vector2 contentSize = CalculateContentSize(dataCount);
            RectTransform content = _scrollRect.content;
            // TODO Directly change sizeDelta is not safe
            content.sizeDelta = contentSize;
        }

        private void OnScrollRectValueChanged(Vector2 position)
        {
#if UNITY_EDITOR
            IReadOnlyList<RecycleScrollGridElement> elementList = ElementList;
            if (_drawActualUIGridElements)
            {
                if (m_listView == null)
                {
                    Debug.LogWarning("there is no listview setup");
                    return;
                }
                else if (elementList.Count != m_viewElementCount)
                {
                    AdjustCachedItems();
                }
                // UpdateGrids();
                UpdateGridPosition();
            }
            else
            {
                // Hide all Items
                for (int i = 0; i < elementList.Count; i++)
                {
                    elementList[i].SetObjectDeactive();
                }
            }
#else
            UpdateGridPosition();
#endif
        }

        private void UpdateGridPosition()
        {
            if (IsCurrentLayoutDataInvalid())
            {
                return;
            }

            bool hasValidListView = null != m_listView;
            int dataCount = hasValidListView ? m_listView.DataElementCount : SimulatedDataCount;
            ScrollGridLayoutData gridLayoutData = _gridLayoutData;
            RectTransform scrollContent = _scrollRect.content;
            Vector2 rawSize = CalculateContentSize(dataCount);
            Vector2 contentPivot = scrollContent.pivot;
            Vector2 pivotLocalPosition = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(scrollContent, contentPivot);
            Vector2 startLocalPosition = pivotLocalPosition;  // The start position of the first grid(bottom left)
            startLocalPosition.x = pivotLocalPosition.x - rawSize.x * contentPivot.x;
            startLocalPosition.y = pivotLocalPosition.y - rawSize.y * contentPivot.y;

            Vector2 gridSize = _gridLayoutData.gridSize;
            switch (_gridLayoutData.startCorner)
            {
                case GridLayoutGroup.Corner.LowerRight:
                    startLocalPosition.x += rawSize.x;
                    startLocalPosition.x -= gridLayoutData.RectPadding.right;
                    startLocalPosition.y += gridLayoutData.RectPadding.bottom;
                    startLocalPosition.x -= gridSize.x;
                    break;
                case GridLayoutGroup.Corner.UpperRight:
                    startLocalPosition += rawSize;
                    startLocalPosition.x -= gridLayoutData.RectPadding.right;
                    startLocalPosition.y -= gridLayoutData.RectPadding.top;
                    startLocalPosition.x -= gridSize.x;
                    startLocalPosition.y -= gridSize.y;
                    break;
                case GridLayoutGroup.Corner.UpperLeft:
                    startLocalPosition.y += rawSize.y;
                    startLocalPosition.x += gridLayoutData.RectPadding.left;
                    startLocalPosition.y -= gridLayoutData.RectPadding.top;
                    startLocalPosition.y -= gridSize.y;
                    break;
                case GridLayoutGroup.Corner.LowerLeft:
                default:
                    startLocalPosition.x += gridLayoutData.RectPadding.left;
                    startLocalPosition.y += gridLayoutData.RectPadding.bottom;
                    break;
            }

            Vector2 gridGroupMoveDirection = CalculateGridGroupMoveDirection();
            Vector2 gridGroupSpacing = Mathf.Approximately(0f, gridGroupMoveDirection.x) ? _gridLayoutData.Spacing.y * Vector2.up : _gridLayoutData.Spacing.x * Vector2.right;
            Vector2 girdMoveDirection = CalculateGridMoveDirectionInGroup();
            Vector2 gridSpacing = Mathf.Approximately(0f, girdMoveDirection.x) ? _gridLayoutData.Spacing.y * Vector2.up : _gridLayoutData.Spacing.x * Vector2.right;

            Rect viewportRect = _scrollRect.viewport.rect;
            Matrix4x4 worldToViewportLocal = _scrollRect.viewport.worldToLocalMatrix;

            RectTransform content = _scrollRect.content;
            Matrix4x4 localToWorld = content.localToWorldMatrix;
            int constraintCount = _gridLayoutData.constraintCount;
            int groupCount = (dataCount % constraintCount > 0) ? (dataCount / constraintCount) + 1 : (dataCount / constraintCount);
            Vector2 groupStartPos = startLocalPosition;
            int gridDataIndex = 0;
            int usedElementIndex = 0;
            int gridElementCount = m_gridElements.Count;
            for (int i = 0; i < groupCount; i++)
            {
                Vector2 gridGroupStartPos = groupStartPos + (i * new Vector2(gridGroupMoveDirection.x * gridGroupSpacing.x, gridGroupMoveDirection.y * gridGroupSpacing.y));
                gridGroupStartPos += i * new Vector2(gridGroupMoveDirection.x * gridSize.x, gridGroupMoveDirection.y * gridSize.y);
                for (int j = 0; j < constraintCount; j++)
                {
                    bool isOutOfDataCount = gridDataIndex >= dataCount;
                    if (isOutOfDataCount || usedElementIndex >= gridElementCount)
                    {
                        ++gridDataIndex;
                        continue;
                    }

                    Vector2 gridStartPos = gridGroupStartPos + (j * new Vector2(girdMoveDirection.x * gridSpacing.x, girdMoveDirection.y * gridSpacing.y));
                    gridStartPos += j * new Vector2(girdMoveDirection.x * gridSize.x, girdMoveDirection.y * gridSize.y);

                    // HACK Have to covert the rect to viewport's local space
                    Vector3 worldPos = localToWorld.MultiplyPoint(gridStartPos);
                    Vector2 rectMinPoint = worldToViewportLocal.MultiplyPoint(worldPos);
                    Rect gridRect = new Rect(rectMinPoint, gridSize);
                    bool isInterestedWithViewport = viewportRect.Overlaps(gridRect);
                    if (isInterestedWithViewport)
                    {
                        RecycleScrollGridElement gridElement = m_gridElements[usedElementIndex];
                        gridElement.ElementRectTransform.localPosition = gridStartPos;
                        int prevIndex = gridElement.ElementIndex;
                        gridElement.SetIndex(gridDataIndex);
                        gridElement.SetObjectActive();
                        if (hasValidListView)
                        {
                            if (0 <= prevIndex) // Prev index valid
                            {
                                m_listView.OnElementIndexChanged(gridElement.ElementRectTransform, prevIndex, gridDataIndex);
                            }
                            else
                            {
                                m_listView.InitElement(gridElement.ElementRectTransform, gridDataIndex);
                            }
                        }
                        ++usedElementIndex;
                    }
                    ++gridDataIndex;
                }
            }

            for (int i = usedElementIndex; i < gridElementCount; i++)
            {
                RecycleScrollGridElement gridElement = m_gridElements[i];
                bool prevIndexValid = 0 <= gridElement.ElementIndex;
                gridElement.SetIndex(-1);
                gridElement.SetObjectDeactive();
                if (hasValidListView && prevIndexValid)
                {
                    m_listView.UnInitElement(gridElement.ElementRectTransform);
                }
            }

        }

        private void ClampVelocityToToStop()
        {
            float sqrLimit = _gridLayoutData.scrollStopVelocityMagSqr;
            sqrLimit *= sqrLimit;
            float velocitySqrMag = _scrollRect.velocity.sqrMagnitude;
            if (velocitySqrMag < sqrLimit && !Mathf.Approximately(0.0f, velocitySqrMag)) // try to clamped move to save 
                _scrollRect.StopMovement();
        }

        private void AdjustElementArray(int size)
        {
            int currentElementCount = ElementList.Count;
            int deltaCount = size - currentElementCount;
            if (0 < deltaCount)
            {
                // Need to add element
                AddElements(deltaCount);
            }
            if (0 > deltaCount && currentElementCount > 0)
            {
                RemoveElements(deltaCount);
            }
        }

        private void ApplySizeOnElements()
        {
            if (null == m_listView)
            {
                return;
            }
            // sync the size form grid data
            Vector2 itemAcutalSize = GridLayoutData.gridSize;
            IReadOnlyList<RecycleScrollGridElement> elementList = ElementList;
            for (int i = 0; i < elementList.Count; i++)
            {
                elementList[i].ElementRectTransform.sizeDelta = itemAcutalSize;
            }
        }

        private Vector2 CalculateContentSize(int dataCount)
        {
            RectOffset m_padding = _gridLayoutData.RectPadding;
            Vector2 gridSize = _gridLayoutData.gridSize;
            Vector2 spacing = _gridLayoutData.Spacing;
            Vector2 result = default;

            int constraintCount = _gridLayoutData.constraintCount;
            int groupCount = (dataCount % constraintCount > 0) ? (dataCount / constraintCount) + 1 : (dataCount / constraintCount);
            if (_gridLayoutData.constraint == ScrollGridLayoutData.Constraint.FixedColumnCount)
            {
                result.x = (constraintCount * gridSize.x) + ((constraintCount - 1) * spacing.x);
                result.y = groupCount * gridSize.y + (groupCount - 1) * spacing.y;
            }
            else if (_gridLayoutData.constraint == ScrollGridLayoutData.Constraint.FixedRowCount)
            {
                result.y = (constraintCount * gridSize.y) + ((constraintCount - 1) * spacing.y);
                result.x = groupCount * gridSize.x + (groupCount - 1) * spacing.x;
            }

            result += new Vector2(m_padding.horizontal, m_padding.vertical);
            return result;
        }

        private Vector2 CalculateGridGroupMoveDirection()
        {
            Vector2 gridGroupMoveDirection = default;
            ScrollGridLayoutData layoutData = _gridLayoutData;
            if (GridLayoutGroup.Axis.Horizontal == layoutData.startAxis)
            {
                if (GridLayoutGroup.Corner.LowerLeft == _gridLayoutData.startCorner ||
                    GridLayoutGroup.Corner.LowerRight == _gridLayoutData.startCorner)
                {
                    gridGroupMoveDirection.y = 1f;
                }
                else
                {
                    gridGroupMoveDirection.y = -1f;
                }
            }
            else // GridLayoutGroup.Axis.Vertical == layoutData.startAxis
            {
                if (GridLayoutGroup.Corner.UpperLeft == _gridLayoutData.startCorner ||
                    GridLayoutGroup.Corner.LowerLeft == _gridLayoutData.startCorner)
                {
                    gridGroupMoveDirection.x = 1f;
                }
                else
                {
                    gridGroupMoveDirection.x = -1f;
                }
            }
            return gridGroupMoveDirection;
        }

        private Vector2 CalculateGridMoveDirectionInGroup()
        {
            Vector2 girdMoveDirection = default;
            ScrollGridLayoutData layoutData = _gridLayoutData;
            if (GridLayoutGroup.Axis.Horizontal == layoutData.startAxis)
            {
                if (GridLayoutGroup.Corner.LowerLeft == _gridLayoutData.startCorner ||
                    GridLayoutGroup.Corner.UpperLeft == _gridLayoutData.startCorner)
                {
                    girdMoveDirection.x = 1f;
                }
                else
                {
                    girdMoveDirection.x = -1f;
                }
            }
            else // GridLayoutGroup.Axis.Vertical == layoutData.startAxis
            {
                if (GridLayoutGroup.Corner.LowerLeft == _gridLayoutData.startCorner ||
                    GridLayoutGroup.Corner.LowerRight == _gridLayoutData.startCorner)
                {
                    girdMoveDirection.y = 1f;
                }
                else
                {
                    girdMoveDirection.y = -1f;
                }
            }
            return girdMoveDirection;
        }

        private bool IsCurrentLayoutDataInvalid()
        {
            bool isInvalid = null == _gridLayoutData ||
                0 > _gridLayoutData.gridSize.x ||
                0 > _gridLayoutData.gridSize.y ||
                0 > _gridLayoutData.constraintCount;
            return isInvalid;
        }

        private void AddElements(int count)
        {
            Vector2 gridSize = _gridLayoutData.gridSize;
            if (null == m_listView)
            {
                for (int i = 0; i < count; i++)
                {
                    RecycleScrollGridElement added;
                    added = RecycleScrollGridElement.Instantiate(_fallbackElementPrefab, _gridContainer);
                    added.SetElementSize(gridSize);
                    m_gridElements.Add(added);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    RectTransform target = m_listView.AddElement(_gridContainer);
                    if (!target.gameObject.TryGetComponent<RecycleScrollGridElement>(out RecycleScrollGridElement added))
                    {
                        Debug.LogError("the element prefab does not have RecycleScrollGridElement component", target.gameObject);
                        return;
                    }
                    added.SetElementSize(gridSize);
                    m_gridElements.Add(added);
                    m_listView.UnInitElement(target);
                }
            }
        }

        private void RemoveElements(int count)
        {
            // Make sure non-used elements on the back
            m_gridElements.Sort(GridElementCompare);
            int elementCount = m_gridElements.Count;
            // Try remove non-used elements first
            if (null == m_listView)
            {
                for (int i = 0; i < count; i++)
                {
                    int elementIndex = elementCount - i - 1;
                    GameObject.Destroy(m_gridElements[elementIndex].gameObject);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    int elementIndex = elementCount - i - 1;
                    m_listView.RemoveElement(m_gridElements[elementIndex].ElementRectTransform);
                }
            }

            if (count == elementCount)
            {
                m_gridElements.Clear();
            }
            else
            {
                m_gridElements.RemoveRange(elementCount - count, count);
            }
        }

        #region mono method

        protected override void Reset()
        {
            if (TryGetComponent<ScrollRect>(out _scrollRect))
            {
                _scrollRect.StopMovement();
                return;
            }
            Debug.LogWarning("RecycleScrollGridController should be on the same GameObject with ScrollRect, please remove this component and add RecycleScrollGridController to ScrollRect GameObject", this.gameObject);
        }

        protected override void OnEnable()
        {
            //UpdateConstraintWithAutoFit();
            if (null == m_onScrollRectValueChanged)
            {
                m_onScrollRectValueChanged = new UnityAction<Vector2>(OnScrollRectValueChanged);
            }
            _scrollRect.onValueChanged.AddListener(m_onScrollRectValueChanged);
        }

        protected override void OnDisable()
        {
            if (null != m_onScrollRectValueChanged)
            {
                _scrollRect.onValueChanged.RemoveListener(m_onScrollRectValueChanged);
            }
        }

        private void LateUpdate()
        {
            ClampVelocityToToStop();
        }

        protected override void OnDestroy()
        {
            if (0 < m_gridElements.Count)
            {
                RemoveElements(m_gridElements.Count);
                m_gridElements.Clear();
            }
        }

        #endregion
    }
}