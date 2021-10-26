using UnityEngine;

public class BoundlessScrollRectElement : MonoBehaviour
{
    /// <summary>
    /// -1 as a invalid index
    /// </summary>
    public readonly static int INVALID_INDEX = -1;

    [SerializeField]
    private CanvasGroup m_canvasGroup = null;

    private RectTransform m_rectTransform = null;

    public RectTransform ItemRectTransform => m_rectTransform;
    public Vector2 ItemSize { get; private set; }

    /// <summary>
    /// -1 as a invalid index
    /// </summary>
    public int ItemIndex { get; protected set; } = INVALID_INDEX;

    public void Setup(int index)
    {
        ItemIndex = index;
    }

    public void Show()
    {
        if (!this.gameObject.activeSelf)
            this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (this.gameObject.activeSelf)
            this.gameObject.SetActive(false);
    }

    public void SetItemSize(Vector2 nextSize)
    {
        ItemSize = nextSize;
        m_rectTransform.sizeDelta = ItemSize;
    }

    #region mono method

    private void Awake()
    {
        m_rectTransform = (RectTransform)this.transform;
    }

    private void Reset()
    {
        m_rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// This callback is called if an associated RectTransform has its dimensions changed. The call is also made to all child rect transforms, even if the child transform itself doesn't change - as it could have, depending on its anchoring.
    /// </summary>
    private void OnRectTransformDimensionsChange() { }
    private void OnBeforeTransformParentChanged() { }
    private void OnTransformParentChanged() { }

    #endregion

}
