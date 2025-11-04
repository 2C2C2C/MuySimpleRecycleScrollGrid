using UnityEngine;
using UnityEngine.UI;

namespace RecycleScrollView.Sample
{
    [RequireComponent(typeof(LayoutElement))]
    public class ChatElementUI : MonoBehaviour
    {
        [SerializeField]
        private Text _mainText;
        [SerializeField]
        private GameObject _quoteGroup;
        [SerializeField]
        private Text _quoteText;

        public void Clear()
        {
            _mainText.text =
            _quoteText.text = string.Empty;
            _quoteGroup.SetActive(false);
        }

        public void SetText(string content, string quoteContent = null)
        {
            _mainText.text = content;
            _quoteText.text = quoteContent;
            _quoteGroup.SetActive(!string.IsNullOrEmpty(quoteContent));
        }

    }
}