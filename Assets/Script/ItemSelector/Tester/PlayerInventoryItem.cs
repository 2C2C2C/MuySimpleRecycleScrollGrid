using Temp.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Temp.ItemSelectorTest
{
    public class PlayerInventoryItem : SelectableItemBase
    {
        [SerializeField]
        Image m_itemBack = null;
        [SerializeField]
        Image m_itemIcon = null;
        [SerializeField]
        Image m_selectFrame = null;
        [SerializeField]
        Text m_itemCountText = null;

        [SerializeField]
        Color m_normalColor = default;
        [SerializeField]
        Color m_selectedColor = default;

        public void InjectData(Sprite iconSprite, int itemCount)
        {
            m_itemIcon.sprite = iconSprite;
            m_itemCountText.text = itemCount.ToString();
            m_itemIcon.enabled = true;
            SetSelected(false);
        }

        public override void SetSelected(bool selected)
        {
            m_selectFrame.enabled = selected;
            m_itemBack.color = selected ? m_selectedColor : m_normalColor;
        }

        public override void SetEmpty()
        {
            m_itemCountText.text = string.Empty;
            m_selectFrame.enabled = false;
            m_itemIcon.enabled = false;
        }

        public override void SetClicked() { }

        public override void SetFilled() { }

        public override void SetHover() { }

        public override void SetLeave() { }

    }
}