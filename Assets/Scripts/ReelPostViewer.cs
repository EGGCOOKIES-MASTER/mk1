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
        if (post == null)
        {
            return;
        }

        currentPost = post;
        currentImages = GetImagesForPost(post);
        currentImageIndex = 0;

        if (currentImages.Length == 0)
        {
            Debug.LogWarning($"Reel '{post.title}' has no thumbnail or postImages to show.");
            return;
        }

        gameObject.SetActive(true);
        ShowCurrent();
    }

    private Sprite[] GetImagesForPost(ThreeReelsGrid.ReelEntry post)
    {
        if (post.postImages != null && post.postImages.Length > 0)
        {
            return post.postImages;
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
    }

    private void ShowNext()
    {
        if (currentImages == null || currentImages.Length == 0)
        {
            return;
        }

        currentImageIndex = (currentImageIndex + 1) % currentImages.Length;
        ShowCurrent();
    }

    private void ShowPrevious()
    {
        if (currentImages == null || currentImages.Length == 0)
        {
            return;
        }

        currentImageIndex = (currentImageIndex - 1 + currentImages.Length) % currentImages.Length;
        ShowCurrent();
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }
}
