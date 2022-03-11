using UnityEngine;

public interface IElementSetup<TData>
{
    void Setup(TData data);
}