using System;
using UnityEngine;
using UnityEngine.UI;

public class ReelPostViewer : MonoBehaviour
{
    [Header("Viewer UI")]
    [SerializeField] private Image postImage;
    [SerializeField] private Text titleText;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button closeButton;

    private ThreeReelsGrid.ReelEntry currentPost;
    private Sprite[] currentImages;
    private int currentImageIndex;
    private Action onViewed;

    private void Awake()
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

        gameObject.SetActive(false);
    }

    public void Open(ThreeReelsGrid.ReelEntry post)
    {
        Open(post, null);
    }

    public void Open(ThreeReelsGrid.ReelEntry post, Action viewedCallback)
    {
        if (post == null)
        {
            return;
        }

        currentPost = post;
        onViewed = viewedCallback;
        currentImages = GetImagesForPost(post);
        currentImageIndex = 0;

        if (currentImages.Length == 0)
        {
            Debug.LogWarning($"Reel '{post.title}' has no thumbnail or pages to show.");
            return;
        }

        gameObject.SetActive(true);
        ShowCurrent();
    }

    private Sprite[] GetImagesForPost(ThreeReelsGrid.ReelEntry post)
    {
        if (post.pages != null && post.pages.Length > 0)
        {
            return post.pages;
        }

        if (post.thumbnail != null)
        {
            return new[] { post.thumbnail };
        }

        return new Sprite[0];
    }

    private void ShowCurrent()
    {
        if (currentImages == null || currentImages.Length == 0)
        {
            return;
        }

        if (postImage != null)
        {
            Sprite currentImage = currentImages[currentImageIndex];
            postImage.sprite = currentImage;
            postImage.color = currentImage != null ? Color.white : Color.black;
            postImage.preserveAspect = true;
        }

        if (titleText != null)
        {
            titleText.text = currentPost.title ?? string.Empty;
        }

        if (previousButton != null)
        {
            previousButton.gameObject.SetActive(currentImages.Length > 1 && currentImageIndex > 0);
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(currentImages.Length > 1 && currentImageIndex < currentImages.Length - 1);
        }
    }

    private void ShowNext()
    {
        if (currentImages == null || currentImages.Length == 0 || currentImageIndex >= currentImages.Length - 1)
        {
            return;
        }

        currentImageIndex++;
        ShowCurrent();

        // Reaching the last image also counts as viewing the reel.
        if (currentImageIndex == currentImages.Length - 1)
        {
            onViewed?.Invoke();
        }
    }

    private void ShowPrevious()
    {
        if (currentImages == null || currentImages.Length == 0 || currentImageIndex <= 0)
        {
            return;
        }

        currentImageIndex--;
        ShowCurrent();
    }

    private void Close()
    {
        onViewed?.Invoke();
        gameObject.SetActive(false);
    }
}
