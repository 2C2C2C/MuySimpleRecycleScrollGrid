using UnityEngine;

public class PositionChecker : MonoBehaviour
{
    RectTransform m_selfRectTransfrom;

    [Header("temp values")]
    public Vector3 m_worldPosition;
    public Vector3 m_localPosition;
    public Vector3 m_anhorPosition;

    private void Awake()
    {
        m_selfRectTransfrom = GetComponent<RectTransform>();
    }

#if UNITY_EDITOR

    private void Reset()
    {
        m_selfRectTransfrom = GetComponent<RectTransform>();
    }

    private void OnDrawGizmos()
    {
        if (m_selfRectTransfrom != null)
        {
            m_worldPosition = m_selfRectTransfrom.position;
            m_localPosition = m_selfRectTransfrom.localPosition;
            m_anhorPosition = m_selfRectTransfrom.anchoredPosition;
        }
        else
            m_selfRectTransfrom = GetComponent<RectTransform>();
    }
#endif
}
