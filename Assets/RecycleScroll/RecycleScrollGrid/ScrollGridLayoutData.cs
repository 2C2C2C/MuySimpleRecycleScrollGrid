using UnityEngine;
using UnityEngine.UI;

namespace RecycleScrollView
{
    [System.Serializable]
    public class ScrollGridLayoutData
    {
        public enum Constraint
        {
            /// <summary> Just extend vertical side </summary>
            FixedColumnCount = 0,
            /// <summary> Just extend horizontal side </summary>
            FixedRowCount = 1,
        }

        public GridLayoutGroup.Axis startAxis = GridLayoutGroup.Axis.Horizontal;
        public GridLayoutGroup.Corner startCorner = GridLayoutGroup.Corner.UpperLeft;
        public Constraint constraint = Constraint.FixedColumnCount;

        // /// <summary> Calculate the constraint count by viewport size </summary>
        // public bool isAutoFit = false;

        [Min(1)]
        public int constraintCount = default;
        public Vector2 gridSize = Vector2.one * 100.0f;
        public Vector2 Spacing = default;

        // Clamp the ScrollRect velocity to cuz I dun want it to scroll at a very low velocity(prevent mesh rebuilding)
        public float scrollStopVelocityMagSqr = 50.0f;

        /// <summary> Padding is to expend/shrink the REAL content </summary>
        [SerializeField]
        private RectOffset m_rectPadding;
        public RectOffset RectPadding => m_rectPadding ??= new RectOffset(0, 0, 0, 0);
    }
}