using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ShooterGameManager : MonoBehaviour
{
    public static ShooterGameManager Instance;

    [Header("프리팹 및 캔버스 연결")]
    public GameObject itemPrefab;
    public Transform canvasTransform;

    [Header("UI 패널 및 텍스트 연결")]
    public GameObject gameOverPanel;
    public GameObject gameClearPanel;
    public TMPro.TextMeshProUGUI timerText;
    public TMPro.TextMeshProUGUI finalScoreText;
    public Image[] heartImages;

    [Header("게임 세팅")]
    public float spawnInterval = 0.6f;   // 빌런 젠 속도
    public float targetClearTime = 30f;  // 목표 버티기 시간
    public int maxLives = 3;

    private float spawnTimer = 0f;
    private float survivalTime = 0f;
    private int score = 0;
    private int currentLives;
    private bool isGameFinished = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentLives = maxLives;
        survivalTime = 0f;
        score = 0;
        isGameFinished = false;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameClearPanel != null) gameClearPanel.SetActive(false);

        UpdateHeartUI();
        UpdateTimerUI();
    }

    void Update()
    {
        if (isGameFinished) return;

        // 1. 시간 흐름 및 승리 체크
        survivalTime += Time.deltaTime;
        UpdateTimerUI();

        if (survivalTime >= targetClearTime)
        {
            GameClear();
            return;
        }

        // 2. 주기적 스폰
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnItem();
        }
    }

    void SpawnItem()
    {
        if (itemPrefab == null || canvasTransform == null || isGameFinished) return;

        GameObject newItem = Instantiate(itemPrefab, canvasTransform);

        float canvasWidth = canvasTransform.GetComponent<RectTransform>().rect.width;
        float minX = -(canvasWidth / 2f) + 150f;
        float maxX = (canvasWidth / 2f) - 150f;
        float randomX = Random.Range(minX, maxX);

        // 화면 밑바닥(-600)에서 리스폰
        newItem.transform.localPosition = new Vector3(randomX, -600f, 0f);

        ShooterItem shooterItem = newItem.GetComponent<ShooterItem>();
        if (shooterItem == null) shooterItem = newItem.AddComponent<ShooterItem>();

        // 버튼의 Image 컴포넌트를 가져옵니다.
        Image itemImage = newItem.GetComponent<Image>();

        // 프리팹 내부 스크립트에 등록한 원본 스프라이트 2개를 받아옵니다.
        Sprite badSprite = shooterItem.badSprite;
        Sprite goodSprite = shooterItem.goodSprite;

        if (Random.value > 0.4f) // 60% 확률로 나쁜 타겟 생성
        {
            shooterItem.isSkull = true;
            shooterItem.riseSpeed = Random.Range(300f, 550f);

            // BAD 이미지로 교체!
            if (itemImage != null && badSprite != null)
            {
                itemImage.sprite = badSprite;
            }
        }
        else // 40% 확률로 맞추면 안 되는 선량한 타겟 생성
        {
            shooterItem.isSkull = false;
            shooterItem.riseSpeed = Random.Range(200f, 400f);

            // GOOD 이미지로 교체!
            if (itemImage != null && goodSprite != null)
            {
                itemImage.sprite = goodSprite;
            }
        }
    }

    public void AddScore(int amount)
    {
        if (isGameFinished) return;
        score += amount;
        GameAudioManager.PlayScoreUp();
    }

    public void TakeDamage()
    {
        if (isGameFinished) return;

        currentLives--;
        GameAudioManager.PlayHealthDown();
        UpdateHeartUI();

        if (currentLives <= 0)
        {
            GameOver();
        }
    }

    public void MissSkull()
    {
        // BAD를 안 쏘고 놓쳐서 화면 위로 보내버리면 목숨이 깎이는 패널티!
        TakeDamage();
    }

    public bool IsFinished()
    {
        return isGameFinished;
    }

    void UpdateHeartUI()
    {
        if (heartImages == null) return;
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
            {
                heartImages[i].gameObject.SetActive(i < currentLives);
            }
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int seconds = Mathf.FloorToInt(survivalTime);
            timerText.text = $"TIME: {seconds} / {targetClearTime}s\nSCORE: {score}";
        }
    }

    public void GameOver()
    {
        isGameFinished = true;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        SetFinalText("GAME OVER");
        ClearAllItems();
    }

    public void GameClear()
    {
        isGameFinished = true;
        if (gameClearPanel != null) gameClearPanel.SetActive(true);
        SetFinalText("STAGE CLEAR!");
        ClearAllItems();
    }

    void SetFinalText(string title)
    {
        if (finalScoreText != null)
        {
            finalScoreText.text = $"{title}\nFINAL SCORE: {score}";
        }
    }

    void ClearAllItems()
    {
        ShooterItem[] items = FindObjectsByType<ShooterItem>(FindObjectsSortMode.None);
        foreach (var item in items)
        {
            if (item != null) Destroy(item.gameObject);
        }
    }

    public void ClickRetry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToLobby()
    {
        GameManager.ReturnToAlgorithmScreen();
    }
}
