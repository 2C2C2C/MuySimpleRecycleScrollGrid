using System;
using UnityEngine.Events;

namespace RecycleScrollView
{
    public enum ScrollerState
    {
        Idle = 0,
        Scroll = 1,
        Inertia = 2,
        MoveByOther = 3,
    }

    [Serializable]
    public class ScrollerStateEvent : UnityEvent<ScrollerState> { }
}