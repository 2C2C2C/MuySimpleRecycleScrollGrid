using UnityEngine;

namespace RecycleScrollGrid
{
    public static class RecycleScrollGridSetupHelper
    {
        [UnityEditor.MenuItem("GameObject/UI/BoundlessScrollRect", false, 12)]
        static void CreateBoundlessScrollRect(UnityEditor.MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;

            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                parent = GetOrCreateCanvasInScene().gameObject;
            }

            //GameObject window = new GameObject("Modal Window");
            //window.transform.SetParent(parent.transform, false);
            //RectTransform rect = window.gameObject.AddComponent<RectTransform>();
            //rect.StretchToFillParent();

            //Canvas windowCanvas = window.gameObject.AddComponent<Canvas>();
            //windowCanvas.sortingOrder = (int)UILayerType.ModalWindow;
            //windowCanvas.pixelPerfect = false;

            //GameObject back = new GameObject("AwakeButton");
            //back.transform.SetParent(window.transform, false);
            //var backRect = back.AddComponent<RectTransform>();
            //backRect.StretchToFillParent();
            //back.AddComponent<CygnusButton>();

            //GameObject canvasGroup = new GameObject("CanvasGroup");
            //canvasGroup.transform.SetParent(window.transform, false);
            //var canvasGroupRect = canvasGroup.AddComponent<RectTransform>();
            //canvasGroupRect.StretchToFillParent();
            //canvasGroup.AddComponent<CanvasGroup>();

            //GameObject content = new GameObject("Content");
            //content.transform.SetParent(canvasGroup.transform, false);
            //var contentRect = content.AddComponent<RectTransform>();
            //contentRect.StretchToFillParent();

            //CreateSubMenu(canvasGroupRect);
        }

        static Canvas GetOrCreateCanvasInScene()
        {
            Canvas result = GameObject.FindObjectOfType<Canvas>();
            if (result)
            {
                return result;
            }
            result = new GameObject("Canvas").AddComponent<Canvas>();
            return result;
        }
    }
}