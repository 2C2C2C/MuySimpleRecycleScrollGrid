using System;
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

    public IListElementUI this[int index]
    {
        get
        {
            if (index < 0 || index >= m_elementList.Count)
            {
                Debug.LogError($"index out of range", this.gameObject);
                return null;
            }
            return m_elementList[index];
        }
    }

    public int Count => m_dataList.Count;

    public IListElementUI ListElementPrefab => m_itemPrefab;

    public IListElementUI Add()
    {
        GuidElementUI added = GuidElementUI.Instantiate(m_itemPrefab, m_scrollRectController.Content);
        m_elementList.Add(added);
        return added;
    }

    public void Remove(IListElementUI instance)
    {
        for (int i = 0; i < this.Count; i++)
            if (this[i] == instance)
                Remove(i);

        Debug.LogError($"cant find ListElement {instance}", this.gameObject);
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


    void OnContentItemFinishDrawing()
    {
        int elementDataIndex = 0;
        for (int i = 0; i < m_elementList.Count; i++)
        {
            elementDataIndex = m_elementList[i].ElementIndex;
            if (elementDataIndex < 0 || elementDataIndex >= m_dataList.Count)
                continue;
            m_elementList[i].Setup(m_dataList[elementDataIndex]);
        }
    }

    private void OnEnable()
    {
        m_scrollRectController.OnContentItemFinishDrawing += OnContentItemFinishDrawing;
    }

    private void OnDisable()
    {
        m_scrollRectController.OnContentItemFinishDrawing -= OnContentItemFinishDrawing;
    }

}
