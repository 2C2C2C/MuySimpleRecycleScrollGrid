using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class GuidItemUI : MonoBehaviour, IListElementUI
{
    [SerializeField]
    private UnityEngine.UI.Text m_dataText = null;
    [SerializeField]
    private GameObject m_contentObject = null;
    private int m_itemIndex;
    private RectTransform m_rectTransform;

    public GuidTempData Data { get; private set; } = null;

    public int ElementIndex => m_itemIndex;

    public RectTransform ElementRectTransform => m_rectTransform;

    Action<GuidTempData> m_onPointerEnter = null;
    Action<GuidTempData> m_onPointerLeave = null;

    public void Setup(GuidTempData data)
    {
        m_dataText.text = data.ItemName;
        Data = data;
        if (!m_contentObject.activeSelf)
        {
            m_contentObject.SetActive(true);
        }
    }

    public void SetEmpty()
    {
        if (m_contentObject.activeSelf)
        {
            m_contentObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_onPointerEnter?.Invoke(Data);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_onPointerLeave?.Invoke(Data);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void SetIndex(int index)
    {
        m_itemIndex = index;
    }
}
