using System;
using System.Collections;
using System.Collections.Generic;
using Temp.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Temp.ItemSelectorTest
{
    public class BuildingItemSelector : ItemSelectorBase<SelectableBuildingItemUI>
    {
        [SerializeField]
        Image m_selectedBuildingIMG = null;
        [SerializeField]
        Text m_selectedBuildingNameText = null;

        List<BuildingData> m_buildingDataList = null;

        public void SetupBuildings(List<BuildingData> buildings)
        {
            if (m_selectableItemList != null)
            {
                for (int i = 0; i < m_selectableItemList.Count; i++)
                {
                    m_selectableItemList[i].ClearCallbacks();
                    Destroy(m_selectableItemList[i].gameObject);
                }
                m_selectableItemList.Clear();
            }

            m_buildingDataList = new List<BuildingData>(buildings);
            SelectableBuildingItemUI item = null;
            for (int i = 0; i < m_buildingDataList.Count; i++)
            {
                item = Instantiate(m_itemPrefab, m_contentParent);
                item.SetupHoverCallback(OnItemHovered, OnItemHoverLeaved);
                item.SetupItemIndex(i);
                item.InjectData(m_buildingDataList[i].BuildingIcon, m_buildingDataList[i].BuildingGuid.ToString().Substring(0, 5));
                m_selectableItemList.Add(item);
            }
        }

        void OnItemClicked(int clickedItemIndex)
        {
            if (clickedItemIndex > m_selectableItemList.Count - 1 || clickedItemIndex < 0)
            {
                Debug.LogError($" index {clickedItemIndex} out of range");
            }
            else
            {
                Debug.Log($"{ m_selectableItemList[clickedItemIndex].gameObject.name} clicked");
                m_selectableItemList[clickedItemIndex].SetClicked();
            }
        }

        void OnItemHovered(int hoverdItemIndex)
        {
            if (hoverdItemIndex > m_selectableItemList.Count - 1 || hoverdItemIndex < 0)
            {
                Debug.LogError($" index {hoverdItemIndex} out of range");
            }
            else
            {
                // Debug.Log($"{ m_selectableItemList[hoverdItemIndex].gameObject.name} hovered");
                m_selectableItemList[hoverdItemIndex].SetHover();
                CurrentSelectedItemIndex = hoverdItemIndex;
            }
            UpdateSelectedBuildingInfo();
        }

        void OnItemHoverLeaved(int hoverdItemIndex)
        {
            if (hoverdItemIndex > m_selectableItemList.Count - 1 || hoverdItemIndex < 0)
            {
                Debug.LogError($" index {hoverdItemIndex} out of range");
            }
            else
            {
                // Debug.Log($"{ m_selectableItemList[hoverdItemIndex].gameObject.name} leved");
                m_selectableItemList[hoverdItemIndex].SetLeave();
                CurrentSelectedItemIndex = NON_EXIST_INDEX;
            }
            UpdateSelectedBuildingInfo();
        }

        void UpdateSelectedBuildingInfo()
        {
            if (CurrentSelectedItemIndex == NON_EXIST_INDEX || CurrentSelectedItemIndex > m_buildingDataList.Count - 1)
            {
                m_selectedBuildingIMG.enabled = false;
                m_selectedBuildingNameText.text = string.Empty;
                return;
            }

            BuildingData selectedData = m_buildingDataList[CurrentSelectedItemIndex];
            m_selectedBuildingIMG.sprite = selectedData.BuildingIcon;
            m_selectedBuildingNameText.text = selectedData.BuidlingDescription;
            m_selectedBuildingIMG.enabled = true;
            // m_selectedBuildingNameText.text = selectedData.BuildingGuid.ToString().Substring(0, 10);
        }

        void OnEnable()
        {
            UpdateSelectedBuildingInfo();
        }

    }
}