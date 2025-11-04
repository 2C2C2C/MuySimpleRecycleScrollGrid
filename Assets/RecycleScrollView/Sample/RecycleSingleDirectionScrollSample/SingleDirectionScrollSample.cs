using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityRandom = UnityEngine.Random;

namespace RecycleScrollView.Sample
{
    public class SingleDirectionScrollSample : MonoBehaviour, ISingleDirectionScrollDataSource
    {
        [SerializeField]
        private RecycleSingleDirectionScroll _scrollController;
        [SerializeField]
        private RectTransform _elementPrefab;
        [SerializeField]
        private ScrollRect _scrollrect;
        [SerializeField]
        private RectTransform _content;
        [SerializeField]
        private RectTransform _viewport;
        [SerializeField]
        private int _dataCount = 50;
        [SerializeField]
        private float _sizeMin = 80;
        [SerializeField]
        private float _sizeMax = 320;

        [SerializeField]
        private int _jumpToTestIndex = 10;

        private Dictionary<int, float> m_sizeMap = new Dictionary<int, float>();

        public int DataElementCount => _dataCount;

        public RectTransform RequestElement(RectTransform parent, int index)
        {
            RectTransform newElement = RectTransform.Instantiate(_elementPrefab, parent);
            if (newElement.TryGetComponent<ChatTextElementUI>(out ChatTextElementUI chatTextElement))
            {
                if (!m_sizeMap.TryGetValue(index, out float tempSize))
                {
                    tempSize = UnityRandom.Range(_sizeMin, _sizeMax);
                    m_sizeMap[index] = tempSize;
                }
                if (_scrollController.IsHorizontal)
                {
                    chatTextElement.SetWidth(tempSize);
                }
                else if (_scrollController.IsVertical)
                {
                    chatTextElement.SetHeight(tempSize);
                }
                chatTextElement.SetText($"ee {index}");
                chatTextElement.ForceCalculateSize();
            }
            return newElement;
        }

        public void ReturnElement(RectTransform element)
        {
            element.SetParent(null);
            GameObject.Destroy(element.gameObject);
        }

        private void Start()
        {
            _scrollController.Init(this);
        }

        [ContextMenu("AddTopTest")]
        private void AddTopTest()
        {
            RectTransform newElement = RectTransform.Instantiate(_elementPrefab, _content);
            Debug.LogError($"1 {_scrollrect.verticalNormalizedPosition}");
            if (newElement.TryGetComponent<ChatTextElementUI>(out ChatTextElementUI chatTextElement))
            {
                float heightee = UnityRandom.Range(80, 560);
                chatTextElement.SetHeight(heightee);
                chatTextElement.SetText($"ee ");
                chatTextElement.ForceCalculateSize();
                newElement.SetAsFirstSibling();

                LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
                Vector2 size = newElement.rect.size;
                float delta = heightee;
                Vector2 contentSize = _content.rect.size;
                if (_scrollController.IsHorizontal)
                {
                    contentSize.x += delta;
                    Debug.LogError($"2 {_scrollrect.verticalNormalizedPosition} {size}");
                    contentSize = _content.rect.size;
                    // _content.anchoredPosition += Vector2.up * delta;
                    _scrollrect.verticalNormalizedPosition -= delta / (contentSize.x - _viewport.rect.height);
                }
                else if (_scrollController.IsVertical)
                {
                    contentSize.y += delta;
                    Debug.LogError($"2 {_scrollrect.verticalNormalizedPosition} {size}");
                    contentSize = _content.rect.size;
                    // _content.anchoredPosition += Vector2.up * delta;
                    _scrollrect.verticalNormalizedPosition -= delta / (contentSize.y - _viewport.rect.height);
                }
            }
        }

        [ContextMenu("RemoveTest")]
        private void RemoveTest()
        {
            RectTransform tempChild = _content.GetChild(0) as RectTransform;
            float heightDelta = tempChild.rect.height;
            Destroy(tempChild.gameObject);
            _content.anchoredPosition -= Vector2.up * heightDelta;
        }

        [ContextMenu(nameof(JumpToTest))]
        private void JumpToTest()
        {
            _scrollController.JumpToElementInstant(_jumpToTestIndex);
        }

        public void ChangeElementIndex(RectTransform element, int prevIndex, int nextIndex)
        {
        }
    }
}