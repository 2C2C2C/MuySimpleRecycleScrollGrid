using UnityEditor;
using UnityEditor.UI;

namespace UnityEngine.UI
{
    [CustomEditor(typeof(UnityScrollRectExtended))]
    public class RecycleScrollGridControllerEditor : ScrollRectEditor
    {
        private SerializedProperty m_velocityStopSqrMagThreshold;
        private SerializedProperty m_velocityMaxSqrMag;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_velocityStopSqrMagThreshold = serializedObject.FindProperty("_velocityStopSqrMagThreshold");
            m_velocityMaxSqrMag = serializedObject.FindProperty("_velocityMaxSqrMag");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Other settings");
            EditorGUILayout.PropertyField(m_velocityStopSqrMagThreshold);
            EditorGUILayout.PropertyField(m_velocityMaxSqrMag);
            serializedObject.ApplyModifiedProperties();
        }

    }
}