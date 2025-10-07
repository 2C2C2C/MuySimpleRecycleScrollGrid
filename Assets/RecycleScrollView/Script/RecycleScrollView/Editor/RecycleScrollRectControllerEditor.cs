using UnityEditor;

namespace RecycleScrollView
{
    [CustomEditor(typeof(RecycleScrollGridController))]
    public class RecycleScrollGridControllerEditor : Editor
    {
        private RecycleScrollGridController m_target;

        public override void OnInspectorGUI()
        {
            m_target = target as RecycleScrollGridController;

            if (EditorApplication.isPlaying)
            {
                EditorGUI.BeginChangeCheck();
                base.OnInspectorGUI();
                bool hasChanged = EditorGUI.EndChangeCheck();
                if (hasChanged)
                {
                    RecycleScrollGridController controller = target as RecycleScrollGridController;
                    controller.RefreshLayoutChanges();
                }
            }
            else
            {
                base.OnInspectorGUI();
            }
        }
        
    }
}