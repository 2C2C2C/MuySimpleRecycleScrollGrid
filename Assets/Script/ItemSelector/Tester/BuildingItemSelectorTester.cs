using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Temp.ItemSelectorTest
{
    public class BuildingItemSelectorTester : MonoBehaviour
    {
        [SerializeField]
        BuildingItemSelector m_selector = null;

        [SerializeField, Range(1, 9)]
        int m_randomBuildingCount = 1;
        [SerializeField]
        Sprite[] m_buildingIcons = null;

        List<BuildingData> m_tempBuildingData = null;

        public void SetupUI()
        {
            GenerateTestData();
            m_selector.SetupBuildings(m_tempBuildingData);
        }

        void GenerateTestData()
        {
            m_tempBuildingData.Clear();
            m_tempBuildingData = new List<BuildingData>();
            for (int i = 0; i < m_randomBuildingCount; i++)
            {
                System.Guid buildingGuid = System.Guid.NewGuid();
                BuildingData data = new BuildingData(buildingGuid, m_buildingIcons[Random.Range(0, m_buildingIcons.Length)], buildingGuid.ToString().Substring(0, 10));
                m_tempBuildingData.Add(data);
            }
        }

        void Start()
        {
            m_tempBuildingData = new List<BuildingData>();
            SetupUI();
        }
    }
}