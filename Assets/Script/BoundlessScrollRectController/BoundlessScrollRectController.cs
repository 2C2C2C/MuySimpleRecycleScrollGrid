using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TODO clean up the member variable
// test if global scale can replace canvas scale
// T is a data for each grid item
[RequireComponent(typeof(ScrollRect))]
public abstract partial class BoundlessScrollRectController<T> : MonoBehaviour
{
    [SerializeField]
    private ScrollRect m_scrollRect = null;

    [SerializeField]
    private RectTransform m_viewport = null;
    // !!! these 2 content anchor are on top left 
    [SerializeField, Tooltip("the content that used to drag")]
    private RectTransform m_dragContent = null; // currently only support 1 type of top left pivor
    [SerializeField, Tooltip("another content hold UI elements")]
    private RectTransform m_actualContent = null;
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

    [SerializeField]
    private bool m_extendEmptySlotsToFitRow = false;

    /* a test component, we will move this component
    * and use this to setup the grid size
    */
    [Space, Header("Grid Layout Setting"), SerializeField]
    private BoundlessGridLayoutData m_gridLayoutGroup = default;

    // may need this to correctly scale the items :(
    [SerializeField]
    private Canvas m_canvas = null;

    [SerializeField]
    private BoundlessBaseScrollRectItem<T> m_gridItemPrefab = null;

    public RectTransform Viewport => m_viewport;
    public RectTransform DragContent => m_dragContent;
    public RectTransform ActualContent => m_actualContent;
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

        // to set actual content correctly?
        m_actualContent.anchorMax = m_viewport.anchorMax;
        m_actualContent.anchorMin = m_viewport.anchorMin;
        m_actualContent.pivot = m_viewport.pivot;

        m_actualContent.localPosition = m_viewport.localPosition;
        m_actualContent.anchoredPosition = m_viewport.anchoredPosition;
        m_actualContent.sizeDelta = m_viewport.sizeDelta;

        // set default simple draw stuff
        CalculateViewportShowCount();
        AdjustCachedItems();
        SyncSize();
        UpdateAcutalContentSizeRaw();

        // refresh
        RefreshItemStartPosition();
        OnScrollRectValueChanged(Vector2.zero);
    }

    protected abstract void ResizeGridItemsListSize(int size);
    protected virtual void BeforedCachedItemRefreshed() { }
    protected virtual void OnCachedItemRefreshed() { }
    protected virtual void OnContentItemFinishDrawing() { }

    public void UpdateConstraintWithAutoFit()
    {
        if (GridLayoutData.IsAutoFit)
        {
            int constraintCount = 0;
            float viewportHeight = 0.0f, viewportWidth = 0.0f;
            Vector2 spacing = GridLayoutData.Spacing;
            viewportHeight = m_viewport.rect.height;
            viewportWidth = m_viewport.rect.width;
            Vector3 globalScale = m_viewport.lossyScale;
            Vector2 itemSize = new Vector2(GridLayoutData.CellSize.x * globalScale.x, GridLayoutData.CellSize.y * globalScale.y);

            if (BoundlessGridLayoutData.Constraint.FixedColumnCount == GridLayoutData.constraint)
            {
                constraintCount = Mathf.FloorToInt(viewportWidth / (itemSize.x + spacing.x));
            }
            else
            {
                constraintCount = Mathf.FloorToInt(viewportHeight / (itemSize.y + spacing.y));
            }
            constraintCount = Mathf.Clamp(constraintCount, 1, int.MaxValue);
            GridLayoutData.constraintCount = constraintCount;
        }
    }

    protected void CalculateViewportShowCount()
    {
        m_viewItemCountInRow = 0;
        m_viewItemCountInColumn = 0;
        Vector3 globalScale = m_viewport.lossyScale;
        Vector2 itemSize = new Vector2(m_gridLayoutGroup.CellSize.x * globalScale.x, m_gridLayoutGroup.CellSize.y * globalScale.y);

        Vector2 spacing = m_gridLayoutGroup.Spacing;
        float viewportHeight = Mathf.Abs(m_viewport.rect.height * m_viewport.localScale.y);
        float viewportWidth = Mathf.Abs(m_viewport.rect.width * m_viewport.localScale.y);
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
        Vector3 dragContentPostion = m_dragContent.position;
        Vector3 dragAnchorContentPostion = m_dragContent.anchoredPosition;
        m_actualContent.anchoredPosition = Vector2.zero;
        Vector3 globalScale = m_viewport.lossyScale;

        Vector2 itemSize = new Vector2(GridLayoutData.CellSize.x * globalScale.x, GridLayoutData.CellSize.y * globalScale.y);
        Vector2 spacing = new Vector2(GridLayoutData.Spacing.x * globalScale.x, GridLayoutData.Spacing.y * globalScale.y);

        // TODO use offset as padding correctly
        RectOffset padding = null;
        if (null != GridLayoutData)
            padding = GridLayoutData.RectPadding;

        // TODO need to know the moving direction, then adjust it to prevent wrong draw
        float xMove = dragAnchorContentPostion.x < 0 ? (-dragAnchorContentPostion.x - padding.horizontal) * globalScale.x : 0;
        xMove = Mathf.Clamp(xMove, 0.0f, Mathf.Abs(xMove));
        float yMove = dragAnchorContentPostion.y > 0 ? (dragAnchorContentPostion.y - padding.vertical) * globalScale.y : 0;
        yMove = Mathf.Clamp(yMove, 0.0f, Mathf.Abs(yMove));

        int tempColumnIndex = Mathf.FloorToInt((xMove + spacing.x) / (itemSize.x + spacing.x));
        if (xMove % (itemSize.x + spacing.x) - itemSize.x > spacing.x)
            tempColumnIndex = Mathf.Clamp(tempColumnIndex - 1, 0, tempColumnIndex);

        int tempRowIndex = Mathf.FloorToInt((yMove + spacing.y) / (itemSize.y + spacing.y));
        if (yMove % (itemSize.y + spacing.y) - itemSize.y > spacing.y)
            tempRowIndex = Mathf.Clamp(tempRowIndex - 1, 0, tempRowIndex);

        // TODO fix temp calculate (now it isfrom top left)
        Vector3 tempMove = new Vector3(tempColumnIndex * (itemSize.x + spacing.x), -tempRowIndex * (itemSize.y + spacing.y), 0.0f);
        Rect contentRect = default;
        Vector2 contentRectSize = (m_actualContentSizeRaw + new Vector2(padding.horizontal, padding.vertical));
        contentRectSize.x *= globalScale.x;
        contentRectSize.y *= globalScale.y;
        contentRect = new Rect(m_dragContent.position, contentRectSize);

        // used for showing slots
        Rect actualContentExpandRect = default;
        if (m_viewItemCount > m_dataList.Count)
        {
            Vector2 viewportSize = m_viewport.rect.size;
            Vector2 actualContentRectSize = (m_actualContentSizeRaw + new Vector2(padding.horizontal, padding.vertical));
            if (m_viewItemCount > m_dataList.Count)
            {
                actualContentRectSize = (viewportSize + new Vector2(padding.horizontal, padding.vertical));
            }
            actualContentExpandRect = new Rect(m_dragContent.position, actualContentRectSize);
        }
        else
            actualContentExpandRect = new Rect(m_dragContent.position, m_dragContent.rect.size);

        // to calculate it somewhere else :)
        int rowDataCount = 0, columnDataCount = 0;
        if (BoundlessGridLayoutData.Constraint.FixedColumnCount == m_gridLayoutGroup.constraint)
        {
            rowDataCount = m_gridLayoutGroup.constraintCount;
            columnDataCount = (int)Mathf.CeilToInt((float)m_dataList.Count / rowDataCount);
        }
        else
        {
            columnDataCount = m_gridLayoutGroup.constraintCount;
            rowDataCount = (int)Mathf.CeilToInt((float)m_dataList.Count / columnDataCount);
        }

        // deal with content from left to right (simple case) first
        int rowFirstDataIndex = 0, dataIndex = 0;
        int uiItemIndex = 0;
        Vector3 rowTopLeftPosition = default, itemTopLeftPosition = default;
        rowTopLeftPosition = dragContentPostion + tempMove;
        rowTopLeftPosition += new Vector3(padding.left * globalScale.x, -padding.top * globalScale.y, 0.0f);
        Rect currentGridRect = new Rect(rowTopLeftPosition, itemSize);

        // draw from left to right for test
        int testDelta = m_viewItemCountInRow - (m_dataList.Count % m_viewItemCountInRow);
        var gridItems = GridItemArray;
        for (int rowIndex = 0; rowIndex < m_viewItemCountInColumn; rowIndex++)
        {
            itemTopLeftPosition = rowTopLeftPosition;
            if (BoundlessGridLayoutData.StartAxis.Horizontal == m_gridLayoutGroup.startAxis)
                rowFirstDataIndex = (tempRowIndex + rowIndex) * rowDataCount;
            else
                rowFirstDataIndex = tempRowIndex + rowIndex + tempColumnIndex * columnDataCount;

            for (int columnIndex = 0; columnIndex < m_viewItemCountInRow; columnIndex++)
            {
                currentGridRect.position = itemTopLeftPosition;

                if (BoundlessGridLayoutData.StartAxis.Horizontal == m_gridLayoutGroup.startAxis)
                    dataIndex = rowFirstDataIndex + tempColumnIndex + columnIndex;
                else
                    dataIndex = rowFirstDataIndex + columnIndex * columnDataCount;

                if (contentRect.Contains(currentGridRect))
                {
                    bool isValid = dataIndex > -1 && dataIndex < m_dataList.Count;
                    isValid = isValid && ((dataIndex / m_viewItemCountInRow) == (tempRowIndex + rowIndex));
                    if (isValid)
                    {
                        gridItems[uiItemIndex].ItemRectTransform.position = itemTopLeftPosition;
                        gridItems[uiItemIndex].Setup(m_dataList[dataIndex]);
                        gridItems[uiItemIndex].Show();
                        uiItemIndex++;
                    }
                    else if (m_extendEmptySlotsToFitRow)
                    {
                        gridItems[uiItemIndex].ItemRectTransform.position = itemTopLeftPosition;
                        gridItems[uiItemIndex].SetEmpty();
                        gridItems[uiItemIndex].Show();
                        uiItemIndex++;
                    }
                    else
                    {
                        gridItems[uiItemIndex].ItemRectTransform.position = Vector2.zero;
                        gridItems[uiItemIndex].Hide();
                    }
                }
                else
                {
                    gridItems[uiItemIndex].ItemRectTransform.position = Vector2.zero;
                    gridItems[uiItemIndex].Hide();
                }

                itemTopLeftPosition.x += spacing.x + itemSize.x;
            }
            rowTopLeftPosition.y -= spacing.y + itemSize.y;
        }

        while (uiItemIndex < gridItems.Length)
        {
            gridItems[uiItemIndex].Hide();
            gridItems[uiItemIndex].ItemRectTransform.anchoredPosition = Vector2.zero;
            uiItemIndex++;
        }

        OnContentItemFinishDrawing();
    }

    private void ClearCachedItems()
    {
        ResizeGridItemsListSize(0);
        DestroyAllChildren(m_actualContent);
    }

    private void ClampVelocityToToStop()
    {
        float sqrLimit = m_gridLayoutGroup.StopMagSqrVel;
        sqrLimit *= sqrLimit;
        float velocitySqrMag = m_scrollRect.velocity.sqrMagnitude;
        // if (!Mathf.Approximately(0.0f, velocitySqrMag))
        //     Debug.Log($"test vel {m_scrollRect.velocity}, test sqr mag {velocitySqrMag}");
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
        // the actual item size will also directly affected by parent canvas's scale factor, so we may not need to multiple it :D
        Vector2 itemAcutalSize = GridLayoutData.CellSize;
        var gridItems = GridItemArray;
        for (int i = 0; i < gridItems.Length; i++)
            gridItems[i].SetItemSize(itemAcutalSize);
    }

    #region mono method

    private void OnEnable()
    {
        m_canvas = GetComponentInParent<Canvas>();
        m_scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
        UpdateConstraintWithAutoFit();
        CalculateViewportShowCount();
        ResizeGridItemsListSize(m_viewItemCount);
        GridLayoutData.OnFitTypeChanged += OnLayoutFitTypeChanged;
        GridLayoutData.OnCellSizeChanged += OnCellSizeChanged;
        GridLayoutData.OnLayoutDataChanged += OnlayoutDataChanged;
    }

    private void OnDisable()
    {
        m_scrollRect.onValueChanged.RemoveListener(OnScrollRectValueChanged);
        GridLayoutData.OnFitTypeChanged -= OnLayoutFitTypeChanged;
        GridLayoutData.OnCellSizeChanged -= OnCellSizeChanged;
        GridLayoutData.OnLayoutDataChanged -= OnlayoutDataChanged;
    }

    private void Update()
    {
        ClampVelocityToToStop();
    }

    #endregion

}
