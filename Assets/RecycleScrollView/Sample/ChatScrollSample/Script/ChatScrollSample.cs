using System;
using System.Collections;
using System.Collections.Generic;
using RecycleScrollView;
using UnityEngine;
using UnityEngine.UI;

public class ChatScrollSample : MonoBehaviour
{
    [SerializeField]
    private InputField _mainTextInput;
    [SerializeField]
    private InputField _quoteTextInput;
    [SerializeField]
    private Button _addButton;

    private void OnAdDButtonClicked()
    {
        string mainContent = _mainTextInput.text;
        string quoteContent = _quoteTextInput.text;
    }

    private void OnEnable()
    {
        // _addButton.onClick.AddListener(OnAdDButtonClicked);
    }

}
