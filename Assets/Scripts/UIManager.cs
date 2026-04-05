using UnityEngine;
using UnityEngine.UI;

/// ============================================================
/// 【 UIManager - UI 화면 전환 시스템 】
/// 
/// 역할:
/// - 모든 화면(Screen)의 참조를 보유
/// - GameManager의 상태 변화에 따라 화면 전환
/// - 한 번에 하나의 화면만 표시 (나머지는 비활성화)
/// - 각 화면의 Initialize() 메소드 호출 (화면 초기 설정)
/// 
/// 특징:
/// - 간단한 전달자 역할: GameManager ← UIManager → Screens
/// - 모든 화면 전환의 중심점
/// - 한 화면에서 다른 화면으로 부드럽게 전환
/// 
/// 흐름:
/// GameManager.ChangeState(newState)
///   ↓
/// UIManager.ShowScreen(newState)
///   ↓
/// 기존 화면 비활성화 + 새 화면 활성화 + Initialize() 호출
/// ============================================================

public class UIManager : MonoBehaviour
{
    /// ===== 【 6개 화면 참조 】 =====
    /// [SerializeField]로 선언해서 Inspector에서 할당 가능
    /// 각 화면은 패널 또는 Canvas의 하위 게임오브젝트
    [SerializeField] private AppClickScreen appClickScreen;      // 📱 앱 클릭
    [SerializeField] private LoginScreen loginScreen;            // 🔐 로그인
    [SerializeField] private AlgorithmScreen algorithmScreen;    // 🎨 테마선택
    [SerializeField] private ReelsScreen reelsScreen;            // 📺 릴스
    [SerializeField] private MiniGameScreen miniGameScreen;      // 🎮 미니게임
    [SerializeField] private EndingScreen endingScreen;          // 💀 엔딩

    // ============================================================
    /// 【 Start() - 초기화 】
    /// 게임 시작 시 모든 화면을 비활성 상태로 설정
    /// (AppClickScreen만 GameManager에서 활성화됨)
    // ============================================================
    void Start()
    {
        /// 모든 화면 비활성화
        /// → GameManager.ChangeState()가 호출될 때까지 아무 화면도 보이지 않음
        /// → 그 후 첫 번째 상태(AppClick)에 따라 AppClickScreen이 활성화됨
        InitializeAllScreens();
    }

    // ============================================================
    /// 【 InitializeAllScreens() - 모든 화면 비활성화 】
    /// 
    /// 역할: 모든 화면 게임오브젝트를 SetActive(false)로 설정
    /// 
    /// 사용 시점:
    /// - Start()에서 처음 호출
    /// - ShowScreen()에서 새 화면 표시 전에 호출
    /// 
    /// 이유:
    /// - 여러 화면이 동시에 보이면 UI가 복잡해짐
    /// - 정확히 하나의 화면만 표시하기 위해 기존 화면들을 먼저 비활성화
    /// 
    /// 최적화:
    /// - null 체크로 오류 방지 (화면이 할당되지 않았을 수도 있음)
    /// - 짧고 간단한 코드 (6줄)
    // ============================================================
    private void InitializeAllScreens()
    {
        /// 6개 화면 모두 null 체크 후 비활성화
        /// 만약 어떤 화면의 참조가 없으면 null이므로 if에서 걸러짐
        if (appClickScreen != null) appClickScreen.gameObject.SetActive(false);
        if (loginScreen != null) loginScreen.gameObject.SetActive(false);
        if (algorithmScreen != null) algorithmScreen.gameObject.SetActive(false);
        if (reelsScreen != null) reelsScreen.gameObject.SetActive(false);
        if (miniGameScreen != null) miniGameScreen.gameObject.SetActive(false);
        if (endingScreen != null) endingScreen.gameObject.SetActive(false);
    }

    // ============================================================
    /// 【 ShowScreen() - 화면 전환의 핵심 함수 】
    /// 
    /// 역할:
    /// 1. 모든 화면 비활성화
    /// 2. 새로운 화면 활성화
    /// 3. 새로운 화면의 Initialize() 호출 (화면 초기 설정)
    /// 
    /// 매개변수:
    /// - state: 표시할 화면의 게임 상태 (GameManager.GameState)
    /// 
    /// 동작 흐름:
    /// state = GameState.Login
    ///   ↓
    /// AppClickScreen 비활성화 (기존 화면)
    ///   ↓
    /// LoginScreen 활성화 (새 화면)
    ///   ↓
    /// loginScreen.Initialize() 호출 (화면 초기화 - 통계 표시 등)
    /// 
    /// 최적화:
    /// - switch문으로 명확하게 각 상태 처리
    /// - null 체크로 오류 방지
    /// - 불필요한 연산 최소화
    /// ============================================================
    public void ShowScreen(GameManager.GameState state)
    {
        /// Step 1: 모든 화면 비활성화
        /// 새 화면을 보여주기 전에 기존 화면들을 숨김
        InitializeAllScreens();

        /// Step 2: 상태에 따라 해당 화면만 활성화
        /// switch문을 사용해서 6가지 상태를 모두 처리
        switch (state)
        {
            /// 📱 AppClick 상태: 앱 아이콘 클릭 화면
            case GameManager.GameState.AppClick:
                if (appClickScreen != null)
                {
                    appClickScreen.gameObject.SetActive(true);  // 화면 표시
                    appClickScreen.Initialize();                 // 화면 초기화
                }
                break;

            /// 🔐 Login 상태: 로그인 화면
            /// 플레이어의 통계(저장한 수, 하트, 사용 시간) 표시
            case GameManager.GameState.Login:
                if (loginScreen != null)
                {
                    loginScreen.gameObject.SetActive(true);     // 화면 표시
                    loginScreen.Initialize();                    // 통계 표시
                }
                break;

            /// 🎨 Algorithm 상태: 테마 선택 화면
            /// 4가지 테마 중 하나 선택 가능
            case GameManager.GameState.Algorithm:
                if (algorithmScreen != null)
                {
                    algorithmScreen.gameObject.SetActive(true);  // 화면 표시
                    algorithmScreen.Initialize();                 // 테마 버튼 활성화
                }
                break;

            /// 📺 Reels 상태: 메인 플레이 화면 (SNS 피드)
            /// 게시물 스크롤, 배터리 표시, 게시물 클릭 반응
            case GameManager.GameState.Reels:
                if (reelsScreen != null)
                {
                    reelsScreen.gameObject.SetActive(true);     // 화면 표시
                    reelsScreen.Initialize();                    // 게시물 생성
                }
                break;

            /// 🎮 MiniGame 상태: 미니게임 화면
            /// 5가지 게임 중 랜덤 선택
            case GameManager.GameState.MiniGame:
                if (miniGameScreen != null)
                {
                    miniGameScreen.gameObject.SetActive(true);  // 화면 표시
                    miniGameScreen.Initialize();                 // 게임 초기화
                }
                break;

            /// 💀 Ending 상태: 엔딩 화면
            /// 배터리 1% 도달 시 표시, 최종 통계 표시
            case GameManager.GameState.Ending:
                if (endingScreen != null)
                {
                    endingScreen.gameObject.SetActive(true);    // 화면 표시
                    endingScreen.Initialize();                   // 통계 표시
                }
                break;

            /// 기본값: 존재하지 않는 상태 (오류)
            default:
                Debug.LogError($"❌ 알 수 없는 게임 상태: {state}");
                break;
        }
    }
}

