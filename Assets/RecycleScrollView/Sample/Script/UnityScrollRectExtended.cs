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

        public void CallUpdateBoundsAndPrevData()
        {
            SetDirtyCaching();
            base.Rebuild(CanvasUpdate.PostLayout);
            // UpdateBounds();
            // UpdatePrevData();
        }

        [ContextMenu(nameof(TestCalculateOffset))]
        public void TestCalculateOffset()
        {
            Debug.LogError(CalculateCurrentOffset(Vector2.zero));
        }

        public Vector2 CalculateCurrentOffset(Vector2 delta)
        {
            var viewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            Bounds contentBounds = m_ContentBounds;
            Vector2 offset = Vector2.zero;
            if (movementType == MovementType.Unrestricted)
                return offset;

            Vector2 min = contentBounds.min;
            Vector2 max = contentBounds.max;

            // min/max offset extracted to check if approximately 0 and avoid recalculating layout every frame (case 1010178)

            if (horizontal)
            {
                min.x += delta.x;
                max.x += delta.x;

                float maxOffset = viewBounds.max.x - max.x;
                float minOffset = viewBounds.min.x - min.x;

                if (minOffset < -0.001f)
                    offset.x = minOffset;
                else if (maxOffset > 0.001f)
                    offset.x = maxOffset;
            }

            if (vertical)
            {
                min.y += delta.y;
                max.y += delta.y;

                float maxOffset = viewBounds.max.y - max.y;
                float minOffset = viewBounds.min.y - min.y;

                if (maxOffset > 0.001f)
                    offset.y = maxOffset;
                else if (minOffset < -0.001f)
                    offset.y = minOffset;
            }

            return offset;
        }
    }
}