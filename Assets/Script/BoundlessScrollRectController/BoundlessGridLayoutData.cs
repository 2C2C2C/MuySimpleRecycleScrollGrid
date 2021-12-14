using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BoundlessGridLayoutData
{
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

    public GridLayoutGroup.Corner startCorner;

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

    /// <summary>
    /// result is 'isAutoFit'
    /// </summary>
    public event System.Action<bool> OnFitTypeChanged;
    /// <summary>
    /// result is 'cellsize'
    /// </summary>
    public event System.Action<Vector2> OnCellSizeChanged;

    public event System.Action OnLayoutDataChanged;

#if UNITY_EDITOR

    public void CallRefresh()
    {
        OnLayoutDataChanged?.Invoke();
    }

#endif

}
