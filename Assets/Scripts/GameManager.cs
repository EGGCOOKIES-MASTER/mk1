using UnityEngine;
using UnityEngine.SceneManagement;

/// ============================================================
/// 【 GameManager - SNS 중독 게임 전체 관리 시스템 】
/// 
/// 역할:
/// - 게임의 핵심 상태 관리 (6가지 상태를 enum으로 제어)
/// - 배터리 시스템 (100% → 0% 감소 및 종료 조건)
/// - 플레이어 통계 추적 (저장한 수, 하트, 사용 시간)
/// - 모든 화면과의 통신 (UIManager를 통해 화면 전환)
/// 
/// 특징:
/// - 싱글톤 패턴: 게임 전체에서 오직 하나만 존재
/// - DontDestroyOnLoad: 씬 전환 후에도 유지됨
/// - 상태 기반 설계: 현재 상태만 알면 무엇을 해야 할지 명확
/// 
/// 사용 예:
/// GameManager.Instance.ChangeState(GameState.Login);
/// GameManager.Instance.OnMiniGameComplete();
/// ============================================================

public class GameManager : MonoBehaviour
{
    /// ===== 【 싱글톤 인스턴스 】 =====
    /// 게임 전체 어디서든 접근 가능한 GameManager 인스턴스
    /// 중요: GameManager.Instance로 접근해서 사용
    public static GameManager Instance { get; private set; }

    /// ===== 【 게임 상태 정의 】 =====
    /// 게임은 이 6가지 상태 중 정확히 하나의 상태만 가짐
    public enum GameState
    {
        AppClick,      // 📱 앱 클릭 화면 - 게임 시작 (진입점)
        Login,         // 🔐 로그인 화면 - 통계 표시 후 로그인
        Algorithm,     // 🎨 알고리즘 화면 - 테마 4가지 중 선택
        Reels,         // 📺 릴스 화면 - 메인 플레이 공간, 게시물 스크롤
        MiniGame,      // 🎮 미니게임 - 게시물 클릭 시 진입
        Ending         // 💀 엔딩 화면 - 배터리 1% 도달 시 게임 종료
    }

    /// ===== 【 게임 상태 변수 】 =====
    /// currentState: 현재 게임이 어느 상태에 있는지 저장
    /// 항상 위의 6가지 상태 중 하나의 값을 가짐
    private GameState currentState;
    public GameState CurrentState => currentState;  // 읽기 전용 프로퍼티

    /// ===== 【 배터리 시스템 변수 】 =====
    /// battery: 게임의 진행 시간을 제한하는 자원
    /// 특징:
    /// - 초기값: 100% (테마 선택 후)
    /// - 감소: 미니게임 완료 시 -10%
    /// - 종료 조건: 1% 이하 도달 시 엔딩
    /// 게임 설계: 배터리 소진 = SNS 사용으로 인한 시간/에너지 낭비 표현
    private float battery = 100f;
    public float Battery => battery;  // 읽기 전용, ReelsScreen에서 배터리 UI 업데이트에 사용

    /// ===== 【 게임 통계 변수 】 =====
    /// savedCount: 일반 게시물을 클릭한 횟수 (저장한 수)
    /// heartCount: 미니게임을 완료한 횟수 (하트 수)
    /// totalPlayTime: 미니게임으로 소비한 총 시간 (초 단위)
    /// 
    /// 이 통계들은:
    /// - LoginScreen에서 처음에 표시됨
    /// - 플레이 중 계속 누적됨
    /// - EndingScreen에서 최종 통계로 표시됨
    private int savedCount = 0;             // 저장한 수 (일반 게시물 클릭)
    private int heartCount = 0;             // 하트 수 (미니게임 완료)
    private float totalPlayTime = 0f;       // 총 사용 시간 (초)

    // 통계는 읽기 전용 프로퍼티로 제공
    public int SavedCount => savedCount;
    public int HeartCount => heartCount;
    public float TotalPlayTime => totalPlayTime;

    /// ===== 【 UI 관리자 참조 】 =====
    /// uiManager: 모든 화면 전환을 담당하는 UIManager
    /// Start()에서 자동으로 찾아지고, ChangeState()에서 사용됨
    private UIManager uiManager;

    // ============================================================
    /// 【 Awake() - 게임 시작 시 가장 먼저 호출 】
    /// 역할: 싱글톤 패턴 초기화
    /// 특징: Awake()는 Start()보다 먼저 실행됨
    // ============================================================
    void Awake()
    {
        /// 싱글톤 체크: GameManager가 이미 존재하는지 확인
        /// 만약 이미 존재한다면 새로 생성된 GameManager는 삭제
        /// 이렇게 해서 게임 전체에서 오직 하나의 GameManager만 존재
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // GameManager를 전역 인스턴스로 설정
        Instance = this;
        
        // 씬 전환 후에도 GameManager를 파괴하지 않음
        // → 게임 전체에서 게속 게임 상태를 추적할 수 있음
        DontDestroyOnLoad(gameObject);
    }

    // ============================================================
    /// 【 Start() - 게임 초기화 】
    /// 역할: UIManager 찾기, 게임 시작 상태 설정
    // ============================================================
    void Start()
    {
        /// UIManager 찾기
        /// FindObjectOfType<T>()는 씬에서 T 타입의 컴포넌트를 찾음
        /// (성능상 주의: 많이 쓰면 느려질 수 있음)
        uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("❌ UIManager를 찾을 수 없습니다! UIManager 컴포넌트가 씬에 있는지 확인하세요.");
        }

        /// 게임 시작
        /// 첫 번째 상태를 AppClick으로 설정
        /// → AppClickScreen 화면 표시
        ChangeState(GameState.AppClick);
    }

    // ============================================================
    /// 【 ChangeState() - 게임 상태 변경의 핵심 함수 】
    /// 역할: 게임 상태를 변경하고 해당 화면을 표시
    /// 
    /// 매개변수:
    /// - newState: 변경할 새로운 상태 (GameState enum 값)
    /// 
    /// 동작:
    /// 1. currentState를 newState로 업데이트
    /// 2. 콘솔에 상태 변경 로그 출력
    /// 3. UIManager에 새 화면 표시 요청
    /// 
    /// 예시:
    /// ChangeState(GameState.Login);    // AppClick → Login
    /// ChangeState(GameState.Reels);    // Algorithm → Reels
    /// ============================================================
    public void ChangeState(GameState newState)
    {
        // 현재 상태 업데이트
        currentState = newState;
        Debug.Log($"🔄 게임 상태 변경: {newState}");

        // UIManager가 존재하는지 확인 후 새 화면 표시
        // (null 체크로 오류 방지)
        if (uiManager != null)
        {
            uiManager.ShowScreen(newState);
        }
        else
        {
            Debug.LogWarning("⚠️ UIManager가 없어서 화면을 표시할 수 없습니다.");
        }
    }

    // ============================================================
    /// 【 RefreshCurrentState() - 현재 상태를 다시 화면에 적용 】
    /// 
    /// 용도:
    /// - 씬이 다시 로드된 뒤 새 UIManager에 현재 상태를 다시 보여줄 때 사용
    /// ============================================================
    public void RefreshCurrentState()
    {
        uiManager = FindFirstObjectByType<UIManager>();

        if (uiManager != null)
        {
            uiManager.ShowScreen(currentState);
        }
    }

    // ============================================================
    /// 【 SetStateWithoutUI() - UI 갱신 없이 상태만 변경 】
    /// 
    /// 용도:
    /// - 씬 전환 직전에 상태만 바꾸고 싶을 때 사용
    /// - 새 씬에서 UIManager가 다시 잡히도록 할 때 유용
    /// ============================================================
    public void SetStateWithoutUI(GameState newState)
    {
        currentState = newState;
        Debug.Log($"🔄 상태만 변경: {newState}");
    }

    // ============================================================
    /// 【 GoNextForDemo() - [TEMP] 버튼 1개로 다음 화면 이동 】
    ///
    /// 용도:
    /// - Inspector 참조가 덜 연결된 상태에서도 화면 전환만 빠르게 테스트
    /// - 기존 로직은 유지하고, 임시 우회 경로만 추가
    // ============================================================
    public void GoNextForDemo()
    {
        Debug.Log($"[TEMP] 다음 화면 이동 요청 - 현재 상태: {currentState}");

        switch (currentState)
        {
            case GameState.AppClick:
                ChangeState(GameState.Login);
                break;

            case GameState.Login:
                ChangeState(GameState.Algorithm);
                break;

            case GameState.Algorithm:
                // OnThemeSelected()의 핵심 효과를 임시로 직접 적용
                battery = 100f;
                ChangeState(GameState.Reels);
                break;

            case GameState.Reels:
                ChangeState(GameState.MiniGame);
                break;

            case GameState.MiniGame:
                // 기존 완료 로직(배터리 감소/엔딩 판정) 재사용
                OnMiniGameComplete();
                break;

            case GameState.Ending:
                ChangeState(GameState.AppClick);
                break;
        }
    }

    // ============================================================
    /// 【 OnAppClicked() - 앱 아이콘 클릭 이벤트 】
    /// 
    /// 호출 시점: AppClickScreen의 앱 버튼을 클릭했을 때
    /// 
    /// 동작:
    /// - AppClick 상태 → Login 상태로 변경
    /// - LoginScreen 화면 표시
    /// - 현재까지의 게임 통계 표시 (처음이면 모두 0)
    /// 
    /// 용도: 게임 시작 → 로그인 화면 진입
    /// ============================================================
    public void OnAppClicked()
    {
        Debug.Log("📱 앱 클릭됨!");
        ChangeState(GameState.Login);
    }

    // ============================================================
    /// 【 OnLoginComplete() - 로그인 완료 이벤트 】
    /// 
    /// 호출 시점: LoginScreen의 로그인 버튼을 클릭했을 때
    /// 
    /// 동작:
    /// - Login 상태 → Algorithm 상태로 변경
    /// - AlgorithmScreen(테마 선택 화면) 표시
    /// - 4가지 테마 버튼 활성화
    /// 
    /// 용도: 로그인 → 테마 선택
    /// ============================================================
    public void OnLoginComplete()
    {
        Debug.Log("🔐 로그인 완료!");
        ChangeState(GameState.Algorithm);
    }

    // ============================================================
    /// 【 OnThemeSelected() - 테마 선택 완료 이벤트 】
    /// 
    /// 호출 시점: AlgorithmScreen에서 4가지 테마 중 하나를 선택했을 때
    /// 
    /// 동작:
    /// 1. Algorithm 상태 → Reels 상태로 변경
    /// 2. ReelsScreen(메인 플레이 화면) 표시
    /// 3. 배터리를 100%로 초기화 ★ 중요
    /// 4. 게시물 6~8개 동적 생성 시작
    /// 
    /// 용도: 테마 선택 → 게임 본격 시작
    /// 
    /// 주의: 배터리를 반드시 초기화해야 새 게임이 시작됨
    /// ============================================================
    public void OnThemeSelected()
    {
        Debug.Log("🎨 테마 선택됨!");
        
        // ★ 배터리 초기화: 이 작업이 없으면 이전 게임의 배터리가 유지됨
        battery = 100f;
        Debug.Log($"⚡ 배터리 초기화: {battery}%");

        // 게임 본편 시작 - Reels 상태로 변경
        ChangeState(GameState.Reels);
    }

    // ============================================================
    /// 【 OnMiniGameStart() - 미니게임 시작 이벤트 】
    /// 
    /// 호출 시점: ReelsScreen에서 미니게임 게시물을 클릭했을 때
    /// (30% 확률로 미니게임 게시물이 나타남)
    /// 
    /// 동작:
    /// - Reels 상태 → MiniGame 상태로 변경
    /// - MiniGameScreen 화면 표시
    /// - 5가지 게임 중 랜덤 선택 (게임 시작)
    /// 
    /// 용도: 게시물 클릭 → 미니게임 진입
    /// ============================================================
    public void OnMiniGameStart()
    {
        Debug.Log("🎮 미니게임 시작!");
        SetStateWithoutUI(GameState.MiniGame);
        SceneManager.LoadScene("MiniGame");
    }

    // ============================================================
    /// 【 OnMiniGameComplete() - 미니게임 완료 이벤트 ★ 배터리 감소 핵심 】
    /// 
    /// 호출 시점: MiniGameScreen에서 "완료" 또는 "스킵" 버튼을 클릭했을 때
    /// 
    /// 동작 순서:
    /// 1. 배터리 -10% 감소
    /// 2. 배터리가 0 이하로 내려가지 않도록 제한 (Mathf.Max)
    /// 3. 게임 통계 업데이트 (하트+1, 시간+5초는 MiniGameScreen에서)
    /// 4. 배터리 상태 확인:
    ///    - 배터리 > 1% → Algorithm으로 돌아감 (다시 테마/이미지 선택)
    ///    - 배터리 ≤ 1% → OnBatteryDepleted() 호출 (게임 종료)
    /// 
    /// 용도: 미니게임 완료 → 배터리 감소 → 게임 계속/종료 판단
    /// 
    /// 중요: 이 함수가 SNS 중독 메커니즘의 핵심
    /// - 미니게임 = 반복적인 SNS 이용
    /// - 배터리 감소 = 시간/에너지 소모
    /// - 배터리 고갈 = 게임 종료 (SNS 중독의 한계)
    /// ============================================================
    public void OnMiniGameComplete()
    {
        Debug.Log("✅ 미니게임 완료!");
        
        /// 배터리 감소 (미니게임 1회 = -10%)
        /// 미니게임을 10번 하면 배터리가 완전히 소진됨
        battery -= 10f;
        
        /// 배터리가 0 이하로 내려가지 않도록 제한
        /// Mathf.Max(a, b)는 a와 b 중 더 큰 값을 반환
        /// → battery가 음수가 되지 않음
        battery = Mathf.Max(0f, battery);
        
        Debug.Log($"🔋 현재 배터리: {battery}%");

        /// 배터리 확인: 게임 계속 vs 게임 종료
        if (battery <= 1f)
        {
            /// 배터리가 거의 없음 - 게임 종료
            OnBatteryDepleted();
        }
        else
        {
            /// 배터리가 아직 남음 - 알고리즘 화면으로 복귀
            ChangeState(GameState.Algorithm);
        }
    }

    // ============================================================
    /// 【 OnBatteryDepleted() - 배터리 완전 소진 이벤트 (게임 종료) 】
    /// 
    /// 호출 시점: 배터리가 1% 이하 도달했을 때
    ///           (OnMiniGameComplete()에서 자동으로 호출됨)
    /// 
    /// 동작:
    /// - MiniGame 상태 → Ending 상태로 변경
    /// - EndingScreen 화면 표시
    /// - 최종 통계 표시 (저장한 수, 하트, 사용 시간)
    /// - 엔딩 메시지 표시: "배터리가 모두 소모되었습니다"
    /// 
    /// 용도: 게임 종료 → 엔딩 화면
    /// ============================================================
    public void OnBatteryDepleted()
    {
        Debug.Log("💀 배터리 소진 - 게임 종료!");
        ChangeState(GameState.Ending);
    }

    // ============================================================
    /// 【 IncreaseSavedCount() - 저장한 수 증가 】
    /// 
    /// 호출 시점: ReelsScreen에서 일반 게시물을 클릭했을 때
    /// 
    /// 동작:
    /// - savedCount 변수 +1
    /// - 다음 로그인 화면/엔딩 화면에서 업데이트된 숫자 표시
    /// 
    /// 용도: 일반 게시물 클릭 → 통계 증가
    /// ============================================================
    public void IncreaseSavedCount()
    {
        savedCount++;
        Debug.Log($"📌 저장한 수 증가! (현재: {savedCount}개)");
    }

    // ============================================================
    /// 【 IncreaseHeartCount() - 하트 수 증가 】
    /// 
    /// 호출 시점: MiniGameScreen에서 미니게임을 완료했을 때
    ///           (MiniGameScreen.OnCompleteButtonClicked()에서 호출)
    /// 
    /// 동작:
    /// - heartCount 변수 +1
    /// - SNS에서 "좋아요" 누른 개수 표현
    /// 
    /// 용도: 미니게임 완료 → 하트 증가
    /// ============================================================
    public void IncreaseHeartCount()
    {
        heartCount++;
        Debug.Log($"❤️ 하트 증가! (현재: {heartCount}개)");
    }

    // ============================================================
    /// 【 AddPlayTime() - 게임 플레이 시간 누적 】
    /// 
    /// 호출 시점: MiniGameScreen에서 미니게임을 완료했을 때
    ///           (MiniGameScreen.OnCompleteButtonClicked()에서 호출)
    /// 
    /// 매개변수:
    /// - seconds: 추가할 시간 (초 단위)
    ///          보통 5초씩 증가 (AddPlayTime(5f))
    /// 
    /// 동작:
    /// - totalPlayTime에 seconds 더하기
    /// - 누적된 게임 시간 추적
    /// 
    /// 용도: 미니게임 완료 → 사용 시간 증가
    /// ============================================================
    public void AddPlayTime(float seconds)
    {
        totalPlayTime += seconds;
        Debug.Log($"⏱️ 사용 시간 증가! (현재: {(int)(totalPlayTime / 60)}분 {(int)(totalPlayTime % 60)}초)");
    }
}

