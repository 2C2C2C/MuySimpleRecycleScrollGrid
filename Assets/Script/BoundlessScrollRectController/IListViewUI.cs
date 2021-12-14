public interface IListViewUI
{
    IListElementUI Add();
    void Remove(int index);
    int Length { get; }
    IListElementUI this[int index] { get; }
    IListElementUI ListElementPrefab { get; }
}
