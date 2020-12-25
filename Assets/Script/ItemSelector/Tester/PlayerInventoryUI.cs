using System;
using System.Collections;
using System.Collections.Generic;
using Temp.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Temp.ItemSelectorTest
{
    public class PlayerInventoryUI : ItemSelectorBase<PlayerInventoryItem>
    {

        [SerializeField, Header("selected grouop")]
        Image m_selectedItemIcon = null;
        [SerializeField]
        Text m_selectedItemName = null;
        [SerializeField]
        Text m_selectedItemTypeText = null;
        [SerializeField]
        Text m_selectedItemDescStr = null;

        [SerializeField, Header("tip grouop")]
        CanvasGroup m_tipGroup = null;
        [SerializeField]
        Image m_hoveredItemIcon = null;
        [SerializeField]
        Text m_hoveredItemCountText = null;
        [SerializeField]
        Text m_hoveredItemNameText = null;

        List<PlayerInventoryStackData> m_itemDataList = null;

        public void Setup(List<PlayerInventoryStackData> itemDatas)
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

            m_itemDataList = new List<PlayerInventoryStackData>(itemDatas);
            PlayerInventoryItem item = null;
            for (int i = 0; i < m_itemDataList.Count; i++)
            {
                item = Instantiate(m_itemPrefab, m_contentParent);
                item.SetupClickCallback(OnItemClicked);
                item.SetupHoverCallback(OnItemHovered, OnItemHoverLeaved);
                item.SetupItemIndex(i);
                item.InjectData(m_itemDataList[i].ItemIcon, m_itemDataList[i].ItemCount);
                m_selectableItemList.Add(item);
            }
        }

        void OnItemClicked(int clickedItemIndex)
        {
            if (CurrentSelectedItemIndex == clickedItemIndex)
                return;

            if (CurrentSelectedItemIndex != NON_EXIST_INDEX)
            {
                CurrentSelectedItem.SetSelected(false);
                ClearShowSelectedItem();
            }

            if (clickedItemIndex != NON_EXIST_INDEX)
            {
                CurrentSelectedItemIndex = clickedItemIndex;
                SetDataFromSelectedItem(m_itemDataList[CurrentSelectedItemIndex]);
                CurrentSelectedItem.SetSelected(true);
            }
        }

        void OnItemHovered(int hoveredItemIndex)
        {
            m_tipGroup.alpha = 1.0f;
            CurrentHorveredItemIndex = hoveredItemIndex;
            SetDataToTip(m_itemDataList[CurrentHorveredItemIndex]);
        }

        void OnItemHoverLeaved(int hoveredItemIndex)
        {
            m_tipGroup.alpha = 0.0f;
            CurrentHorveredItemIndex = NON_EXIST_INDEX;
        }

        void SetDataToTip(PlayerInventoryStackData data)
        {
            m_hoveredItemIcon.sprite = data.ItemIcon;
            m_hoveredItemCountText.text = data.ItemCount.ToString();
            m_hoveredItemNameText.text = data.ItemType.ToString();
        }

        void SetDataFromSelectedItem(PlayerInventoryStackData data)
        {
            m_selectedItemIcon.sprite = data.ItemIcon;
            m_selectedItemName.text = data.StackGuid.ToString().Substring(0, 5);
            m_selectedItemTypeText.text = data.ItemType.ToString();
            m_selectedItemDescStr.text = data.ItemDescription;
            m_selectedItemIcon.enabled = true;
        }

        void ClearShowSelectedItem()
        {
            m_selectedItemIcon.enabled = false;
            m_selectedItemName.text = string.Empty;
            m_selectedItemTypeText.text = string.Empty;
            m_selectedItemDescStr.text = string.Empty;
        }

        void Start()
        {
            OnItemHoverLeaved(0);
            ClearShowSelectedItem();
        }

    }
}