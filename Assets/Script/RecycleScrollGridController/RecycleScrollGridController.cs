using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RecycleScrollGrid
{
    /// <summary>
    /// TODO @Hiko
    /// how to give those item to the other controller to let them setup stuff
    /// solve data setup issue
    /// did some editor stuff (maybe I can directly use grid layout)
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(ScrollRect))]
    public partial class RecycleScrollGridController : UIBehaviour
    {
        [SerializeField]
        private ScrollRect _scrollRect = null;

        [SerializeField]
        private RectTransform _viewport = null;

        /// <summary>
        /// anchor should be top left 
        /// </summary>
        [SerializeField, Tooltip("the content that used to drag")]
        private RectTransform _content = null; // currently only support 1 type of top left pivor

        // <summary>
        // the actual item count may show in the viewport
        // </summary>
        private int m_viewItemCount = -1;

        private int m_viewItemCountInRow = 0;
        private int m_viewItemCountInColumn = 0;

        /// <summary>
        /// including spacing
        /// </summary>
        private Vector2 m_actualContentSizeRaw = default;

        // TODO @Hiko fix the value serialized issues
        [Space, Header("Grid Layout Setting"), SerializeField]
        private ScrollGridLayoutData _gridLayoutData = new ScrollGridLayoutData();

        [SerializeField]
        private TempListView _listView;
        [SerializeField, ReadOnly]
        /// <summary>
        /// value should >= 0
        /// </summary>
        private int m_simulatedDataCount = 0;

        static readonly RecycleScrollGridElement[] s_emptyElementArray = new RecycleScrollGridElement[0];

        [SerializeField]
        private bool m_drawActualUIItems = true;

        public event Action OnContentItemFinishDrawing;
        public event Action BeforedItemArrayResized;
        public event Action OnItemArrayResized;

        public int ViewItemCount => m_viewItemCount;
        public int ViewItemCountInRow => m_viewItemCountInRow;
        public int ViewItemCountInColumn => m_viewItemCountInColumn;

        public IReadOnlyList<RecycleScrollGridElement> ElementList => _listView != null ? _listView.ElementList : s_emptyElementArray;
        public ScrollGridLayoutData GridLayoutData => _gridLayoutData;
        public RectTransform Content => _content;

        public void Setup(TempListView listView, int dataCount)
        {
            _listView = listView;
            if (dataCount < 0)
                m_simulatedDataCount = _listView.Count;
            else
                m_simulatedDataCount = dataCount;
            AdjustCachedItems();
            ApplySizeOnElements();
            UpdateAcutalContentSizeRaw();
            // refresh
            OnScrollRectValueChanged(Vector2.zero);
        }

        public void UpdateConstraintWithAutoFit()
        {
            float viewportHeight, viewportWidth;
            Vector2 spacing = _gridLayoutData.Spacing;
            viewportHeight = _viewport.rect.height;
            viewportWidth = _viewport.rect.width;
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
            // if (_gridLayoutGroup.isAutoFit)
            // {
            // }
        }

        public void RefreshLayoutChanges()
        {
            UpdateConstraintWithAutoFit();
            UpdateAcutalContentSizeRaw();
            AdjustCachedItems();
            ApplySizeOnElements();
            OnScrollRectValueChanged(Vector2.zero);
        }

        private void NotifyOnContentItemFinishDrawing() { OnContentItemFinishDrawing?.Invoke(); }

        [ContextMenu("Adjust Cached Items")]
        private int CalculateCurrentViewportShowCount()
        {
            m_viewItemCountInRow = 0;
            m_viewItemCountInColumn = 0;
            Vector2 itemSize = new Vector2(_gridLayoutData.gridSize.x, _gridLayoutData.gridSize.y);

            Vector2 spacing = _gridLayoutData.Spacing;
            float viewportHeight = Mathf.Abs(_viewport.rect.height);
            float viewportWidth = Mathf.Abs(_viewport.rect.width);
            m_viewItemCountInColumn = Mathf.FloorToInt(viewportHeight / (itemSize.y + spacing.y));
            m_viewItemCountInRow = Mathf.FloorToInt(viewportWidth / (itemSize.x + spacing.x));

            if (viewportHeight % (itemSize.y + spacing.y) > 0)
                m_viewItemCountInColumn += 2;
            else
                m_viewItemCountInColumn += 1;

            if (viewportWidth % (itemSize.x + spacing.x) > 0)
                m_viewItemCountInRow += 2;
            else
                m_viewItemCountInRow += 1;

            if (ScrollGridLayoutData.Constraint.FixedColumnCount == _gridLayoutData.constraint)
                m_viewItemCountInRow = Mathf.Clamp(m_viewItemCountInRow, 1, _gridLayoutData.constraintCount);
            else
                m_viewItemCountInColumn = Mathf.Clamp(m_viewItemCountInColumn, 1, _gridLayoutData.constraintCount);

            int result = m_viewItemCountInRow * m_viewItemCountInColumn;
            return result;
        }

        private void AdjustCachedItems()
        {
            BeforedItemArrayResized?.Invoke();
            m_viewItemCount = CalculateCurrentViewportShowCount();
            AdjustElementArray(m_viewItemCount);
            ApplySizeOnElements();
            OnItemArrayResized?.Invoke();
        }

        private void UpdateAcutalContentSizeRaw()
        {
            int dataCount = m_simulatedDataCount;
            RectOffset m_padding = _gridLayoutData.RectPadding;
            Vector2 itemSize = _gridLayoutData.gridSize;
            Vector2 spacing = _gridLayoutData.Spacing;
            Vector2 result = default;

            // too bad
            Vector2 viewportSize = _viewport.rect.size;
            int viewItemCountInColumn = Mathf.FloorToInt(viewportSize.y / (itemSize.y + spacing.y));
            int viewItemCountInRow = Mathf.FloorToInt(viewportSize.x / (itemSize.x + spacing.x));
            int viewItemCount = viewItemCountInColumn * viewItemCountInRow;

            // TODO @Hiko when calaulate size, should also deal with padding
            int constraintCount = _gridLayoutData.constraintCount;
            int dynamicCount = (dataCount % constraintCount > 0) ? (dataCount / constraintCount) + 1 : (dataCount / constraintCount);
            if (_gridLayoutData.constraint == ScrollGridLayoutData.Constraint.FixedColumnCount)
            {
                if (dataCount <= viewItemCount)
                    dynamicCount = viewItemCountInColumn;
                result.x = (constraintCount * itemSize.x) + ((constraintCount - 1) * spacing.x);
                result.y = dynamicCount * itemSize.y + (dynamicCount - 1) * spacing.y;
            }
            else if (_gridLayoutData.constraint == ScrollGridLayoutData.Constraint.FixedRowCount)
            {
                if (dataCount <= viewItemCount)
                    dynamicCount = viewItemCountInRow;
                result.y = (constraintCount * itemSize.y) + ((constraintCount - 1) * spacing.y);
                result.x = dynamicCount * itemSize.x + (dynamicCount - 1) * spacing.x;
            }

            m_actualContentSizeRaw = result;
            _content.sizeDelta = m_actualContentSizeRaw;
            if (null != _gridLayoutData)
            {
                RectOffset padding = _gridLayoutData.RectPadding;
                _content.sizeDelta += new Vector2(padding.horizontal, padding.vertical);
            }
        }

        private void OnScrollRectValueChanged(Vector2 position)
        {
#if UNITY_EDITOR
            IReadOnlyList<RecycleScrollGridElement> elementList = ElementList;
            if (m_drawActualUIItems)
            {
                if (_listView == null)
                {
                    Debug.LogWarning("there is no listview setup");
                    return;
                }
                else if (elementList.Count != m_viewItemCount)
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
            IReadOnlyList<RecycleScrollGridElement> elementList = ElementList;
            int dataCount = m_simulatedDataCount;
            // TODO @Hiko use a general calculation
            bool test = _content.anchorMin != Vector2.up || _content.anchorMax != Vector2.up || _content.pivot != Vector2.up;
            if (test)
            {
                _content.anchorMin = Vector2.up;
                _content.anchorMax = Vector2.up;
                _content.pivot = Vector2.up;
            }
            Vector3 dragContentAnchorPostion = _content.anchoredPosition;
            Vector3 contentMove = dragContentAnchorPostion - SomeUtils.GetOffsetLocalPosition(_content, SomeUtils.UIOffsetType.TopLeft);
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
            for (int columnIndex = 0; columnIndex < m_viewItemCountInColumn; columnIndex++)
            {
                if (columnIndex + rowTopLeftItemIndex.x == columnDataCount)
                    break;

                Vector3 rowTopLeftPosition = new Vector3(padding.left, -padding.top, 0.0f) + Vector3.down * (columnIndex + rowTopLeftItemIndex.x) * (itemSize.y + spacing.y);
                for (int rowIndex = 0; rowIndex < m_viewItemCountInRow; rowIndex++)
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

            NotifyOnContentItemFinishDrawing();
        }

        private int CaculateDataIndex(Vector2Int rowColumnIndex, Vector2Int rowColumnSize, GridLayoutGroup.Axis startAxis, GridLayoutGroup.Corner startCorner)
        {
            // for row column index
            // for temp row column indes
            // x -> index on horizontal axis
            // y -> index on vertical axis

            // for row column size
            // x -> element amount on horizontal axis
            // y -> element amount on vertical axis

            // tempIndex and rowColumn size are all start from topLeft
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
            if (_listView == null) return;
            int currentElementCount = ElementList.Count;
            while (size > currentElementCount)
            {
                _listView.Add();
                currentElementCount = ElementList.Count;
            }
            while (size < currentElementCount)
            {
                _listView.RemoveAt(currentElementCount - 1);
                currentElementCount = ElementList.Count;
            }
        }

        private void ApplySizeOnElements()
        {
            if (_listView == null) return;
            // sync the size form grid data
            Vector2 itemAcutalSize = GridLayoutData.gridSize;
            IReadOnlyList<RecycleScrollGridElement> elementList = ElementList;
            for (int i = 0; i < elementList.Count; i++)
                elementList[i].ElementRectTransform.sizeDelta = itemAcutalSize;
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

        #region mono method

        protected override void Reset()
        {
            if (TryGetComponent<ScrollRect>(out _scrollRect))
            {
                _scrollRect.StopMovement();
                _content = _scrollRect.content;
                return;
            }
            Debug.LogWarning("RecycleScrollGridController should be on the same GameObject with ScrollRect, please remove this component and add RecycleScrollGridController to ScrollRect GameObject", this.gameObject);
        }


        protected override void OnEnable()
        {
            UpdateConstraintWithAutoFit();
            _scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
        }

        protected override void OnDisable()
        {
            _scrollRect.onValueChanged.RemoveListener(OnScrollRectValueChanged);
        }

        private void Update()
        {
            ClampVelocityToToStop();
#if UNITY_EDITOR
            if (Application.isEditor && !Application.isPlaying) EditorUpdata();
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
                m_simulatedDataCount = m_editorTimeSimulateDataCount;
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