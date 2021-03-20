using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(BoundlessGridLayoutData))]
public class BoundlessGridLayoutDataPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(position, property, label, true);

        bool hasChanged = EditorGUI.EndChangeCheck();
        if (hasChanged)
        {
            BoundlessGridLayoutData data = fieldInfo.GetValue(property.serializedObject.targetObject) as BoundlessGridLayoutData;
            data.CallRefresh();
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property);
    }

}
