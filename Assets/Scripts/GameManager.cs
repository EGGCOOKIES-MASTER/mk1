using UnityEngine;

/// <summary>
/// 게임 메인 관리자 (싱글톤)
/// 모든 화면 매니저를 통합 관리하고, 게임 상태를 제어
/// 각 매니저는 자신의 화면만 담당하고, GameManager가 전체 흐름을 조율
/// </summary>
public class GameManager : MonoBehaviour
{
    // ================================ 싱글톤 ================================
    public static GameManager Instance;

    // ================================ 게임 상태 ================================
    public enum GameState { AppScreen, Login, Algorithm, Reels, MiniGame, Ending }
    private GameState currentState;

    // ================================ 화면 매니저들 ================================
    private AppScreenManager appScreenManager;
    private LoginManager loginManager;
    private AlgorithmManager algorithmManager;
    private ReelsManager reelsManager;
    private MiniGameScreenManager miniGameManager;
    private EndingManager endingManager;

    // ================================ 배터리 시스템 ================================
    private float batteryLevel = 100f;
    private const float BATTERY_DRAIN_RATE = 2f;
    private const float BATTERY_CHECK_INTERVAL = 0.1f;
    private float batteryCheckTimer = 0f;

    // ================================ 플레이어 데이터 ================================
    private int savedCount = 45;
    private int heartsPressed = 128;
    private int usedMinutes = 247;

    // ================================ 현재 상태 ================================
    private string currentTheme = "";

    // ================================ 상수 ================================
    private const int MINI_GAME_TIME_ADD = 5;

    // ================================ 싱글톤 초기화 ================================
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 매니저 초기화
        InitializeManagers();
        
        // 게임 시작
        ShowAppScreen();
    }

    private void Update()
    {
        // 릴스 화면에서 배터리 감소
        if (currentState == GameState.Reels)
        {
            batteryCheckTimer += Time.deltaTime;
            if (batteryCheckTimer >= BATTERY_CHECK_INTERVAL)
            {
                batteryCheckTimer = 0f;
                // 배터리 UI 업데이트 (실제 감소는 미니게임 완료 시)
                reelsManager.UpdateBattery(batteryLevel);
            }
        }
    }

    // ================================ 매니저 초기화 ================================
    /// <summary>모든 화면 매니저 초기화</summary>
    private void InitializeManagers()
    {
        // 캔버스 생성
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // 각 매니저 생성
        appScreenManager = gameObject.AddComponent<AppScreenManager>();
        loginManager = gameObject.AddComponent<LoginManager>();
        algorithmManager = gameObject.AddComponent<AlgorithmManager>();
        reelsManager = gameObject.AddComponent<ReelsManager>();
        miniGameManager = gameObject.AddComponent<MiniGameScreenManager>();
        endingManager = gameObject.AddComponent<EndingManager>();
    }

    // ================================ 화면 전환 메서드 ================================

    /// <summary>앱 화면 표시</summary>
    private void ShowAppScreen()
    {
        currentState = GameState.AppScreen;
        appScreenManager.Show(() => ShowLoginScreen());
    }

    /// <summary>로그인 화면 표시</summary>
    private void ShowLoginScreen()
    {
        currentState = GameState.Login;
        appScreenManager.Hide();
        loginManager.Show(savedCount, heartsPressed, usedMinutes, () => ShowAlgorithmScreen());
    }

    /// <summary>알고리즘(테마 선택) 화면 표시</summary>
    private void ShowAlgorithmScreen()
    {
        currentState = GameState.Algorithm;
        loginManager.Hide();
        algorithmManager.Show((theme) => OnThemeSelected(theme));
    }

    /// <summary>테마 선택 완료 처리</summary>
    private void OnThemeSelected(string theme)
    {
        currentTheme = theme;
        ShowReelsScreen();
    }

    /// <summary>릴스 화면 표시</summary>
    private void ShowReelsScreen()
    {
        currentState = GameState.Reels;
        algorithmManager.Hide();
        reelsManager.Show(currentTheme, batteryLevel, 
            () => ShowAlgorithmScreen(), // 돌아가기
            () => ShowMiniGameScreen()); // 미니게임 클릭
    }

    /// <summary>미니게임 화면 표시</summary>
    private void ShowMiniGameScreen()
    {
        currentState = GameState.MiniGame;
        reelsManager.Hide();
        miniGameManager.Show(() => OnMiniGameComplete());
    }

    /// <summary>미니게임 완료 처리</summary>
    private void OnMiniGameComplete()
    {
        // 배터리 소모
        batteryLevel -= BATTERY_DRAIN_RATE;
        usedMinutes += MINI_GAME_TIME_ADD;

        // 배터리 체크
        if (batteryLevel <= 1)
        {
            ShowEndingScreen();
        }
        else
        {
            miniGameManager.Hide();
            ShowReelsScreen(); // 다시 릴스 화면으로
        }
    }

    /// <summary>엔딩 화면 표시</summary>
    private void ShowEndingScreen()
    {
        currentState = GameState.Ending;
        miniGameManager.Hide();
        endingManager.Show();
    }

    // ================================ 공용 메서드 ================================

    /// <summary>현재 게임 상태 반환</summary>
    public GameState GetCurrentState() => currentState;

    /// <summary>배터리 레벨 반환</summary>
    public float GetBatteryLevel() => batteryLevel;

    /// <summary>플레이어 통계 업데이트</summary>
    public void UpdateStats(int saved, int hearts, int minutes)
    {
        savedCount = saved;
        heartsPressed = hearts;
        usedMinutes = minutes;
    }
}

