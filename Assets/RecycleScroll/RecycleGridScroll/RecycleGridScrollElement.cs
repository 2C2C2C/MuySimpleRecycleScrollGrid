using UnityEngine;

namespace RecycleScrollView
{
    public class RecycleGridScrollElement : MonoBehaviour
    {
        [SerializeField]
        private int m_index = -1; // This value should be NonSerialized but better to show it in inspector
        private RectTransform m_elementTransform;

        public int ElementIndex => m_index;

        public RectTransform ElementTransform
        {
            get
            {
                if (null == m_elementTransform)
                {
                    m_elementTransform = this.transform as RectTransform;
                }
                return m_elementTransform;
            }
        }

        public void SetElementSize(Vector2 size)
        {
            RectTransform rectTransform = ElementTransform;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
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

        public void SetIndex(int index)
        {
            if (index == m_index)
            {
                return;
            }
            m_index = index;
        }

        private void Awake()
        {
            if (null == m_elementTransform)
            {
                m_elementTransform = this.transform as RectTransform;
            }
        }

#if UNITY_EDITOR

        private void Reset()
        {
            m_elementTransform = this.transform as RectTransform;
        }

#endif

    }
}