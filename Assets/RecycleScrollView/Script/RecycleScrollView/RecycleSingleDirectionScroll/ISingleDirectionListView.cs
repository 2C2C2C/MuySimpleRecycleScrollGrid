using UnityEngine;

namespace RecycleScrollView
{
    public interface ISingleDirectionListView
    {
        int DataElementCount { get; }

        RectTransform RequestElement(RectTransform parent, int index);
        void ReturnElement(RectTransform element);
    }
}