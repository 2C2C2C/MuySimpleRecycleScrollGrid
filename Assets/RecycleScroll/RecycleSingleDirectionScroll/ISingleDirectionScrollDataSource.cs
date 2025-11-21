using System;
using UnityEngine;

namespace RecycleScrollView
{
    public interface ISingleDirectionScrollDataSource
    {
        int DataElementCount { get; }

        RectTransform RequestElement(RectTransform parent, int index);
        void ReturnElement(RectTransform element);
        void ChangeElementIndex(RectTransform element, int prevIndex, int nextIndex);

        event Action<int> OnDataElementCountChanged;
    }
}