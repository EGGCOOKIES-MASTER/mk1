using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ReelCardButton : MonoBehaviour
{
    [Header("Card Data")]
    [SerializeField] private string title;
    [SerializeField] private Sprite thumbnail;
    [SerializeField] private Sprite[] pages;

    [Header("References")]
    [SerializeField] private ReelViewerController viewer;
    [SerializeField] private Image thumbnailImage;
    [SerializeField] private Button button;

    private void Awake()
    {
        AutoBind();
        ApplyThumbnail();
    }

    private void OnEnable()
    {
        AutoBind();
    }

    public void Open()
    {
        if (viewer == null)
        {
            viewer = FindFirstObjectByType<ReelViewerController>(FindObjectsInactive.Include);
        }

        if (viewer == null)
        {
            Debug.LogWarning($"ReelCardButton '{name}' has no ReelViewerController.");
            return;
        }

        Sprite[] openPages = pages != null && pages.Length > 0
            ? pages
            : BuildFallbackPages();

        viewer.Open(openPages, title);
    }

    public void ApplyThumbnail()
    {
        if (thumbnailImage == null)
        {
            thumbnailImage = GetComponent<Image>();
        }

        if (thumbnailImage == null)
        {
            return;
        }

        Sprite visibleThumbnail = thumbnail != null ? thumbnail : GetFirstPage();
        thumbnailImage.sprite = visibleThumbnail;
        thumbnailImage.color = visibleThumbnail != null ? Color.white : Color.clear;
    }

    private void AutoBind()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (thumbnailImage == null)
        {
            thumbnailImage = GetComponent<Image>();
        }

        if (button != null)
        {
            button.onClick.RemoveListener(Open);
            button.onClick.AddListener(Open);
        }
    }

    private Sprite[] BuildFallbackPages()
    {
        Sprite firstPage = GetFirstPage();
        return firstPage != null ? new[] { firstPage } : new Sprite[0];
    }

    private Sprite GetFirstPage()
    {
        if (pages != null && pages.Length > 0)
        {
            return pages[0];
        }

        return thumbnail;
    }
}
