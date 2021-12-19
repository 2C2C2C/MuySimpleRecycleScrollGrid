using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Only support the situation that item start from top left
/// TODO @Hiko
/// how to give those item to the other controller to let them setup stuff
/// remove some nolonger used methods
/// did some editor stuff (maybe I can directly use grid layout)
/// </summary>
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
    private RectTransform m_content = null; // currently only support 1 type of top left pivor

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
    private BoundlessGridLayoutData m_gridLayoutGroup = new BoundlessGridLayoutData();

    private IListElementUI[] m_elementArray = new IListElementUI[0];
    private IListViewUI m_listView;

    private bool m_drawActualUIItems = true;

    public BoundlessGridLayoutData GridLayoutData => m_gridLayoutGroup;

    public RectTransform Content => m_content;

    private int CurrentCount => m_listView == null ? 0 : m_listView.Length;

    public event Action OnContentItemFinishDrawing;
    public event Action BeforedItemArrayResized;
    public event Action OnItemArrayResized;

    public void Setup(IListViewUI listView)
    {
        m_listView = listView;
        AdjustCachedItems();
        ApplySizeOnElements();
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
        ApplySizeOnElements();
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
        AdjustElementArray(m_viewItemCount);
        ApplySizeOnElements();
        OnItemArrayResized?.Invoke();
    }

    private void UpdateAcutalContentSizeRaw()
    {
        int dataCount = CurrentCount;
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
        m_content.sizeDelta = m_actualContentSizeRaw;
        if (null != m_gridLayoutGroup)
        {
            RectOffset padding = m_gridLayoutGroup.RectPadding;
            m_content.sizeDelta += new Vector2(padding.horizontal, padding.vertical);
        }
    }

    private void OnScrollRectValueChanged(Vector2 position)
    {
#if UNITY_EDITOR
        if (m_drawActualUIItems)
            DrawContentItem();
        else if (m_elementArray != null)
        {
            // hide all Items
            for (int i = 0; i < m_elementArray.Length; i++)
            {
                m_elementArray[i].Hide();
            }
        }
#else
        DrawContentItem();
#endif
    }

    private void DrawContentItem()
    {
        int dataCount = CurrentCount;
        // TODO @Hiko use a general calculation
        bool test = m_content.anchorMin != Vector2.up || m_content.anchorMax != Vector2.up || m_content.pivot != Vector2.up;
        if (test)
        {
            m_content.anchorMin = Vector2.up;
            m_content.anchorMax = Vector2.up;
            m_content.pivot = Vector2.up;
        }
        Vector3 dragContentAnchorPostion = m_content.anchoredPosition;
        Vector3 contentMove = dragContentAnchorPostion - SomeUtils.GetOffsetLocalPosition(m_content, SomeUtils.UIOffsetType.TopLeft);
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

        Vector2Int rowTopLeftItemIndex = new Vector2Int(tempRowIndex, tempColumnIndex);

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

        // x -> element amount on horizontal axis
        // y -> element amount on vertical axis
        Vector2Int contentRowColumnSize = new Vector2Int(rowDataCount, columnDataCount);

        // deal with content from left to right (simple case)
        int dataIndex = 0, uiItemIndex = 0;
        Vector3 rowTopLeftPosition = new Vector3(padding.left, -padding.top, 0.0f), itemTopLeftPosition = Vector3.zero;
        for (int columnIndex = 0; columnIndex < m_viewItemCountInColumn; columnIndex++)
        {
            if (columnIndex + rowTopLeftItemIndex.x == columnDataCount)
                break;

            rowTopLeftPosition = new Vector3(padding.left, -padding.top, 0.0f) + Vector3.down * (columnIndex + rowTopLeftItemIndex.x) * (itemSize.y + spacing.y);
            for (int rowIndex = 0; rowIndex < m_viewItemCountInRow; rowIndex++)
            {
                if (rowIndex + rowTopLeftItemIndex.y == rowDataCount)
                    break;

                Vector2Int elementIndex = new Vector2Int(rowIndex + rowTopLeftItemIndex.y, columnIndex + rowTopLeftItemIndex.x);
                dataIndex = CaculateDataIndex(elementIndex, contentRowColumnSize, GridLayoutData.startAxis, GridLayoutData.startCorner);
                itemTopLeftPosition = rowTopLeftPosition + Vector3.right * (rowIndex + rowTopLeftItemIndex.y) * (itemSize.x + spacing.x);
                if (dataIndex > -1 && dataIndex < dataCount)
                {
                    m_elementArray[uiItemIndex].ElementRectTransform.localPosition = itemTopLeftPosition;
                    m_elementArray[uiItemIndex].SetIndex(dataIndex);
                    uiItemIndex++;
                }
                else
                {
                    m_elementArray[uiItemIndex].SetIndex(-1);
                    m_elementArray[uiItemIndex].Hide();
                    m_elementArray[uiItemIndex].ElementRectTransform.position = Vector3.zero;
                }
            }
        }

        while (uiItemIndex < m_elementArray.Length)
        {
            m_elementArray[uiItemIndex].Hide();
            m_elementArray[uiItemIndex].ElementRectTransform.position = Vector3.zero;
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
        float sqrLimit = m_gridLayoutGroup.StopMagSqrVel;
        sqrLimit *= sqrLimit;
        float velocitySqrMag = m_scrollRect.velocity.sqrMagnitude;
        if (velocitySqrMag < sqrLimit && !Mathf.Approximately(0.0f, velocitySqrMag)) // try to clamped move to save 
            m_scrollRect.StopMovement();
    }

    private void AdjustElementArray(int size)
    {
        if (m_listView == null) return;
        int index = 0, currentSize = m_elementArray.Length;
        if (size > currentSize)
        {
            // directly add
            Array.Resize<IListElementUI>(ref m_elementArray, size);
            index = currentSize;
            while (index < size)
                m_elementArray[index++] = m_listView.Add();
        }
        else if (size < currentSize)
        {
            index = currentSize - 1;
            while (index >= size)
            {
                // TODO @Hiko
                m_listView.Remove(index);
                m_elementArray[index--] = null;
                index--;
            }
            Array.Resize<IListElementUI>(ref m_elementArray, size);
        }
    }

    private void ApplySizeOnElements()
    {
        // sync the size form grid data
        Vector2 itemAcutalSize = GridLayoutData.CellSize;
        for (int i = 0; i < m_elementArray.Length; i++)
            m_elementArray[i].ElementRectTransform.sizeDelta = itemAcutalSize;
    }

    #region mono method

    protected override void OnEnable()
    {
        UpdateConstraintWithAutoFit();
        m_scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
    }

    protected override void OnDisable()
    {
        m_scrollRect.onValueChanged.RemoveListener(OnScrollRectValueChanged);
    }

    private void Update()
    {
        ClampVelocityToToStop();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        // something may got changed on inspector
        // try refresh all?
        RefreshLayoutChanges();
    }
#endif

    #endregion
}
