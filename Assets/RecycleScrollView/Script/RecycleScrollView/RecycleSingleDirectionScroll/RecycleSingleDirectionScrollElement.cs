using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace RecycleScrollView
{
    [RequireComponent(typeof(LayoutElement))]
    public class RecycleSingleDirectionScrollElement : MonoBehaviour
    {
        public static MethodInfo GetTotalPreferredSizeMethod
        {
            get
            {
                if (null == s_getTotalPreferredSizeMethod)
                {
                    s_getTotalPreferredSizeMethod = typeof(LayoutGroup).GetMethod("GetTotalPreferredSize", BindingFlags.Instance | BindingFlags.NonPublic);
                }
                return s_getTotalPreferredSizeMethod;
            }
        }
        private static MethodInfo s_getTotalPreferredSizeMethod;
        private static readonly object[] s_boxedInt0 = new object[] { 0 };
        private static readonly object[] s_boxedInt1 = new object[] { 1 };

        [SerializeField]
        private LayoutElement _layoutElement;
        [SerializeField]
        private bool _directlyUseSizeFromLayoutElement;
        [SerializeField]
        private bool _forceConvertFlexiableToPreferred;
        [SerializeField]
        private LayoutElementSizeSetter _elementSizeSetter;
        [SerializeField] // TODO This value should be NonSerialized but better to show it in inspector
        private int m_index = -1;

        private RectTransform m_rectTransform;

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
        public int ElementIndex => m_index;
        public Vector2 ElementPreferredSize { get; private set; }

        public void SetIndex(int index)
        {
            m_index = index;
        }

        public void ClearPreferredSize()
        {
            // HACK
            if (_forceConvertFlexiableToPreferred)
            {
                const int LAYOUT_SIZE_DISABLE = -1;
                if (Mathf.Approximately(1f, _layoutElement.flexibleHeight))
                {
                    _layoutElement.preferredHeight = LAYOUT_SIZE_DISABLE;
                }
                if (Mathf.Approximately(1f, _layoutElement.flexibleWidth))
                {
                    _layoutElement.preferredWidth = LAYOUT_SIZE_DISABLE;
                }
            }
        }

        [ContextMenu("ForceCalculateSize")]
        public void CalculatePreferredSize()
        {
            Vector2 rectSize = ElementTransform.rect.size;
            if (TryGetComponent<HorizontalOrVerticalLayoutGroup>(out HorizontalOrVerticalLayoutGroup layoutGroup))
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(ElementTransform);
                if (_forceConvertFlexiableToPreferred)
                {
                    const int LAYOUT_SIZE_DISABLE = -1;
                    // HACK IDK if this is cheap or not. If do this, remember to reset actual values when return to pool
                    if (Mathf.Approximately(1f, _layoutElement.flexibleHeight))
                    {
                        _layoutElement.preferredHeight = LAYOUT_SIZE_DISABLE;
                        layoutGroup.CalculateLayoutInputVertical();
                        layoutGroup.SetLayoutVertical();
                        rectSize.y = (float)GetTotalPreferredSizeMethod.Invoke(layoutGroup, s_boxedInt1);
                        _layoutElement.preferredHeight = rectSize.y;
                    }
                    if (Mathf.Approximately(1f, _layoutElement.flexibleWidth))
                    {
                        _layoutElement.preferredWidth = LAYOUT_SIZE_DISABLE;
                        layoutGroup.CalculateLayoutInputHorizontal();
                        layoutGroup.SetLayoutHorizontal();
                        rectSize.x = (float)GetTotalPreferredSizeMethod.Invoke(layoutGroup, s_boxedInt0);
                        _layoutElement.preferredWidth = rectSize.x;
                    }
                }
            }

            float width, height;
            if (_directlyUseSizeFromLayoutElement)
            {
                width = 1f <= _layoutElement.flexibleWidth ? rectSize.x : _layoutElement.preferredWidth;
                height = 1f <= _layoutElement.flexibleHeight ? rectSize.y : _layoutElement.preferredHeight;
                ElementPreferredSize = new Vector2(width, height);
                return;
            }

            // TODO Maybe remove this
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