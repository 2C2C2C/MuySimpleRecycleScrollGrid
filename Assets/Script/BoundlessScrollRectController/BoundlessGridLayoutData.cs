using UnityEngine;

[SerializeField]
public class BoundlessGridLayoutData
{
    public enum StartCorner
    {
        UpperLeft = 0,
        UpperRight = 1,
        LowerLeft = 2,
        LowerRight = 3
    }

    public enum Constraint
    {
        FixedColumnCount = 0,
        FixedRowCount = 1,
    }

    public RectOffset m_padding = null;
    public Vector2 m_cellSize = default;
    public Vector2 m_spacing = default;

}
