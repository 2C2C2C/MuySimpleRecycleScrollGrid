using UnityEngine;

public class ToxicStuff : MonoBehaviour
{
    public RectTransform m_rectTransform;

    public RectTransform m_target;

    public Vector2 m_testPivot = Vector2.one * 0.5f;

    public bool m_testButton = false;

    private void PrintPosition()
    {
        if (m_rectTransform != null)
        {
            string msg = string.Empty;
            msg = $"pos1 {m_rectTransform.position}, center pos from rect {m_rectTransform.rect.center}";
        }
    }

    private void CheckRect()
    {
        Rect rect = m_rectTransform.rect;
        string msg = $"test {m_rectTransform.position}";
        Debug.Log(msg);
        Debug.Log($"result {m_rectTransform.rect.Contains(m_target.rect)}");
    }

    private void Update()
    {
        if (m_testButton)
        {
            PrintPosition();
            CheckRect();
            m_testButton = false;
        }
    }

}
