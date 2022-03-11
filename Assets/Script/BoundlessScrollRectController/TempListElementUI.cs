using UnityEngine;

public class TempListElementUI : MonoBehaviour
{
    [Header("must have"), Tooltip("should inherit from ISetupable")]
    [SerializeField]
    Component m_dataReceiver;
    [SerializeField, Tooltip("better to manual drag it in")]
    RectTransform m_elementTransform;
    [SerializeField, ReadOnly]
    private int m_index = -1;
    public int ElementIndex => m_index;
    public RectTransform ElementRectTransform
    {
        get
        {
            if (m_elementTransform == null)
                m_elementTransform = GetComponent<RectTransform>();
            return m_elementTransform;
        }
    }
    
    public void Setup<TData>(TData data)
    {
        SomeUtils.ISetup<Component, TData>(m_dataReceiver, data);
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

    public void SetIndex(int index)
    {
        m_index = index;
    }

    private void Awake()
    {
        if (m_elementTransform == null)
            m_elementTransform = this.transform as RectTransform;
    }

#if UNITY_EDITOR
    private void Reset()
    {
        m_elementTransform = this.transform as RectTransform;
    }
#endif
}
