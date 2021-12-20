using System.Collections.Generic;
using UnityEngine;

public class GuidElementListUI : MonoBehaviour, IListViewUI
{
    [SerializeField]
    private BoundlessScrollRectController m_scrollRectController;
    [SerializeField]
    private GuidElementUI m_itemPrefab;

    private List<GuidElementUI> m_elementList = new List<GuidElementUI>();
    private List<GuidTempData> m_dataList = new List<GuidTempData>();

    public IListElementUI this[int index] => throw new System.NotImplementedException();

    public int Length => m_dataList.Count;

    public IListElementUI ListElementPrefab => m_itemPrefab;

    public IListElementUI Add()
    {
        GuidElementUI added = GuidElementUI.Instantiate(m_itemPrefab, m_scrollRectController.Content);
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

        GuidElementUI toRemove = m_elementList[index];
        m_elementList.RemoveAt(index);
        GameObject.Destroy(toRemove.gameObject);
    }

    public void Setup(List<GuidTempData> dataList)
    {
        m_dataList.Clear();
        m_dataList.AddRange(dataList);
        m_scrollRectController.Setup(this);
    }

    private void TempSetup(GuidElementUI uiItem, GuidTempData data)
    {
        uiItem.Setup(data);
    }

}
