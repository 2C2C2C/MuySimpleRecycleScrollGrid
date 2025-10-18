using System;

namespace RecycleScrollView
{
    [Serializable]
    public struct SingleDirectionScrollParam
    {
        public enum ScrollDirection
        {
            None = 0,
            Horizontal_LeftToRight = 1,
            Horizontal_RightToLeft = 2,
            Vertical_UpToDown = 3,
            Vertical_DownToUp = 4,
        }

        public ScrollDirection scrollDirection;

        // Reverse data elements into UI elment
        public bool reverseArrangement;

        // TODO
        // public float spacing;
        // public float frontPadding;
        // public float rearPadding;

        public readonly bool IsHorizontal => ScrollDirection.Horizontal_LeftToRight == scrollDirection || ScrollDirection.Horizontal_RightToLeft == scrollDirection;
        public readonly bool IsVertical => ScrollDirection.Vertical_UpToDown == scrollDirection || ScrollDirection.Vertical_DownToUp == scrollDirection;

    }
}