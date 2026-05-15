using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ThreeReelsGrid : MonoBehaviour
{
    [System.Serializable]
    public class ReelEntry
    {
        public string title;
        public Sprite thumbnail;
        public string sceneName = "MiniGame";
    }

    [Header("Reel Slots")]
    [SerializeField] private Image leftImage;
    [SerializeField] private Image centerImage;
    [SerializeField] private Image rightImage;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button centerButton;
    [SerializeField] private Button rightButton;

    [Header("Controls")]
    [SerializeField] private Button refreshButton;

    [Header("Reel Data")]
    [SerializeField] private ReelEntry[] reels;
    [SerializeField] private bool preventDuplicateOnScreen = true;

    private readonly ReelEntry[] currentReels = new ReelEntry[3];

    private void Awake()
    {
        AutoBindButtons();
        BindButtonEvents();
    }

    private void OnEnable()
    {
        RefreshReels();
    }

    private void AutoBindButtons()
    {
        if (leftButton == null && leftImage != null)
        {
            leftButton = leftImage.GetComponent<Button>();
        }

        if (centerButton == null && centerImage != null)
        {
            centerButton = centerImage.GetComponent<Button>();
        }

        if (rightButton == null && rightImage != null)
        {
            rightButton = rightImage.GetComponent<Button>();
        }
    }

    private void BindButtonEvents()
    {
        BindSlotButton(leftButton, 0);
        BindSlotButton(centerButton, 1);
        BindSlotButton(rightButton, 2);

        if (refreshButton != null)
        {
            refreshButton.onClick.RemoveAllListeners();
            refreshButton.onClick.AddListener(RefreshReels);
        }
    }

    private void BindSlotButton(Button button, int slotIndex)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OpenReel(slotIndex));
    }

    public void RefreshReels()
    {
        if (reels == null || reels.Length == 0)
        {
            Debug.LogWarning("ThreeReelsGrid has no reel data. Fill the Reels array in the Inspector.");
            return;
        }

        List<int> usedIndices = new List<int>();
        for (int i = 0; i < currentReels.Length; i++)
        {
            currentReels[i] = PickRandomReel(usedIndices);
            ApplySlotVisual(i, currentReels[i]);
        }
    }

    private ReelEntry PickRandomReel(List<int> usedIndices)
    {
        if (!preventDuplicateOnScreen || reels.Length <= currentReels.Length)
        {
            return reels[Random.Range(0, reels.Length)];
        }

        int index;
        do
        {
            index = Random.Range(0, reels.Length);
        }
        while (usedIndices.Contains(index));

        usedIndices.Add(index);
        return reels[index];
    }

    private void ApplySlotVisual(int slotIndex, ReelEntry reel)
    {
        Image targetImage = GetSlotImage(slotIndex);
        if (targetImage == null || reel == null)
        {
            return;
        }

        targetImage.sprite = reel.thumbnail;
        targetImage.color = reel.thumbnail != null ? Color.white : new Color(0.12f, 0.12f, 0.12f, 1f);
        targetImage.preserveAspect = false;
    }

    private Image GetSlotImage(int slotIndex)
    {
        switch (slotIndex)
        {
            case 0:
                return leftImage;
            case 1:
                return centerImage;
            case 2:
                return rightImage;
            default:
                return null;
        }
    }

    private void OpenReel(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= currentReels.Length)
        {
            return;
        }

        ReelEntry selectedReel = currentReels[slotIndex];
        if (selectedReel == null)
        {
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.BeginReelsSession();
        }

        if (string.IsNullOrWhiteSpace(selectedReel.sceneName))
        {
            Debug.LogWarning($"Reel '{selectedReel.title}' has an empty sceneName.");
            return;
        }

        Debug.Log($"Open reel: {selectedReel.title} -> {selectedReel.sceneName}");
        SceneManager.LoadScene(selectedReel.sceneName);
    }
}
