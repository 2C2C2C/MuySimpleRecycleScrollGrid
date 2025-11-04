// using UnityEditor;

// namespace RecycleScrollView
// {
//     [CustomEditor(typeof(RecycleScrollGrid))]
//     public class RecycleScrollGridEditor : Editor
//     {
//         private RecycleScrollGrid m_target;

//         public override void OnInspectorGUI()
//         {
//             m_target = target as RecycleScrollGrid;

//             if (EditorApplication.isPlaying)
//             {
//                 EditorGUI.BeginChangeCheck();
//                 base.OnInspectorGUI();
//                 bool hasChanged = EditorGUI.EndChangeCheck();
//                 if (hasChanged)
//                 {
//                     RecycleScrollGrid controller = target as RecycleScrollGrid;
//                     controller.RefreshLayoutChanges();
//                 }
//             }
//             else
//             {
//                 base.OnInspectorGUI();
//             }
//         }
        
//     }
// }