using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Only support the situation that item start from top left
/// </summary>
/// <typeparam name="T">T is a data for each grid item</typeparam>
[RequireComponent(typeof(ScrollRect))]
public abstract partial class BoundlessScrollRectController<T> : UIBehaviour
{
    [SerializeField]
    private ScrollRect m_scrollRect = null;

    [SerializeField]
    private RectTransform m_viewport = null;
    // !!! these 2 content anchor are on top left 
    [SerializeField, Tooltip("the content that used to drag")]
    private RectTransform m_dragContent = null; // currently only support 1 type of top left pivor
    // !!! these 2 content anchor are on top left 

    // Transform m_rootTransfrom = null;
    private int m_viewItemCount = 0;
    ///// <summary>
    ///// the actual item count can show in the viewport
    ///// </summary>
    //private int m_actualViewItemCount = 0;
    private int m_viewItemCountInRow = 0;
    private int m_viewItemCountInColumn = 0;

    /// <summary>
    /// including spacing
    /// </summary>
    private Vector2 m_actualContentSizeRaw = default;

    private IReadOnlyList<T> m_dataList = null;

    //[SerializeField]
    //private bool m_extendEmptySlotsToFitRow = false;

    /* a test component, we will move this component
    * and use this to setup the grid size
    */
    [Space, Header("Grid Layout Setting"), SerializeField]
    private BoundlessGridLayoutData m_gridLayoutGroup = default;

    [SerializeField]
    private BoundlessBaseScrollRectItem<T> m_gridItemPrefab = null;

    public RectTransform Viewport => m_viewport;
    public RectTransform DragContent => m_dragContent;
    public BoundlessGridLayoutData GridLayoutData => m_gridLayoutGroup;
    public abstract BoundlessBaseScrollRectItem<T> GridItemPrefab { get; }
    protected abstract BoundlessBaseScrollRectItem<T>[] GridItemArray { get; }

    public void RefreshLayout()
    {
        // if value on inspector got changed or some value being changed by code, should also call this
        if (null == m_dataList)
            return;
        UpdateAcutalContentSizeRaw();
        OnScrollRectValueChanged(Vector2.zero);
    }

    public void Setup(IReadOnlyList<T> dataList)
    {
        m_dataList = dataList;
        CalculateViewportShowCount();
        AdjustCachedItems();
        SyncSize();
        UpdateAcutalContentSizeRaw();

        // refresh
        RefreshItemStartPosition();
        OnScrollRectValueChanged(Vector2.zero);
    }

    protected abstract void ResizeGridItemsListSize(int size);
    protected abstract void BeforedCachedItemRefreshed();
    protected abstract void OnCachedItemRefreshed();
    protected abstract void OnContentItemFinishDrawing();

    public void UpdateConstraintWithAutoFit()
    {
        if (GridLayoutData.IsAutoFit)
        {
            int constraintCount = 0;
            float viewportHeight = 0.0f, viewportWidth = 0.0f;
            Vector2 spacing = GridLayoutData.Spacing;
            viewportHeight = m_viewport.rect.height;
            viewportWidth = m_viewport.rect.width;
            Vector2 itemSize = new Vector2(GridLayoutData.CellSize.x, GridLayoutData.CellSize.y);

            if (BoundlessGridLayoutData.Constraint.FixedColumnCount == GridLayoutData.constraint)
                constraintCount = Mathf.FloorToInt(viewportWidth / (itemSize.x + spacing.x));
            else
                constraintCount = Mathf.FloorToInt(viewportHeight / (itemSize.y + spacing.y));

            constraintCount = Mathf.Clamp(constraintCount, 1, int.MaxValue);
            GridLayoutData.constraintCount = constraintCount;
        }
    }

    protected void CalculateViewportShowCount()
    {
        m_viewItemCountInRow = 0;
        m_viewItemCountInColumn = 0;
        Vector2 itemSize = new Vector2(m_gridLayoutGroup.CellSize.x, m_gridLayoutGroup.CellSize.y);

        Vector2 spacing = m_gridLayoutGroup.Spacing;
        float viewportHeight = Mathf.Abs(m_viewport.rect.height);
        float viewportWidth = Mathf.Abs(m_viewport.rect.width);
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

        if (BoundlessGridLayoutData.Constraint.FixedColumnCount == GridLayoutData.constraint)
            m_viewItemCountInRow = Mathf.Clamp(m_viewItemCountInRow, 1, GridLayoutData.constraintCount);
        else
            m_viewItemCountInColumn = Mathf.Clamp(m_viewItemCountInColumn, 1, GridLayoutData.constraintCount);

        m_viewItemCount = m_viewItemCountInRow * m_viewItemCountInColumn;
    }

    private void AdjustCachedItems()
    {
        BeforedCachedItemRefreshed();
        ResizeGridItemsListSize(m_viewItemCount);
        OnCachedItemRefreshed();
    }

    private void UpdateAcutalContentSizeRaw()
    {
        RectOffset m_padding = GridLayoutData.RectPadding;
        Vector2 itemSize = m_gridLayoutGroup.CellSize;
        Vector2 spacing = m_gridLayoutGroup.Spacing;
        int dataCount = m_dataList.Count;
        Vector2 result = default;

        // too bad
        Vector2 viewportSize = Viewport.rect.size;
        int viewItemCountInColumn = Mathf.FloorToInt(viewportSize.y / (itemSize.y + spacing.y));
        int viewItemCountInRow = Mathf.FloorToInt(viewportSize.x / (itemSize.x + spacing.x));
        int viewItemCount = viewItemCountInColumn * viewItemCountInRow;

        // TODO @Hiko when calaulate size, should also deal with padding
        int constraintCount = m_gridLayoutGroup.constraintCount;
        int dynamicCount = (dataCount % constraintCount > 0) ? (dataCount / constraintCount) + 1 : (dataCount / constraintCount);
        if (m_gridLayoutGroup.constraint == BoundlessGridLayoutData.Constraint.FixedColumnCount)
        {
            if (dataCount <= viewItemCount)
                dynamicCount = viewItemCountInColumn;
            result.x = (constraintCount * itemSize.x) + ((constraintCount - 1) * spacing.x);
            result.y = dynamicCount * itemSize.y + (dynamicCount - 1) * spacing.y;
        }
        else if (m_gridLayoutGroup.constraint == BoundlessGridLayoutData.Constraint.FixedRowCount)
        {
            if (dataCount <= viewItemCount)
                dynamicCount = viewItemCountInRow;
            result.y = (constraintCount * itemSize.y) + ((constraintCount - 1) * spacing.y);
            result.x = dynamicCount * itemSize.x + (dynamicCount - 1) * spacing.x;
        }

        m_actualContentSizeRaw = result;
        m_dragContent.sizeDelta = m_actualContentSizeRaw;
        if (null != GridLayoutData)
        {
            RectOffset padding = GridLayoutData.RectPadding;
            m_dragContent.sizeDelta += new Vector2(padding.horizontal, padding.vertical);
        }
    }

    private void OnScrollRectValueChanged(Vector2 position)
    {
        RefreshItemStartPosition();
#if UNITY_EDITOR
        if (m_drawActualUIItems)
            DrawContentItem();
        else
        {
            // hide all Items
            var gridItems = GridItemArray;
            for (int i = 0; i < gridItems.Length; i++)
            {
                gridItems[i].Hide();
            }
        }
#else
        DrawContentItem();
#endif
    }

    private void RefreshItemStartPosition()
    {
        UpdateAcutalContentSizeRaw();

        float minStartPosX = 0.0f, maxStartPosX = 0.0f;
        float minStartPosY = 0.0f, maxStartPosY = 0.0f;
        RectOffset padding = GridLayoutData.RectPadding;
        Vector2 actualContentWithPaddingSizeRaw = m_actualContentSizeRaw + new Vector2(padding.horizontal, padding.vertical);
        if (BoundlessGridLayoutData.Constraint.FixedColumnCount == m_gridLayoutGroup.constraint)
        {
            // content may move vertical
            // start from left to right for test
            // start from up to down for test
            minStartPosY = 0.0f;
            maxStartPosY = actualContentWithPaddingSizeRaw.y - m_viewport.rect.height;

            minStartPosX = 0.0f;
            maxStartPosX = m_viewport.rect.width - actualContentWithPaddingSizeRaw.x;
        }
        else if (BoundlessGridLayoutData.Constraint.FixedRowCount == m_gridLayoutGroup.constraint)
        {
            // content may move horizontal or...
            // start from left to right for test
            // start from up to down for test
            minStartPosY = 0.0f;
            maxStartPosY = actualContentWithPaddingSizeRaw.y - m_viewport.rect.height;

            minStartPosX = 0.0f;
            maxStartPosX = m_viewport.rect.width - actualContentWithPaddingSizeRaw.x;
        }

        Vector2 nextTopPos = new Vector2(m_dragContent.anchoredPosition.x, m_dragContent.anchoredPosition.y);
        nextTopPos.x = Mathf.Clamp(nextTopPos.x, Mathf.Min(minStartPosX, maxStartPosX), Mathf.Max(minStartPosX, maxStartPosX));
        nextTopPos.y = Mathf.Clamp(nextTopPos.y, Mathf.Min(minStartPosY, maxStartPosY), Mathf.Max(minStartPosY, maxStartPosY));
    }

    private void DrawContentItem()
    {
        // TODO @Hiko use a general calculation
        bool test = m_dragContent.anchorMin != Vector2.up || m_dragContent.anchorMax != Vector2.up || m_dragContent.pivot != Vector2.up;
        if (test)
        {
            m_dragContent.anchorMin = Vector2.up;
            m_dragContent.anchorMax = Vector2.up;
            m_dragContent.pivot = Vector2.up;
        }
        Vector3 dragContentAnchorPostion = m_dragContent.anchoredPosition;
        Vector3 contentMove = dragContentAnchorPostion - SomeUtils.GetOffsetLocalPosition(m_dragContent, SomeUtils.UIOffsetType.TopLeft);
        Vector2 itemSize = GridLayoutData.CellSize, spacing = GridLayoutData.Spacing;

        RectOffset padding = null;
        if (null != GridLayoutData)
            padding = GridLayoutData.RectPadding;

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

        Vector2Int ropLeftItemIndex = new Vector2Int(tempRowIndex, tempColumnIndex);

        int rowDataCount = 0, columnDataCount = 0;
        if (BoundlessGridLayoutData.Constraint.FixedColumnCount == m_gridLayoutGroup.constraint)
        {
            rowDataCount = m_gridLayoutGroup.constraintCount;
            columnDataCount = Mathf.CeilToInt((float)m_dataList.Count / rowDataCount);
        }
        else
        {
            columnDataCount = m_gridLayoutGroup.constraintCount;
            rowDataCount = Mathf.CeilToInt((float)m_dataList.Count / columnDataCount);
        }

        // deal with content from left to right (simple case)
        int dataIndex = 0, uiItemIndex = 0;
        Vector3 rowTopLeftPosition = new Vector3(padding.left, -padding.top, 0.0f), itemTopLeftPosition = Vector3.zero;
        var gridItems = GridItemArray;
        for (int rowIndex = 0; rowIndex < m_viewItemCountInColumn; rowIndex++)
        {
            if (rowIndex + ropLeftItemIndex.x == columnDataCount)
                break;

            rowTopLeftPosition = new Vector3(padding.left, -padding.top, 0.0f) + Vector3.down * (rowIndex + ropLeftItemIndex.x) * (itemSize.y + spacing.y);
            for (int columnIndex = 0; columnIndex < m_viewItemCountInRow; columnIndex++)
            {
                if (columnIndex + ropLeftItemIndex.y == rowDataCount)
                    break;

                itemTopLeftPosition = rowTopLeftPosition + Vector3.right * (columnIndex + ropLeftItemIndex.y) * (itemSize.x + spacing.x);

                if (BoundlessGridLayoutData.StartAxis.Horizontal == m_gridLayoutGroup.startAxis)
                    dataIndex = (rowIndex + ropLeftItemIndex.x) * rowDataCount + (columnIndex + ropLeftItemIndex.y);
                else
                    dataIndex = (rowIndex + ropLeftItemIndex.x) + columnDataCount * (columnIndex + ropLeftItemIndex.y);

                if (dataIndex > -1 && dataIndex < m_dataList.Count) 
                {
                    gridItems[uiItemIndex].ItemRectTransform.localPosition = itemTopLeftPosition;
                    gridItems[uiItemIndex].Setup(m_dataList[dataIndex]);
                    gridItems[uiItemIndex].Show();
                    uiItemIndex++;
                }
                else
                {
                    gridItems[uiItemIndex].ItemRectTransform.position = Vector2.zero;
                    gridItems[uiItemIndex].Hide();
                }
            }
        }

        while (uiItemIndex < gridItems.Length)
        {
            gridItems[uiItemIndex].Hide();
            gridItems[uiItemIndex].ItemRectTransform.position = Vector2.zero;
            uiItemIndex++;
        }

        OnContentItemFinishDrawing();
    }

    private void ClearCachedItems()
    {
        ResizeGridItemsListSize(0);
        DestroyAllChildren(m_dragContent);
    }

    private void ClampVelocityToToStop()
    {
        float sqrLimit = m_gridLayoutGroup.StopMagSqrVel;
        sqrLimit *= sqrLimit;
        float velocitySqrMag = m_scrollRect.velocity.sqrMagnitude;
        if (velocitySqrMag < sqrLimit && !Mathf.Approximately(0.0f, velocitySqrMag)) // try to clamped move to save 
            m_scrollRect.StopMovement();
    }

    private void OnLayoutFitTypeChanged(bool autoFit)
    {
        UpdateConstraintWithAutoFit();
        CalculateViewportShowCount();
        ResizeGridItemsListSize(m_viewItemCount);
        RefreshLayout();
    }

    private void OnCellSizeChanged(Vector2 cellSize)
    {
        SyncSize();
        CalculateViewportShowCount();
        ResizeGridItemsListSize(m_viewItemCount);
        RefreshLayout();
    }

    private void OnlayoutDataChanged()
    {
        OnCellSizeChanged(GridLayoutData.CellSize);
        OnLayoutFitTypeChanged(GridLayoutData.IsAutoFit);
    }

    private void DestroyAllChildren(Transform target)
    {
        if (null == target)
            return;

        int childCount = target.childCount;
        for (int i = childCount - 1; i >= 0; i--)
            Destroy(target.GetChild(i).gameObject);
    }

    private void SyncSize()
    {
        // sync the size form grid data
        Vector2 itemAcutalSize = GridLayoutData.CellSize;
        var gridItems = GridItemArray;
        for (int i = 0; i < gridItems.Length; i++)
            gridItems[i].SetItemSize(itemAcutalSize);
    }

    #region mono method

    protected override void OnEnable()
    {
        m_scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
        UpdateConstraintWithAutoFit();
        CalculateViewportShowCount();
        ResizeGridItemsListSize(m_viewItemCount);
        GridLayoutData.OnFitTypeChanged += OnLayoutFitTypeChanged;
        GridLayoutData.OnCellSizeChanged += OnCellSizeChanged;
        GridLayoutData.OnLayoutDataChanged += OnlayoutDataChanged;
        OnMonoEnable();
    }

    protected abstract void OnMonoEnable();

    protected override void OnDisable()
    {
        m_scrollRect.onValueChanged.RemoveListener(OnScrollRectValueChanged);
        GridLayoutData.OnFitTypeChanged -= OnLayoutFitTypeChanged;
        GridLayoutData.OnCellSizeChanged -= OnCellSizeChanged;
        GridLayoutData.OnLayoutDataChanged -= OnlayoutDataChanged;
        OnMonoDisable();
    }

    protected abstract void OnMonoDisable();

    private void Update()
    {
        ClampVelocityToToStop();
    }

    #endregion

}
