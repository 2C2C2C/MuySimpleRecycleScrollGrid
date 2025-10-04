#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static partial class RectTransformEx
{
    [MenuItem("CONTEXT/RectTransform/Convert size to anchor")]
    public static void ConvertSize2Anchor(MenuCommand command)
    {
        RectTransform self = command.context as RectTransform;
        if (self == null || self.parent == null)
        {
            return;
        }
        Undo.RecordObject(self, "Set Anchors");
        ConvertToAnchorMode(self);
    }
}
#endif