using UnityEngine;

namespace RecycleScrollView.Sample
{
    public class GuidElementUI : MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.UI.Text m_dataText = null;

        private RectTransform m_rectTransform;

        public GuidElementData Data { get; private set; } = null;
        public RectTransform ElementRectTransform => m_rectTransform != null ? m_rectTransform : m_rectTransform = transform as RectTransform;

        public void Setup(GuidElementData data)
        {
            Data = data;
            m_dataText.text = Data.ItemName;
        }

        public void Clear()
        {
            Data = null;
            m_dataText.text = $"EMPTY";
        }

        private void Awake()
        {
            m_rectTransform = this.transform as RectTransform;
        }

    }
}