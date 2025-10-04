namespace UnityEngine.UI
{
    public class EmptyRaycastTarget : MaskableGraphic
    {
        protected EmptyRaycastTarget() { }
        public override void SetAllDirty() { }
        public override void Rebuild(CanvasUpdate update) { }
        public override void LayoutComplete() { }
        public override void GraphicUpdateComplete() { }
        public override void SetNativeSize() { }

        protected override void OnRectTransformDimensionsChange() { }
        protected override void OnDidApplyAnimationProperties() { }
        protected override void UpdateMaterial() { }
        protected override void UpdateGeometry() { }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }

#if UNITY_EDITOR
        public override void OnRebuildRequested() { }
        protected override void Reset() { }
#endif
    }
}
