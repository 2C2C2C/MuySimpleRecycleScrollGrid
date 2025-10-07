using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace RecycleScrollView.Sample
{
    [RequireComponent(typeof(LayoutElement))]
    public class ChatTextElementUI : MonoBehaviour
    {
        [SerializeField]
        private LayoutElement _layoutElement;
        [SerializeField]
        private Text _tempText;

        public void SetText(string content)
        {
            _tempText.text = content;
        }

        [ContextMenu("ForceCalculateSize")]
        public Vector2 ForceCalculateSize()
        {
            RectTransform self = transform as RectTransform;
            // float tempHeight = LayoutUtility.GetFlexibleHeight(self);
            if (1f <= _layoutElement.flexibleHeight)
            {
                float preferredHeight = LayoutUtility.GetPreferredSize(self, 1);
                Debug.LogError($"element prefer height {preferredHeight}");
            }
            // Debug.LogError($"text prefer height {_tempText.preferredHeight}");
            return default;
        }

        public void SetHeight(float height)
        {
            _layoutElement.preferredHeight = height;
        }

        private void HandleSelfFittingAlongAxis(int axis)
        {
            RectTransform rectTransform = transform as RectTransform;
            float preferredHeight = LayoutUtility.GetPreferredSize(rectTransform, axis);
        }

    }
}