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
        m_canvas = GetComponentInParent<Canvas>();
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

    // TODO should apply canvas scale to make item size and content size correctly :)

    private void DrawDebugContentSize()
    {
        if (null == m_dataList)
            return;

        RectOffset padding = GridLayoutData.RectPadding;
        Vector2 paddingValueRaw = new Vector2(padding.horizontal, padding.vertical);
        Vector3 rootScale = m_viewport.lossyScale;

        Vector2 actualContentSize = (m_actualContentSizeRaw + paddingValueRaw);
        actualContentSize.x *= rootScale.x;
        actualContentSize.y *= rootScale.y;

        Vector3 topLeftPoint = m_dragContent.position;
        Vector3 topRightPoint = topLeftPoint;
        topRightPoint.x += actualContentSize.x;

        Vector3 bottomLeftPoint = topLeftPoint;
        Vector3 BottomRightPoint = topRightPoint;
        bottomLeftPoint.y -= actualContentSize.y;
        BottomRightPoint.y -= actualContentSize.y;

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
        Vector3 columnStartItemTopLeftPos = m_dragContent.position;
        columnStartItemTopLeftPos.z = 0.0f;
        Vector3 rootScale = m_viewport.lossyScale;

        // TODO use offset as padding correctly
        if (null != GridLayoutData)
        {
            RectOffset padding = GridLayoutData.RectPadding;
            columnStartItemTopLeftPos += new Vector3(padding.left * rootScale.x, -padding.top * rootScale.y, 0.0f);
        }
        Vector2 spacing = new Vector2(m_gridLayoutGroup.Spacing.x * rootScale.x, m_gridLayoutGroup.Spacing.y * rootScale.y);
        Vector2 itemSize = new Vector2(m_gridLayoutGroup.CellSize.x * rootScale.x, m_gridLayoutGroup.CellSize.y * rootScale.y);

        // should know which axis get constrained
        int constraintCount = m_gridLayoutGroup.constraintCount;
        int dynamicCount = (dataCount % constraintCount > 0) ? (dataCount / constraintCount) + 1 : (dataCount / constraintCount);
        if (BoundlessGridLayoutData.Constraint.FixedColumnCount == m_gridLayoutGroup.constraint)
        {
            for (int i = 0; i < dynamicCount; i++)
            {
                rowItemTopLeftPos = columnStartItemTopLeftPos;
                for (int j = 0; j < constraintCount; j++)
                {
                    DrawOneDebugGridItem(rowItemTopLeftPos, itemSize, Color.blue);
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
                    DrawOneDebugGridItem(rowItemTopLeftPos, itemSize, Color.blue);
                    rowItemTopLeftPos.x += spacing.x + itemSize.x;
                }
                columnStartItemTopLeftPos.y -= itemSize.y + spacing.y;
            }
        }
    }

    private void DrawDebugShowingGrids()
    {
        Vector3 dragContentPostion = m_dragContent.position;
        Vector3 dragAnchorContentPostion = m_dragContent.anchoredPosition;
        Vector3 globalScale = m_viewport.lossyScale;

        RectOffset padding = null;
        if (null != GridLayoutData)
            padding = GridLayoutData.RectPadding;

        // TODO need to know the moving direction, then adjust it to prevent wrong draw
        float xMove = dragAnchorContentPostion.x < 0 ? (-dragAnchorContentPostion.x - padding.horizontal) * globalScale.x : 0;
        xMove = Mathf.Clamp(xMove, 0.0f, Mathf.Abs(xMove));
        float yMove = dragAnchorContentPostion.y > 0 ? (dragAnchorContentPostion.y - padding.vertical) * globalScale.y : 0;
        yMove = Mathf.Clamp(yMove, 0.0f, Mathf.Abs(yMove));

        Vector2 itemSize = new Vector2(m_gridLayoutGroup.CellSize.x * globalScale.x, m_gridLayoutGroup.CellSize.y * globalScale.y);
        Vector2 spacing = new Vector2(m_gridLayoutGroup.Spacing.x * globalScale.x, m_gridLayoutGroup.Spacing.y * globalScale.y);

        // re calculate
        int tempXIndex = Mathf.FloorToInt((xMove + spacing.x) / (itemSize.x + spacing.x));
        if (xMove % (itemSize.x + spacing.x) - itemSize.x > spacing.x)
            tempXIndex = Mathf.Clamp(tempXIndex - 1, 0, tempXIndex);

        int tempYIndex = Mathf.FloorToInt((yMove + spacing.y) / (itemSize.y + spacing.y));
        if (yMove % (itemSize.y + spacing.y) - itemSize.y > spacing.y)
            tempYIndex = Mathf.Clamp(tempYIndex - 1, 0, tempYIndex);

        // TODO temp calculate from top left
        Vector3 tempMove = new Vector3(tempXIndex * (itemSize.x + spacing.x), -tempYIndex * (itemSize.y + spacing.y), 0.0f);
        Rect contentRect = default;
        Vector2 contentRectSize = (m_actualContentSizeRaw + new Vector2(padding.horizontal, padding.vertical));
        contentRectSize.x *= globalScale.x;
        contentRectSize.y *= globalScale.y;
        contentRect = new Rect(m_dragContent.position, contentRectSize);

        // deal with content from left to right (simple case) first
        Vector3 rowTopLeftPosition = default, itemTopLeftPosition = default;
        rowTopLeftPosition = dragContentPostion + tempMove;
        rowTopLeftPosition += new Vector3(padding.left * globalScale.x, -padding.top * globalScale.y, 0.0f);
        Rect gridRect = new Rect(rowTopLeftPosition, itemSize);
        for (int rowIndex = 0; rowIndex < m_viewItemCountInColumn; rowIndex++)
        {
            itemTopLeftPosition = rowTopLeftPosition;
            for (int columnIndex = 0; columnIndex < m_viewItemCountInRow; columnIndex++)
            {
                gridRect.position = itemTopLeftPosition;

                if (contentRect.Contains(gridRect))
                    DrawOneDebugGridItem(itemTopLeftPosition, itemSize, Color.white); // the real grid in the content
                else
                    DrawOneDebugGridItem(itemTopLeftPosition, itemSize, Color.yellow); // the grid should not show

                itemTopLeftPosition.x += spacing.x + itemSize.x;
            }
            rowTopLeftPosition.y -= spacing.y + itemSize.y;
        }
    }

    private void DrawOneDebugGridItem(Vector3 topLeftPoint, Vector3 itemSize, Color color)
    {
        Vector3 topRightPoint = topLeftPoint;
        topRightPoint.x += itemSize.x;

        Vector3 bottomLeftPoint = topLeftPoint;
        bottomLeftPoint.y -= itemSize.y;

        Vector3 bottomRightPoint = topRightPoint;
        bottomRightPoint.y -= itemSize.y;

        Debug.DrawLine(topLeftPoint, topRightPoint, color);
        Debug.DrawLine(bottomLeftPoint, bottomRightPoint, color);

        Debug.DrawLine(topLeftPoint, bottomLeftPoint, color);
        Debug.DrawLine(topRightPoint, bottomRightPoint, color);

        Debug.DrawLine(topLeftPoint, bottomRightPoint, color);
        Debug.DrawLine(topRightPoint, bottomLeftPoint, color);
    }

}

#endif