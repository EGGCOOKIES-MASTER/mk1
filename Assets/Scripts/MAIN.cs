using UnityEngine;
using UnityEngine.UI;

// UIManager 클래스: 게임의 모든 UI를 관리하는 싱글톤 클래스
public class UIManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static UIManager Instance;

    // UI 패널들
    public GameObject homeUI; // 홈 화면 UI
    public GameObject snsUI; // SNS 피드 화면 UI
    public GameObject miniGameUI; // 미니게임 화면 UI
    public GameObject endingUI; // 엔딩 화면 UI

    // 배터리 시스템
    public Text batteryText; // 배터리 표시 텍스트
    public float batteryLevel = 100f; // 현재 배터리 레벨
    public float batteryDrainRate = 1f; // 배터리 감소 속도 (초당)

    // 게시물 컨텐츠를 위한 게임 오브젝트
    private GameObject contentGO;

    // 싱글톤 초기화
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 게임 시작 시 UI 생성 및 초기화
    private void Start()
    {
        CreateUI();
        ShowHomeUI();
        UpdateBatteryUI();
    }

    // 매 프레임마다 배터리 감소 처리
    private void Update()
    {
        if (snsUI.activeSelf)
        {
            DrainBattery();
        }
    }

    // 모든 UI 요소를 생성하는 메서드
    private void CreateUI()
    {
        // 캔버스 생성
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // 홈 UI 생성
        homeUI = new GameObject("HomeUI");
        homeUI.transform.SetParent(canvasGO.transform);
        RectTransform homeRect = homeUI.AddComponent<RectTransform>();
        homeRect.anchorMin = Vector2.zero;
        homeRect.anchorMax = Vector2.one;
        homeRect.offsetMin = Vector2.zero;
        homeRect.offsetMax = Vector2.zero;

        // 타이틀 텍스트 생성
        GameObject titleTextGO = new GameObject("TitleText");
        titleTextGO.transform.SetParent(homeUI.transform);
        Text titleText = titleTextGO.AddComponent<Text>();
        titleText.text = "1%: SNS 중독 시뮬레이터";
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 40;
        titleText.alignment = TextAnchor.MiddleCenter;
        RectTransform titleRect = titleTextGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleRect.sizeDelta = new Vector2(400, 50);

        // 시작 버튼 생성
        GameObject startButtonGO = new GameObject("StartButton");
        startButtonGO.transform.SetParent(homeUI.transform);
        Button startButton = startButtonGO.AddComponent<Button>();
        startButton.onClick.AddListener(StartGame);
        Image startImage = startButtonGO.AddComponent<Image>();
        startImage.color = Color.blue;
        Text startText = new GameObject("StartText").AddComponent<Text>();
        startText.transform.SetParent(startButtonGO.transform);
        startText.text = "시작";
        startText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        startText.alignment = TextAnchor.MiddleCenter;
        RectTransform startRect = startButtonGO.GetComponent<RectTransform>();
        startRect.anchorMin = new Vector2(0.5f, 0.4f);
        startRect.anchorMax = new Vector2(0.5f, 0.4f);
        startRect.sizeDelta = new Vector2(200, 50);
        RectTransform startTextRect = startText.GetComponent<RectTransform>();
        startTextRect.anchorMin = Vector2.zero;
        startTextRect.anchorMax = Vector2.one;
        startTextRect.offsetMin = Vector2.zero;
        startTextRect.offsetMax = Vector2.zero;

        // SNS UI 생성
        snsUI = new GameObject("SNSUI");
        snsUI.transform.SetParent(canvasGO.transform);
        RectTransform snsRect = snsUI.AddComponent<RectTransform>();
        snsRect.anchorMin = Vector2.zero;
        snsRect.anchorMax = Vector2.one;
        snsRect.offsetMin = Vector2.zero;
        snsRect.offsetMax = Vector2.zero;

        // 배터리 텍스트 생성
        GameObject batteryGO = new GameObject("BatteryText");
        batteryGO.transform.SetParent(snsUI.transform);
        batteryText = batteryGO.AddComponent<Text>();
        batteryText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        batteryText.fontSize = 20;
        RectTransform batteryRect = batteryGO.GetComponent<RectTransform>();
        batteryRect.anchorMin = new Vector2(1, 1);
        batteryRect.anchorMax = new Vector2(1, 1);
        batteryRect.anchoredPosition = new Vector2(-50, -50);
        batteryRect.sizeDelta = new Vector2(100, 30);

        // 새로고침 버튼 생성
        GameObject refreshButtonGO = new GameObject("RefreshButton");
        refreshButtonGO.transform.SetParent(snsUI.transform);
        Button refreshButton = refreshButtonGO.AddComponent<Button>();
        refreshButton.onClick.AddListener(RefreshPosts);
        Image refreshImage = refreshButtonGO.AddComponent<Image>();
        refreshImage.color = Color.green;
        Text refreshText = new GameObject("RefreshText").AddComponent<Text>();
        refreshText.transform.SetParent(refreshButtonGO.transform);
        refreshText.text = "새로고침";
        refreshText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        refreshText.alignment = TextAnchor.MiddleCenter;
        RectTransform refreshRect = refreshButtonGO.GetComponent<RectTransform>();
        refreshRect.anchorMin = new Vector2(0, 1);
        refreshRect.anchorMax = new Vector2(0, 1);
        refreshRect.anchoredPosition = new Vector2(50, -50);
        refreshRect.sizeDelta = new Vector2(100, 30);
        RectTransform refreshTextRect = refreshText.GetComponent<RectTransform>();
        refreshTextRect.anchorMin = Vector2.zero;
        refreshTextRect.anchorMax = Vector2.one;
        refreshTextRect.offsetMin = Vector2.zero;
        refreshTextRect.offsetMax = Vector2.zero;

        // 스크롤 뷰 생성 (피드용)
        GameObject scrollViewGO = new GameObject("ScrollView");
        scrollViewGO.transform.SetParent(snsUI.transform);
        ScrollRect scrollRect = scrollViewGO.AddComponent<ScrollRect>();
        Image scrollImage = scrollViewGO.AddComponent<Image>();
        scrollImage.color = Color.white;
        RectTransform scrollRectTransform = scrollViewGO.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = Vector2.zero;
        scrollRectTransform.anchorMax = Vector2.one;
        scrollRectTransform.offsetMin = new Vector2(0, 50);
        scrollRectTransform.offsetMax = new Vector2(0, -50);

        // 뷰포트 생성
        GameObject viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollViewGO.transform);
        RectTransform viewportRect = viewportGO.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewportGO.AddComponent<Mask>();
        scrollRect.viewport = viewportRect;

        // 컨텐츠 생성
        contentGO = new GameObject("Content");
        contentGO.transform.SetParent(viewportGO.transform);
        RectTransform contentRect = contentGO.AddComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 1000); // 높이 조정 필요
        VerticalLayoutGroup vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.childControlHeight = false;
        vlg.childForceExpandHeight = false;
        scrollRect.content = contentRect;

        // 미니게임 UI 생성
        miniGameUI = new GameObject("MiniGameUI");
        miniGameUI.transform.SetParent(canvasGO.transform);
        RectTransform miniRect = miniGameUI.AddComponent<RectTransform>();
        miniRect.anchorMin = Vector2.zero;
        miniRect.anchorMax = Vector2.one;
        miniRect.offsetMin = Vector2.zero;
        miniRect.offsetMax = Vector2.zero;

        // 미니게임 타이틀 텍스트 생성
        GameObject miniTextGO = new GameObject("MiniGameText");
        miniTextGO.transform.SetParent(miniGameUI.transform);
        Text miniText = miniTextGO.AddComponent<Text>();
        miniText.text = "좋아요 연타 미니게임!";
        miniText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        miniText.fontSize = 30;
        miniText.alignment = TextAnchor.MiddleCenter;
        RectTransform miniTextRect = miniTextGO.GetComponent<RectTransform>();
        miniTextRect.anchorMin = new Vector2(0.5f, 0.8f);
        miniTextRect.anchorMax = new Vector2(0.5f, 0.8f);
        miniTextRect.sizeDelta = new Vector2(400, 50);

        // 좋아요 버튼 생성
        GameObject likeButtonGO = new GameObject("LikeButton");
        likeButtonGO.transform.SetParent(miniGameUI.transform);
        Button likeButton = likeButtonGO.AddComponent<Button>();
        Image likeImage = likeButtonGO.AddComponent<Image>();
        likeImage.color = Color.red;
        Text likeText = new GameObject("LikeText").AddComponent<Text>();
        likeText.transform.SetParent(likeButtonGO.transform);
        likeText.text = "좋아요!";
        likeText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        likeText.alignment = TextAnchor.MiddleCenter;
        RectTransform likeRect = likeButtonGO.GetComponent<RectTransform>();
        likeRect.anchorMin = new Vector2(0.5f, 0.5f);
        likeRect.anchorMax = new Vector2(0.5f, 0.5f);
        likeRect.sizeDelta = new Vector2(200, 100);
        RectTransform likeTextRect = likeText.GetComponent<RectTransform>();
        likeTextRect.anchorMin = Vector2.zero;
        likeTextRect.anchorMax = Vector2.one;
        likeTextRect.offsetMin = Vector2.zero;
        likeTextRect.offsetMax = Vector2.zero;

        // 카운트 텍스트 생성
        GameObject countTextGO = new GameObject("CountText");
        countTextGO.transform.SetParent(miniGameUI.transform);
        Text countText = countTextGO.AddComponent<Text>();
        countText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        countText.fontSize = 20;
        countText.alignment = TextAnchor.MiddleCenter;
        RectTransform countRect = countTextGO.GetComponent<RectTransform>();
        countRect.anchorMin = new Vector2(0.5f, 0.6f);
        countRect.anchorMax = new Vector2(0.5f, 0.6f);
        countRect.sizeDelta = new Vector2(200, 30);

        // 타이머 텍스트 생성
        GameObject timerTextGO = new GameObject("TimerText");
        timerTextGO.transform.SetParent(miniGameUI.transform);
        Text timerText = timerTextGO.AddComponent<Text>();
        timerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        timerText.fontSize = 20;
        timerText.alignment = TextAnchor.MiddleCenter;
        RectTransform timerRect = timerTextGO.GetComponent<RectTransform>();
        timerRect.anchorMin = new Vector2(0.5f, 0.4f);
        timerRect.anchorMax = new Vector2(0.5f, 0.4f);
        timerRect.sizeDelta = new Vector2(200, 30);

        // 돌아가기 버튼 생성
        GameObject backButtonGO = new GameObject("BackButton");
        backButtonGO.transform.SetParent(miniGameUI.transform);
        Button backButton = backButtonGO.AddComponent<Button>();
        backButton.onClick.AddListener(ReturnToSNS);
        Image backImage = backButtonGO.AddComponent<Image>();
        backImage.color = Color.gray;
        Text backText = new GameObject("BackText").AddComponent<Text>();
        backText.transform.SetParent(backButtonGO.transform);
        backText.text = "돌아가기";
        backText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        backText.alignment = TextAnchor.MiddleCenter;
        RectTransform backRect = backButtonGO.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0.5f, 0.2f);
        backRect.anchorMax = new Vector2(0.5f, 0.2f);
        backRect.sizeDelta = new Vector2(200, 50);
        RectTransform backTextRect = backText.GetComponent<RectTransform>();
        backTextRect.anchorMin = Vector2.zero;
        backTextRect.anchorMax = Vector2.one;
        backTextRect.offsetMin = Vector2.zero;
        backTextRect.offsetMax = Vector2.zero;

        // 미니게임 매니저 컴포넌트 추가
        MiniGameManager miniGameManager = miniGameUI.AddComponent<MiniGameManager>();
        miniGameManager.likeButton = likeButton;
        miniGameManager.likeCountText = countText;
        miniGameManager.timerText = timerText;

        // 엔딩 UI 생성
        endingUI = new GameObject("EndingUI");
        endingUI.transform.SetParent(canvasGO.transform);
        RectTransform endingRect = endingUI.AddComponent<RectTransform>();
        endingRect.anchorMin = Vector2.zero;
        endingRect.anchorMax = Vector2.one;
        endingRect.offsetMin = Vector2.zero;
        endingRect.offsetMax = Vector2.zero;

        // 엔딩 텍스트 생성
        GameObject endingTextGO = new GameObject("EndingText");
        endingTextGO.transform.SetParent(endingUI.transform);
        Text endingText = endingTextGO.AddComponent<Text>();
        endingText.text = "배터리가 1%가 되었습니다. SNS 중독의 끝은 무엇일까요?";
        endingText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        endingText.fontSize = 25;
        endingText.alignment = TextAnchor.MiddleCenter;
        RectTransform endingTextRect = endingTextGO.GetComponent<RectTransform>();
        endingTextRect.anchorMin = new Vector2(0.5f, 0.5f);
        endingTextRect.anchorMax = new Vector2(0.5f, 0.5f);
        endingTextRect.sizeDelta = new Vector2(500, 100);

        // 초기 게시물 생성
        GeneratePosts();
    }

    // 게시물을 생성하는 메서드
    private void GeneratePosts()
    {
        // 기존 게시물 제거
        foreach (Transform child in contentGO.transform)
        {
            Destroy(child.gameObject);
        }

        // 6~8개의 게시물 생성
        int postCount = Random.Range(6, 9);
        for (int i = 0; i < postCount; i++)
        {
            CreatePost();
        }
    }

    // 개별 게시물을 생성하는 메서드
    private void CreatePost()
    {
        // 게시물 게임 오브젝트 생성
        GameObject postGO = new GameObject("Post");
        postGO.transform.SetParent(contentGO.transform);
        RectTransform postRect = postGO.AddComponent<RectTransform>();
        postRect.sizeDelta = new Vector2(0, 100);

        // 이미지 컴포넌트 추가
        Image postImage = postGO.AddComponent<Image>();
        postImage.color = Color.gray;

        // 텍스트 게임 오브젝트 생성
        GameObject textGO = new GameObject("PostText");
        textGO.transform.SetParent(postGO.transform);
        Text postText = textGO.AddComponent<Text>();
        postText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        postText.fontSize = 20;
        postText.alignment = TextAnchor.MiddleCenter;
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // 버튼 컴포넌트 추가
        Button postButton = postGO.AddComponent<Button>();
        PostItem postItem = postGO.AddComponent<PostItem>();
        postItem.postText = postText;
        postItem.postButton = postButton;

        // 랜덤으로 미니게임 게시물 여부 결정 (20% 확률)
        bool isMiniGame = Random.value < 0.2f;
        string postContent = isMiniGame ? "미니게임에 참여해보세요!" : "일반 게시물: " + Random.Range(1, 100);
        postItem.SetPost(postContent, isMiniGame);
    }

    // 홈 UI를 표시하는 메서드
    public void ShowHomeUI()
    {
        homeUI.SetActive(true);
        snsUI.SetActive(false);
        miniGameUI.SetActive(false);
        endingUI.SetActive(false);
    }

    // SNS UI를 표시하는 메서드
    public void ShowSNSUI()
    {
        homeUI.SetActive(false);
        snsUI.SetActive(true);
        miniGameUI.SetActive(false);
        endingUI.SetActive(false);
    }

    // 미니게임 UI를 표시하는 메서드
    public void ShowMiniGameUI()
    {
        homeUI.SetActive(false);
        snsUI.SetActive(false);
        miniGameUI.SetActive(true);
        endingUI.SetActive(false);
    }

    // 엔딩 UI를 표시하는 메서드
    public void ShowEndingUI()
    {
        homeUI.SetActive(false);
        snsUI.SetActive(false);
        miniGameUI.SetActive(false);
        endingUI.SetActive(true);
    }

    // 배터리를 감소시키는 메서드
    private void DrainBattery()
    {
        batteryLevel -= batteryDrainRate * Time.deltaTime;
        if (batteryLevel <= 1)
        {
            batteryLevel = 1;
            ShowEndingUI();
        }
        UpdateBatteryUI();
    }

    // 배터리 UI를 업데이트하는 메서드
    private void UpdateBatteryUI()
    {
        if (batteryText != null)
        {
            batteryText.text = Mathf.CeilToInt(batteryLevel) + "%";
        }
    }

    // 게임을 시작하는 메서드
    public void StartGame()
    {
        ShowSNSUI();
    }

    // SNS로 돌아가는 메서드
    public void ReturnToSNS()
    {
        ShowSNSUI();
    }

    // 게시물을 새로고침하는 메서드
    public void RefreshPosts()
    {
        GeneratePosts();
    }
}
