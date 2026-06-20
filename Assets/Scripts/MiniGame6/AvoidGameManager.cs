using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AvoidGameManager : MonoBehaviour
{
    public static AvoidGameManager Instance;

    [Header("오브젝트 및 프리팹 연결")]
    public GameObject itemPrefab;
    public Transform spawnParent;

    [Header("UI 패널 연결")]
    public GameObject gameOverPanel;
    public GameObject gameClearPanel;
    public TMPro.TextMeshProUGUI scoreText;
    public TMPro.TextMeshProUGUI finalScoreText;
    public Image[] heartImages;

    [Header("게임 설정")]
    public float spawnInterval = 0.4f;   // 생성 주기 (0.4초당 하나씩 비처럼 쏟아짐)
    public float targetClearTime = 30f;  // 버텨야 하는 시간
    public int maxLives = 3;

    private string[] badComments = {
        "어그로 ㄴㄴ", "노잼이네ㅋㅋㅋ", "이게 왜 추천에 뜸?",
        "팔로우 취소함", "주작이네", "할많하않...",
        "그만 좀 올려라", "이게 맞음??", "응 노인정~"
    };

    private float spawnTimer = 0f;
    private float canvasWidth = 1920f;
    private float spawnPositionY = 600f;

    private float survivalTime = 0f;
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
        isGameFinished = false;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameClearPanel != null) gameClearPanel.SetActive(false);

        if (spawnParent == null) spawnParent = FindFirstObjectByType<Canvas>().transform;

        if (spawnParent != null)
        {
            canvasWidth = spawnParent.GetComponent<RectTransform>().rect.width;
        }

        UpdateHeartUI();
        UpdateTimerUI();
    }

    void Update()
    {
        if (isGameFinished) return;

        // 1. 시간 계산
        survivalTime += Time.deltaTime;
        UpdateTimerUI();

        // 2. 승리 체크
        if (survivalTime >= targetClearTime)
        {
            GameClear();
            return;
        }

        // 3. 스폰 주기 계산
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnFallingItem();
            spawnTimer = 0f;
        }
    }

    void SpawnFallingItem()
    {
        if (itemPrefab == null || spawnParent == null || isGameFinished) return;

        float minX = -(canvasWidth / 2f) + 150f;
        float maxX = (canvasWidth / 2f) - 150f;
        float randomX = Random.Range(minX, maxX);

        Vector3 spawnPosition = new Vector3(randomX, spawnPositionY, 0f);

        GameObject newItem = Instantiate(itemPrefab, spawnParent);
        newItem.transform.localPosition = spawnPosition;

        FallingItem itemScript = newItem.GetComponent<FallingItem>();
        if (itemScript != null)
        {
            int randomIndex = Random.Range(0, badComments.Length);
            itemScript.SetText(badComments[randomIndex], Color.red);
        }
    }

    public void PlayerHit()
    {
        if (isGameFinished) return;

        currentLives--;
        UpdateHeartUI();

        if (currentLives <= 0)
        {
            GameOver();
        }
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
            if (heartImages[i] != null) heartImages[i].gameObject.SetActive(i < currentLives);
        }
    }

    void UpdateTimerUI()
    {
        if (scoreText != null)
        {
            int minutes = Mathf.FloorToInt(survivalTime / 60f);
            int seconds = Mathf.FloorToInt(survivalTime % 60f);
            scoreText.text = string.Format("TIME: {0:00}:{1:00}", minutes, seconds);
        }
    }

    public void GameOver()
    {
        isGameFinished = true;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        SetFinalScoreText("GAME OVER");
        ClearAllFallingItems();
    }

    public void GameClear()
    {
        isGameFinished = true;
        survivalTime = targetClearTime;
        UpdateTimerUI();

        if (gameClearPanel != null) gameClearPanel.SetActive(true);
        SetFinalScoreText("STAGE CLEAR!");
        ClearAllFallingItems();
    }

    void SetFinalScoreText(string titleMessage)
    {
        if (finalScoreText != null)
        {
            int minutes = Mathf.FloorToInt(survivalTime / 60f);
            int seconds = Mathf.FloorToInt(survivalTime % 60f);
            finalScoreText.text = string.Format("{0}\nTIME: {1:00}:{2:00}", titleMessage, minutes, seconds);
        }
    }

    void ClearAllFallingItems()
    {
        FallingItem[] activeItems = FindObjectsByType<FallingItem>(FindObjectsSortMode.None);
        foreach (var item in activeItems)
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
        if (survivalTime >= targetClearTime && GameManager.Instance != null)
        {
            GameManager.Instance.OnMiniGameComplete();
            return;
        }
        GameManager.SetPendingInitialState(GameManager.GameState.Algorithm);
        SceneManager.LoadScene("MainScene");
    }
}