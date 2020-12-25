using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Temp.UI;

namespace Temp.ItemSelectorTest
{
    public class SelectableBuildingItemUI : SelectableItemBase
    {
        [SerializeField]
        Image m_buildingIconIMG = null;
        [SerializeField]
        Image m_backIMG = null;
        [SerializeField]
        Text m_buildingName = null;

        [SerializeField]
        Color m_normalColor = default;
        [SerializeField]
        Color m_selectedColor = default;
        [SerializeField]
        Color m_hoveredColor = default;

        public void InjectData(Sprite iconSprite, string itemName)
        {
            m_buildingIconIMG.sprite = iconSprite;
            m_buildingIconIMG.enabled = true;
            m_buildingName.text = itemName;
        }

        public override void SetClicked()
        { }

        public override void SetHover()
        {
            m_backIMG.color = m_hoveredColor;
        }

        public override void SetLeave()
        {
            m_backIMG.color = m_normalColor;
        }

        public override void SetSelected(bool selected)
        { }

        public override void SetEmpty()
        {
            m_buildingIconIMG.enabled = false;
            m_buildingName.text = string.Empty;
        }

        public override void SetFilled()
        {
            m_buildingIconIMG.enabled = true;
        }

    }
}