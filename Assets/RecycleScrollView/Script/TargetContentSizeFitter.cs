using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RecycleScrollView
{
    [ExecuteAlways]
    /// <summary>
    /// Resizes a RectTransform to fit the size of its content.
    /// </summary>
    /// <remarks>
    /// The ContentSizeFitter can be used on GameObjects that have one or more ILayoutElement components, such as Text, Image, HorizontalLayoutGroup, VerticalLayoutGroup, and GridLayoutGroup.
    /// </remarks>
    public class TargetContentSizeFitter : UIBehaviour, ILayoutGroup
    {
        /// <summary>
        /// The size fit modes avaliable to use.
        /// </summary>
        public enum FitMode
        {
            /// <summary>
            /// Don't perform any resizing.
            /// </summary>
            Unconstrained,

            /// <summary>
            /// Resize to the minimum size of the content.
            /// </summary>
            MinSize,

            /// <summary>
            /// Resize to the preferred size of the content.
            /// </summary>
            PreferredSize
        }

        [SerializeField] protected FitMode m_HorizontalFit = FitMode.Unconstrained;

        /// <summary>
        /// The fit mode to use to determine the width.
        /// </summary>
        public FitMode horizontalFit
        {
            get { return m_HorizontalFit; }
            set
            {
                if (m_HorizontalFit != value)
                {
                    m_HorizontalFit = value;
                    SetDirty();
                }
            }
        }

        [SerializeField] protected FitMode m_VerticalFit = FitMode.Unconstrained;

        /// <summary>
        /// The fit mode to use to determine the height.
        /// </summary>
        public FitMode verticalFit
        {
            get { return m_VerticalFit; }
            set
            {
                if (m_VerticalFit != value)
                {
                    m_VerticalFit = value;
                    SetDirty();
                }
            }
        }

        [SerializeField] protected int m_HorizontalPadding = 0;

        /// <summary>
        /// The height addition.
        /// </summary>
        public int horizontalPadding
        {
            get { return m_HorizontalPadding; }
            set
            {
                if (m_HorizontalPadding != value)
                {
                    m_HorizontalPadding = value;
                    SetDirty();
                }
            }
        }

        [SerializeField] protected int m_VerticalPadding = 0;

        /// <summary>
        /// The width addition.
        /// </summary>
        public int verticalPadding
        {
            get { return m_VerticalPadding; }
            set
            {
                if (m_VerticalPadding != value)
                {
                    m_VerticalPadding = value;
                    SetDirty();
                }
            }
        }

        [SerializeField] private RectTransform m_TargetRect;
        [SerializeField] private RectTransform m_SourceRect;

        private DrivenRectTransformTracker m_Tracker;

        protected TargetContentSizeFitter()
        {
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(m_TargetRect);
            base.OnDisable();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        private void HandleSelfFittingAlongAxis(int axis)
        {
            FitMode fitting = (axis == 0 ? horizontalFit : verticalFit);
            int padding = (axis == 0 ? horizontalPadding : verticalPadding);
            if (fitting == FitMode.Unconstrained)
            {
                // Keep a reference to the tracked transform, but don't control its properties:
                m_Tracker.Add(this, m_TargetRect, DrivenTransformProperties.None);
                return;
            }

            m_Tracker.Add(this, m_TargetRect,
                (axis == 0 ? DrivenTransformProperties.SizeDeltaX : DrivenTransformProperties.SizeDeltaY));

            // Set size to min or preferred size
            if (fitting == FitMode.MinSize)
                m_TargetRect.SetSizeWithCurrentAnchors((RectTransform.Axis) axis,
                    LayoutUtility.GetMinSize(m_SourceRect, axis) + padding);
            else
                m_TargetRect.SetSizeWithCurrentAnchors((RectTransform.Axis) axis,
                    LayoutUtility.GetPreferredSize(m_SourceRect, axis) + padding);
        }

        /// <summary>
        /// Calculate and apply the horizontal component of the size to the RectTransform
        /// </summary>
        public virtual void SetLayoutHorizontal()
        {
            m_Tracker.Clear();
            HandleSelfFittingAlongAxis(0);
        }

        /// <summary>
        /// Calculate and apply the vertical component of the size to the RectTransform
        /// </summary>
        public virtual void SetLayoutVertical()
        {
            HandleSelfFittingAlongAxis(1);
        }

        protected void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(m_TargetRect);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }

        [ContextMenu("SetSelfAsTarget")]
        private void SetSelfAsTarget()
        {
            RectTransform self = transform as RectTransform;
            m_TargetRect = self;
        }

#endif
    }
}