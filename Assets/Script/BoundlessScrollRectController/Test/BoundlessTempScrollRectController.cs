using System;
using UnityEngine;

public class BoundlessTempScrollRectController : BoundlessScrollRectController<BoundlessTempData>
{
    [SerializeField]
    private BoundlessTempScrollRectItem m_grieItemPrefab;

    private BoundlessTempScrollRectItem[] m_gridItems;

    protected override BoundlessBaseScrollRectItem<BoundlessTempData>[] GridItemArray => m_gridItems;
    public override BoundlessBaseScrollRectItem<BoundlessTempData> GridItemPrefab => m_grieItemPrefab;

    protected override void ResizeGridItemsListSize(int size)
    {
        BoundlessTempScrollRectItem tempGridItem = null;
        if (size < m_gridItems.Length)
        {
            int tail = m_gridItems.Length - 1;
            while (tail > size - 1)
            {
                GameObject.Destroy(m_gridItems[tail].gameObject);
                tail--;
            }
            System.Array.Resize(ref m_gridItems, size);
        }
        else if (size > m_gridItems.Length)
        {
            int tail = m_gridItems.Length;
            Array.Resize(ref m_gridItems, size);
            Vector3 globalScale = Viewport.lossyScale;
            Vector2 itemSize = new Vector2(GridLayoutData.CellSize.x * globalScale.x, GridLayoutData.CellSize.y * globalScale.y);
            for (; tail < size; tail++)
            {
                tempGridItem = Instantiate(m_grieItemPrefab, ActualContent);
                tempGridItem.SetItemSize(itemSize);
                m_gridItems[tail] = tempGridItem;
                tempGridItem.Hide();
            }
        }
    }

    private void Awake()
    {
        m_gridItems = new BoundlessTempScrollRectItem[0];
        CalculateViewportShowCount();
    }

}
