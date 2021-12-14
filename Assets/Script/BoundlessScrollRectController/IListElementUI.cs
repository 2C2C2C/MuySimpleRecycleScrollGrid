using UnityEngine;

public interface IListElementUI
{
    void Show();
    void Hide();
    void SetIndex(int index);
    int ElementIndex { get; }
    RectTransform ElementRectTransform { get; }
}
