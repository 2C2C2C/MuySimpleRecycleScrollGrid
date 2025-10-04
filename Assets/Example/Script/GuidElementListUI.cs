﻿using System.Collections.Generic;
using RecycleScrollGrid;
using UnityEngine;

public class GuidElementListUI : MonoBehaviour
{
    [SerializeField]
    private RecycleScrollGridController m_scrollRectController;
    [SerializeField]
    private TempListView m_elementListView;

    private List<GuidTempData> m_dataList = new List<GuidTempData>();

    public void Setup(List<GuidTempData> dataList)
    {
        m_dataList.Clear();
        m_dataList.AddRange(dataList);
        m_scrollRectController.Setup(m_elementListView, dataList.Count);
    }

    void OnContentItemFinishDrawing()
    {
        int elementDataIndex;
        for (int i = 0; i < m_elementListView.Count; i++)
        {
            elementDataIndex = m_elementListView[i].ElementIndex;
            if (elementDataIndex < 0 || elementDataIndex >= m_dataList.Count)
            {
                continue;
            }

            // TODO @Hiko setup data
            if (m_elementListView[i].NeedRefreshData)
            {
                m_elementListView[i].Setup<GuidTempData>(m_dataList[elementDataIndex]);
            }
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
