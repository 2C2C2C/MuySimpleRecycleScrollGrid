using UnityEngine;
using UnityEngine.EventSystems;

public class BoundlessTempScrollRectItem : BoundlessBaseScrollRectItem<BoundlessTempData>, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private UnityEngine.UI.Text m_dataText = null;

    System.Action<BoundlessTempData> m_onPointerEnter = null;
    System.Action<BoundlessTempData> m_onPointerLeave = null;

    public override void InjectData(BoundlessTempData data)
    {
        m_dataText.text = data.ItemName;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_onPointerEnter?.Invoke(ItemData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_onPointerLeave?.Invoke(ItemData);
    }

}
