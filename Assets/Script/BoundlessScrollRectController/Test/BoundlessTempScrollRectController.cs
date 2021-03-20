using UnityEngine;

public class BoundlessTempScrollRectController : BoundlessScrollRectController<TempDataItem>
{
    [SerializeField]
    private BoundlessTempScrollRectItem m_grieItemPrefab;
    public override BoundlessBaseScrollRectItem<TempDataItem> GridItemPrefab => m_grieItemPrefab;
}
