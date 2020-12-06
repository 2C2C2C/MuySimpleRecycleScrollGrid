using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BoundlessBaseScrollRectItem : MonoBehaviour
{
    protected RectTransform m_rectTransform = null;

    [SerializeField]
    private CanvasGroup m_canvasGroup = null;
    private Vector2 m_itemSize = default;

    public RectTransform ItemRectTransform => m_rectTransform;
    public Vector2 ItemSize => m_itemSize;

    /// <summary>
    /// to inject data and cast data
    /// </summary>
    /// <param name="data"></param>
    public abstract void InjectData(IBoundlessScrollRectItemData data);

    public void Show()
    {
        m_canvasGroup.alpha = 1.0f;
    }

    public void Hide()
    {
        m_canvasGroup.alpha = 0.0f;
    }

    public void SetItemSize(Vector2 nextSize)
    {
        m_itemSize = nextSize;
        m_rectTransform.sizeDelta = m_itemSize;
    }

    #region mono method

    private void Reset()
    {
        m_rectTransform = GetComponent<RectTransform>();
    }

    private void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
    }

    #endregion

}
