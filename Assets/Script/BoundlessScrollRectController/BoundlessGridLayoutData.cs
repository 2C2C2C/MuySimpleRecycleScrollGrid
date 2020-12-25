using UnityEngine;

/*
TODO
apply start axis
apply padding
*/

[System.Serializable]
public class BoundlessGridLayoutData
{
    /// <summary>
    /// TODO to find a way to do those  
    /// </summary>
    // public enum StartCorner
    // {
    //     UpperLeft = 0,
    //     UpperRight = 1,
    //     LowerLeft = 2,
    //     LowerRight = 3
    // }

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

    // // TODO use it
    // public StartCorner m_startCorner = StartCorner.UpperLeft;
    public Constraint constraint = Constraint.FixedColumnCount;

    [SerializeField, Tooltip("auto fit means calculate the constraint count by viewport size")]
    private bool m_autoFit = false;
    public bool IsAutoFit
    {
        get => m_autoFit;
        set
        {
            m_autoFit = value;
            OnFitTypeChanged?.Invoke(m_autoFit);
        }
    }

    [Min(1)]
    public int constraintCount = default;
    public StartAxis startAxis = StartAxis.Horizontal;

    // Padding is to expend/shrink the REAL content
    public RectOffset RectPadding = null;
    [SerializeField]
    private Vector2 m_cellSize = Vector2.one * 100.0f;
    public Vector2 CellSize
    {
        get => m_cellSize;
        set
        {
            m_cellSize = value;
            OnCellSizeChanged?.Invoke(m_cellSize);
        }
    }
    public Vector2 Spacing = default;

    public float StopMagSqrVel = 50.0f;

    [SerializeField]
    private BoundlessTempScrollRectItem m_gridItemPrefab = null;
    public BoundlessTempScrollRectItem GridItemPrefab => m_gridItemPrefab;

    /// <summary>
    /// result is 'autofit'
    /// </summary>
    public event System.Action<bool> OnFitTypeChanged;
    /// <summary>
    /// result is 'cellsize'
    /// </summary>
    public event System.Action<Vector2> OnCellSizeChanged;
}
