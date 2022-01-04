using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class GuidElementUI : MonoBehaviour, IListElementUI
{
    [SerializeField]
    private UnityEngine.UI.Text m_dataText = null;

    private int m_itemIndex;
    private RectTransform m_rectTransform;

    public GuidTempData Data { get; private set; } = null;

    public int ElementIndex => m_itemIndex;

    public RectTransform ElementRectTransform => m_rectTransform;

    Action<GuidTempData> m_onPointerEnter = null;
    Action<GuidTempData> m_onPointerLeave = null;

    public void Setup(GuidTempData data)
    {
        m_dataText.text = $"no.\n{ElementIndex}";
        Data = data;
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
        // m_dataText.text = $"no.\n{ElementIndex}";
    }

    private void Awake()
    {
        m_rectTransform = this.transform as RectTransform;
    }
}
