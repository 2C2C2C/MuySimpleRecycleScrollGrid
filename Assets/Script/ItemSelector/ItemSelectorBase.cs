using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Temp.UI
{
    /*
    TODO to know if the selector works like toggle group or...
    */
    public abstract class ItemSelectorBase<T> : MonoBehaviour where T : SelectableItemBase
    {
        protected const int NON_EXIST_INDEX = -1;

        [SerializeField]
        protected T m_itemPrefab = null;
        [SerializeField]
        protected Transform m_contentParent = null;

        // but wat if we use a boundless scrollrect to show items
        protected List<T> m_selectableItemList = null;
        private int m_currentSelectedItemIndex = NON_EXIST_INDEX;
        private int m_currentHoveredItemIndex = NON_EXIST_INDEX;

        public int CurrentSelectedItemIndex
        {
            get => m_currentSelectedItemIndex;
            protected set
            {
                if (value < 0 || value > m_selectableItemList.Count - 1)
                {
                    m_currentSelectedItemIndex = NON_EXIST_INDEX;
                }
                else
                {
                    m_currentSelectedItemIndex = value;
                }
            }
        }

        public int CurrentHorveredItemIndex
        {
            get => m_currentHoveredItemIndex;
            protected set
            {
                if (value > m_selectableItemList.Count - 1 || value < 0)
                {
                    m_currentHoveredItemIndex = NON_EXIST_INDEX;
                }
                else
                {
                    m_currentHoveredItemIndex = value;
                }
            }
        }

        public T CurrentSelectedItem
        {
            get => (m_currentSelectedItemIndex == NON_EXIST_INDEX || m_currentSelectedItemIndex > m_selectableItemList.Count - 1) ?
                null : m_selectableItemList[m_currentSelectedItemIndex];
        }

        public T CurrentHoveredItem
        {
            get => (m_currentHoveredItemIndex == NON_EXIST_INDEX || m_currentHoveredItemIndex > m_selectableItemList.Count - 1) ?
                null : m_selectableItemList[m_currentHoveredItemIndex];
        }

        public void UnselectAll()
        {
            for (int i = 0; i < m_selectableItemList.Count; i++)
            {
                // is it safe to do it?
                m_selectableItemList[i].SetSelected(false);
            }
        }

        public void ClearItems()
        {
            if (m_selectableItemList == null)
            {
                m_selectableItemList = new List<T>();
                return;
            }

            for (int i = 0; i < m_selectableItemList.Count; i++)
            {
                m_selectableItemList[i].ClearCallbacks();
                GameObject.Destroy(m_selectableItemList[i].gameObject);
            }
            m_selectableItemList.Clear();
        }

        public T GetItem(int index)
        {
            if (index == NON_EXIST_INDEX || index > m_selectableItemList.Count - 1)
            {
                return null;
            }

            return m_selectableItemList[index];
        }

        void Awake()
        {
            m_selectableItemList = new List<T>();
        }

    }
}