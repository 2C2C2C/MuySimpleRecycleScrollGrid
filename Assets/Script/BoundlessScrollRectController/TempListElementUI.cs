using UnityEngine;

public class TempListElementUI : MonoBehaviour
{
    // [SerializeField]
    // IElementSetup<T> m_dataReceiver;
    [Header("must have")]
    [SerializeField]
    RectTransform m_elementTransform;
    private int m_index = -1;
    public int ElementIndex => m_index;
    public RectTransform ElementRectTransform => m_elementTransform;

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

    public void SetIndex(int index)
    {
        m_index = index;
    }

    public void SetupData<TData>(TData data)
    {
        // if (m_dataReceiver == null && !this.TryGetComponent<IElementSetup>(out m_dataReceiver))
        //     return;
        // m_dataReceiver.Setup<TData>(data);
    }

    void OnEnable()
    {
        // m_dataReceiver = this.GetComponent<IElementSetup>();
    }
}
