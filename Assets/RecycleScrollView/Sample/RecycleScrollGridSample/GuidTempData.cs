namespace RecycleScrollView.Sample
{
    public class GuidTempData
    {
        public readonly System.Guid ItemGuid;
        public readonly string ItemName;

        public GuidTempData(in System.Guid itemId, in string itemName)
        {
            ItemGuid = itemId;
            ItemName = itemName;
        }

        public GuidTempData(in System.Guid itemId)
        {
            ItemGuid = itemId;
            ItemName = itemId.ToString().Substring(0, 5);
        }

        public GuidTempData()
        {
            ItemGuid = System.Guid.NewGuid();
            ItemName = ItemGuid.ToString().Substring(0, 5);
        }

    }
}