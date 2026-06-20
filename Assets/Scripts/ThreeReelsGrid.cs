using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ThreeReelsGrid : MonoBehaviour
{
    [System.Serializable]
    public class ReelEntry
    {
        public string title;
        public Sprite thumbnail;
        [FormerlySerializedAs("postImages")]
        public Sprite[] pages;
        public bool isMiniGamePost;
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
    [SerializeField] private ReelViewerController postViewer;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text refreshMessageTMPText;
    [SerializeField] private string refreshMessageObjectName = "RefreshMessageText";

    [Header("Reel Data")]
    [SerializeField] private ReelEntry[] reels;
    [SerializeField] private ReelEntry[] strangeReels;
    [Range(0f, 1f)]
    [SerializeField] private float miniGameReelChance = 0.3f;
    [SerializeField] private bool preventDuplicateOnScreen = true;

    [Header("Low Mental State")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite normalBackground;
    [SerializeField] private Sprite lowMentalBackground;
    [SerializeField] private Graphic[] lowMentalTintTargets;
    [SerializeField] private Color normalUIColor = Color.white;
    [SerializeField] private Color lowMentalUIColor = new Color(0.36f, 0.18f, 0.24f, 1f);

    private readonly ReelEntry[] currentReels = new ReelEntry[3];
    private readonly bool[] currentReelCompleted = new bool[3];
    private readonly List<int> usedReelIndicesInCycle = new List<int>();
    private bool isLowMentalState;
    private bool wasUsingStrangePool;
    private Coroutine refreshMessageRoutine;

    private static readonly ReelEntry[] savedReels = new ReelEntry[3];
    private static readonly bool[] savedCompleted = new bool[3];
    private static readonly List<int> savedUsedReelIndicesInCycle = new List<int>();
    private static bool hasSavedSession;
    private static bool savedLowMentalState;
    private static bool savedWasUsingStrangePool;
    private static int pendingMiniGameCompletedSlot = -1;

    private void Awake()
    {
        AutoBindButtons();
        AutoBindPostViewer();
        AutoBindRefreshMessageText();
        BindButtonEvents();
    }

    private void OnEnable()
    {
        SubscribeMentalEvents();

        bool shouldRestoreSession = hasSavedSession;
        bool shouldRefreshForNewLowMentalState = MentalManager.Instance != null
            && MentalManager.Instance.IsLowMentalState
            && !savedLowMentalState;

        if (MentalManager.Instance != null && MentalManager.Instance.IsLowMentalState)
        {
            ApplyLowMentalState(false);
        }

        if (shouldRestoreSession)
        {
            RestoreSavedSession();
        }
        else
        {
            GenerateNewReels();
        }

        if (pendingMiniGameCompletedSlot >= 0)
        {
            CompleteReel(pendingMiniGameCompletedSlot, true);
            pendingMiniGameCompletedSlot = -1;
        }

        if (shouldRefreshForNewLowMentalState)
        {
            RefreshReels();
        }
    }

    private void OnDisable()
    {
        UnsubscribeMentalEvents();
    }

    private void Start()
    {
        SubscribeMentalEvents();

        if (MentalManager.Instance != null && MentalManager.Instance.IsLowMentalState)
        {
            ApplyLowMentalState(false);
        }
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
            refreshButton.onClick.AddListener(OnRefreshButtonClicked);
        }

        if (backButton != null && backButton.GetComponent<LowMentalBackButton>() == null)
        {
            backButton.onClick.RemoveListener(GoToLoginScreen);
            backButton.onClick.AddListener(GoToLoginScreen);
        }
    }

    private void AutoBindPostViewer()
    {
        if (postViewer != null)
        {
            return;
        }

        postViewer = FindFirstObjectByType<ReelViewerController>(FindObjectsInactive.Include);
        if (postViewer != null)
        {
            return;
        }

        Canvas parentCanvas = GetComponentInParent<Canvas>();
        Transform parent = parentCanvas != null ? parentCanvas.transform : transform.root;
        postViewer = ReelViewerController.CreateDefault(parent);
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

    public void OnRefreshButtonClicked()
    {
        if (!CanRefresh())
        {
            ShowRefreshMessage("아직 모든 릴스를 확인하지 않았습니다.");
            return;
        }

        RefreshReels();
    }

    public bool CanRefresh()
    {
        for (int i = 0; i < currentReelCompleted.Length; i++)
        {
            if (!currentReelCompleted[i])
            {
                return false;
            }
        }

        return true;
    }

    private void GoToLoginScreen()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.Login);
        }
    }

    public void RefreshReels()
    {
        GenerateNewReels();
    }

    private void GenerateNewReels()
    {
        ReelEntry[] activePool = GetActiveReelPool();
        if (activePool == null || activePool.Length == 0)
        {
            Debug.LogWarning("ThreeReelsGrid has no reel data. Fill Reels/Normal Reels/Strange Reels in the Inspector.");
            return;
        }

        bool isUsingStrangePool = IsUsingStrangePool();
        ResetUsedHistoryIfPoolChanged(isUsingStrangePool);
        ResetUsedHistoryIfNotEnoughReels(activePool);

        List<int> usedIndices = new List<int>();
        int miniGameCountInBatch = 0;
        for (int i = 0; i < currentReels.Length; i++)
        {
            currentReels[i] = PickRandomReel(activePool, usedIndices, miniGameCountInBatch == 0);
            if (currentReels[i] != null && currentReels[i].isMiniGamePost)
            {
                miniGameCountInBatch++;
            }

            currentReelCompleted[i] = false;
            ApplySlotVisual(i, currentReels[i]);
        }

        SaveCurrentSession();
        ShowRefreshMessage(string.Empty);
    }

    private ReelEntry[] GetActiveReelPool()
    {
        if (isLowMentalState && strangeReels != null && strangeReels.Length > 0)
        {
            return strangeReels;
        }

        return reels;
    }

    private bool IsUsingStrangePool()
    {
        return isLowMentalState && strangeReels != null && strangeReels.Length > 0;
    }

    private void ResetUsedHistoryIfPoolChanged(bool isUsingStrangePool)
    {
        if (wasUsingStrangePool == isUsingStrangePool)
        {
            return;
        }

        usedReelIndicesInCycle.Clear();
        wasUsingStrangePool = isUsingStrangePool;
    }

    private void ResetUsedHistoryIfNotEnoughReels(ReelEntry[] activePool)
    {
        int remainingCount = CountRemainingReels(activePool);
        if (remainingCount == 0)
        {
            usedReelIndicesInCycle.Clear();
        }
    }

    private int CountRemainingReels(ReelEntry[] activePool)
    {
        int count = 0;
        for (int i = 0; i < activePool.Length; i++)
        {
            if (activePool[i] != null && !usedReelIndicesInCycle.Contains(i))
            {
                count++;
            }
        }

        return count;
    }

    private ReelEntry PickRandomReel(ReelEntry[] activePool, List<int> usedIndices, bool canPickMiniGame)
    {
        bool wantsMiniGame = canPickMiniGame && Random.value < miniGameReelChance;
        List<int> candidateIndices = BuildCandidateIndices(activePool, usedIndices, wantsMiniGame);

        if (candidateIndices.Count == 0 && canPickMiniGame)
        {
            candidateIndices = BuildCandidateIndices(activePool, usedIndices, !wantsMiniGame);
        }

        if (candidateIndices.Count == 0)
        {
            candidateIndices = BuildCandidateIndices(activePool, usedIndices, canPickMiniGame ? null : false);
        }

        if (candidateIndices.Count == 0)
        {
            usedReelIndicesInCycle.Clear();
            candidateIndices = BuildCandidateIndices(activePool, usedIndices, canPickMiniGame ? null : false);
        }

        if (candidateIndices.Count == 0)
        {
            return null;
        }

        int index = candidateIndices[Random.Range(0, candidateIndices.Count)];
        usedIndices.Add(index);
        usedReelIndicesInCycle.Add(index);
        return activePool[index];
    }

    private List<int> BuildCandidateIndices(ReelEntry[] activePool, List<int> usedIndices, bool? miniGameFilter)
    {
        List<int> candidateIndices = new List<int>();
        bool checkDuplicate = preventDuplicateOnScreen && activePool.Length > currentReels.Length;

        for (int i = 0; i < activePool.Length; i++)
        {
            ReelEntry entry = activePool[i];
            if (entry == null)
            {
                continue;
            }

            if (checkDuplicate && usedIndices.Contains(i))
            {
                continue;
            }

            if (usedReelIndicesInCycle.Contains(i))
            {
                continue;
            }

            if (miniGameFilter.HasValue && entry.isMiniGamePost != miniGameFilter.Value)
            {
                continue;
            }

            candidateIndices.Add(i);
        }

        return candidateIndices;
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

        Button targetButton = GetSlotButton(slotIndex);
        if (targetButton != null)
        {
            ColorBlock colors = targetButton.colors;
            colors.normalColor = currentReelCompleted[slotIndex] ? new Color(0.55f, 0.55f, 0.55f, 1f) : Color.white;
            targetButton.colors = colors;
        }
    }

    private void AutoBindRefreshMessageText()
    {
        if (refreshMessageTMPText != null)
        {
            refreshMessageTMPText.text = string.Empty;
            return;
        }

        TMP_Text[] textCandidates = transform.root.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < textCandidates.Length; i++)
        {
            TMP_Text candidate = textCandidates[i];
            if (candidate != null && candidate.name == refreshMessageObjectName)
            {
                refreshMessageTMPText = candidate;
                refreshMessageTMPText.text = string.Empty;
                return;
            }
        }

        for (int i = 0; i < textCandidates.Length; i++)
        {
            TMP_Text candidate = textCandidates[i];
            if (candidate == null)
            {
                continue;
            }

            string lowerName = candidate.name.ToLowerInvariant();
            if (lowerName.Contains("refresh") && lowerName.Contains("message"))
            {
                refreshMessageTMPText = candidate;
                refreshMessageTMPText.text = string.Empty;
                return;
            }
        }

        CreateRefreshMessageText();
    }

    private void CreateRefreshMessageText()
    {
        Transform parent = transform.parent != null ? transform.parent : transform;
        GameObject messageObject = new GameObject(refreshMessageObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        messageObject.transform.SetParent(parent, false);
        messageObject.transform.SetAsLastSibling();

        RectTransform rectTransform = messageObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0f, 105f);
        rectTransform.sizeDelta = new Vector2(760f, 44f);

        TextMeshProUGUI messageText = messageObject.GetComponent<TextMeshProUGUI>();
        messageText.text = string.Empty;
        messageText.fontSize = 24f;
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.color = new Color(0.75f, 0.12f, 0.12f, 1f);
        messageText.raycastTarget = false;

        refreshMessageTMPText = messageText;
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

    private Button GetSlotButton(int slotIndex)
    {
        switch (slotIndex)
        {
            case 0:
                return leftButton;
            case 1:
                return centerButton;
            case 2:
                return rightButton;
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

        if (selectedReel.isMiniGamePost)
        {
            OpenMiniGameReel(selectedReel, slotIndex);
            return;
        }

        CompleteReel(slotIndex, true);
        OpenPostViewer(selectedReel, slotIndex);
    }

    private void OpenMiniGameReel(ReelEntry selectedReel, int slotIndex)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.BeginReelsSession();
        }

        if (string.IsNullOrWhiteSpace(selectedReel.sceneName))
        {
            Debug.LogWarning($"Reel '{selectedReel.title}' has an empty sceneName.");
            return;
        }

        pendingMiniGameCompletedSlot = slotIndex;
        SaveCurrentSession();
        Debug.Log($"Open mini game reel: {selectedReel.title} -> {selectedReel.sceneName}");
        SceneManager.LoadScene(selectedReel.sceneName);
    }

    private void OpenPostViewer(ReelEntry selectedReel, int slotIndex)
    {
        if (postViewer == null)
        {
            Debug.LogWarning($"Reel '{selectedReel.title}' is not a mini game post, but no ReelViewerController is assigned.");
            return;
        }

        postViewer.Open(selectedReel, () => CompleteReel(slotIndex, true));
    }

    private void CompleteReel(int slotIndex, bool decreaseMental)
    {
        if (slotIndex < 0 || slotIndex >= currentReelCompleted.Length || currentReelCompleted[slotIndex])
        {
            return;
        }

        currentReelCompleted[slotIndex] = true;
        SaveCurrentSession();

        // 릴스 하나를 확인하면 정신력 -3.
        if (decreaseMental && MentalManager.Instance != null)
        {
            MentalManager.Instance.DecreaseMental(3);
        }

        ApplySlotVisual(slotIndex, currentReels[slotIndex]);
    }

    public static void NotifyMiniGameCompleted()
    {
        if (pendingMiniGameCompletedSlot < 0)
        {
            return;
        }

        savedCompleted[pendingMiniGameCompletedSlot] = true;
    }

    public static void ResetPersistentSession()
    {
        hasSavedSession = false;
        savedLowMentalState = false;
        pendingMiniGameCompletedSlot = -1;
        savedUsedReelIndicesInCycle.Clear();
        savedWasUsingStrangePool = false;

        for (int i = 0; i < savedReels.Length; i++)
        {
            savedReels[i] = null;
            savedCompleted[i] = false;
        }
    }

    private void SaveCurrentSession()
    {
        for (int i = 0; i < currentReels.Length; i++)
        {
            savedReels[i] = currentReels[i];
            savedCompleted[i] = currentReelCompleted[i];
        }

        hasSavedSession = true;
        savedUsedReelIndicesInCycle.Clear();
        savedUsedReelIndicesInCycle.AddRange(usedReelIndicesInCycle);
        savedWasUsingStrangePool = wasUsingStrangePool;
    }

    private void RestoreSavedSession()
    {
        usedReelIndicesInCycle.Clear();
        usedReelIndicesInCycle.AddRange(savedUsedReelIndicesInCycle);
        wasUsingStrangePool = savedWasUsingStrangePool;

        for (int i = 0; i < currentReels.Length; i++)
        {
            currentReels[i] = savedReels[i];
            currentReelCompleted[i] = savedCompleted[i];
            ApplySlotVisual(i, currentReels[i]);
        }

        ShowRefreshMessage(string.Empty);
    }

    private void ShowRefreshMessage(string message)
    {
        if (refreshMessageRoutine != null)
        {
            StopCoroutine(refreshMessageRoutine);
            refreshMessageRoutine = null;
        }

        if (refreshMessageTMPText == null)
        {
            return;
        }

        refreshMessageTMPText.text = message ?? string.Empty;

        if (!string.IsNullOrEmpty(message))
        {
            refreshMessageRoutine = StartCoroutine(ClearRefreshMessageAfterDelay(3f));
        }
    }

    private IEnumerator ClearRefreshMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (refreshMessageTMPText != null)
        {
            refreshMessageTMPText.text = string.Empty;
        }

        refreshMessageRoutine = null;
    }

    private void SubscribeMentalEvents()
    {
        if (MentalManager.Instance != null)
        {
            MentalManager.Instance.OnLowMentalState -= OnLowMentalState;
            MentalManager.Instance.OnLowMentalState += OnLowMentalState;
        }
    }

    private void UnsubscribeMentalEvents()
    {
        if (MentalManager.Instance != null)
        {
            MentalManager.Instance.OnLowMentalState -= OnLowMentalState;
        }
    }

    private void OnLowMentalState()
    {
        ApplyLowMentalState(true);
    }

    private void ApplyLowMentalState(bool refreshContent)
    {
        if (isLowMentalState)
        {
            return;
        }

        isLowMentalState = true;
        savedLowMentalState = true;

        if (backgroundImage != null)
        {
            backgroundImage.sprite = lowMentalBackground != null ? lowMentalBackground : normalBackground;
            backgroundImage.color = Color.white;
        }

        if (backButton != null)
        {
            backButton.gameObject.SetActive(true);
        }

        if (lowMentalTintTargets != null)
        {
            for (int i = 0; i < lowMentalTintTargets.Length; i++)
            {
                if (lowMentalTintTargets[i] != null)
                {
                    lowMentalTintTargets[i].color = lowMentalUIColor;
                }
            }
        }

        if (refreshContent)
        {
            RefreshReels();
        }
    }

}
