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

    [Header("Paging / Pooling")]
    public int pageSize = 6; // 한 번에 로딩할 아이템 수
    public float loadThreshold = 0.15f; // 스크롤 하단에서 다음 페이지 로드 시작 위치

    // 내부 상태
    private int currentPage = 0;
    private bool isLoading = false;

    void Awake() {
        if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();
        if (scrollRect != null) scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
    }

    void Start() {
        if (FeedManager.Instance == null) {
            Debug.LogError("FeedManager instance not found in scene. Add FeedManager component to a GameObject.");
            return;
        }

        // 첫 페이지 로딩
        LoadNextPage();
    }

    void OnDestroy() {
        if (scrollRect != null) scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
    }

    // 스크롤 위치 변화 감지 → 대기 아래 다다르면 다음 페이지 로드
    private void OnScrollValueChanged(Vector2 normalizedPos) {
        if (isLoading) return;
        if (scrollRect.verticalNormalizedPosition <= loadThreshold) {
            LoadNextPage();
        }
    }

    /// <summary>
    /// 다음 페이지를 로드하여 Content에 아이템 인스턴스화
    /// </summary>
    public void LoadNextPage() {
        if (isLoading) return;
        isLoading = true;

        var posts = FeedManager.Instance.GetNextPage(pageSize, currentPage);
        if (posts == null || posts.Count == 0) {
            isLoading = false;
            return;
        }

        // 각 포스트를 인스턴스화하고 바인딩
        foreach (var p in posts) {
            var go = Instantiate(itemPrefab, content);
            go.transform.SetParent(content, false);
            go.SetActive(true);

            var item = go.GetComponent<PostItem>();
            if (item != null) item.Bind(p);
        }

        currentPage++;
        isLoading = false;

        // 레이아웃 갱신
        Canvas.ForceUpdateCanvases();
    }
}

