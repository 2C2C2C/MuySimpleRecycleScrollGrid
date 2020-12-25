using UnityEngine;

namespace Temp.ItemSelectorTest
{
    public enum PlayerItemType
    {
        Item = 0,
        Food = 1,
        Weapon = 2,
        Resource = 3,
    }

    public readonly struct PlayerInventoryStackData
    {
        public readonly System.Guid StackGuid;
        public readonly int ItemCount;
        public readonly PlayerItemType ItemType;
        public readonly Sprite ItemIcon;
        public readonly string ItemDescription;

        public PlayerInventoryStackData(System.Guid guid, int count, PlayerItemType type, Sprite icon, string descStr)
        {
            StackGuid = guid;
            ItemCount = count;
            ItemType = type;
            ItemIcon = icon;
            ItemDescription = descStr;
        }
    }
}