using UnityEngine;
using UnityRandom = UnityEngine.Random;

namespace RecycleScrollView.Sample
{
    public class SingleDirScrollSample : MonoBehaviour, ISingleDirectionListView
    {
        [SerializeField]
        private RecycleSingleDirectionScrollController _scrollController;
        [SerializeField]
        private RectTransform _elementPrefab;

        public int DataElementCount => 50;

        public RectTransform RequestElement(RectTransform parent, int index)
        {
            RectTransform newElement = RectTransform.Instantiate(_elementPrefab, parent);
            if (newElement.TryGetComponent<ChatTextElementUI>(out ChatTextElementUI chatTextElement))
            {
                chatTextElement.SetHeight(UnityRandom.Range(80, 560));
                chatTextElement.SetText($"ee {index}");
                chatTextElement.ForceCalculateSize();
            }
            return newElement;
        }

        public void ReturnElement(RectTransform element)
        {
            GameObject.Destroy(element.gameObject);
        }

        private void Start()
        {
            _scrollController.Init(this);
        }


        [SerializeField]
        private RectTransform ee;
        [ContextMenu("RemoveTest")]
        private void RemoveTest()
        {
            RectTransform tempChild = ee.GetChild(0) as RectTransform;
            float heightDelta = tempChild.rect.height;
            Destroy(tempChild.gameObject);
            ee.anchoredPosition -= Vector2.up * heightDelta;
        }

    }
}