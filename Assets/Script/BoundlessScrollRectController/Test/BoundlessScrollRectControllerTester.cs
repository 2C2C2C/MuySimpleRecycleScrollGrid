using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class BoundlessScrollRectControllerTester : MonoBehaviour
{
    [Range(4, 800)]
    public int m_dataCount = 10;
    private BoundlessTempData[] m_dataArr = null;

    public BoundlessTempScrollRectController m_gridConrtoller = null;
    ReadOnlyCollection<BoundlessTempData> m_itemList = null;

    public string[] m_dataNames = null;

    // Start is called before the first frame update
    void Start()
    {

        m_dataArr = new BoundlessTempData[m_dataCount];
        m_dataNames = new string[m_dataCount];
        for (int i = 0; i < m_dataCount; i++)
        {
            m_dataArr[i] = new BoundlessTempData();
            m_dataNames[i] = m_dataArr[i].ItemName;
        }

        // then give data or?

        m_itemList = new ReadOnlyCollection<BoundlessTempData>(new List<BoundlessTempData>(m_dataArr));
        m_gridConrtoller.InjectData(m_itemList);

        this.enabled = false;
    }
}
