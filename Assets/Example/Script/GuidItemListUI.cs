using System.Collections.Generic;
using UnityEngine;

public class GuidItemListUI : MonoBehaviour, IListViewUI
{
    [SerializeField]
    private BoundlessScrollRectController m_scrollRectController;
    [SerializeField]
    private GuidItemUI m_itemPrefab;

    private List<GuidItemUI> m_elementList = new List<GuidItemUI>();
    private List<GuidTempData> m_dataList = new List<GuidTempData>();

    public IListElementUI this[int index] => throw new System.NotImplementedException();

    public int Length => m_dataList.Count;

    public IListElementUI ListElementPrefab => m_itemPrefab;

    public IListElementUI Add()
    {
        GuidItemUI added = GuidItemUI.Instantiate(m_itemPrefab, m_scrollRectController.Content);
        m_elementList.Add(added);
        return added;
    }

    public void Remove(int index)
    {
        if (index < 0 || index >= m_elementList.Count)
        {
            Debug.LogError($"index_{index} out of range", this.gameObject);
            return;
        }

        GuidItemUI toRemove = m_elementList[index];
        m_elementList.RemoveAt(index);
        GameObject.Destroy(toRemove.gameObject);
    }

    public void Setup(List<GuidTempData> dataList)
    {
        m_dataList.Clear();
        m_dataList.AddRange(dataList);
        m_scrollRectController.Setup(this);
    }

    private void TempSetup(GuidItemUI uiItem, GuidTempData data)
    {
        uiItem.Setup(data);
    }

}
