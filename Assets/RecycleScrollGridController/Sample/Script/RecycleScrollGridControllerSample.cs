using System.Collections.Generic;
using UnityEngine;

public class RecycleScrollGridControllerSample : MonoBehaviour
{
    [Range(4, 800)]
    public int m_dataCount = 10;
    private GuidTempData[] m_dataArr = null;

    public GuidElementListUI m_gridListUI = null;

    public string[] m_dataNames = null;

    [ContextMenu("setup data")]
    private void SetupData()
    {
        m_dataArr = new GuidTempData[m_dataCount];
        m_dataNames = new string[m_dataCount];
        for (int i = 0; i < m_dataCount; i++)
        {
            m_dataArr[i] = new GuidTempData();
            m_dataNames[i] = m_dataArr[i].ItemName;
        }

        m_gridListUI.Setup(new List<GuidTempData>(m_dataArr));
    }

    // Start is called before the first frame update
    private void Start()
    {
        SetupData();
    }
}
