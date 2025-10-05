using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RecycleScrollGrid
{
    public class TempListView : MonoBehaviour
    {
#if UNITY_EDITOR
#endif
        // check when prefab change
        // [OnValueChanged("OnPrefabChanged")]
        [SerializeField, Header("place holder, the listview should only contains one type of element")]
        private RecycleScrollGridElement _elementPrefab;

        [SerializeField]
        private List<RecycleScrollGridElement> _actualUsedComponents = new List<RecycleScrollGridElement>(0);

        RectTransform Container => transform as RectTransform;
        public int Count => _actualUsedComponents.Count;
        public RecycleScrollGridElement this[int index] => _actualUsedComponents[index];
        public IReadOnlyList<RecycleScrollGridElement> ElementList => _actualUsedComponents;

        public RecycleScrollGridElement Add()
        {
            RecycleScrollGridElement element = InternalAdd();
            _actualUsedComponents.Add(element);
            element.SetObjectActive();
            return element;
        }

        public void Clear()
        {
            while (_actualUsedComponents.Count > 0)
            {
                RecycleScrollGridElement element = _actualUsedComponents[_actualUsedComponents.Count - 1];
                _actualUsedComponents.RemoveAt(_actualUsedComponents.Count - 1);
                InternalRemove(element);
            }
        }

        public void Remove(RecycleScrollGridElement instance)
        {
            if (_actualUsedComponents.Remove(instance))
            {
                instance.SetObjectDeactive();
            }
            else
            {
                Debug.LogError($"listview_{this.gameObject.name} does not contains {instance.ElementRectTransform.name}, remove failed");
            }
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _actualUsedComponents.Count)
            {
                Debug.LogError($"{index} is invalid for SimpleListView", this.gameObject);
                return;
            }

            RecycleScrollGridElement toRemove = _actualUsedComponents[index];
            _actualUsedComponents.RemoveAt(index);
            InternalRemove(toRemove);
        }

        public void InnerSwap(int indexA, int indexB)
        {
            if (indexA < 0 || indexA > _actualUsedComponents.Count - 1 || indexB < 0 || indexB > _actualUsedComponents.Count - 1)
            {
                return;
            }

            RecycleScrollGridElement temp = _actualUsedComponents[indexA];
            int transformIndexA = temp.ElementRectTransform.GetSiblingIndex();
            int transformIndexB = _actualUsedComponents[indexB].ElementRectTransform.GetSiblingIndex();
            _actualUsedComponents[indexA] = _actualUsedComponents[indexB];
            _actualUsedComponents[indexB] = temp;
            _actualUsedComponents[indexA].ElementRectTransform.SetSiblingIndex(transformIndexA);
            _actualUsedComponents[indexB].ElementRectTransform.SetSiblingIndex(transformIndexB);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>-1 means element is not in the list</returns>
        public int IndexOf(RecycleScrollGridElement instance)
        {
            try
            {
                return _actualUsedComponents.IndexOf(instance);
            }
            catch (ArgumentOutOfRangeException)
            {
                return -1;
            }
        }

        protected virtual RecycleScrollGridElement InternalAdd()
        {
            if (Application.isEditor && !Application.isPlaying)
                return UnityEditor.PrefabUtility.InstantiatePrefab(_elementPrefab, Container) as RecycleScrollGridElement;
            return Instantiate(_elementPrefab, Container);
        }

        protected virtual void InternalRemove(RecycleScrollGridElement element)
        {
            if (element == null) return;
            element.SetObjectDeactive();
            if (Application.isEditor && !Application.isPlaying)
            {
                GameObject.DestroyImmediate(element.ElementRectTransform.gameObject);
            }
            else
            {
                GameObject.Destroy(element.ElementRectTransform.gameObject);
            }
        }

        private void Awake()
        {
            // FindPrefabInstances();
        }

#if UNITY_EDITOR

        private void OnTransformChildrenChanged()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                FindPrefabInstances();
            }
        }

        private void OnPrefabChanged()
        {
            // remove pre objects
            int amount = _actualUsedComponents.Count;
            for (int i = 0; i < _actualUsedComponents.Count; i++)
            {
                GameObject.DestroyImmediate(_actualUsedComponents[i].ElementRectTransform.gameObject);
            }

            if (_elementPrefab != null)
            {
                for (int i = 0; i < amount; i++)
                {
                    RectTransform rectTransform = (RectTransform)UnityEditor.PrefabUtility.InstantiatePrefab(_elementPrefab, Container);
                    _actualUsedComponents[i] = (rectTransform.GetComponent<RecycleScrollGridElement>());
                }
            }
            else
            {
                _actualUsedComponents.Clear();
            }
        }

        /// <summary>
        /// Retrieves prefab instances in the transform
        /// </summary>
        [ContextMenu("find prefab instances")]
        private void FindPrefabInstances()
        {
            bool hasPrefab = !(_elementPrefab == null);
            RecycleScrollGridElement elementPrefab = _elementPrefab.GetComponent<RecycleScrollGridElement>();
            _actualUsedComponents.Clear();
            List<GameObject> toDeleteObjectList = new List<GameObject>();
            foreach (Transform child in Container)
            {
                RecycleScrollGridElement childElement = child.GetComponent<RecycleScrollGridElement>();
                if (childElement == null)
                {
                    toDeleteObjectList.Add(child.gameObject);
                    continue;
                }

                if (hasPrefab)
                {
                    GameObject detectPrefabGo = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
                    RecycleScrollGridElement detectPrefab = (detectPrefabGo == null) ? null : detectPrefabGo.GetComponent<RecycleScrollGridElement>();
                    if (elementPrefab == detectPrefab)
                    {
                        // same source prefab
                        _actualUsedComponents.Add(childElement);
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
                    _elementPrefab = prefab.GetComponent<RecycleScrollGridElement>();
                    _actualUsedComponents.Add(childElement);
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
            if (_elementPrefab == null)
            {
                Debug.LogError("listview is missing element prefab");
                return;
            }
            RecycleScrollGridElement spawnObject = (RecycleScrollGridElement)UnityEditor.PrefabUtility.InstantiatePrefab(_elementPrefab, Container);
            _actualUsedComponents.Add(spawnObject);
        }

        [ContextMenu("editor time clear")]
        private void EditorTimeClear()
        {
            if (Application.isPlaying) return;
            // remove pre objects
            RecycleScrollGridElement[] preObjects = Container.GetComponentsInChildren<RecycleScrollGridElement>();
            for (int i = 0; i < preObjects.Length; i++)
            {
                GameObject.DestroyImmediate(preObjects[i].ElementRectTransform.gameObject);
            }
            _actualUsedComponents.Clear();
        }

        [ContextMenu("test print elements")]
        private void TestPrintListElements()
        {
            if (_actualUsedComponents != null)
            {
                StringBuilder printText = new StringBuilder($"temp list view children_{_actualUsedComponents.Count} :\n");
                for (int i = 0; i < _actualUsedComponents.Count; i++)
                {
                    printText.AppendLine(_actualUsedComponents[i].ElementRectTransform.name);
                }
                Debug.Log(printText);
            }
        }

#endif
    }
}