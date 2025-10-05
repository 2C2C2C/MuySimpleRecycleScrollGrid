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
    private Dictionary<RectTransform, GuidElementUI> m_viewElementMap = new Dictionary<RectTransform, GuidElementUI>();

    public int DataElementCount => m_dataList.Count;

    public void Setup(List<GuidTempData> dataList)
    {
        m_dataList.Clear();
        m_dataList.AddRange(dataList);
        // TODO Find a better way to notify data change
        _scrollRectController.Uninit();
        _scrollRectController.Init(this);
    }

    public RectTransform AddElement(RectTransform parent)
    {
        RectTransform element = RectTransform.Instantiate(_elementPrefab, parent);
        if (element.TryGetComponent<GuidElementUI>(out GuidElementUI viewElement))
        {
            m_viewElementMap.Add(element, viewElement);
        }
        return element;
    }

    public void RemoveElement(RectTransform element)
    {
        m_viewElementMap.Remove(element);
        GameObject.Destroy(element.gameObject);
        element = null;
    }

    public void InitElement(RectTransform element, int index)
    {
        if (m_viewElementMap.TryGetValue(element, out GuidElementUI viewElement))
        {
            viewElement.Setup(m_dataList[index]);
        }
    }

    public void UnInitElement(RectTransform element)
    {
        if (m_viewElementMap.TryGetValue(element, out GuidElementUI viewElement))
        {
            viewElement.Clear();
        }
    }

    public void OnElementIndexChanged(RectTransform element, int prevIndex, int nextIndex)
    {
        if (m_viewElementMap.TryGetValue(element, out GuidElementUI viewElement))
        {
            viewElement.Setup(m_dataList[nextIndex]);
        }
    }
}
