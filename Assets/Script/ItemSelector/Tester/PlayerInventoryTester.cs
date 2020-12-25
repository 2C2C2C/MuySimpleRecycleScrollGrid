using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Temp.ItemSelectorTest
{
    public class PlayerInventoryTester : MonoBehaviour
    {
        [SerializeField]
        PlayerInventoryUI m_inventoryUI = null;
        [SerializeField]
        Sprite[] m_itemIcons = null;
        [SerializeField]
        string[] m_itemDesc = null;
        [SerializeField, Range(1, 9)]
        int m_dataCount = 1;

        List<PlayerInventoryStackData> m_testDataList = null;

        public void DoTest()
        {
            GenerateTestData();
            m_inventoryUI.Setup(m_testDataList);
        }

        void GenerateTestData()
        {
            m_testDataList.Clear();
            for (int i = 0; i < m_dataCount; i++)
            {
                System.Guid itemGuid = System.Guid.NewGuid();
                PlayerItemType itemType = (PlayerItemType)UnityEngine.Random.Range(0, (int)PlayerItemType.Resource + 1);
                int iconIndex = UnityEngine.Random.Range(0, m_itemIcons.Length);
                int descIndex = UnityEngine.Random.Range(0, m_itemDesc.Length);
                PlayerInventoryStackData data = new PlayerInventoryStackData(itemGuid, Random.Range(1, 89), itemType, m_itemIcons[iconIndex], m_itemDesc[descIndex]);
                m_testDataList.Add(data);
            }
        }

        void Start()
        {
            m_testDataList = new List<PlayerInventoryStackData>();
            DoTest();
        }

    }
}