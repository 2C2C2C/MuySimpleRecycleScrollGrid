using System.Collections.Generic;
using UnityEngine;

public class GuidItemListUI : MonoBehaviour
{
    [SerializeField]
    private BoundlessScrollRectController m_scrollRectController;
    [SerializeField]
    private GuidItemUI m_itemPrefab;

    private List<GuidTempData> m_dataList = new List<GuidTempData>();

    public void Setup(List<GuidTempData> dataList)
    {
        m_dataList.Clear();
        m_dataList.AddRange(dataList);
        m_scrollRectController.Setup(new UIModelContainer<GuidItemUI, GuidTempData>(m_itemPrefab, m_dataList, m_scrollRectController.Content, TempSetup));
    }

    private void TempSetup(GuidItemUI uiItem, GuidTempData data)
    {
        uiItem.Setup(data);
    }

}
