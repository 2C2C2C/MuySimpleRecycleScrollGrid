using System;
using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteAlways]
public class GuidElementUI : MonoBehaviour, ISetupable<GuidTempData>
{
    [SerializeField]
    private UnityEngine.UI.Text m_dataText = null;

    private RectTransform m_rectTransform;

    public GuidTempData Data { get; private set; } = null;

    public RectTransform ElementRectTransform => (m_rectTransform != null ? m_rectTransform : m_rectTransform = transform as RectTransform);

    Action<GuidTempData> m_onPointerEnter = null;
    Action<GuidTempData> m_onPointerLeave = null;

    public void Setup(GuidTempData data)
    {
        Data = data;
        m_dataText.text = $"no.\n{Data.ItemName}";
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

    private void Awake()
    {
        m_rectTransform = this.transform as RectTransform;
    }

}
