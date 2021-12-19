#if UNITY_EDITOR
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public partial class BoundlessScrollRectController : UIBehaviour
{

    [Space, Header("Debug draw style B")]
    public bool m_drawContentSizeB = true;
    public bool m_drawGridsB = true;
    public bool m_drawShowingGridsB = true;

    void DebugDrawStyleB()
    {
        if (m_drawContentSizeB) DrawDebugContentSizeNew();

        if (m_drawGridsB) DebugDrawCotentGridNew();

        if (m_drawShowingGridsB) DrawDebugShowingGridsNew();
    }

    private void DrawDebugContentSizeNew()
    {
        if (0 == CurrentCount)
            return;

        Vector2 rawSize = Vector2.zero;
        RectOffset padding = m_gridLayoutGroup.RectPadding;
        Vector2 paddingValueRaw = new Vector2(padding.horizontal, padding.vertical);
        Vector2 elementSize = m_gridLayoutGroup.CellSize;
        Vector2 spacing = m_gridLayoutGroup.Spacing;

        int rowCount = 0;
        int columnCount = 0;
        if (m_gridLayoutGroup.constraint == BoundlessGridLayoutData.Constraint.FixedColumnCount)
        {
            columnCount = m_gridLayoutGroup.constraintCount;
            rowCount = Mathf.CeilToInt(CurrentCount / (float)columnCount);
        }
        else if (m_gridLayoutGroup.constraint == BoundlessGridLayoutData.Constraint.FixedRowCount)
        {
            rowCount = m_gridLayoutGroup.constraintCount;
            columnCount = Mathf.CeilToInt(CurrentCount / (float)rowCount);
        }

        rawSize = new Vector2(
            columnCount * elementSize.x + (columnCount - 1) * spacing.x,
            rowCount * elementSize.y + (rowCount - 1) * spacing.y);

        switch (m_gridLayoutGroup.startCorner)
        {
            case GridLayoutGroup.Corner.UpperLeft:
                break;
            case GridLayoutGroup.Corner.UpperRight:
                break;
            case GridLayoutGroup.Corner.LowerLeft:
                break;
            case GridLayoutGroup.Corner.LowerRight:
                break;
            default:
                break;
        }

        Vector3 topLeftPoint = new Vector3(paddingValueRaw.x, -paddingValueRaw.y);
        Vector3 topRightPoint = topLeftPoint;
        topRightPoint.x += rawSize.x;

        Vector3 bottomLeftPoint = topLeftPoint;
        Vector3 BottomRightPoint = topRightPoint;
        bottomLeftPoint.y -= rawSize.y;
        BottomRightPoint.y -= rawSize.y;

        Matrix4x4 localToWorld = m_content.localToWorldMatrix;
        topLeftPoint = localToWorld.MultiplyPoint(topLeftPoint);
        topRightPoint = localToWorld.MultiplyPoint(topRightPoint);
        bottomLeftPoint = localToWorld.MultiplyPoint(bottomLeftPoint);
        BottomRightPoint = localToWorld.MultiplyPoint(BottomRightPoint);

        Debug.DrawLine(topLeftPoint, topRightPoint, Color.magenta);
        Debug.DrawLine(topLeftPoint, bottomLeftPoint, Color.magenta);
        Debug.DrawLine(topRightPoint, BottomRightPoint, Color.magenta);
        Debug.DrawLine(bottomLeftPoint, BottomRightPoint, Color.magenta);
    }

    private void DebugDrawCotentGridNew()
    {
        int dataCount = CurrentCount;
        if (0 == dataCount) return;

        Vector3 rowItemTopLeftPos = default;
        Vector3 columnStartItemTopLeftPos = Vector3.zero;

        // TODO use offset as padding correctly
        if (null != m_gridLayoutGroup)
        {
            RectOffset padding = m_gridLayoutGroup.RectPadding;
            columnStartItemTopLeftPos += new Vector3(padding.left, -padding.top, 0.0f);
        }
        Vector2 spacing = new Vector2(m_gridLayoutGroup.Spacing.x, m_gridLayoutGroup.Spacing.y);
        Vector2 itemSize = new Vector2(m_gridLayoutGroup.CellSize.x, m_gridLayoutGroup.CellSize.y);

        // should know which axis get constrained
        Matrix4x4 localToWorld = m_content.localToWorldMatrix;
        int constraintCount = m_gridLayoutGroup.constraintCount;
        int dynamicCount = (dataCount % constraintCount > 0) ? (dataCount / constraintCount) + 1 : (dataCount / constraintCount);
        if (BoundlessGridLayoutData.Constraint.FixedColumnCount == m_gridLayoutGroup.constraint)
        {
            for (int i = 0; i < dynamicCount; i++)
            {
                rowItemTopLeftPos = columnStartItemTopLeftPos;
                for (int j = 0; j < constraintCount; j++)
                {
                    DrawOneDebugGridItem(rowItemTopLeftPos, itemSize, localToWorld, Color.blue);
                    rowItemTopLeftPos.x += spacing.x + itemSize.x;
                }
                columnStartItemTopLeftPos.y -= itemSize.y + spacing.y;
            }
        }
        else // if (BoundlessGridLayoutData.Constraint.FixedRowCount == m_gridLayoutGroup.constraint)
        {
            for (int i = 0; i < constraintCount; i++)
            {
                rowItemTopLeftPos = columnStartItemTopLeftPos;
                for (int j = 0; j < dynamicCount; j++)
                {
                    DrawOneDebugGridItem(rowItemTopLeftPos, itemSize, localToWorld, Color.blue);
                    rowItemTopLeftPos.x += spacing.x + itemSize.x;
                }
                columnStartItemTopLeftPos.y -= itemSize.y + spacing.y;
            }
        }
    }

    private void DrawDebugShowingGridsNew()
    {
        int dataCount = CurrentCount;
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

        Vector2Int ropLeftElementIndex = new Vector2Int(tempRowIndex, tempColumnIndex);

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
        Matrix4x4 localToWorldMatrix = m_content.localToWorldMatrix;
        int dataIndex = 0;
        Vector3 rowTopLeftPosition = new Vector3(padding.left, -padding.top, 0.0f), itemTopLeftPosition = Vector3.zero;
        for (int rowIndex = 0; rowIndex < m_viewItemCountInColumn; rowIndex++)
        {
            if (rowIndex + ropLeftElementIndex.x == columnDataCount)
                break;

            rowTopLeftPosition = new Vector3(padding.left, -padding.top, 0.0f) + Vector3.down * (rowIndex + ropLeftElementIndex.x) * (itemSize.y + spacing.y);
            for (int columnIndex = 0; columnIndex < m_viewItemCountInRow; columnIndex++)
            {
                if (columnIndex + ropLeftElementIndex.y == rowDataCount)
                    break;

                // TODO @Hiko clean up here
                Vector2Int elementIndex = new Vector2Int(columnIndex + ropLeftElementIndex.y, rowIndex + ropLeftElementIndex.x);
                dataIndex = CaculateDataIndex(elementIndex, contentRowColumnSize, GridLayoutData.startAxis, GridLayoutData.startCorner);

                itemTopLeftPosition = rowTopLeftPosition + Vector3.right * (columnIndex + ropLeftElementIndex.y) * (itemSize.x + spacing.x);
                if (dataIndex > -1 && dataIndex < dataCount)
                {
                    // the item can show
                    DrawOneDebugGridItem(itemTopLeftPosition, itemSize, localToWorldMatrix, Color.white);
                }
                else
                {
                    // the item wont shows up
                    DrawOneDebugGridItem(itemTopLeftPosition, itemSize, localToWorldMatrix, Color.yellow);
                }
            }
        }
    }

    private int CaculateDataIndex(Vector2Int rowColumnIndex, Vector2Int rowColumnSize, GridLayoutGroup.Axis startAxis, GridLayoutGroup.Corner startCorner)
    {
        // for row column size
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

}

#endif