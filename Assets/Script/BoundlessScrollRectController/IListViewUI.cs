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
}
