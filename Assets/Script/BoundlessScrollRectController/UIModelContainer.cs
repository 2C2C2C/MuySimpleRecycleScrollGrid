using System;
using System.Collections.Generic;
using UnityEngine;

public interface IElementBuilder
{
    void ResizeArray(int nextCount);
    void SetupItem(int itemIndex, int dataIndex);
    void HideItem(int itemIndex);
    int DataCount { get; }
    IReadOnlyList<RectTransform> ItemRectTransformArray { get; }
}

public class UIModelContainer<TComponent, TData> : IElementBuilder where TComponent : MonoBehaviour
{
    List<TData> m_dataList;
    TComponent[] m_componentArray;
    RectTransform[] m_componentRectTransformArray;

    Transform m_componentContainer;
    TComponent m_componentPrefab;

    Action<TComponent, TData> m_setup;

    public int DataCount => m_dataList.Count;
    public IReadOnlyList<TComponent> ComponentArray => m_componentArray;
    public IReadOnlyList<RectTransform> ItemRectTransformArray => m_componentRectTransformArray;

    public UIModelContainer(TComponent prefab, List<TData> dataList, Transform componentContainer, Action<TComponent, TData> setupAction)
    {
        m_componentPrefab = prefab;
        m_componentContainer = componentContainer;

        m_dataList = new List<TData>();
        m_dataList.AddRange(dataList);

        m_componentArray = new TComponent[0];
        m_componentRectTransformArray = new RectTransform[0];

        m_setup = setupAction;
    }

    public void ResizeArray(int size)
    {
        int index = 0;
        int currentSize = m_componentArray.Length;
        if (size > currentSize)
        {
            // directly add
            Array.Resize<TComponent>(ref m_componentArray, size);
            Array.Resize<RectTransform>(ref m_componentRectTransformArray, size);
            index = currentSize;
            while (index < size)
            {
                m_componentArray[index] = MonoBehaviour.Instantiate(m_componentPrefab, m_componentContainer);
                m_componentRectTransformArray[index] = (RectTransform)m_componentArray[index].transform;
                index++;
            }
        }
        else if (size < currentSize)
        {
            index = currentSize - 1;
            while (index >= size)
            {
                GameObject.Destroy(m_componentArray[index].gameObject);
                m_componentArray[index] = null;
                m_componentRectTransformArray[index] = null;
                index--;
            }
            Array.Resize<TComponent>(ref m_componentArray, size);
            Array.Resize<RectTransform>(ref m_componentRectTransformArray, size);
        }
    }

    public void SetupItem(int elementIndex, int dataIndex)
    {
        bool invalid = elementIndex < 0 || elementIndex > m_componentArray.Length - 1 || dataIndex < 0 || dataIndex > m_dataList.Count - 1;
        if (invalid)
            return;

        m_setup?.Invoke(m_componentArray[elementIndex], m_dataList[dataIndex]);
        if (!m_componentArray[elementIndex].gameObject.activeSelf)
            m_componentArray[elementIndex].gameObject.SetActive(true);
    }

    public void HideItem(int elementIndex)
    {
        bool invalid = elementIndex < 0 || elementIndex > m_componentArray.Length - 1;
        if (invalid)
            return;

        m_componentArray[elementIndex].gameObject.SetActive(false);
    }
}
