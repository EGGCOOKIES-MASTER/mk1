using UnityEngine;
using UnityEngine.UI;

/// ============================================================
/// 【 LoginScreen - 로그인 화면 (통계 표시) 】
/// 
/// 역할:
/// - 게임을 시작하기 전에 플레이어의 통계를 표시
/// - 첫 게임: 모든 통계가 0
/// - 재게임: 이전 게임의 누적 통계 표시
/// - 로그인 버튼을 눌러 게임 시작
/// 
/// 특징:
/// - GameManager에서 통계 데이터 가져오기
/// - 3가지 통계 표시: 저장한 수, 하트, 사용 시간
/// - 프로필 이미지 표시 (선택)
/// - 로그인 버튼 클릭 시 테마 선택 화면으로 전환
/// 
/// 통계 의미:
/// - 저장한 수: 일반 게시물을 몇 번 클릭했는가
/// - 하트: 미니게임을 몇 번 완료했는가
/// - 사용 시간: 게임에 얼마나 오래 있었는가
/// ============================================================

public class LoginScreen : MonoBehaviour
{
    /// ===== 【 UI 요소 참조 】 =====
    [SerializeField] private Text savedCountText;       // 📌 저장한 수 표시
    [SerializeField] private Text heartCountText;       // ❤️ 하트 수 표시
    [SerializeField] private Text playTimeText;         // ⏱️ 사용 시간 표시
    [SerializeField] private Image profileImage;        // 👤 프로필 이미지
    [SerializeField] private Button loginButton;        // 🔓 로그인 버튼
    [SerializeField] private Text usernameText;         // 사용자명 표시 (선택)

    // ============================================================
    /// 【 Initialize() - 화면 초기화 】
    /// 
    /// 호출 시점: UIManager.ShowScreen(GameState.Login)에서 호출
    /// 
    /// 역할:
    /// 1. GameManager에서 현재 통계 가져오기
    /// 2. 각 통계를 텍스트로 표시
    /// 3. 프로필 이미지 표시
    /// 4. 로그인 버튼 이벤트 등록
    /// 
    /// 동작:
    /// - GameManager.Instance.SavedCount 가져오기
    /// - GameManager.Instance.HeartCount 가져오기
    /// - GameManager.Instance.TotalPlayTime 가져오기
    /// - 텍스트 업데이트
    /// - 사용 시간을 분:초 형식으로 변환
    /// ============================================================
    public void Initialize()
    {
        Debug.Log("🔐 로그인 화면 초기화됨");

        /// GameManager 가져오기
        GameManager gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogError("❌ GameManager를 찾을 수 없습니다! GameManager 싱글톤이 초기화되지 않았습니다.");
            return;
        }

        /// ===== 【 통계 표시 】 =====
        
        /// 1️⃣ 저장한 수 표시
        /// - 정의: 일반 게시물을 클릭한 횟수
        /// - 형식: "📌 저장한 수\n{개수}"
        if (savedCountText != null)
        {
            savedCountText.text = $"📌 저장한 수\n{gm.SavedCount}";
        }

        /// 2️⃣ 하트 수 표시
        /// - 정의: 미니게임을 완료한 횟수
        /// - 형식: "❤️ 하트\n{개수}"
        if (heartCountText != null)
        {
            heartCountText.text = $"❤️ 하트\n{gm.HeartCount}";
        }

        /// 3️⃣ 사용 시간 표시
        /// - 정의: 게임에 소비한 총 시간 (초 단위)
        /// - 변환: 초 → 분:초 형식
        ///   예) 125초 → 2분 5초
        /// - 계산: 분 = (int)(시간 / 60), 초 = (int)(시간 % 60)
        if (playTimeText != null)
        {
            int minutes = (int)(gm.TotalPlayTime / 60f);      // 분 계산
            int seconds = (int)(gm.TotalPlayTime % 60f);      // 초 계산 (나머지)
            playTimeText.text = $"⏱️ 사용 시간\n{minutes}분 {seconds}초";
        }

        /// 4️⃣ 프로필 이미지
        /// - 임시로 회색으로 표시 (나중에 플레이어 이미지로 변경 가능)
        if (profileImage != null)
        {
            profileImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);  // 회색 (RGB: 200, 200, 200)
        }

        /// 5️⃣ 사용자명 표시
        /// - 임시 사용자명 "사용자123"으로 표시
        /// - 나중에 플레이어 이름으로 변경 가능
        if (usernameText != null)
        {
            usernameText.text = "사용자123";
        }

        /// ===== 【 로그인 버튼 설정 】 =====
        if (loginButton != null)
        {
            /// 기존 리스너 제거 (중복 등록 방지)
            loginButton.onClick.RemoveAllListeners();
            
            /// 클릭 이벤트 등록
            loginButton.onClick.AddListener(OnLoginButtonClicked);
        }
        else
        {
            Debug.LogWarning("⚠️ loginButton이 할당되지 않았습니다!");
        }
    }

    // ============================================================
    /// 【 OnLoginButtonClicked() - 로그인 버튼 클릭 이벤트 】
    /// 
    /// 호출 시점: 사용자가 로그인 버튼을 클릭했을 때
    /// 
    /// 역할:
    /// - 로그인 버튼 클릭 로그 출력
    /// - GameManager.OnLoginComplete() 호출
    /// - Login → Algorithm 상태 전환
    /// - 테마 선택 화면 표시
    /// ============================================================
    private void OnLoginButtonClicked()
    {
        Debug.Log("🔓 로그인 버튼 클릭됨!");
        
        /// GameManager에 로그인 완료 알림
        /// → 상태가 Login → Algorithm으로 변경
        /// → 화면이 LoginScreen → AlgorithmScreen으로 전환
        GameManager.Instance.OnLoginComplete();
    }
}
