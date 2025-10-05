using System.Collections.Generic;
using RecycleScrollGrid;
using UnityEngine;

public class GuidElementListUI : MonoBehaviour, IListView
{
    [SerializeField]
    private RecycleScrollGridController _scrollRectController;
    [SerializeField]
    private RectTransform _elementPrefab;

    private List<GuidTempData> m_dataList = new List<GuidTempData>();

    public int DataElementCount => m_dataList.Count;

    public void Setup(List<GuidTempData> dataList)
    {
        m_dataList.Clear();
        m_dataList.AddRange(dataList);
        _scrollRectController.Init(this);
    }

    private void OnContentItemFinishDrawing()
    {
        // int elementDataIndex;
        // for (int i = 0; i < m_elementListView.Count; i++)
        // {
        //     elementDataIndex = m_elementListView[i].ElementIndex;
        //     if (elementDataIndex < 0 || elementDataIndex >= m_dataList.Count)
        //     {
        //         continue;
        //     }

        //     // TODO @Hiko setup data
        //     if (m_elementListView[i].NeedRefreshData)
        //     {
        //         // m_elementListView[i].Setup<GuidTempData>(m_dataList[elementDataIndex]);
        //     }
        // }
    }

    private void OnEnable()
    {
        _scrollRectController.OnGridLayoutEnd += OnContentItemFinishDrawing;
    }

    private void OnDisable()
    {
        _scrollRectController.OnGridLayoutEnd -= OnContentItemFinishDrawing;
    }


    public RectTransform AddElement(RectTransform parent)
    {
        RectTransform element = RectTransform.Instantiate(_elementPrefab, parent);
        return element;
    }

    public void RemoveElement(RectTransform element)
    {
        GameObject.Destroy(element.gameObject);
        element = null;
    }

    public void InitElement(RectTransform element, int index)
    {
    }

    public void UnInitElement(RectTransform element)
    {
    }

    public void OnElementIndexChanged(RectTransform elementTransform, int prevIndex, int nextIndex)
    {
    }
}
