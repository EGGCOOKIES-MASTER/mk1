using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// ============================================================
/// 【 ReelsScreen - 릴스 화면 (메인 플레이 공간) 】
/// 
/// 역할:
/// - SNS 릴스처럼 게시물 스크롤
/// - 게시물을 위아래로 스크롤 가능
/// - 일반 게시물과 미니게임 게시물 표시
/// - 배터리 실시간 업데이트
/// - 게시물 클릭 반응
/// ============================================================

public class ReelsScreen : MonoBehaviour
{
    // ===== UI 요소 =====
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private Text batteryText;
    [SerializeField] private Image batteryIcon;
    [SerializeField] private Button refreshButton;
    [SerializeField] private GameObject postPrefab;
    [SerializeField] private Sprite[] reelSprites;

    // ===== 게시물 데이터 =====
    private List<PostData> _posts = new List<PostData>();
    private float _miniGameProbability = 0.3f;

    private const int GridColumnCount = 3;
    private const int GridPostCount = 3;
    private GridLayoutGroup _cachedGridLayout;

    private const float GridCellWidth = 360f;
    private const float GridCellHeight = 240f;
    private const float GridSpacing = 5f;
    private const float CellAspectRatio = GridCellHeight / GridCellWidth;

    private float _runtimeCellWidth;
    private float _runtimeCellHeight;

    private readonly List<Image> _activeReelImages = new List<Image>();

    // ============================================================
    /// 【 Initialize() - 화면 초기화 】
    // ============================================================
    public void Initialize()
    {
        Debug.Log("📺 릴스 화면 초기화됨");

        ResolveReferences();
        if (content == null || scrollRect == null)
        {
            Debug.LogError("❌ ReelsScreen 초기화 실패: ScrollRect/Content 연결을 확인하세요.");
            return;
        }

        scrollRect.content = content;
        // 한 화면에 좌/중/우 3개를 고정 노출하므로 스크롤은 잠근다.
        scrollRect.horizontal = false;
        scrollRect.vertical = false;

        RectTransform contentRect = content;
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;

        LayoutElement existingLayoutElement = content.GetComponent<LayoutElement>();
        if (existingLayoutElement != null)
        {
            DestroyImmediate(existingLayoutElement);
        }

        EnsureGridLayout();
        ApplyGridLayoutSettings();

        if (refreshButton != null)
        {
            refreshButton.onClick.RemoveAllListeners();
            refreshButton.onClick.AddListener(OnRefreshButtonClicked);
        }

        GeneratePosts();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        UpdateBattery();
    }

    private void ResolveReferences()
    {
        if (scrollRect == null)
        {
            scrollRect = GetComponentInChildren<ScrollRect>(true);
        }

        if (scrollRect == null)
        {
            return;
        }

        RectTransform resolvedContent = null;
        if (scrollRect.content != null)
        {
            resolvedContent = scrollRect.content;
        }
        else
        {
            Transform guessed = scrollRect.transform.Find("Viewport/Content");
            if (guessed != null)
            {
                resolvedContent = guessed as RectTransform;
            }
        }

        if (resolvedContent != null && content != resolvedContent)
        {
            if (content != null)
            {
                Debug.LogWarning($"⚠️ content 참조가 ScrollRect.content와 달라 자동 교정합니다. old={content.name}, new={resolvedContent.name}");
            }
            content = resolvedContent;
        }
    }

    private void ApplyGridLayoutSettings()
    {
        float viewportWidth = GetViewportWidth();
        float availableWidth = Mathf.Max(1f, viewportWidth - (GridSpacing * (GridColumnCount - 1)));

        _runtimeCellWidth = Mathf.Floor(availableWidth / GridColumnCount);
        _runtimeCellHeight = Mathf.Round(_runtimeCellWidth * CellAspectRatio);

        _cachedGridLayout.cellSize = new Vector2(_runtimeCellWidth, _runtimeCellHeight);
        _cachedGridLayout.spacing = new Vector2(GridSpacing, GridSpacing);
        _cachedGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        _cachedGridLayout.constraintCount = GridColumnCount;
        _cachedGridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        _cachedGridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        _cachedGridLayout.childAlignment = TextAnchor.UpperCenter;

        // stretch 앵커에서는 좌/우 패딩으로 폭을 제어하는 편이 안전하다.
        _cachedGridLayout.padding = new RectOffset(0, 0, 0, 0);

        Debug.Log($"✅ GridLayout 적용: viewport={viewportWidth:F1}, cell=({_runtimeCellWidth}, {_runtimeCellHeight}), spacing={GridSpacing}");
    }

    private float GetViewportWidth()
    {
        RectTransform viewport = scrollRect.viewport;
        if (viewport != null && viewport.rect.width > 0f)
        {
            return viewport.rect.width;
        }

        RectTransform scrollRectTransform = scrollRect.transform as RectTransform;
        if (scrollRectTransform != null && scrollRectTransform.rect.width > 0f)
        {
            return scrollRectTransform.rect.width;
        }

        return (GridCellWidth * GridColumnCount) + (GridSpacing * (GridColumnCount - 1));
    }

    private void EnsureGridLayout()
    {
        _cachedGridLayout = content.GetComponent<GridLayoutGroup>();
        if (_cachedGridLayout == null)
        {
            _cachedGridLayout = content.gameObject.AddComponent<GridLayoutGroup>();
        }

        if (content.GetComponent<ContentSizeFitter>() == null)
        {
            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }


    // ============================================================
    /// 【 GeneratePosts() - 게시물 생성 】
    // ============================================================
    private void GeneratePosts()
    {
        Debug.Log("🎬 게시물 생성 중...");

        // 기존 게시물 제거
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        _activeReelImages.Clear();
        _posts.Clear();

        // 인스타 3x3 느낌을 위해 9개 생성
        int postCount = GridPostCount;
        int refreshSeed = Random.Range(1000, 999999);
        for (int i = 0; i < postCount; i++)
        {
            bool isMiniGame = Random.value < _miniGameProbability;

            PostData newPost = new PostData
            {
                id = refreshSeed + i,
                isMiniGame = isMiniGame,
                title = isMiniGame ? "⭐ 특별 미니게임!" : GenerateRandomTitle(),
                description = isMiniGame ? "탭해서 미니게임을 해보세요!" : GenerateRandomDescription()
            };

            _posts.Add(newPost);
            DisplayPost(newPost);
        }

        int miniGameCount = _posts.FindAll(p => p.isMiniGame).Count;
        Debug.Log($"✅ {postCount}개의 게시물 생성됨 (미니게임: {miniGameCount}개)");

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    // ============================================================
    /// 【 DisplayPost() - 게시물 UI 표시 】
    // ============================================================
    private void DisplayPost(PostData post)
    {
        bool useFallback = postPrefab == null;
        GameObject postGo = useFallback
            ? CreateFallbackPostObject(post)
            : Instantiate(postPrefab, content, false);

        postGo.SetActive(true);

        Image backgroundImage = postGo.GetComponent<Image>();
        if (useFallback)
        {
            if (backgroundImage == null)
            {
                backgroundImage = postGo.AddComponent<Image>();
            }
        }

        if (backgroundImage != null)
        {
            backgroundImage.raycastTarget = true;
            ApplyRandomVisual(backgroundImage, post);
            _activeReelImages.Add(backgroundImage);
        }

        Button postButton = postGo.GetComponent<Button>();
        if (postButton == null)
        {
            postButton = postGo.AddComponent<Button>();
        }

        if (postButton.targetGraphic == null)
        {
            Graphic targetGraphic = backgroundImage != null
                ? backgroundImage
                : postGo.GetComponentInChildren<Graphic>(true);
            postButton.targetGraphic = targetGraphic;
        }

        postButton.onClick.RemoveAllListeners();
        postButton.onClick.AddListener(() => OnPostClicked(post));

        PostHandler postHandler = postGo.GetComponent<PostHandler>();
        if (postHandler == null)
        {
            postHandler = postGo.AddComponent<PostHandler>();
        }
        postHandler.SetPostData(post);

        HideAllTexts(postGo.transform);
    }

    private void ApplyRandomVisual(Image image, PostData post)
    {
        if (reelSprites != null && reelSprites.Length > 0)
        {
            int spriteIndex = Random.Range(0, reelSprites.Length);
            image.sprite = reelSprites[spriteIndex];
            image.color = Color.white;
            image.preserveAspect = true;
            return;
        }

        // 스프라이트를 아직 연결하지 않은 경우 색상으로만 구분
        image.sprite = null;
        image.color = post.isMiniGame
            ? new Color(1f, 0.82f, 0.2f, 1f)
            : GenerateTileColor(post);
    }

    private void HideAllTexts(Transform root)
    {
        Text[] texts = root.GetComponentsInChildren<Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].gameObject.SetActive(false);
        }
    }

    private GameObject CreateFallbackPostObject(PostData post)
    {
        GameObject postGo = new GameObject($"Post_{post.id}");
        postGo.transform.SetParent(content, false);

        RectTransform rectTransform = postGo.AddComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;

        LayoutElement layoutElement = postGo.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = _runtimeCellWidth > 0f ? _runtimeCellWidth : GridCellWidth;
        layoutElement.preferredHeight = _runtimeCellHeight > 0f ? _runtimeCellHeight : GridCellHeight;

        return postGo;
    }

    private void BindOrCreateTexts(Transform root, PostData post)
    {
        Text[] texts = root.GetComponentsInChildren<Text>(true);
        if (texts.Length > 0)
        {
            texts[0].text = post.title;
            if (texts.Length > 1)
            {
                texts[1].text = post.description;
            }
            return;
        }

        // 직사각형 셀에서는 제목을 크게 강조
        CreateOverlayText(root, "Title", post.title, 18, TextAnchor.LowerLeft, new Vector2(10f, 8f), new Vector2(-10f, -10f));
        if (post.isMiniGame)
        {
            CreateBadgeText(root, "미니게임", new Vector2(12f, -12f));
        }
    }

    private void CreateOverlayText(Transform parent, string objectName, string value, int fontSize, TextAnchor alignment, Vector2 leftBottom, Vector2 rightTop)
    {
        GameObject textGo = new GameObject(objectName);
        textGo.transform.SetParent(parent, false);

        RectTransform rectTransform = textGo.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = leftBottom;
        rectTransform.offsetMax = rightTop;

        Text text = textGo.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = value ?? string.Empty;
        text.alignment = alignment;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
    }

    private void CreateBadgeText(Transform parent, string value, Vector2 offset)
    {
        GameObject badgeGo = new GameObject("Badge");
        badgeGo.transform.SetParent(parent, false);

        RectTransform rectTransform = badgeGo.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.up;
        rectTransform.anchorMax = Vector2.up;
        rectTransform.pivot = Vector2.up;
        rectTransform.anchoredPosition = offset;
        rectTransform.sizeDelta = new Vector2(120f, 34f);

        Image badgeImage = badgeGo.AddComponent<Image>();
        badgeImage.color = new Color(0f, 0f, 0f, 0.45f);

        Text badgeText = badgeGo.AddComponent<Text>();
        badgeText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        badgeText.text = value;
        badgeText.alignment = TextAnchor.MiddleCenter;
        badgeText.fontSize = 14;
        badgeText.fontStyle = FontStyle.Bold;
        badgeText.color = Color.white;
        badgeText.raycastTarget = false;
    }

    private Color GenerateTileColor(PostData post)
    {
        int hash = Mathf.Abs(post.id.ToString().GetHashCode());
        float hue = (hash % 360) / 360f;
        float saturation = 0.28f + ((hash % 25) / 100f);
        float value = 0.65f + ((hash % 20) / 100f);
        Color color = Color.HSVToRGB(hue, Mathf.Clamp01(saturation), Mathf.Clamp01(value));
        color.a = 1f;
        return color;
    }

    // ============================================================
    /// 【 OnPostClicked() - 게시물 클릭 이벤트 】
    // ============================================================
    private void OnPostClicked(PostData post)
    {
        Debug.Log($"👆 게시물 클릭: {post.title}");

        if (post.isMiniGame)
        {
            GameManager.Instance.OnMiniGameStart();
        }
        else
        {
            GameManager.Instance.IncreaseSavedCount();
            Debug.Log($"❤️ 저장한 수 증가!");
        }
    }

    // ============================================================
    /// 【 UpdateBattery() - 배터리 UI 업데이트 】
    // ============================================================
    public void UpdateBattery()
    {
        if (GameManager.Instance == null) return;

        float battery = GameManager.Instance.Battery;

        if (batteryText != null)
        {
            batteryText.text = $"🔋 {battery:F1}%";
        }

        if (batteryIcon != null)
        {
            if (battery > 50f)
            {
                batteryIcon.color = new Color(0f, 1f, 0f, 1f);
            }
            else if (battery > 20f)
            {
                batteryIcon.color = new Color(1f, 1f, 0f, 1f);
            }
            else
            {
                batteryIcon.color = new Color(1f, 0f, 0f, 1f);
            }
        }
    }

    // ============================================================
    /// 【 OnRefreshButtonClicked() - 새로고침 버튼 클릭 】
    // ============================================================
    private void OnRefreshButtonClicked()
    {
        Debug.Log("🔄 새로고침!");
        if (content != null)
        {
            if (_activeReelImages.Count == GridPostCount)
            {
                for (int i = 0; i < _activeReelImages.Count; i++)
                {
                    if (_activeReelImages[i] == null) continue;
                    ApplyRandomVisual(_activeReelImages[i], _posts[i]);
                }
            }
            else
            {
                GeneratePosts();
            }
        }
        else if (GameManager.Instance != null)
        {
            // [TEMP] 화면 데이터가 없을 때는 새로고침 버튼을 다음 화면 이동 버튼처럼 사용
            GameManager.Instance.GoNextForDemo();
        }
    }

    // ============================================================
    /// 【 Update() - 매 프레임 배터리 업데이트 】
    // ============================================================
    void Update()
    {
        if (GameManager.Instance == null) return;
        UpdateBattery();
    }

    // ============================================================
    /// 【 게시물 데이터 구조체 】
    // ============================================================
    [System.Serializable]
    public class PostData
    {
        public int id;
        public bool isMiniGame;
        public string title;
        public string description;
    }

    // ============================================================
    /// 【 랜덤 제목 생성 】
    // ============================================================
    private string GenerateRandomTitle()
    {
        string[] titles = new string[]
        {
            "오늘의 일상 📸",
            "맛있는 음식 🍔",
            "여행 후기 ✈️",
            "운동 일지 💪",
            "공부하는 중... 📖",
            "너무 재밌음 😂",
            "이걸 봤어? 👀"
        };
        return titles[Random.Range(0, titles.Length)];
    }

    // ============================================================
    /// 【 랜덤 설명 생성 】
    // ============================================================
    private string GenerateRandomDescription()
    {
        string[] descriptions = new string[]
        {
            "정말 좋은 시간이었어요!",
            "꼭 봐야 해요 이거!",
            "너무 감동했어요...",
            "추천합니다! 👍",
            "이거 봤어? 대박이야!",
            "하루의 하이라이트 ✨",
            "놓치면 후회할 거야!"
        };
        return descriptions[Random.Range(0, descriptions.Length)];
    }
}

// ============================================================
/// 【 PostHandler - 게시물 데이터 핸들러 】
// ============================================================
public class PostHandler : MonoBehaviour
{
    private ReelsScreen.PostData _postData;

    public void SetPostData(ReelsScreen.PostData data)
    {
        _postData = data;
    }

    public ReelsScreen.PostData GetPostData()
    {
        return _postData;
    }
}
