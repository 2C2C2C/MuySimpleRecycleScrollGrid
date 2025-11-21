using UnityEngine;
using UnityEngine.UI;

namespace RecycleScrollView.Sample
{
    [RequireComponent(typeof(LayoutElement))]
    public class TextElementUI : MonoBehaviour
    {
        [SerializeField]
        private LayoutElement _layoutElement;
        [SerializeField]
        private Text _tempText;

        public void SetText(string content)
        {
            _tempText.text = content;
        }

        public void SetHeight(float height)
        {
            _layoutElement.preferredHeight = height;
        }

        public void SetWidth(float height)
        {
            _layoutElement.preferredWidth = height;
        }

    }
}