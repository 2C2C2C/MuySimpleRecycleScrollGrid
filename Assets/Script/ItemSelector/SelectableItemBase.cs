using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Temp.UI
{
    /*
    handle hover/leave click behaviour
    let child class handle appearance
    */
    public abstract class SelectableItemBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        protected int m_selfIndex = 0;

        System.Action<int> m_onItemClicked = null;
        System.Action<int> m_onHoveredCallback = null;
        System.Action<int> m_onHoverLeavedCallback = null;

        public abstract void SetClicked();
        public abstract void SetEmpty();
        public abstract void SetFilled();
        public abstract void SetSelected(bool selected);
        public abstract void SetHover();
        public abstract void SetLeave();

        public void SetupHoverCallback(System.Action<int> onHoveredCallback, System.Action<int> onHoverLeavedCallback)
        {
            m_onHoveredCallback = onHoveredCallback;
            m_onHoverLeavedCallback = onHoverLeavedCallback;
        }

        public void SetupClickCallback(System.Action<int> onItemClicked)
        {
            m_onItemClicked = onItemClicked;
        }

        public void SetupItemIndex(int index)
        {
            m_selfIndex = index;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_onHoveredCallback?.Invoke(m_selfIndex);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_onHoverLeavedCallback?.Invoke(m_selfIndex);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            m_onItemClicked?.Invoke(m_selfIndex);
        }

        public void OnItemClicked()
        {
            m_onItemClicked?.Invoke(m_selfIndex);
        }

        public void ClearCallbacks()
        {
            m_onItemClicked = null;
            m_onHoveredCallback = null;
            m_onHoverLeavedCallback = null;
        }
    }
}