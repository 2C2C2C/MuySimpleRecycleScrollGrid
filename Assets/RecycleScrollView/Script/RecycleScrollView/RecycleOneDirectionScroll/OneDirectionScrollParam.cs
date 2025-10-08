using System;

namespace RecycleScrollView
{
    [Serializable]
    public struct SingleDirectionScrollParam
    {
        public float spacing;
        public float frontPadding;
        public float rearPadding;

        public bool reverseArrangement;
    }
}