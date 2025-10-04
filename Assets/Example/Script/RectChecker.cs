using UnityEngine;

public class RectChecker : MonoBehaviour
{
    [SerializeField]
    RectTransform m_rectTransform;

    [SerializeField]
    bool m_runTimeCheck = false;

    [ContextMenu("Check")]
    public void Check()
    {
        Rect rect = m_rectTransform.rect;
        Debug.Log($"{rect} min_{rect.min} max_{rect.max}");
    }

    private void Awake()
    {
        if (m_rectTransform == null)
            m_rectTransform = transform as RectTransform;
    }

    private void Reset()
    {
        if (m_rectTransform == null)
            m_rectTransform = transform as RectTransform;
    }

    private void Update()
    {
        if (m_runTimeCheck)
        {
            Check();
            m_runTimeCheck = false;
        }
    }
}
