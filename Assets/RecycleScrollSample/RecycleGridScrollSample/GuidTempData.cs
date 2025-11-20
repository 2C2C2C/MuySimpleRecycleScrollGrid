using Guid = System.Guid;

namespace RecycleScrollView.Sample
{
    public class GuidElementData
    {
        public readonly Guid ItemGuid;
        public readonly string ItemName;

        public GuidElementData(in Guid itemId, in string itemName)
        {
            ItemGuid = itemId;
            ItemName = itemName;
        }

        public GuidElementData(in Guid itemId)
        {
            ItemGuid = itemId;
            ItemName = itemId.ToString().Substring(0, 5);
        }

        public GuidElementData()
        {
            ItemGuid = Guid.NewGuid();
            ItemName = ItemGuid.ToString().Substring(0, 5);
        }

    }
}