#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract partial class BoundlessScrollRectController<T> : UIBehaviour
{
    [Space, Header("Debug settings")]
    public bool m_drawContentSize = true;
    public bool m_drawGrids = true;
    public bool m_drawShowingGrids = true;
    public bool m_drawActualUIItems = true;

#if UNITY_EDITOR
    protected override void Reset()
    {
        m_scrollRect.GetComponent<ScrollRect>();
        m_scrollRect.StopMovement();
        m_dragContent = m_scrollRect.content;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        if (m_drawContentSize)
            DrawDebugContentSize();

        if (m_drawGrids)
            DrawDebugGrids();

        if (m_drawShowingGrids)
            DrawDebugShowingGrids();
    }
#endif

    // TODO @Hiko remove global scale

    private void DrawDebugContentSize()
    {
        if (null == m_dataList)
            return;

        RectOffset padding = GridLayoutData.RectPadding;
        Vector2 paddingValueRaw = new Vector2(padding.horizontal, padding.vertical);
        Vector2 actualContentSize = m_actualContentSizeRaw;

        Vector3 topLeftPoint = new Vector3(paddingValueRaw.x, -paddingValueRaw.y);
        Vector3 topRightPoint = topLeftPoint;
        topRightPoint.x += actualContentSize.x;

        Vector3 bottomLeftPoint = topLeftPoint;
        Vector3 BottomRightPoint = topRightPoint;
        bottomLeftPoint.y -= actualContentSize.y;
        BottomRightPoint.y -= actualContentSize.y;

        Matrix4x4 localToWorld = m_dragContent.localToWorldMatrix;
        topLeftPoint = localToWorld.MultiplyPoint(topLeftPoint);
        topRightPoint = localToWorld.MultiplyPoint(topRightPoint);
        bottomLeftPoint = localToWorld.MultiplyPoint(bottomLeftPoint);
        BottomRightPoint = localToWorld.MultiplyPoint(BottomRightPoint);

        Debug.DrawLine(topLeftPoint, topRightPoint, Color.magenta);
        Debug.DrawLine(topLeftPoint, bottomLeftPoint, Color.magenta);
        Debug.DrawLine(topRightPoint, BottomRightPoint, Color.magenta);
        Debug.DrawLine(bottomLeftPoint, BottomRightPoint, Color.magenta);
    }

    private void DrawDebugGrids()
    {
        if (null == m_dataList)
            return;

        int dataCount = m_dataList.Count;
        Vector3 rowItemTopLeftPos = default;
        Vector3 columnStartItemTopLeftPos = Vector3.zero;

        // TODO use offset as padding correctly
        if (null != GridLayoutData)
        {
            RectOffset padding = GridLayoutData.RectPadding;
            columnStartItemTopLeftPos += new Vector3(padding.left, -padding.top, 0.0f);
        }
        Vector2 spacing = new Vector2(m_gridLayoutGroup.Spacing.x, m_gridLayoutGroup.Spacing.y);
        Vector2 itemSize = new Vector2(m_gridLayoutGroup.CellSize.x, m_gridLayoutGroup.CellSize.y);

        // should know which axis get constrained
        Matrix4x4 localToWorld = m_dragContent.localToWorldMatrix;
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

    private void DrawDebugShowingGrids()
    {
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
        Matrix4x4 localToWorldMatrix = m_dragContent.localToWorldMatrix;
        int dataIndex = 0;
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

    private void DrawOneDebugGridItem(Vector3 topLeftPoint, Vector3 itemSize, Matrix4x4 additionalMatrix, Color color)
    {
        Vector3 topRightPoint = topLeftPoint;
        topRightPoint.x += itemSize.x;

        Vector3 bottomLeftPoint = topLeftPoint;
        bottomLeftPoint.y -= itemSize.y;

        Vector3 bottomRightPoint = topRightPoint;
        bottomRightPoint.y -= itemSize.y;

        topLeftPoint = additionalMatrix.MultiplyPoint(topLeftPoint);
        topRightPoint = additionalMatrix.MultiplyPoint(topRightPoint);
        bottomLeftPoint = additionalMatrix.MultiplyPoint(bottomLeftPoint);
        bottomRightPoint = additionalMatrix.MultiplyPoint(bottomRightPoint);

        Debug.DrawLine(topLeftPoint, topRightPoint, color);
        Debug.DrawLine(bottomLeftPoint, bottomRightPoint, color);

        Debug.DrawLine(topLeftPoint, bottomLeftPoint, color);
        Debug.DrawLine(topRightPoint, bottomRightPoint, color);

        Debug.DrawLine(topLeftPoint, bottomRightPoint, color);
        Debug.DrawLine(topRightPoint, bottomLeftPoint, color);
    }
}

#endif