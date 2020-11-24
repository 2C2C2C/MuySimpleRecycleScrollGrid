using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseScrollRectItem : MonoBehaviour
{
    public Vector2 ItemSize => m_itemSize;


    private Vector2 m_itemSize;


    public void InjectData()
    {

    }

}
