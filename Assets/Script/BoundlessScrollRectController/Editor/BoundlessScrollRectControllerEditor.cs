using UnityEditor;

[CustomEditor(typeof(BoundlessScrollRectController))]
public class BoundlessScrollRectControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (EditorApplication.isPlaying)
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            bool hasChanged = EditorGUI.EndChangeCheck();
            if (hasChanged)
            {
                BoundlessScrollRectController controller = target as BoundlessScrollRectController;
                controller.RefreshLayoutChanges();
            }
        }
        else
            base.OnInspectorGUI();
    }
}
