using UnityEngine;
using UnityEngine.UI;

namespace RecycleScrollView
{
    [RequireComponent(typeof(LayoutElement))]
    public class RecycleOneDirectionScrollElement : MonoBehaviour
    {
        [SerializeField]
        private LayoutElement _layoutElement;
        [SerializeField]
        private bool _directlyUseSizeFromLayoutElement;
        [SerializeField]
        private LayoutElementSizeSetter _elementSizeSetter;

        [SerializeField]
        private int m_index = -1; // This value should be NonSerialized but better to show it in inspector

        private RectTransform m_rectTransform;

        public int ElementIndex => m_index;

        public RectTransform ElementTransform
        {
            get
            {
                if (null == m_rectTransform)
                {
                    m_rectTransform = transform as RectTransform;
                }
                return m_rectTransform;
            }
        }

        public Vector2 ElementPreferredSize { get; private set; }

        public void SetIndex(int index)
        {
            m_index = index;
        }

        [ContextMenu("ForceCalculateSize")]
        public void CalculatePreferredSize()
        {
            // HACK
            if (TryGetComponent<HorizontalOrVerticalLayoutGroup>(out _))
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(ElementTransform);
            }

            float width = 0f, height = 0f;
            if (_directlyUseSizeFromLayoutElement)
            {
                width = 1f <= _layoutElement.flexibleWidth ? ElementTransform.rect.width : _layoutElement.preferredWidth;
                height = 1f <= _layoutElement.flexibleHeight ? ElementTransform.rect.height : _layoutElement.preferredHeight;
                ElementPreferredSize = new Vector2(width, height);
                return;
            }

            if (null != _elementSizeSetter)
            {
                _elementSizeSetter.ForceSetSize();
            }
            width = 1f <= _layoutElement.flexibleWidth ? ElementTransform.rect.width : _layoutElement.preferredWidth;
            height = 1f <= _layoutElement.flexibleHeight ? ElementTransform.rect.height : _layoutElement.preferredHeight;
            ElementPreferredSize = new Vector2(width, height);
        }

#if UNITY_EDITOR

        private void Reset()
        {
            TryGetComponent<LayoutElement>(out _layoutElement);
        }

#endif
    }
}