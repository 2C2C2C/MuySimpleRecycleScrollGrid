using System;

namespace RecycleScrollView
{
    [Serializable]
    public struct SingleDirectionScrollParam
    {
        public enum ScrollDirection
        {
            None = 0,
            /// <summary>
            /// Default arrangement is top to down
            /// </summary>
            Horizontal = 1,
            /// <summary>
            /// Default arrangement is left to right
            /// </summary>
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