public interface IListViewUI
{
    IListElementUI Add();
    void Remove(IListElementUI instance);
    void Remove(int index);
    /// <summary>
    /// the actual UI element count
    /// </summary>
    /// <value></value>
    int Count { get; }
    IListElementUI this[int index] { get; }
    IListElementUI ListElementPrefab { get; }
}
