using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HikoShit.UI;
using UnityEngine;

public class TempTester : MonoBehaviour
{
    [Range(4, 20)]
    public int m_dataCount = 10;
    private TempDataItem[] m_dataArr = null;

    public TempScrollGridController m_gridConrtoller = null;
    ReadOnlyCollection<TempDataItem> m_itemList = null;

    public string[] m_dataNames = null;

    // Start is called before the first frame update
    void Start()
    {
        // create data

        m_dataArr = new TempDataItem[m_dataCount];
        m_dataNames = new string[m_dataCount];
        for (int i = 0; i < m_dataCount; i++)
        {
            m_dataArr[i] = new TempDataItem();
            m_dataNames[i] = m_dataArr[i].TempName;
        }

        // then give data or?

        m_itemList = new ReadOnlyCollection<TempDataItem>(m_dataArr);
        m_gridConrtoller.InjectData(m_itemList);

        this.enabled = false;
    }


}
