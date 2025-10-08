namespace UnityEngine.UI
{
    [AddComponentMenu("UI/UnityScrollRectExtended", 1)]
    [SelectionBase]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class UnityScrollRectExtended : ScrollRect
    {
        public Vector2 ContentStartPos
        {
            get => m_ContentStartPosition;
            set
            {
                m_ContentStartPosition = value;
            }
        }
    }
}