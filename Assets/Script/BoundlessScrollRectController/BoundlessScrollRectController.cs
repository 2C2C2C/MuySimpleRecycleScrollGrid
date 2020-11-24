
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect), typeof(GridLayoutGroup))]
public class BoundlessScrollRectController : MonoBehaviour
{
    [SerializeField]
    private ScrollRect m_scrollRect = null;

    [SerializeField]
    private RectTransform m_viewport = null;
    [SerializeField, Tooltip("the content that used to drag")]
    private RectTransform m_dragContent = null;
    [SerializeField, Tooltip("another content hold UI elements")]
    private RectTransform m_actualContent = null;

    // do vertical first
    private int m_viewItemCount = 0;
    private int m_viewItemCountInRow = 0;
    private int m_viewItemCountInColumn = 0;

    private bool m_isVertical = true;
    private int m_startIndex = 0;
    private const int CACHE_COUNT = 2;

    private Vector2 m_itemSize = Vector2.zero;
    private Vector2 m_itemStartPos = default; // the showing first item start pos in virwport
    private Vector2 m_actualContentSize = default;

    private List<TempGridItem> m_uiItems = null;
    private IReadOnlyList<TempDataItem> m_dataList = null;

    /* a test component, we will move this component
    * and use this to setup the grid size
    */
    [SerializeField, Tooltip("must have fixed row/column")]
    private GridLayoutGroup m_gridLayoutGroup = null;

    [Header("test shit")]
    public TempGridItem m_itemPrefab = null;

    public void InjectData(IReadOnlyList<TempDataItem> dataList)
    {
        m_dataList = dataList;
        m_startIndex = 0;

        m_actualContent.anchorMax = m_viewport.anchorMax;
        m_actualContent.anchorMin = m_viewport.anchorMin;
        m_actualContent.pivot = m_viewport.pivot;

        m_actualContent.localPosition = m_viewport.localPosition;
        m_actualContent.anchoredPosition = m_viewport.anchoredPosition;
        m_actualContent.sizeDelta = m_viewport.sizeDelta;

        // set default simple draw stuff
        for (int i = 0; i < m_uiItems.Count; i++)
        {
            m_uiItems[i].gameObject.SetActive(true);
            if (i < dataList.Count)
                m_uiItems[i].Setup(dataList[i].TempName);
            Vector3 pos = m_uiItems[i].ItemRectTransform.anchoredPosition3D;
            pos -= i * m_uiItems[i].ItemRectTransform.up * m_itemSize.y;
            m_uiItems[i].ItemRectTransform.anchoredPosition = pos;
        }

        m_itemStartPos.y = 1.0f;
        SyncSize();
        UpdateAcutalContentSize();
        OnScrollRectValueChanged(Vector2.zero);
    }

    private void UpdateAcutalContentSize()
    {
        Vector2 result = default;
        Vector2 cellSize = m_itemSize;
        RectOffset padding = m_gridLayoutGroup.padding;
        Vector2 spacing = m_gridLayoutGroup.spacing;
        int dataCount = m_dataList.Count;

        // TODO @Hiko when calaulate size, should also deal with padding
        int constraintCount = m_gridLayoutGroup.constraintCount;
        int dynamicCount = (dataCount % constraintCount > 0) ? (dataCount / constraintCount) + 1 : (dataCount / constraintCount);
        if (m_gridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
        {
            result.x = (constraintCount * m_itemSize.x) + ((constraintCount - 1) * spacing.x);
            result.y = dynamicCount * m_itemSize.y + (dynamicCount - 1) * spacing.y;
        }
        else if (m_gridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedRowCount)
        {
            result.y = (constraintCount * m_itemSize.y) + ((constraintCount - 1) * spacing.y);
            result.x = dynamicCount * m_itemSize.x + (dynamicCount - 1) * spacing.x;
        }

        m_actualContentSize = result;
        m_dragContent.sizeDelta = m_actualContentSize;
    }

    private void OnScrollRectValueChanged(Vector2 position)
    {
        RefreshItemStartPosition();
        //RefreshUIItemPosition();
        RefreshContent();
    }

    private void RefreshItemStartPosition()
    {
        // to get content anchor position correctly
        Vector2 spacing = m_gridLayoutGroup.spacing;
        float contentHeight = m_dataList.Count * m_itemSize.y + ((m_dataList.Count - 1) * spacing.y);
        float contentWidth = m_dataList.Count * m_itemSize.x + ((m_dataList.Count - 1) * spacing.x);
        float maxStartPosY = contentHeight - m_viewport.rect.height;
        float maxStartPosX = contentWidth - m_viewport.rect.width;

        Vector2 nextTopPos = new Vector2(m_dragContent.anchoredPosition.x, m_dragContent.anchoredPosition.y);
        nextTopPos.x = Mathf.Clamp(nextTopPos.x, 0.0f, maxStartPosX);
        nextTopPos.y = Mathf.Clamp(nextTopPos.y, 0.0f, maxStartPosY);
        m_itemStartPos = nextTopPos;
    }

    private void RefreshUIItemPosition()
    {
        Vector2 spacing = m_gridLayoutGroup.spacing;
        Vector2 pos = Vector2.zero;

        // should we move actual content a bit to...
        m_startIndex = (int)Mathf.FloorToInt(m_itemStartPos.y / (spacing.y + m_itemSize.y));
        float ytest = m_itemStartPos.y;
        spacing.x = 0.0f;
        Vector2 testOffset = Vector2.zero;
        //testOffset.y = ytest % (m_itemSize.y + spacing.y);
        if (m_scrollRect.movementType == ScrollRect.MovementType.Clamped)
        {
            testOffset.y = m_itemStartPos.y - m_startIndex * (m_itemSize.y + spacing.y);
        }
        else //if (m_scrollRect.movementType == ScrollRect.MovementType.Elastic)
        {
            testOffset.y = m_dragContent.anchoredPosition.y - m_startIndex * (m_itemSize.y + spacing.y);
        }

        for (int i = 0; i < m_viewItemCount + 2; i++)
        {
            if (i > m_viewItemCount || i + m_startIndex >= m_dataList.Count)
            {
                m_uiItems[i].Hide();
                m_uiItems[i].ItemRectTransform.anchoredPosition = pos - ((m_viewItemCount + 1) * Vector2.up * m_itemSize.y);
            }
            else if (i + m_startIndex < m_dataList.Count)
            {
                m_uiItems[i].Setup(m_dataList[i + m_startIndex].TempName);
                m_uiItems[i].ItemRectTransform.anchoredPosition = pos - (i * Vector2.up * m_itemSize.y) - (i * spacing);
            }
        }

        m_actualContent.anchoredPosition = testOffset;
    }

    private void RefreshContent()
    {
        // TODO @Hiko need to calculate actual 
        CalculateViewportShowCount();
        Vector2 spacing = m_gridLayoutGroup.spacing;
        Vector2 contentPos = m_dragContent.anchoredPosition;
        Vector2 testOffset = Vector2.zero;

        int startXIndex = (int)Mathf.FloorToInt(m_itemStartPos.x / (spacing.x + m_itemSize.x));
        int startYIndex = (int)Mathf.FloorToInt(m_itemStartPos.y / (spacing.y + m_itemSize.y));

        //Debug.Log($"test show start: x{startXIndex}; y{startYIndex }");
        //Debug.Log($"test show count: row-{m_viewItemCountInColumn}; column-{m_viewItemCountInRow}");

        if (m_gridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
        {
            // draw horizontal first
            int constraintCount = m_gridLayoutGroup.constraintCount;
            int dynamicCount = (m_viewItemCount + 2 * constraintCount);
            dynamicCount = (dynamicCount / constraintCount);
            dynamicCount = (dynamicCount % constraintCount) > 0 ? dynamicCount + 1 : dynamicCount;
            int uiItemIndex = 0;
            int startIndex = startYIndex * constraintCount + startXIndex;

            // row 
            for (int i = 0; i < m_viewItemCountInColumn; i++)
            {
                // column
                for (int j = 0; j < m_viewItemCountInRow + 1; j++)
                {
                    Vector2 anchorPosition = default;
                    int dataIndex = startIndex + j + i * constraintCount;
                    if (uiItemIndex < m_uiItems.Count)
                    {
                        if (j >= m_viewItemCountInRow - 1 || dataIndex >= m_dataList.Count)
                        {
                            // hide it
                            m_uiItems[uiItemIndex].Hide();
                        }
                        else
                        {
                            // show it
                            m_uiItems[uiItemIndex].Setup(m_dataList[dataIndex].TempName);
                            anchorPosition = (j * Vector2.right * m_itemSize.x) + (j * spacing.x) * Vector2.right;
                            anchorPosition += -(i * Vector2.up * m_itemSize.y) - (i * spacing.y) * Vector2.up;

                            m_uiItems[uiItemIndex].ItemRectTransform.anchoredPosition = anchorPosition;
                        }

                        uiItemIndex++;
                    }
                }
            }

            while (uiItemIndex < m_uiItems.Count)
            {
                m_uiItems[uiItemIndex++].Hide();
            }

            testOffset.x = m_dragContent.anchoredPosition.x - startXIndex * (m_itemSize.x + spacing.x);
            testOffset.y = m_dragContent.anchoredPosition.y - startYIndex * (m_itemSize.y + spacing.y);

            m_actualContent.anchoredPosition = testOffset;

        }
        //else if (m_gridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedRowCount)
        //{
        //    // draw vertical first
        //}

    }

    private void CalculateViewportShowCount()
    {
        m_viewItemCountInRow = 0;
        m_viewItemCountInColumn = 0;

        // set it height of content
        Vector2 spacing = m_gridLayoutGroup.spacing;
        float viewportHeight = Mathf.Abs(m_viewport.rect.height * m_viewport.localScale.y);
        float viewportWidth = Mathf.Abs(m_viewport.rect.height * m_viewport.localScale.y);
        m_viewItemCountInColumn = (int)(viewportHeight / (m_itemSize.y + spacing.y));
        m_viewItemCountInRow = (int)(viewportWidth / (m_itemSize.x + spacing.x));

        m_viewItemCountInColumn++;
        if (viewportHeight % (m_itemSize.y + spacing.y) > 0)
            m_viewItemCountInColumn++;

        m_viewItemCountInRow++;
        if (viewportWidth % (m_itemSize.x + spacing.x) > 0)
            m_viewItemCountInRow++;
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

        var tempShit = Instantiate(m_itemPrefab, m_actualContent);
        m_itemSize = tempShit.ItemSize;

        // set it height of content
        float viewportHeight = Mathf.Abs(m_viewport.rect.height * m_viewport.localScale.y);
        m_viewItemCount = (int)(viewportHeight / m_itemSize.y);

        if (viewportHeight % m_itemSize.y > 0)
            m_viewItemCount++;

        CalculateViewportShowCount();

        m_viewItemCount = m_viewItemCountInRow * m_viewItemCountInColumn;

        if (null == m_uiItems)
            m_uiItems = new List<TempGridItem>();

        int spawnCount = m_viewItemCount + CACHE_COUNT;
        m_uiItems.Add(tempShit);
        tempShit.gameObject.SetActive(false);
        for (int i = 1; i < spawnCount; i++)
        {
            tempShit = Instantiate(m_itemPrefab, m_actualContent);
            m_uiItems.Add(tempShit);
            tempShit.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        m_scrollRect.onValueChanged.RemoveListener(OnScrollRectValueChanged);
        DestroyAllChildren(m_actualContent);
        m_uiItems.Clear();
    }

    #endregion

    private void DestroyAllChildren(Transform target)
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

    private void SyncSize()
    {
        // sync the size form grid component to actual content size
        m_itemSize = m_gridLayoutGroup.cellSize;
        for (int i = 0; i < m_uiItems.Count; i++)
        {
            m_uiItems[i].SetSize(m_itemSize);
        }
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        DrawContentSize();
    }

    private void DrawContentSize()
    {
        if (null == m_dataList)
        {
            return;
        }

        Vector2 actualContentSize = m_actualContentSize;

        Vector3 topLeftPoint = m_dragContent.position;
        Vector3 topRightPoint = topLeftPoint;
        topRightPoint.x += actualContentSize.x;

        Vector3 bottomLeftPoint = topLeftPoint;
        Vector3 BottomRightPoint = topRightPoint;
        bottomLeftPoint.y -= actualContentSize.y;
        BottomRightPoint.y -= actualContentSize.y;

        Debug.DrawLine(topLeftPoint, topRightPoint, Color.magenta);
        Debug.DrawLine(topLeftPoint, bottomLeftPoint, Color.magenta);
        Debug.DrawLine(topRightPoint, BottomRightPoint, Color.magenta);
        Debug.DrawLine(bottomLeftPoint, BottomRightPoint, Color.magenta);
    }

#endif

}
