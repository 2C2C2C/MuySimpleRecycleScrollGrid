#if UNITY_EDITOR
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public partial class BoundlessScrollRectController : UIBehaviour
{

    [Space, Header("Debug draw style A")]
    public bool m_drawContentSizeB = true;
    public bool m_drawGridsB = true;
    public bool m_drawShowingGridsB = true;
    public bool m_drawActualUIItemsB = true;

    void DebugDrawStyleB()
    {
        if (m_drawContentSizeB) DrawDebugContentSizeNew();
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

}

#endif