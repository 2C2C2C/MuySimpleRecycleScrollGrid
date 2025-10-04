using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RecycleScrollGrid
{
    [ExecuteAlways]
    [RequireComponent(typeof(ScrollRect))]
    public partial class RecycleScrollGridController : UIBehaviour
    {
        [SerializeField]
        private ScrollRect _scrollRect = null;
        [SerializeField]
        private RectTransform _gridContainer = null;
        [SerializeField]
        private RecycleScrollGridElement _fallbackElementPrefab = null;

        [SerializeField]
        private bool _drawActualUIGridElements = true;

        // TODO @Hiko fix the value serialized issues
        [Space, Header("Grid Layout Setting"), SerializeField]
        private ScrollGridLayoutData _gridLayoutData = new ScrollGridLayoutData();

        [SerializeField, ReadOnly]
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

        public event Action OnContentItemFinishDrawing;
        public event Action BeforedGridElementsListResized;
        public event Action AfterGridElementListResized;

        public int ViewItemCount => m_viewElementCount;
        public int ViewItemCountInRow => m_viewElementCountInRow;
        public int ViewItemCountInColumn => m_viewElementCountInColumn;

        public IReadOnlyList<RecycleScrollGridElement> ElementList => m_gridElements ??= new List<RecycleScrollGridElement>();
        public ScrollGridLayoutData GridLayoutData => _gridLayoutData;

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
            UpdateConstraintWithAutoFit();
            ApplySizeToScrollContent();
            AdjustCachedItems();
            ApplySizeOnElements();
            OnScrollRectValueChanged(Vector2.zero);
        }

        private void NotifyOnGridLayoutEndFinishDrawing()
        {
            OnContentItemFinishDrawing?.Invoke();
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
            _drawActualUIGridElements = false;
            IReadOnlyList<RecycleScrollGridElement> elementList = ElementList;
            if (_drawActualUIGridElements)
            {
                if (m_listView == null)
                {
                    Debug.LogWarning("there is no listview setup");
                    return;
                }
                else if (elementList.Count != m_viewElementCount)
                    AdjustCachedItems();
                DrawContentItem();
            }
            else
            {
                // hide all Items
                for (int i = 0; i < elementList.Count; i++)
                {
                    elementList[i].SetObjectDeactive();
                }
            }
#else
        DrawContentItem();
#endif
        }

        private void DrawContentItem()
        {
            return;
            RectTransform content = _scrollRect.content;
            IReadOnlyList<RecycleScrollGridElement> elementList = ElementList;
            int dataCount = m_simulatedDataCount;
            // TODO @Hiko use a general calculation
            bool test = content.anchorMin != Vector2.up || content.anchorMax != Vector2.up || content.pivot != Vector2.up;
            if (test)
            {
                content.anchorMin = Vector2.up;
                content.anchorMax = Vector2.up;
                content.pivot = Vector2.up;
            }
            Vector3 dragContentAnchorPostion = content.anchoredPosition;
            Vector3 contentMove = dragContentAnchorPostion - SomeUtils.GetOffsetLocalPosition(content, SomeUtils.UIOffsetType.TopLeft);
            Vector2 itemSize = _gridLayoutData.gridSize, spacing = _gridLayoutData.Spacing;

            RectOffset padding = null;
            if (null != _gridLayoutData)
                padding = _gridLayoutData.RectPadding;

            // TODO need to know the moving direction, then adjust it to prevent wrong draw
            float xMove = contentMove.x < 0 ? (-contentMove.x - padding.horizontal) : 0;
            xMove = Mathf.Clamp(xMove, 0.0f, Mathf.Abs(xMove));
            float yMove = contentMove.y > 0 ? (contentMove.y - padding.vertical) : 0;
            yMove = Mathf.Clamp(yMove, 0.0f, Mathf.Abs(yMove));

            // the column index of the top left item
            int tempColumnIndex = Mathf.FloorToInt((xMove + spacing.x) / (itemSize.x + spacing.x));
            if (xMove % (itemSize.x + spacing.x) - itemSize.x > spacing.x)
                tempColumnIndex = Mathf.Clamp(tempColumnIndex - 1, 0, tempColumnIndex);

            // the row index of the top left item
            int tempRowIndex = Mathf.FloorToInt((yMove + spacing.y) / (itemSize.y + spacing.y));
            if (yMove % (itemSize.y + spacing.y) - itemSize.y > spacing.y)
                tempRowIndex = Mathf.Clamp(tempRowIndex - 1, 0, tempRowIndex);

            Vector2Int rowTopLeftItemIndex = new Vector2Int(tempRowIndex, tempColumnIndex);
            int columnDataCount, rowDataCount;
            if (ScrollGridLayoutData.Constraint.FixedColumnCount == _gridLayoutData.constraint)
            {
                rowDataCount = _gridLayoutData.constraintCount;
                columnDataCount = Mathf.CeilToInt((float)dataCount / rowDataCount);
            }
            else
            {
                columnDataCount = _gridLayoutData.constraintCount;
                rowDataCount = Mathf.CeilToInt((float)dataCount / columnDataCount);
            }

            // x -> element amount on horizontal axis
            // y -> element amount on vertical axis
            Vector2Int contentRowColumnSize = new Vector2Int(rowDataCount, columnDataCount);

            // deal with content from left to right (simple case)
            int dataIndex = 0, uiItemIndex = 0;
            for (int columnIndex = 0; columnIndex < m_viewElementCountInColumn; columnIndex++)
            {
                if (columnIndex + rowTopLeftItemIndex.x == columnDataCount)
                    break;

                Vector3 rowTopLeftPosition = new Vector3(padding.left, -padding.top, 0.0f) + Vector3.down * (columnIndex + rowTopLeftItemIndex.x) * (itemSize.y + spacing.y);
                for (int rowIndex = 0; rowIndex < m_viewElementCountInRow; rowIndex++)
                {
                    if (rowIndex + rowTopLeftItemIndex.y == rowDataCount)
                        break;

                    Vector2Int elementIndex = new Vector2Int(rowIndex + rowTopLeftItemIndex.y, columnIndex + rowTopLeftItemIndex.x);
                    dataIndex = CaculateDataIndex(elementIndex, contentRowColumnSize, GridLayoutData.startAxis, GridLayoutData.startCorner);
                    Vector3 itemTopLeftPosition = rowTopLeftPosition + Vector3.right * (rowIndex + rowTopLeftItemIndex.y) * (itemSize.x + spacing.x);

                    // TODO @Hiko avoid overdraw
                    if (uiItemIndex > 0 && elementList[uiItemIndex - 1].ElementIndex == dataIndex)
                        continue; // over draw case
                    if (dataIndex > -1 && dataIndex < dataCount)
                    {
                        elementList[uiItemIndex].ElementRectTransform.localPosition = itemTopLeftPosition;
                        elementList[uiItemIndex].SetIndex(dataIndex);
                        elementList[uiItemIndex].SetObjectActive();
                        uiItemIndex++;
                    }
                }
            }

            while (uiItemIndex < elementList.Count)
            {
                elementList[uiItemIndex].SetIndex(-1);
                elementList[uiItemIndex].SetObjectDeactive();
                elementList[uiItemIndex].ElementRectTransform.position = Vector3.zero;
                uiItemIndex++;
            }

            NotifyOnGridLayoutEndFinishDrawing();
        }

        private int CaculateDataIndex(Vector2Int rowColumnIndex, Vector2Int rowColumnSize, GridLayoutGroup.Axis startAxis, GridLayoutGroup.Corner startCorner)
        {
            // For row column index
            // For temp row column indes
            // x -> index on horizontal axis
            // y -> index on vertical axis

            // For row column size
            // x -> element amount on horizontal axis
            // y -> element amount on vertical axis

            // TempIndex and rowColumn size are all start from topLeft
            int result = 0;
            if (startAxis == GridLayoutGroup.Axis.Horizontal)
            {
                switch (startCorner)
                {
                    case GridLayoutGroup.Corner.UpperLeft:
                        result = rowColumnIndex.y * rowColumnSize.x + rowColumnIndex.x;
                        break;
                    case GridLayoutGroup.Corner.LowerLeft:
                        result = (rowColumnSize.y - rowColumnIndex.y - 1) * rowColumnSize.x + rowColumnIndex.x;
                        break;
                    case GridLayoutGroup.Corner.UpperRight:
                        result = rowColumnIndex.y * rowColumnSize.x + rowColumnSize.x - rowColumnIndex.x - 1;
                        break;
                    case GridLayoutGroup.Corner.LowerRight:
                        result = (rowColumnSize.y - rowColumnIndex.y - 1) * rowColumnSize.x + rowColumnSize.x - rowColumnIndex.x - 1;
                        break;
                    default:
                        Debug.LogError("start corner type error", this.gameObject);
                        break;
                }
            }
            else //if (startAxis == GridLayoutGroup.Axis.Vertical)
            {
                switch (startCorner)
                {
                    case GridLayoutGroup.Corner.UpperLeft:
                        result = rowColumnIndex.x * rowColumnSize.y + rowColumnIndex.y;
                        break;
                    case GridLayoutGroup.Corner.LowerLeft:
                        result = rowColumnIndex.x * rowColumnSize.y + rowColumnSize.y - rowColumnIndex.y - 1;
                        break;
                    case GridLayoutGroup.Corner.UpperRight:
                        result = (rowColumnSize.x - rowColumnIndex.x - 1) * rowColumnSize.y + rowColumnIndex.y;
                        break;
                    case GridLayoutGroup.Corner.LowerRight:
                        result = (rowColumnSize.x - rowColumnIndex.x - 1) * rowColumnSize.y + rowColumnSize.y - rowColumnIndex.y - 1;
                        break;
                    default:
                        Debug.LogError("start corner type error", this.gameObject);
                        break;
                }
            }

            return result;
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
                0 >= _gridLayoutData.gridSize.x ||
                0 >= _gridLayoutData.gridSize.y ||
                0 >= _gridLayoutData.constraintCount;
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
                    m_listView.InitElement(target, -1);
                }
            }
        }

        private void RemoveElements(int count)
        {
            // Try remove non-used elements first
            if (null == m_listView)
            {
                for (int i = 0; i < count; i++)
                {
                    // m_gridElements
                }
            }
            else
            {
                // RecycleScrollGridElement element
                // RectTransform target = element.ElementRectTransform;
                // m_listView.UninitElement(target);
                // m_listView.RemoveElement(target);
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
            UpdateConstraintWithAutoFit();
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

        private void Update()
        {
            ClampVelocityToToStop();
#if UNITY_EDITOR
            if (Application.isEditor && !Application.isPlaying)
            {
                EditorUpdata();
            }
#endif
        }

#if UNITY_EDITOR

        [Header("editor time test")]
        [SerializeField]
        int m_editorTimeSimulateDataCount = 5;
        [SerializeField]
        bool m_showEditorPreview = false;
        private void EditorUpdata()
        {
            if (m_showEditorPreview)
            {
                // m_simulatedDataCount = m_editorTimeSimulateDataCount;
                OnScrollRectValueChanged(Vector2.zero);
                m_showEditorPreview = false;
            }
            return;

            //// TODO @Hiko for editor loop
            //if (Content.hasChanged)
            //{
            //    // Debug.Log("editor time tick");
            //    m_simulatedDataCount = m_editorTimeSimulateDataCount;
            //    OnScrollRectValueChanged(Vector2.zero);

            //    // get current position by scrolbar
            //    Scrollbar scrollerBar = m_scrollRect.verticalScrollbar;
            //    float normalizedVerticalScrollValue = 0.0f;
            //    if (scrollerBar != null)
            //        normalizedVerticalScrollValue = scrollerBar.direction == Scrollbar.Direction.TopToBottom ? scrollerBar.value :
            //        scrollerBar.direction == Scrollbar.Direction.BottomToTop ? 1.0f - scrollerBar.value : 0.0f;

            //    float normalizedHorizontalScrollValue = 0.0f;
            //    scrollerBar = m_scrollRect.horizontalScrollbar;
            //    if (scrollerBar != null)
            //        normalizedHorizontalScrollValue = scrollerBar.direction == Scrollbar.Direction.LeftToRight ? scrollerBar.value :
            //        scrollerBar.direction == Scrollbar.Direction.RightToLeft ? 1.0f - scrollerBar.value : 0.0f;

            //    // from top left
            //}
        }

#endif

        #endregion
    }
}