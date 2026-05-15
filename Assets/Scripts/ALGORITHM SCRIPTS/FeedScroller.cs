using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 FeedScroller (최적화 버전)

 사용법:
 1. Canvas > ScrollRect > Viewport > Content 구성
 2. Content에 GridLayoutGroup 또는 VerticalLayoutGroup 추가
 3. PostItem prefab을 만들고 inspector의 itemPrefab에 할당
 4. 씬에 FeedManager 컴포넌트가 있어야 함
 */
public class FeedScroller : MonoBehaviour {
    [Header("References (assign in Inspector)")]
    public GameObject itemPrefab; // (필수) PostItem 스크립트가 붙은 prefab
    public RectTransform content;  // (필수) ScrollRect의 Content
    public ScrollRect scrollRect;  // (선택) 자동으로 GetComponent에서 찾음
    public Button refreshButton;   // (선택) 새로고침 버튼

    [Header("Visible Reels")]
    public int visibleReelCount = 3; // 좌/중/우 고정 3개
    public float reelSpacing = 12f;

    private bool _isRefreshing = false;

    void Awake() {
        if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();

        if (refreshButton != null) {
            refreshButton.onClick.RemoveListener(RefreshReels);
            refreshButton.onClick.AddListener(RefreshReels);
        }
    }

    void Start() {
        if (FeedManager.Instance == null) {
            Debug.LogError("FeedManager instance not found in scene. Add FeedManager component to a GameObject.");
            return;
        }

        RefreshReels();
    }

    void OnDestroy() {
        if (refreshButton != null) {
            refreshButton.onClick.RemoveListener(RefreshReels);
        }
    }

    /// <summary>
    /// 화면의 릴스 3개를 새로 뽑아 다시 표시
    /// </summary>
    public void RefreshReels() {
        if (_isRefreshing) return;
        _isRefreshing = true;

        if (FeedManager.Instance == null || itemPrefab == null || content == null) {
            Debug.LogError("FeedScroller is missing FeedManager/itemPrefab/content reference.");
            _isRefreshing = false;
            return;
        }

        ConfigureThreeColumnLayout();
        ClearContentChildren();

        List<PostData> posts = FeedManager.Instance.GetRandomPosts(visibleReelCount);
        if (posts == null || posts.Count == 0) {
            _isRefreshing = false;
            return;
        }

        foreach (var p in posts) {
            var go = Instantiate(itemPrefab, content);
            go.SetActive(true);

            var item = go.GetComponent<PostItem>();
            if (item != null) item.Bind(p);
        }

        Canvas.ForceUpdateCanvases();
        _isRefreshing = false;
    }

    // 기존 외부 호출 호환용: 다음 페이지 대신 현재 화면 새로고침으로 동작
    public void LoadNextPage() {
        RefreshReels();
    }

    private void ClearContentChildren() {
        for (int i = content.childCount - 1; i >= 0; i--) {
            Destroy(content.GetChild(i).gameObject);
        }
    }

    private void ConfigureThreeColumnLayout() {
        GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
        if (grid == null) {
            grid = content.gameObject.AddComponent<GridLayoutGroup>();
        }

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = Mathf.Max(1, visibleReelCount);
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.spacing = new Vector2(reelSpacing, 0f);
        grid.childAlignment = TextAnchor.UpperCenter;

        float baseWidth = content.rect.width;
        if (baseWidth <= 0f && content.parent is RectTransform parentRect) {
            baseWidth = parentRect.rect.width;
        }
        if (baseWidth <= 0f) {
            baseWidth = 1080f;
        }

        float totalSpacing = reelSpacing * (grid.constraintCount - 1);
        float cellWidth = Mathf.Max(80f, (baseWidth - totalSpacing) / grid.constraintCount);
        float cellHeight = cellWidth * 1.2f;
        grid.cellSize = new Vector2(cellWidth, cellHeight);

        if (scrollRect != null) {
            scrollRect.horizontal = false;
            scrollRect.vertical = false;
        }
    }
}

