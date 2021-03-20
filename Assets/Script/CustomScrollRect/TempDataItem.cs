using System;

[Serializable]
public class TempDataItem : IBoundlessGridData
{
    private readonly System.Guid m_guid = default;
    private readonly string m_tempName = null;

    public System.Guid Guid => m_guid;
    public string TempName => m_tempName;

    public TempDataItem()
    {
        m_guid = System.Guid.NewGuid();
        m_tempName = m_guid.ToString().Substring(0, 5);
    }
}
