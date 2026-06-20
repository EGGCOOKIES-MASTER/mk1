using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class RhythmLaneManager : MonoBehaviour
{
    [Header("오브젝트 연결")]
    public GameObject notePrefab;
    public Transform spawnPoint;
    public Transform judgmentLine;
    public TextMeshProUGUI scoreText;

    [Header("UI 연결 (게임오버/클리어 분리)")]
    public JudgmentEffect judgmentEffect;
    public GameObject gameOverPanel;     // ⭐️ [변경] 게임오버 전용 판넬
    public GameObject gameClearPanel;    // ⭐️ [변경] STAGE CLEAR 전용 판넬
    public TextMeshProUGUI finalScoreText; // 결과창에 띄워줄 최종 점수 텍스트
    public Image[] heartImages;

    [Header("새로 추가된 콤보 UI")]
    public TextMeshProUGUI comboText;    // ComboText 오브젝트 연결

    [Header("게임 설정 (랜덤 박자용)")]
    public float minSpawnInterval = 0.4f; // 가장 빠른 노트 간격 (0.4초)
    public float maxSpawnInterval = 1.2f; // 가장 느린 노트 간격 (1.2초)
    public float gameTimer = 30f;

    private int score = 0;
    private int combo = 0;               // 현재 연속 콤보 수
    private int currentHP;
    private float nextSpawnTime = 0f;    // 다음 노트가 생성될 무작위 목표 시간
    private float spawnTimer = 0f;
    private bool isGameOver = false;
    private bool didClearGame = false;

    void Start()
    {
        // 시작할 때 두 판넬을 모두 안전하게 닫고 시작합니다.
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameClearPanel != null) gameClearPanel.SetActive(false);

        if (comboText != null) comboText.text = ""; // 시작할 땐 콤보 숨기기

        currentHP = heartImages.Length;

        // 첫 번째 노트가 생성될 무작위 간격을 정합니다.
        SetNextSpawnTime();
    }

    void Update()
    {
        // 게임오버거나 클리어 상태라면 타이머 및 노트를 멈춥니다.
        if (isGameOver || didClearGame) return;

        // 1. 제한 시간 타이머
        gameTimer -= Time.deltaTime;
        if (gameTimer <= 0)
        {
            // 제한 시간을 모두 버티면 성공! (isFailed = false)
            EndGame(false);
            return;
        }

        // 2. 불규칙한(랜덤) 박자로 노트 생성 구조
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= nextSpawnTime)
        {
            SpawnNote();
            spawnTimer = 0f;
            SetNextSpawnTime(); // 무작위 다음 간격 재설정!
        }

        // 3. 입력 감지
        if (UnityEngine.InputSystem.Keyboard.current?.spaceKey.wasPressedThisFrame == true ||
            UnityEngine.InputSystem.Pointer.current?.press.wasPressedThisFrame == true)
        {
            CheckHit();
        }
    }

    // 다음 노트 생성 간격을 무작위로 정해주는 함수
    void SetNextSpawnTime()
    {
        nextSpawnTime = Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    void SpawnNote()
    {
        if (notePrefab == null || spawnPoint == null) return;
        GameObject newNote = Instantiate(notePrefab, spawnPoint.position, Quaternion.identity, spawnPoint.parent);
        newNote.GetComponent<RhythmNote>().Setup(judgmentLine.position.y);
    }

    void CheckHit()
    {
        RhythmNote[] activeNotes = FindObjectsByType<RhythmNote>(FindObjectsSortMode.None);

        foreach (RhythmNote note in activeNotes)
        {
            float distance = Mathf.Abs(note.transform.position.y - judgmentLine.position.y);

            if (distance < 50f) // PERFECT
            {
                score += 100 + (combo * 10); // [콤보 보너스] 콤보가 높을수록 보너스 점수!
                GameAudioManager.PlayNoteHit();
                GameAudioManager.PlayScoreUp();
                combo++;                     // 콤보 상승
                judgmentEffect.ShowText("PERFECT!!!", Color.yellow);
                UpdateScoreUI();
                UpdateComboUI();
                note.DestroyNote();
                return;
            }
            else if (distance < 100f) // GOOD
            {
                score += 50 + (combo * 5);   // [콤보 보너스]
                GameAudioManager.PlayNoteHit();
                GameAudioManager.PlayScoreUp();
                combo++;                     // 콤보 상승
                judgmentEffect.ShowText("GOOD!", Color.green);
                UpdateScoreUI();
                UpdateComboUI();
                note.DestroyNote();
                return;
            }
        }
    }

    // 노트를 놓쳤을 때 (MISS)
    public void MissNote()
    {
        if (isGameOver || didClearGame) return;

        judgmentEffect.ShowText("MISS...", Color.red);

        combo = 0; // 💥 콤보가 깨집니다!
        UpdateComboUI();

        currentHP--;
        GameAudioManager.PlayHealthDown();
        if (heartImages != null && currentHP >= 0 && currentHP < heartImages.Length)
        {
            heartImages[currentHP].gameObject.SetActive(false);
        }

        // 목숨이 0 이하가 되면 게임오버! (isFailed = true)
        if (currentHP <= 0)
        {
            EndGame(true);
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
    }

    // 콤보 실시간 반영 UI 함수
    void UpdateComboUI()
    {
        if (comboText == null) return;

        if (combo >= 3) // 3콤보 이상일 때부터 화면에 콤보 표시!
        {
            comboText.text = combo + " COMBO";
        }
        else
        {
            comboText.text = ""; // 0~2콤보일 때는 숨기기
        }
    }

    // ⭐️ [핵심 업그레이드] 조건에 따라 판넬을 분리하여 활성화시키는 함수
    void EndGame(bool isFailed)
    {
        // 현재 화면에 남아있는 모든 노트들을 깔끔하게 지워줍니다.
        RhythmNote[] activeNotes = FindObjectsByType<RhythmNote>(FindObjectsSortMode.None);
        foreach (var note in activeNotes)
        {
            note.DestroyNote();
        }

        if (isFailed)
        {
            // 💀 게임 오버 처리
            isGameOver = true;
            didClearGame = false;

            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            if (finalScoreText != null)
            {
                finalScoreText.text = "GAME OVER\nSCORE: " + score;
            }
        }
        else
        {
            // 🎉 스테이지 클리어 처리
            isGameOver = false;
            didClearGame = true;

            if (gameClearPanel != null) gameClearPanel.SetActive(true);
            if (finalScoreText != null)
            {
                finalScoreText.text = "STAGE CLEAR!\nSCORE: " + score;
            }
        }
    }

    // 다시하기 버튼 (현재 리듬게임 씬 재로드)
    public void ClickRetry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 로비/홈으로 돌아가기 버튼
    public void GoToLobby()
    {
        if (didClearGame && GameManager.Instance != null)
        {
            GameManager.Instance.OnMiniGameComplete();
            return;
        }

        GameManager.SetPendingInitialState(GameManager.GameState.Algorithm);
        SceneManager.LoadScene("MainScene");
    }
}
