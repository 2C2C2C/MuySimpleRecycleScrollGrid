using UnityEngine;

namespace RecycleScrollGrid
{
    public class RecycleScrollGridElement : MonoBehaviour
    {
        [Header("must have"), Tooltip("should inherit from ISetupable")]
        [SerializeField]
        Component _dataReceiver;
        [SerializeField, Tooltip("better to manual drag it in")]
        RectTransform _elementTransform;

        [SerializeField, ReadOnly]
        private int m_index = -1;

        public int ElementIndex => m_index;
        public bool NeedRefreshData { get; private set; } = false;

        public RectTransform ElementRectTransform
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

        public void Setup<TData>(TData data)
        {
            SomeUtils.ISetup<Component, TData>(_dataReceiver, data);
        }

        public void SetObjectActive()
        {
            if (!this.gameObject.activeSelf)
            {
                this.gameObject.SetActive(true);
            }
        }

        public void SetObjectDeactive()
        {
            if (this.gameObject.activeSelf)
            {
                this.gameObject.SetActive(false);
            }
        }

        public void SetIndex(int index)
        {
            if (index == m_index)
            {
                return;
            }
            m_index = index;
            NeedRefreshData = true;
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