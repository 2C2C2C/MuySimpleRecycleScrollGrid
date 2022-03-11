using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class TempListView : MonoBehaviour
{
#if UNITY_EDITOR
#endif
    // check when prefab change
    // [OnValueChanged("OnPrefabChanged")]
    [SerializeField, Header("place holder, the listview should only contains one type of element")]
    TempListElementUI m_elementPrefab;
    RectTransform Container => transform as RectTransform;
    [SerializeField, ReadOnly]
    List<TempListElementUI> m_actualUsedComponents = new List<TempListElementUI>(0);

    public int Count => m_actualUsedComponents.Count;
    public TempListElementUI this[int index] => m_actualUsedComponents[index];
    public IReadOnlyList<TempListElementUI> ElementList => m_actualUsedComponents;

    public TempListElementUI Add()
    {
        TempListElementUI element = InternalAdd();
        m_actualUsedComponents.Add(element);
        element.Show();
        return element;
    }

    public void Clear()
    {
        TempListElementUI element = null;
        while (m_actualUsedComponents.Count > 0)
        {
            element = m_actualUsedComponents[m_actualUsedComponents.Count - 1];
            m_actualUsedComponents.RemoveAt(m_actualUsedComponents.Count - 1);
            InternalRemove(element);
        }
    }

    public void Remove(TempListElementUI instance)
    {
        if (m_actualUsedComponents.Remove(instance))
        {
            instance.Hide();
        }
        else
        {
            Debug.LogError($"listview_{this.gameObject.name} does not contains {instance.ElementRectTransform.name}, remove failed");
        }
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= m_actualUsedComponents.Count)
        {
            Debug.LogError($"{index} is invalid for SimpleListView", this.gameObject);
            return;
        }

        TempListElementUI toRemove = m_actualUsedComponents[index];
        m_actualUsedComponents.RemoveAt(index);
        InternalRemove(toRemove);
    }

    public void InnerSwap(int indexA, int indexB)
    {
        if (indexA < 0 || indexA > m_actualUsedComponents.Count - 1 || indexB < 0 || indexB > m_actualUsedComponents.Count - 1)
        {
            return;
        }

        TempListElementUI temp = m_actualUsedComponents[indexA];
        int transformIndexA = temp.ElementRectTransform.GetSiblingIndex();
        int transformIndexB = m_actualUsedComponents[indexB].ElementRectTransform.GetSiblingIndex();
        m_actualUsedComponents[indexA] = m_actualUsedComponents[indexB];
        m_actualUsedComponents[indexB] = temp;
        m_actualUsedComponents[indexA].ElementRectTransform.SetSiblingIndex(transformIndexA);
        m_actualUsedComponents[indexB].ElementRectTransform.SetSiblingIndex(transformIndexB);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>-1 means element is not in the list</returns>
    public int IndexOf(TempListElementUI instance)
    {
        try
        {
            return m_actualUsedComponents.IndexOf(instance);
        }
        catch (ArgumentOutOfRangeException)
        {
            return -1;
        }
    }

    protected virtual TempListElementUI InternalAdd()
    {
        if (Application.isEditor && !Application.isPlaying)
            return UnityEditor.PrefabUtility.InstantiatePrefab(m_elementPrefab, Container) as TempListElementUI;
        return Instantiate(m_elementPrefab, Container);
    }

    protected virtual void InternalRemove(TempListElementUI element)
    {
        element.Hide();
        if (Application.isEditor && !Application.isPlaying)
            GameObject.DestroyImmediate(element.ElementRectTransform.gameObject);
        else
            GameObject.Destroy(element.ElementRectTransform.gameObject);
    }

    private void Awake()
    {
        // FindPrefabInstances();
    }

#if UNITY_EDITOR

    private void OnTransformChildrenChanged()
    {
        if (Application.isEditor && !Application.isPlaying)
            FindPrefabInstances();
    }

    private void OnPrefabChanged()
    {
        // remove pre objects
        int amount = m_actualUsedComponents.Count;
        for (int i = 0; i < m_actualUsedComponents.Count; i++)
        {
            GameObject.DestroyImmediate(m_actualUsedComponents[i].ElementRectTransform.gameObject);
        }

        if (m_elementPrefab != null)
        {
            for (int i = 0; i < amount; i++)
            {
                RectTransform rectTransform = (RectTransform)UnityEditor.PrefabUtility.InstantiatePrefab(m_elementPrefab, Container);
                m_actualUsedComponents[i] = (rectTransform.GetComponent<TempListElementUI>());
            }
        }
        else
        {
            m_actualUsedComponents.Clear();
        }
    }

    /// <summary>
    /// Retrieves prefab instances in the transform
    /// </summary>
    [ContextMenu("find prefab instances")]
    private void FindPrefabInstances()
    {
        bool hasPrefab = !(m_elementPrefab == null);
        TempListElementUI elementPrefab = m_elementPrefab.GetComponent<TempListElementUI>();
        m_actualUsedComponents.Clear();
        List<GameObject> toDeleteObjectList = new List<GameObject>();
        foreach (Transform child in Container)
        {
            TempListElementUI childElement = child.GetComponent<TempListElementUI>();
            if (childElement == null)
            {
                toDeleteObjectList.Add(child.gameObject);
                continue;
            }

            if (hasPrefab)
            {
                GameObject detectPrefabGo = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
                TempListElementUI detectPrefab = (detectPrefabGo == null) ? null : detectPrefabGo.GetComponent<TempListElementUI>();
                if (elementPrefab == detectPrefab)
                {
                    // same source prefab
                    m_actualUsedComponents.Add(childElement);
                }
                else
                {
                    // different source prefab, delete this one
                    toDeleteObjectList.Add(child.gameObject);
                }
            }
            else if (UnityEditor.PrefabUtility.IsAnyPrefabInstanceRoot(child.gameObject))
            {
                // find the first prefab
                GameObject prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
                m_elementPrefab = prefab.GetComponent<TempListElementUI>();
                m_actualUsedComponents.Add(childElement);
                hasPrefab = true;
            }
        }

        for (int i = 0; i < toDeleteObjectList.Count; i++)
        {
            if (Application.isPlaying)
                GameObject.Destroy(toDeleteObjectList[i]);
            else
                GameObject.DestroyImmediate(toDeleteObjectList[i]);
        }
    }

    [ContextMenu("editor time add")]
    private void EditorTimeAdd()
    {
        if (Application.isPlaying) return;
        if (m_elementPrefab == null)
        {
            Debug.LogError("listview is missing element prefab");
            return;
        }
        RectTransform spawnObject = (RectTransform)UnityEditor.PrefabUtility.InstantiatePrefab(m_elementPrefab, Container);
        if (spawnObject.TryGetComponent<TempListElementUI>(out TempListElementUI element))
            m_actualUsedComponents.Add(element);
    }

    [ContextMenu("editor time clear")]
    private void EditorTimeClear()
    {
        if (Application.isPlaying) return;
        // remove pre objects
        TempListElementUI[] preObjects = Container.GetComponentsInChildren<TempListElementUI>();
        for (int i = 0; i < preObjects.Length; i++)
        {
            GameObject.DestroyImmediate(preObjects[i].ElementRectTransform.gameObject);
        }
        m_actualUsedComponents.Clear();
    }

    [ContextMenu("test print elements")]
    private void TestPrintListElements()
    {
        if (m_actualUsedComponents != null)
        {
            StringBuilder printText = new StringBuilder($"temp list view children_{m_actualUsedComponents.Count} :\n");
            for (int i = 0; i < m_actualUsedComponents.Count; i++)
            {
                printText.AppendLine(m_actualUsedComponents[i].ElementRectTransform.name);
            }
            Debug.Log(printText);
        }
    }

#endif
}