using UnityEngine;
using UnityEngine.EventSystems;
public class BoundlessTempScrollRectItem : BoundlessBaseScrollRectItem<TempDataItem>, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private UnityEngine.UI.Text m_dataText = null;

    System.Action<TempDataItem> m_onPointerEnter = null;
    System.Action<TempDataItem> m_onPointerLeave = null;

    public override void InjectData(TempDataItem data)
    {
        ItemData = data;
        m_dataText.text = ItemData.TempName;
    }

    public void SetupHoverCallback()
    {

    }

    public void OnPointerClick(PointerEventData eventData)
    {

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
