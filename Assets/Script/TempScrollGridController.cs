using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// test it for vertical shit
namespace HikoShit.UI
{
    [RequireComponent(typeof(TempScrollRect))]
    public class TempScrollGridController : MonoBehaviour
    {
        [SerializeField]
        private TempScrollRect m_scrollRect = null;
        [SerializeField]
        private Scrollbar m_verticalScrollBar = null;
        private RectTransform m_scrollRectTransfrom = null;
        [SerializeField]
        private RectTransform m_viewport = null;
        [SerializeField]
        private RectTransform m_realContent = null;

        private int m_viewItemCount = 0;
        private bool m_isVertical = true;
        private int m_startIndex = 0;
        private const int CACHE_COUNT = 2;

        private Vector2 m_itemSize = Vector2.zero;
        private Vector2 m_itemStartPos = default; // the showing first item start pos in virwport

        private bool m_hasInited = false;

        List<TempGridItem> m_fakeItems = null;
        IReadOnlyList<TempDataItem> m_dataList = null;

        #region test shit
        [Header("test shit")]

        public TempGridItem m_itemPrefab = null;

        #endregion

        [ContextMenu("funs")]
        public void TestSet()
        {
            m_scrollRect.normalizedPosition = Vector2.right;
        }

        public void InjectData(IReadOnlyList<TempDataItem> dataList)
        {
            m_dataList = dataList;
            // time to spawn shit

            m_startIndex = 0;

            // set default simple draw stuff
            for (int i = 0; i < m_fakeItems.Count; i++)
            {
                m_fakeItems[i].gameObject.SetActive(true);
                if (i < dataList.Count)
                    m_fakeItems[i].Setup(dataList[i].TempName);
                Vector3 pos = m_fakeItems[i].ItemRectTransform.anchoredPosition3D;
                pos -= i * m_fakeItems[i].ItemRectTransform.up * m_itemSize.y;
                m_fakeItems[i].ItemRectTransform.anchoredPosition = pos;
            }

            //
            if (null != m_verticalScrollBar)
            {
                m_verticalScrollBar.value = 0.0f;
                m_verticalScrollBar.size = 1.0f / m_dataList.Count;
            }
            m_itemStartPos.y = 1.0f;
        }

        private void InitController()
        {
            m_hasInited = true;
        }

        private void OnDeltaPosition(Vector2 deltaPos) // 0 ~ 1 for
        {
            // test vertical
            deltaPos.x = 0;
            float contentHeight = m_dataList.Count * m_itemSize.y;
            float maxStartPosY = contentHeight - m_viewport.rect.height;

            Vector2 nextTopPos = m_itemStartPos + deltaPos;
            nextTopPos.y = Mathf.Clamp(nextTopPos.y, 0.0f, maxStartPosY);
            m_itemStartPos = nextTopPos;

            if (null != m_verticalScrollBar)
                m_verticalScrollBar.SetValueWithoutNotify(m_itemStartPos.y / maxStartPosY);
            // Debug.Log($"content height {contentHeight}; max pos {maxStartPos}; next pos{m_itemStartPos}");
            RefreshUIItem();
        }

        private void OnScrollBarValueChanged(float nextValue)
        {
            float maxStartPosY = m_dataList.Count * m_itemSize.y - m_viewport.rect.height;
            float prevValue = m_itemStartPos.y / maxStartPosY;
            float deltaValue = nextValue - prevValue;
            Vector2 deltaPos = new Vector2(0.0f, maxStartPosY * deltaValue);

            Vector2 nextTopPos = m_itemStartPos + deltaPos;
            nextTopPos.y = Mathf.Clamp(nextTopPos.y, 0.0f, maxStartPosY);
            m_itemStartPos = nextTopPos;

            RefreshUIItem();
        }

        private void RefreshUIItem()
        {
            Vector2 pos = m_itemStartPos;
            pos.y = m_itemStartPos.y % m_itemSize.y;
            m_startIndex = (int)Mathf.FloorToInt(m_itemStartPos.y / m_itemSize.y);
            // Debug.Log($"check draw offset {pos}");
            for (int i = 0; i < m_viewItemCount + 1; i++)
            {
                if (i + m_startIndex < m_dataList.Count)
                    m_fakeItems[i].Setup(m_dataList[i + m_startIndex].TempName);
                m_fakeItems[i].ItemRectTransform.anchoredPosition = pos - (i * Vector2.up * m_itemSize.y);
            }
        }

        #region mono method

        private void Reset()
        {
            m_scrollRect.GetComponent<ScrollRect>();
            m_scrollRectTransfrom = m_scrollRect.transform as RectTransform;
            m_realContent = m_scrollRect.dragContent;
            m_isVertical = m_scrollRect.vertical;
        }

        private void Awake()
        {
            m_fakeItems = new List<TempGridItem>();
            DestroyAllChildren(m_realContent);
        }

        private void OnEnable()
        {
            m_scrollRect.onValueChangedDelta.AddListener(OnDeltaPosition);
            if (null != m_verticalScrollBar)
                m_verticalScrollBar.onValueChanged.AddListener(OnScrollBarValueChanged);
            // get some fake stuff to test

            var tempShit = Instantiate(m_itemPrefab, m_realContent);
            m_itemSize = tempShit.ItemSize;
            // set it height of content
            float tempHeight = Mathf.Abs(m_viewport.rect.y * m_viewport.localScale.y);
            m_viewItemCount = (int)(tempHeight / m_itemSize.y);
            Debug.Log($"test viewport item count {m_viewItemCount}");
            if (tempHeight % m_itemSize.y > 0)
                m_viewItemCount++;

            Vector2 size = m_realContent.sizeDelta;
            size.y = m_itemSize.y * (m_viewItemCount + CACHE_COUNT);
            m_realContent.sizeDelta = size;

            int spawnCount = m_viewItemCount + CACHE_COUNT;
            m_fakeItems.Add(tempShit);
            tempShit.gameObject.SetActive(false);
            for (int i = 1; i < spawnCount; i++)
            {
                tempShit = Instantiate(m_itemPrefab, m_realContent);
                m_fakeItems.Add(tempShit);
                tempShit.gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            m_scrollRect.onValueChangedDelta.RemoveListener(OnDeltaPosition);
            if (null != m_verticalScrollBar)
                m_verticalScrollBar.onValueChanged.RemoveListener(OnScrollBarValueChanged);
        }

        private void Start()
        {
            // wat should I do here
        }

        #endregion

        public static void DestroyAllChildren(Transform target)
        {
            if (null == target)
            {
                return;
            }

            int childCount = target.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Destroy(target.GetChild(i).gameObject);
            }
        }

    }
}