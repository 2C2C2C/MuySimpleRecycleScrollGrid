using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BoundlessScrollRectController))]
public class BoundlessScrollRectControllerEditor : Editor
{
    private BoundlessScrollRectController m_target = null;

    public override void OnInspectorGUI()
    {
        if (null == m_target)
            m_target = base.target as BoundlessScrollRectController;

        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        bool hasChanged = EditorGUI.EndChangeCheck();

        if (hasChanged)
        {
            m_target.UpdateConstraintWithAutoFit();
            m_target.RefreshLayout();
        }
    }
}
