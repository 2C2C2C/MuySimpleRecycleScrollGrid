using UnityEngine;
using UnityEngine.EventSystems;
public class BoundlessTempScrollRectItem : BoundlessBaseScrollRectItem, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private UnityEngine.UI.Text m_dataText = null;
    private BoundlessTempData m_data = default;

    System.Action<BoundlessBaseScrollRectItem> m_onPointerEnter = null;
    System.Action<BoundlessBaseScrollRectItem> m_onPointerLeave = null;

    public override void InjectData(IBoundlessScrollRectItemData data)
    {
        m_data = data as BoundlessTempData;
        m_dataText.text = m_data.ItemName;
    }

    public void SetupHoverCallback()
    {

    }

    public void OnPointerClick(PointerEventData eventData)
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_onPointerEnter?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_onPointerLeave?.Invoke(this);
    }
}
