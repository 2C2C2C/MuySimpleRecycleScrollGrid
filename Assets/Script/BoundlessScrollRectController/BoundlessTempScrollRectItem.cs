using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundlessTempScrollRectItem : BoundlessBaseScrollRectItem
{
    [SerializeField]
    private UnityEngine.UI.Text m_dataText = null;

    private BoundlessTempData m_data = default;

    public override void InjectData(IBoundlessScrollRectItemData data)
    {
        m_data = data as BoundlessTempData;
        m_dataText.text = m_data.ItemName;
    }

}
