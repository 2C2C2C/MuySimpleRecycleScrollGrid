using UnityEngine;
using UnityEngine.EventSystems;

public class BoundlessTempScrollRectItem : BoundlessBaseScrollRectItem<BoundlessTempData>, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private UnityEngine.UI.Text m_dataText = null;
    [SerializeField]
    private GameObject m_contentObject = null;

    System.Action<BoundlessTempData> m_onPointerEnter = null;
    System.Action<BoundlessTempData> m_onPointerLeave = null;

    public override void Setup(BoundlessTempData data)
    {
        m_dataText.text = data.ItemName;
        if (!m_contentObject.activeSelf)
        {
            m_contentObject.SetActive(true);
        }
    }

    public override void SetEmpty()
    {
        if (m_contentObject.activeSelf)
        {
            m_contentObject.SetActive(false);
        }
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
