using UnityEditor;

[CustomEditor(typeof(BoundlessScrollRectController<IBoundlessGridData>))]
public class BoundlessScrollRectControllerEditor : Editor
{
    private BoundlessScrollRectController<IBoundlessGridData> m_target = null;

    public override void OnInspectorGUI()
    {
        if (null == m_target)
            m_target = base.target as BoundlessScrollRectController<IBoundlessGridData>;

        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        bool hasChanged = EditorGUI.EndChangeCheck();

        if (hasChanged)
        {
            // too bad :)
            UnityEngine.Debug.Log("test change");
            m_target.GridLayoutData.IsAutoFit = m_target.GridLayoutData.IsAutoFit;
            m_target.GridLayoutData.CellSize = m_target.GridLayoutData.CellSize;
            m_target.RefreshLayout();
        }
    }
}
