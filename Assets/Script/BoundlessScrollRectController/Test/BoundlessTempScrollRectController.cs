using UnityEngine;

public class BoundlessTempScrollRectController : BoundlessScrollRectController<BoundlessTempData>
{
    [SerializeField]
    private BoundlessTempScrollRectItem m_grieItemPrefab;

    private BoundlessTempScrollRectItem[] m_gridItems;

    protected override BoundlessBaseScrollRectItem<BoundlessTempData>[] GridItemArray
    {
        get => m_gridItems;
        set
        {
            var gridItems = value;
            System.Array.Resize(ref m_gridItems, gridItems.Length);
            for (int i = 0; i < gridItems.Length; i++)
            {
                m_gridItems[i] = (BoundlessTempScrollRectItem)gridItems[i];
            }
        }
    }

    public override BoundlessBaseScrollRectItem<BoundlessTempData> GridItemPrefab => m_grieItemPrefab;

    private void Awake()
    {
        m_gridItems = new BoundlessTempScrollRectItem[0];
        CalculateViewportShowCount();
    }
}
