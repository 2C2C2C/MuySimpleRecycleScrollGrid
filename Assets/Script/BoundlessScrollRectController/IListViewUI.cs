using System.Collections.Generic;
using UnityEngine;

public abstract class IListViewUI : MonoBehaviour
{
    public abstract IListElementUI Add();
    public abstract void Remove(IListElementUI instance);
    public abstract void RemoveAt(int index);
    /// <summary>
    /// the actual UI element count
    /// </summary>
    /// <value></value>
    public abstract int Count { get; }
    public abstract IListElementUI this[int index] { get; }
    public abstract IReadOnlyList<IListElementUI> ElementList { get; }
    public abstract Transform ElementContainer { get; }

    protected List<IListElementUI> m_elementList = new List<IListElementUI>(0);

    public void InitGetExistElements()
    {
        Transform container = ElementContainer;
        int childCount = container.childCount;
        List<IListElementUI> elementList = new List<IListElementUI>(childCount);

        IListElementUI elementUI = null;
        for (int i = 0; i < childCount; i++)
        {
            elementUI = container.GetChild(childCount).GetComponent<IListElementUI>();
            if (elementUI != null) elementList.Add(elementUI);
        }
        m_elementList = elementList;
    }
}
