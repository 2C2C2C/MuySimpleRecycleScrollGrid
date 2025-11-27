using System;
using UnityEngine;

namespace RecycleScrollView
{
    public interface ISingleDirectionScrollDataSource
    {
        int DataElementCount { get; }

        RectTransform RequestElement(RectTransform parent, int dataIndex);
        void ReturnElement(RectTransform element);
        void ChangeElementIndex(RectTransform element, int prevDataIndex, int nextDataIndex);

        event Action<int> OnDataElementCountChanged;
    }
}