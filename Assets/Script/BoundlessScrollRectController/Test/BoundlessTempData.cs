
public class BoundlessTempData
{
    public readonly System.Guid ItemGuid;
    public readonly string ItemName;

    public BoundlessTempData(in System.Guid itemId, in string itemName)
    {
        ItemGuid = itemId;
        ItemName = itemName;
    }

    public BoundlessTempData(in System.Guid itemId)
    {
        ItemGuid = itemId;
        ItemName = itemId.ToString().Substring(0, 5);
    }

}
