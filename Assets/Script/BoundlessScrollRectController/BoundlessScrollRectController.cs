using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Only support the situation that item start from top left
/// TODO @Hiko
/// how to give those item to the other controller to let them setup stuff
/// remove some nolonger used methods
/// fix editor issues
/// </summary>
/// <typeparam name="T">T is a data for each grid item</typeparam>
[RequireComponent(typeof(ScrollRect))]
public partial class BoundlessScrollRectController : UIBehaviour
{
    [SerializeField]
    private ScrollRect m_scrollRect = null;

    [SerializeField]
    private RectTransform m_viewport = null;

    /// <summary>
    /// anchor should be top left 
    /// </summary>
    [SerializeField, Tooltip("the content that used to drag")]
    private RectTransform m_dragContent = null; // currently only support 1 type of top left pivor

    // <summary>
    // the actual item count may show in the viewport
    // </summary>
    private int m_viewItemCount = 0;

    private int m_viewItemCountInRow = 0;
    private int m_viewItemCountInColumn = 0;

    /// <summary>
    /// including spacing
    /// </summary>
    private Vector2 m_actualContentSizeRaw = default;

    // TODO @Hiko fix the value serialized issues
    [Space, Header("Grid Layout Setting"), SerializeField]
    private BoundlessGridLayoutData m_gridLayoutGroup = new BoundlessGridLayoutData();

    IElementBuilder m_modelContainer;

    private bool m_hasLayoutChanged = false;

    public BoundlessGridLayoutData GridLayoutData => m_gridLayoutGroup;

    public RectTransform Content => m_dragContent;

    public event Action OnContentItemFinishDrawing;

    public event Action BeforedItemArrayResized;
    public event Action OnItemArrayResized;

    public void Setup(IElementBuilder itemBuilder)
    {
        m_modelContainer = itemBuilder;
        int currentShowCount = CalculateCurrentViewportShowCount();
        if (currentShowCount != m_viewItemCount)
        {
            m_viewItemCount = currentShowCount;
            AdjustCachedItems();
        }
        SyncSize();
        UpdateAcutalContentSizeRaw();
        // refresh
        OnScrollRectValueChanged(Vector2.zero);
    }

    public void UpdateConstraintWithAutoFit()
    {
        if (m_gridLayoutGroup.IsAutoFit)
        {
            int constraintCount = 0;
            float viewportHeight = 0.0f, viewportWidth = 0.0f;
            Vector2 spacing = m_gridLayoutGroup.Spacing;
            viewportHeight = m_viewport.rect.height;
            viewportWidth = m_viewport.rect.width;
            Vector2 itemSize = new Vector2(m_gridLayoutGroup.CellSize.x, m_gridLayoutGroup.CellSize.y);

            if (BoundlessGridLayoutData.Constraint.FixedColumnCount == m_gridLayoutGroup.constraint)
                constraintCount = Mathf.FloorToInt(viewportWidth / (itemSize.x + spacing.x));
            else
                constraintCount = Mathf.FloorToInt(viewportHeight / (itemSize.y + spacing.y));

            constraintCount = Mathf.Clamp(constraintCount, 1, int.MaxValue);
            m_gridLayoutGroup.constraintCount = constraintCount;
        }
    }

    public void RefreshLayoutChanges()
    {
        UpdateConstraintWithAutoFit();
        UpdateAcutalContentSizeRaw();
        AdjustCachedItems();
        SyncSize();
        OnScrollRectValueChanged(Vector2.zero);
    }

    private void NotifyOnContentItemFinishDrawing() { OnContentItemFinishDrawing?.Invoke(); }

    private int CalculateCurrentViewportShowCount()
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

        if (BoundlessGridLayoutData.Constraint.FixedColumnCount == m_gridLayoutGroup.constraint)
            m_viewItemCountInRow = Mathf.Clamp(m_viewItemCountInRow, 1, m_gridLayoutGroup.constraintCount);
        else
            m_viewItemCountInColumn = Mathf.Clamp(m_viewItemCountInColumn, 1, m_gridLayoutGroup.constraintCount);

        int result = m_viewItemCountInRow * m_viewItemCountInColumn;
        return result;
    }

    private void AdjustCachedItems()
    {
        BeforedItemArrayResized?.Invoke();
        m_viewItemCount = CalculateCurrentViewportShowCount();
        m_modelContainer.ResizeArray(m_viewItemCount);
        SyncSize();
        OnItemArrayResized?.Invoke();
    }

    private void UpdateAcutalContentSizeRaw()
    {
        int dataCount = m_modelContainer.DataCount;
        RectOffset m_padding = m_gridLayoutGroup.RectPadding;
        Vector2 itemSize = m_gridLayoutGroup.CellSize;
        Vector2 spacing = m_gridLayoutGroup.Spacing;
        Vector2 result = default;

        // too bad
        Vector2 viewportSize = m_viewport.rect.size;
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
        if (null != m_gridLayoutGroup)
        {
            RectOffset padding = m_gridLayoutGroup.RectPadding;
            m_dragContent.sizeDelta += new Vector2(padding.horizontal, padding.vertical);
        }
    }

    private void OnScrollRectValueChanged(Vector2 position)
    {
#if UNITY_EDITOR
        if (m_drawActualUIItems)
            DrawContentItem();
        else
        {
            // hide all Items
            var gridItems = m_modelContainer.ItemRectTransformArray;
            for (int i = 0; i < gridItems.Count; i++)
            {
                // TODO @Hiko
                // put it into some where else so we can hid it?
                m_modelContainer.HideItem(i);
            }
        }
#else
        DrawContentItem();
#endif
    }

    private void DrawContentItem()
    {
        int dataCount = m_modelContainer.DataCount;
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
        Vector2 itemSize = m_gridLayoutGroup.CellSize, spacing = m_gridLayoutGroup.Spacing;

        RectOffset padding = null;
        if (null != m_gridLayoutGroup)
            padding = m_gridLayoutGroup.RectPadding;

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
            columnDataCount = Mathf.CeilToInt((float)dataCount / rowDataCount);
        }
        else
        {
            columnDataCount = m_gridLayoutGroup.constraintCount;
            rowDataCount = Mathf.CeilToInt((float)dataCount / columnDataCount);
        }

        // deal with content from left to right (simple case)
        int dataIndex = 0, uiItemIndex = 0;
        Vector3 rowTopLeftPosition = new Vector3(padding.left, -padding.top, 0.0f), itemTopLeftPosition = Vector3.zero;
        var rectTransformArray = m_modelContainer.ItemRectTransformArray;
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

                if (dataIndex > -1 && dataIndex < dataCount)
                {
                    rectTransformArray[uiItemIndex].localPosition = itemTopLeftPosition;
                    m_modelContainer.SetupItem(uiItemIndex, dataIndex);
                    uiItemIndex++;
                }
                else
                {
                    m_modelContainer.HideItem(uiItemIndex);
                    rectTransformArray[uiItemIndex].position = Vector3.zero;
                }
            }
        }

        while (uiItemIndex < rectTransformArray.Count)
        {
            m_modelContainer.HideItem(uiItemIndex);
            rectTransformArray[uiItemIndex].position = Vector3.zero;
            uiItemIndex++;
        }

        NotifyOnContentItemFinishDrawing();
    }

    private void ClampVelocityToToStop()
    {
        float sqrLimit = m_gridLayoutGroup.StopMagSqrVel;
        sqrLimit *= sqrLimit;
        float velocitySqrMag = m_scrollRect.velocity.sqrMagnitude;
        if (velocitySqrMag < sqrLimit && !Mathf.Approximately(0.0f, velocitySqrMag)) // try to clamped move to save 
            m_scrollRect.StopMovement();
    }

    // optimize those 3 methods

    private void OnLayoutFitTypeChanged(bool autoFit)
    {
        m_hasLayoutChanged = true;
    }

    private void OnCellSizeChanged(Vector2 cellSize)
    {
        m_hasLayoutChanged = true;
    }

    private void OnlayoutDataChanged()
    {
        m_hasLayoutChanged = true;
    }

    // optimize those 3 methods

    private void SyncSize()
    {
        // sync the size form grid data
        Vector2 itemAcutalSize = GridLayoutData.CellSize;
        var rectTransformArray = m_modelContainer.ItemRectTransformArray;
        for (int i = 0; i < rectTransformArray.Count; i++)
            rectTransformArray[i].sizeDelta = itemAcutalSize;
    }

    #region mono method

    protected override void OnEnable()
    {
        UpdateConstraintWithAutoFit();
        m_scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
        m_gridLayoutGroup.OnFitTypeChanged += OnLayoutFitTypeChanged;
        m_gridLayoutGroup.OnCellSizeChanged += OnCellSizeChanged;
        m_gridLayoutGroup.OnLayoutDataChanged += OnlayoutDataChanged;
    }

    protected override void OnDisable()
    {
        m_scrollRect.onValueChanged.RemoveListener(OnScrollRectValueChanged);
        m_gridLayoutGroup.OnFitTypeChanged -= OnLayoutFitTypeChanged;
        m_gridLayoutGroup.OnCellSizeChanged -= OnCellSizeChanged;
        m_gridLayoutGroup.OnLayoutDataChanged -= OnlayoutDataChanged;
    }

    private void Update()
    {
        if (m_hasLayoutChanged)
        {
            RefreshLayoutChanges();
            m_hasLayoutChanged = false;
        }
        ClampVelocityToToStop();
    }

    #endregion
}
