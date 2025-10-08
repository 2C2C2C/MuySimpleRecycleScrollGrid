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

        public void SetObjectActive()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }

        public void SetObjectDeactive()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }


        [ContextMenu("ForceCalculateSize")]
        public void CalculatePreferredSize()
        {
            RectTransform self = transform as RectTransform;
            float width, height;

            // Width
            if (1f <= _layoutElement.flexibleWidth)
            {
                width = LayoutUtility.GetPreferredSize(self, 1);
            }
            else if (0f < _layoutElement.preferredWidth)
            {
                width = _layoutElement.preferredWidth;
            }
            else if (0f < _layoutElement.minWidth)
            {
                width = _layoutElement.minWidth;
            }
            else
            {
                width = self.rect.width;
            }

            // Height
            if (1f <= _layoutElement.flexibleHeight)
            {
                height = LayoutUtility.GetPreferredSize(self, 1);
            }
            else if (0f < _layoutElement.preferredHeight)
            {
                height = _layoutElement.preferredHeight;
            }
            else if (0f < _layoutElement.minHeight)
            {
                height = _layoutElement.minHeight;
            }
            else
            {
                height = self.rect.height;
            }

            Vector2 size = new Vector2(width, height);
            ElementPreferredSize = size;
            // Debug.LogError($"Element preferred size {size}");
        }

        private void Reset()
        {
            TryGetComponent<LayoutElement>(out _layoutElement);
        }
    }
}