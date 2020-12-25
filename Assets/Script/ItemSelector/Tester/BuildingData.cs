using UnityEngine;

namespace Temp.ItemSelectorTest
{
    public readonly struct BuildingData
    {
        public readonly System.Guid BuildingGuid;
        public readonly Sprite BuildingIcon;
        public readonly string BuidlingDescription;

        public BuildingData(System.Guid guid, Sprite sprite, string str)
        {
            BuildingGuid = guid;
            BuildingIcon = sprite;
            BuidlingDescription = str;
        }
    }
}