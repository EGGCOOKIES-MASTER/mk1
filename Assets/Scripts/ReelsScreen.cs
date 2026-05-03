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

    // ===== 게시물 데이터 =====
    private List<PostData> posts = new List<PostData>();
    private float miniGameProbability = 0.3f;

    // ============================================================
    /// 【 Initialize() - 화면 초기화 】
    // ============================================================
    public void Initialize()
    {
        Debug.Log("📺 릴스 화면 초기화됨");

        if (scrollRect != null)
        {
            scrollRect.content = content;
        }

        if (refreshButton != null)
        {
            refreshButton.onClick.RemoveAllListeners();
            refreshButton.onClick.AddListener(OnRefreshButtonClicked);
        }

        // [TEMP] refreshButton이 비어 있으면 하위 버튼 하나를 찾아 다음 화면 테스트에 사용
        if (refreshButton == null)
        {
            Button fallbackButton = GetComponentInChildren<Button>();
            if (fallbackButton != null)
            {
                fallbackButton.onClick.RemoveAllListeners();
                fallbackButton.onClick.AddListener(() =>
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.GoNextForDemo();
                    }
                });
                Debug.Log("[TEMP] ReelsScreen fallback 버튼에 GoNextForDemo 연결 완료");
            }
        }

        // GeneratePosts();
        // UpdateBattery();

        // [TEMP] 참조가 비어 있어도 화면 전환 테스트가 끊기지 않도록 안전 호출
        if (content != null)
        {
            GeneratePosts();
        }
        else
        {
            Debug.LogWarning("⚠️ content가 비어 있어 게시물 생성을 건너뜁니다.");
        }

        UpdateBattery();
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
        posts.Clear();

        // 게시물 6~8개 생성
        int postCount = Random.Range(6, 9);
        for (int i = 0; i < postCount; i++)
        {
            bool isMiniGame = Random.value < miniGameProbability;

            PostData newPost = new PostData
            {
                id = i,
                isMiniGame = isMiniGame,
                title = isMiniGame ? "⭐ 특별 미니게임!" : GenerateRandomTitle(),
                description = isMiniGame ? "탭해서 미니게임을 해보세요!" : GenerateRandomDescription()
            };

            posts.Add(newPost);
            DisplayPost(newPost);
        }

        int miniGameCount = posts.FindAll(p => p.isMiniGame).Count;
        Debug.Log($"✅ {postCount}개의 게시물 생성됨 (미니게임: {miniGameCount}개)");
    }

    // ============================================================
    /// 【 DisplayPost() - 게시물 UI 표시 】
    // ============================================================
    private void DisplayPost(PostData post)
    {
        GameObject postGO;
        
        if (postPrefab != null)
        {
            postGO = Instantiate(postPrefab, content);
        }
        else
        {
            postGO = new GameObject($"Post_{post.id}");
            postGO.transform.SetParent(content);
            postGO.AddComponent<RectTransform>().sizeDelta = new Vector2(1080, 1920);
        }

        Button postButton = postGO.GetComponent<Button>();
        if (postButton == null)
        {
            postButton = postGO.AddComponent<Button>();
        }

        PostHandler postHandler = postGO.GetComponent<PostHandler>();
        if (postHandler == null)
        {
            postHandler = postGO.AddComponent<PostHandler>();
        }
        postHandler.SetPostData(post);

        postButton.onClick.RemoveAllListeners();
        postButton.onClick.AddListener(() => OnPostClicked(post));

        Text[] texts = postGO.GetComponentsInChildren<Text>();
        if (texts.Length > 0)
        {
            texts[0].text = post.title;
        }
        if (texts.Length > 1)
        {
            texts[1].text = post.description;
        }

        Image[] images = postGO.GetComponentsInChildren<Image>();
        if (images.Length > 0)
        {
            if (post.isMiniGame)
            {
                images[0].color = new Color(1f, 0.8f, 0f, 1f);
            }
            else
            {
                images[0].color = new Color(1f, 1f, 1f, 1f);
            }
        }
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
            GeneratePosts();
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
    private ReelsScreen.PostData postData;

    public void SetPostData(ReelsScreen.PostData data)
    {
        postData = data;
    }

    public ReelsScreen.PostData GetPostData()
    {
        return postData;
    }
}

