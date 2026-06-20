using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RunGameManager : MonoBehaviour
{
    [Header("UI 및 프리팹")]
    public GameObject obstaclePrefab;
    public Transform canvasTransform;
    public GameObject gameOverPanel;
    public GameObject gameClearPanel;
    public Image[] heartImages;
    public TMPro.TextMeshProUGUI scoreText;
    public TMPro.TextMeshProUGUI finalScoreText;

    [Header("게임 난이도 및 목표 세팅")]
    public float targetClearTime = 20f;  // ⭐️ [여기!] 유니티 인스펙터 창에서 클리어 목표 시간을 자유롭게 수정할 수 있도록 변수화했습니다!
    public float spawnInterval = 1.5f;
    public float initialSpeed = 500f;
    public float maxSpeed = 1300f;
    public float speedIncreaseRate = 20f;
    public float spawnX = 1100f;
    public float obstacleSpawnYOffset = 0f;

    [Header("라이프 세팅")]
    public int maxLives = 3;
    private int currentLives;

    [Header("악플 리스트 관리")]
    public string[] badWords = { "노잼", "실화냐?", "악플", "응 안봐", "에바임", "노답", "ㄴㅈ" };

    private float currentSpeed;
    private float spawnTimer = 0f;
    private bool isGameOver = false;
    private bool isGameClear = false;
    private float survivalTime = 0f;

    void Start()
    {
        currentSpeed = initialSpeed;
        currentLives = maxLives;
        survivalTime = 0f;
        isGameOver = false;
        isGameClear = false;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameClearPanel != null) gameClearPanel.SetActive(false);

        UpdateHeartUI();
        UpdateTimerUI();

        if (badWords == null || badWords.Length == 0)
        {
            badWords = new string[] { "BAD", "노잼", "악플" };
        }
    }

    void Update()
    {
        if (isGameOver || isGameClear) return;

        survivalTime += Time.deltaTime;
        UpdateTimerUI();

        // ⭐️ 설정한 목표 시간(예: 20초) 이상 살아남으면 클리어 처리됩니다.
        if (survivalTime >= targetClearTime)
        {
            GameClear();
            return;
        }

        if (currentSpeed < maxSpeed)
        {
            currentSpeed += speedIncreaseRate * Time.deltaTime;
        }

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnObstacle();
        }
    }

    void SpawnObstacle()
    {
        if (obstaclePrefab == null || canvasTransform == null) return;

        GameObject obstacle = Instantiate(obstaclePrefab, canvasTransform);
        float spawnY = -257f;
        float spawnPositionX = spawnX;
        RectTransform obstacleRect = obstacle.transform as RectTransform;
        RectTransform canvasRect = canvasTransform as RectTransform;
        RunPlayer player = FindFirstObjectByType<RunPlayer>();
        if (player != null)
        {
            float obstacleBottomOffset = obstacleRect != null
                ? obstacleRect.rect.height * obstacleRect.pivot.y
                : 0f;
            spawnY = player.FloorTopY + obstacleBottomOffset + obstacleSpawnYOffset;
        }

        if (canvasRect != null)
        {
            float obstacleHalfWidth = obstacleRect != null ? obstacleRect.rect.width * 0.5f : 0f;
            spawnPositionX = canvasRect.rect.width * 0.5f + obstacleHalfWidth;
        }

        obstacle.transform.localPosition = new Vector3(spawnPositionX, spawnY, 0f);

        string randomBadWord = badWords[Random.Range(0, badWords.Length)];

        MoveObstacle moveScript = obstacle.GetComponent<MoveObstacle>();
        if (moveScript == null) moveScript = obstacle.AddComponent<MoveObstacle>();

        moveScript.speed = currentSpeed;
        moveScript.SetText(randomBadWord);
    }

    public void PlayerHit()
    {
        if (isGameOver || isGameClear) return;

        currentLives--;
        GameAudioManager.PlayHealthDown();
        UpdateHeartUI();

        if (currentLives <= 0)
        {
            GameOver();
        }
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
        if (scoreText != null)
        {
            int minutes = Mathf.FloorToInt(survivalTime / 60f);
            int seconds = Mathf.FloorToInt(survivalTime % 60f);
            scoreText.text = string.Format("TIME: {0:00}:{1:00}", minutes, seconds);
        }
    }

    public void GameClear()
    {
        isGameClear = true;

        // ⭐️ [버그 해결!] 시간을 강제로 10초로 꺾지 않고, 실제 최종 도달한 목표 시간으로 세팅해줍니다.
        survivalTime = targetClearTime;
        UpdateTimerUI();

        // ⭐️ 게임 클리어 결과창(GameClearPanel)의 텍스트가 있는 경우, 실제 최종 시간 포맷(TIME: 00:20)을 넣어줍니다.
        if (finalScoreText != null)
        {
            int minutes = Mathf.FloorToInt(survivalTime / 60f);
            int seconds = Mathf.FloorToInt(survivalTime % 60f);
            finalScoreText.text = string.Format("TIME: {0:00}:{1:00}", minutes, seconds);
        }

        if (gameClearPanel != null) gameClearPanel.SetActive(true);
        StopAllObstacles();
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (finalScoreText != null)
        {
            int minutes = Mathf.FloorToInt(survivalTime / 60f);
            int seconds = Mathf.FloorToInt(survivalTime % 60f);
            finalScoreText.text = string.Format("TIME: {0:00}:{1:00}", minutes, seconds);
        }

        StopAllObstacles();
    }

    void StopAllObstacles()
    {
        MoveObstacle[] activeObstacles = FindObjectsByType<MoveObstacle>(FindObjectsSortMode.None);
        foreach (var obs in activeObstacles)
        {
            obs.speed = 0;
        }
    }

    public void ClickRetry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ClickHome()
    {
        GameManager.ReturnToAlgorithmScreen();
    }
}

public class MoveObstacle : MonoBehaviour
{
    public float speed;
    private RunPlayer player;
    private bool hasCollided = false;
    private string myText = "";

    void Start()
    {
        player = FindFirstObjectByType<RunPlayer>();
    }

    public void SetText(string textContent)
    {
        myText = textContent;
        TMPro.TextMeshProUGUI txt = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (txt != null)
        {
            txt.text = myText;
        }
    }

    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        if (player != null && !hasCollided)
        {
            float dist = Vector3.Distance(transform.localPosition, player.transform.localPosition);
            if (dist < 70f)
            {
                hasCollided = true;
                GameAudioManager.PlayCollision();

                RunGameManager manager = FindFirstObjectByType<RunGameManager>();
                if (manager != null)
                {
                    manager.PlayerHit();
                }

                Destroy(gameObject);
            }
        }

        if (transform.localPosition.x < -1100f)
        {
            Destroy(gameObject);
        }
    }
}
