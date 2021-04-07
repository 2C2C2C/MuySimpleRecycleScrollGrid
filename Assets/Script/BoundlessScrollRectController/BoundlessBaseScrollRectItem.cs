using UnityEngine;

// T is a data for each grid item
public abstract class BoundlessBaseScrollRectItem<T> : MonoBehaviour
{
    protected RectTransform m_rectTransform = null;

    [SerializeField]
    private CanvasGroup m_canvasGroup = null;

    public RectTransform ItemRectTransform => m_rectTransform;
    public Vector2 ItemSize { get; private set; }

    public T ItemData { get; protected set; }

    /// <summary>
    /// to inject data and cast data
    /// </summary>
    /// <param name="data"></param>
    public abstract void InjectData(T data);

    public void Show()
    {
        m_canvasGroup.alpha = 1.0f;
        m_canvasGroup.interactable = true;
        m_canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        m_canvasGroup.alpha = 0.0f;
        m_canvasGroup.interactable = false;
        m_canvasGroup.blocksRaycasts = false;
    }

    public void SetItemSize(Vector2 nextSize)
    {
        ItemSize = nextSize;
        m_rectTransform.sizeDelta = ItemSize;
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
