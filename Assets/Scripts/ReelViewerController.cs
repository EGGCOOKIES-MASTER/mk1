using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReelViewerController : MonoBehaviour
{
    [Header("Viewer UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Image expandedImage;
    [SerializeField] private Text titleText;
    [SerializeField] private TMP_Text titleTMPText;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button closeButton;

    private Sprite[] currentPages = Array.Empty<Sprite>();
    private int currentPageIndex;
    private string currentTitle;
    private Action closeCallback;
    private bool closeCallbackInvoked;

    private void Awake()
    {
        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        BindButtons();
        HidePanel();
    }

    private void BindButtons()
    {
        if (previousButton != null)
        {
            previousButton.onClick.RemoveAllListeners();
            previousButton.onClick.AddListener(ShowPrevious);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(ShowNext);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }
    }

    public void Open(ThreeReelsGrid.ReelEntry reel)
    {
        Open(reel, null);
    }

    public void Open(ThreeReelsGrid.ReelEntry reel, Action viewedCallback)
    {
        if (reel == null)
        {
            return;
        }

        Sprite[] pages = reel.pages != null && reel.pages.Length > 0
            ? reel.pages
            : BuildSinglePage(reel.thumbnail);

        Open(pages, reel.title, viewedCallback);
    }

    public void Open(Sprite[] pages)
    {
        Open(pages, string.Empty, null);
    }

    public void Open(Sprite[] pages, string title)
    {
        Open(pages, title, null);
    }

    public void Open(Sprite[] pages, string title, Action viewedCallback)
    {
        if (pages == null || pages.Length == 0)
        {
            Debug.LogWarning("ReelViewerController was opened without pages.");
            return;
        }

        currentPages = pages;
        currentPageIndex = 0;
        currentTitle = title ?? string.Empty;
        closeCallback = viewedCallback;
        closeCallbackInvoked = false;

        ShowPanel();
        ShowCurrentPage();
    }

    public void ShowNext()
    {
        if (currentPages == null || currentPageIndex >= currentPages.Length - 1)
        {
            return;
        }

        currentPageIndex++;
        ShowCurrentPage();
    }

    public void ShowPrevious()
    {
        if (currentPages == null || currentPageIndex <= 0)
        {
            return;
        }

        currentPageIndex--;
        ShowCurrentPage();
    }

    public void Close()
    {
        InvokeCloseCallbackOnce();
        HidePanel();
    }

    public void CloseViewer()
    {
        Close();
    }

    public static ReelViewerController CreateDefault(Transform parent)
    {
        GameObject panel = new GameObject("ReelViewerPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ReelViewerController));
        panel.transform.SetParent(parent, false);
        panel.transform.SetAsLastSibling();

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        StretchToFill(panelRect);

        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.88f);

        GameObject dim = CreateImageObject("DimBackground", panel.transform, new Color(0f, 0f, 0f, 0.88f));
        StretchToFill(dim.GetComponent<RectTransform>());
        dim.transform.SetAsFirstSibling();

        GameObject imageObject = CreateImageObject("ExpandedImage", panel.transform, Color.white);
        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.anchorMin = new Vector2(0.14f, 0.08f);
        imageRect.anchorMax = new Vector2(0.86f, 0.92f);
        imageRect.pivot = new Vector2(0.5f, 0.5f);
        imageRect.anchoredPosition = Vector2.zero;
        imageRect.sizeDelta = Vector2.zero;
        Image expanded = imageObject.GetComponent<Image>();
        expanded.preserveAspect = true;
        expanded.raycastTarget = false;

        Button previous = CreateButton("PrevButton", panel.transform, "<", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(84f, 0f));
        Button next = CreateButton("NextButton", panel.transform, ">", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-84f, 0f));
        Button close = CreateButton("CloseButton", panel.transform, "X", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-64f, -64f));

        ReelViewerController controller = panel.GetComponent<ReelViewerController>();
        controller.panelRoot = panel;
        controller.expandedImage = expanded;
        controller.previousButton = previous;
        controller.nextButton = next;
        controller.closeButton = close;
        controller.BindButtons();
        controller.HidePanel();

        return controller;
    }

    private void ShowCurrentPage()
    {
        if (currentPages == null || currentPages.Length == 0)
        {
            return;
        }

        Sprite page = currentPages[currentPageIndex];
        if (expandedImage != null)
        {
            expandedImage.sprite = page;
            expandedImage.color = page != null ? Color.white : Color.clear;
            expandedImage.preserveAspect = true;
        }

        if (titleText != null)
        {
            titleText.text = currentTitle;
        }

        if (titleTMPText != null)
        {
            titleTMPText.text = currentTitle;
        }

        UpdateButtonVisibility();
    }

    private void UpdateButtonVisibility()
    {
        int pageCount = currentPages != null ? currentPages.Length : 0;

        if (previousButton != null)
        {
            previousButton.gameObject.SetActive(pageCount > 1 && currentPageIndex > 0);
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(pageCount > 1 && currentPageIndex < pageCount - 1);
        }
    }

    private void ShowPanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
            panelRoot.transform.SetAsLastSibling();
        }
    }

    private void HidePanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void InvokeCloseCallbackOnce()
    {
        if (closeCallbackInvoked)
        {
            return;
        }

        closeCallbackInvoked = true;
        closeCallback?.Invoke();
    }

    private static Sprite[] BuildSinglePage(Sprite sprite)
    {
        return sprite != null ? new[] { sprite } : Array.Empty<Sprite>();
    }

    private static GameObject CreateImageObject(string objectName, Transform parent, Color color)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        Image image = imageObject.GetComponent<Image>();
        image.color = color;

        return imageObject;
    }

    private static Button CreateButton(string objectName, Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = anchorMin;
        buttonRect.anchorMax = anchorMax;
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = anchoredPosition;
        buttonRect.sizeDelta = new Vector2(76f, 76f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0f, 0f, 0f, 0.72f);

        GameObject labelObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        labelObject.transform.SetParent(buttonObject.transform, false);
        StretchToFill(labelObject.GetComponent<RectTransform>());

        Text text = labelObject.GetComponent<Text>();
        text.text = label;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.fontSize = 42;
        text.raycastTarget = false;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return buttonObject.GetComponent<Button>();
    }

    private static void StretchToFill(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
