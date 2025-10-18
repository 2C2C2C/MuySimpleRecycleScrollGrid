using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Playground_20250407
{
    public class TempList_20250407 : MonoBehaviour
    {
        public TempElement_20250407 prefab;
        public RectTransform spawnRoot;
        public int spawnCount = 10;

        public GameObject[] elements;

        [ContextMenu(nameof(Spawn))]
        public void Spawn()
        {
            Clear();
            elements = new GameObject[spawnCount];
            for (int i = 0; i < spawnCount; i++)
            {
                TempElement_20250407 spawned = TempElement_20250407.Instantiate(prefab, spawnRoot);
                spawned.Init();
                elements[i] = spawned.gameObject;
            }
        }

        [ContextMenu(nameof(Clear))]
        public void Clear()
        {
            if (null == elements)
            {
                return;
            }
            for (int i = 0, length = elements.Length; i < length; i++)
            {
                GameObject.DestroyImmediate(elements[i]);
            }
            elements = null;
        }

    }
}