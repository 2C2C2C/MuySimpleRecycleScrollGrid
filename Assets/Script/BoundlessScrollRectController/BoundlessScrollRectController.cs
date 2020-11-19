using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class BoundlessScrollRectController : MonoBehaviour
{
    [SerializeField]
    private ScrollRect m_scrollRect = null;

    [SerializeField]
    RectTransform m_viewport = null;
    [SerializeField]
    RectTransform m_content = null;

    // do vertical first
    private int m_viewItemCount = 0;
    private bool m_isVertical = true;
    private int m_startIndex = 0;
    private const int CACHE_COUNT = 2;

    private Vector2 m_itemSize = Vector2.zero;
    private Vector2 m_itemStartPos = default; // the showing first item start pos in virwport

    List<TempGridItem> m_fakeItems = null;
    IReadOnlyList<TempDataItem> m_dataList = null;

    [Header("test shit")]
    public TempGridItem m_itemPrefab = null;

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

        m_itemStartPos.y = 1.0f;

        UpdateContentSize();
    }

    private void UpdateContentSize()
    {
        // test for vertical shit
        Vector2 size = m_itemSize;
        size.y = size.y * m_dataList.Count;
        m_content.sizeDelta = size;
    }

    private void OnScrollRectValueChanged(Vector2 position)
    {
        float contentHeight = m_dataList.Count * m_itemSize.y;
        float maxStartPosY = contentHeight - m_viewport.rect.height;

        Vector2 nextTopPos = new Vector2(0.0f, m_content.anchoredPosition.y);
        nextTopPos.y = Mathf.Clamp(nextTopPos.y, 0.0f, maxStartPosY);
        m_itemStartPos = nextTopPos;
        RefreshUIItem();
    }

    private void RefreshUIItem()
    {
        Vector2 pos = default;
        m_startIndex = (int)Mathf.FloorToInt(m_itemStartPos.y / m_itemSize.y);
        for (int i = 0; i < m_viewItemCount + 1; i++)
        {
            if (i + m_startIndex < m_dataList.Count)
                m_fakeItems[i].Setup(m_dataList[i + m_startIndex].TempName);
            m_fakeItems[i].ItemRectTransform.anchoredPosition = pos - ((i + m_startIndex) * Vector2.up * m_itemSize.y);
            //Debug.Log($"check draw pos {m_fakeItems[i].ItemRectTransform.anchoredPosition}");
        }
    }

    #region mono method

    private void Reset()
    {
        m_scrollRect.GetComponent<ScrollRect>();
        m_isVertical = m_scrollRect.vertical;
    }

    private void OnEnable()
    {
        m_scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);

        var tempShit = Instantiate(m_itemPrefab, m_content);
        m_itemSize = tempShit.ItemSize;

        // set it height of content
        float viewportHeight = Mathf.Abs(m_viewport.rect.height * m_viewport.localScale.y);
        m_viewItemCount = (int)(viewportHeight / m_itemSize.y);
        //Debug.Log($"test viewport item count {m_viewItemCount}");
        if (viewportHeight % m_itemSize.y > 0)
            m_viewItemCount++;

        if (null == m_fakeItems)
            m_fakeItems = new List<TempGridItem>();

        int spawnCount = m_viewItemCount + CACHE_COUNT;
        m_fakeItems.Add(tempShit);
        tempShit.gameObject.SetActive(false);
        for (int i = 1; i < spawnCount; i++)
        {
            tempShit = Instantiate(m_itemPrefab, m_content);
            m_fakeItems.Add(tempShit);
            tempShit.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        m_scrollRect.onValueChanged.RemoveListener(OnScrollRectValueChanged);
        DestroyAllChildren(m_content);
        m_fakeItems.Clear();
    }

    #endregion

    void DestroyAllChildren(Transform target)
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
