using UnityEngine;

namespace RecycleScrollView
{
    public interface IOneDirectionScrollDataSource
    {
        int DataElementCount { get; }

        RectTransform RequestElement(RectTransform parent, int index);
        void ReturnElement(RectTransform element);
    }
}