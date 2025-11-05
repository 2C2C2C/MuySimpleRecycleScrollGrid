using UnityEngine;

namespace RecycleScrollView
{
    public class RecycleScrollGridElement : MonoBehaviour
    {
        [Header("must have"), Tooltip("should inherit from ISetupable")]
        [SerializeField]
        Component _dataReceiver;
        [SerializeField, Tooltip("better to manual drag it in")]
        RectTransform _elementTransform;

        [SerializeField]
        private int m_index = -1; // This value should be NonSerialized but better to show it in inspector

        public int ElementIndex => m_index;

        public RectTransform ElementTransform
        {
            get
            {
                if (TryGetComponent<RectTransform>(out _elementTransform))
                {
                    return _elementTransform;
                }
                return null;
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
            if (null == _elementTransform)
            {
                _elementTransform = this.transform as RectTransform;
            }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _elementTransform = this.transform as RectTransform;
        }
#endif
    }
}