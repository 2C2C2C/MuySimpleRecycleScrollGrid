using UnityEngine;

[System.Serializable]
public class BoundlessGridLayoutData
{
    // @Hiko use this later
    /// <summary>
    /// TODO to find a way to do those  
    /// </summary>
    public enum StartCorner
    {
        UpperLeft = 0,
        UpperRight = 1,
        LowerLeft = 2,
        LowerRight = 3
    }

    public enum Constraint
    {
        // in this mode they will just extend vertical side
        FixedColumnCount = 0,
        // in this mode they will just extend horizontal side
        FixedRowCount = 1,
    }

    public enum StartAxis
    {
        Horizontal = 0,
        Vertical = 1,
    }

    public StartCorner m_startCorner = StartCorner.UpperLeft;
    public Constraint constraint = Constraint.FixedColumnCount;
    [Min(1)]
    public int constraintCount = default;
    public StartAxis startAxis = StartAxis.Horizontal;

    public RectOffset m_padding = null;
    public Vector2 cellSize = Vector2.one * 100.0f;
    public Vector2 spacing = default;

    public float StopMagSqrVel = 50.0f;

    public BoundlessTempScrollRectItem GridItemPrefab => m_gridItemPrefab;
    [SerializeField]
    private BoundlessTempScrollRectItem m_gridItemPrefab = null;
}
