using System;

namespace RecycleScrollView
{
    [Serializable]
    public struct SingleDirectionScrollParam
    {
        public enum ScrollDirection
        {
            None = 0,
            Horizontal = 1,
            vertical = 2,
        }

        public float spacing;
        // TODO
        // public float frontPadding;
        // public float rearPadding;

        public ScrollDirection scrollDirection;
        public bool reverseArrangement;
    }
}