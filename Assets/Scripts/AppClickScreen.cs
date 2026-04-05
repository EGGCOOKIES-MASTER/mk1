using UnityEngine;
using UnityEngine.UI;

/// ============================================================
/// 【 AppClickScreen - 앱 클릭 화면 (게임 진입점) 】
/// 
/// 역할:
/// - 게임의 첫 화면 표시
/// - 플레이어에게 앱 아이콘을 보여주고 클릭 유도
/// - 클릭 시 로그인 화면으로 전환
/// 
/// 특징:
/// - 게임의 시작점 (처음 화면)
/// - 매우 간단한 화면 (아이콘 + 제목)
/// - 버튼 클릭 이벤트만 처리
/// 
/// 흐름:
/// [게임 시작]
///   ↓
/// AppClickScreen 표시 (이 화면)
///   ↓
/// 사용자가 앱 아이콘 클릭
///   ↓
/// OnAppButtonClicked() 호출
///   ↓
/// GameManager.OnAppClicked()
///   ↓
/// LoginScreen 표시
/// ============================================================

public class AppClickScreen : MonoBehaviour
{
    /// ===== 【 UI 요소 참조 】 =====
    /// [SerializeField]로 선언해서 Inspector에서 할당
    [SerializeField] private Button appButton;          // 📱 앱 아이콘 버튼
    [SerializeField] private Text titleText;            // 게임 제목
    [SerializeField] private Image appIconImage;        // 앱 아이콘 이미지 (선택사항)

    // ============================================================
    /// 【 Initialize() - 화면 초기화 】
    /// 
    /// 호출 시점: UIManager.ShowScreen()에서 이 화면을 표시할 때
    /// 
    /// 역할:
    /// 1. 게임 제목 표시
    /// 2. 앱 버튼에 클릭 이벤트 등록
    /// 3. 화면 준비 완료
    /// 
    /// 동작 순서:
    /// 1. 기존 리스너 제거 (RemoveAllListeners)
    ///    - 같은 버튼을 여러 번 사용할 때 중복 등록 방지
    /// 2. 새 리스너 추가 (AddListener)
    ///    - 클릭 시 OnAppButtonClicked() 호출
    /// ============================================================
    public void Initialize()
    {
        Debug.Log("📱 앱 클릭 화면 초기화됨");

        /// 게임 제목 설정
        if (titleText != null)
        {
            titleText.text = "1%\nSNS 중독 시뮬레이터";  // 제목 + 부제
        }
        else
        {
            Debug.LogWarning("⚠️ titleText가 할당되지 않았습니다!");
        }

        /// 앱 버튼에 클릭 이벤트 등록
        if (appButton != null)
        {
            /// 기존 리스너 제거
            /// → 같은 버튼에 여러 리스너가 등록되는 것을 방지
            /// → 화면을 다시 표시할 때 중복 등록되지 않음
            appButton.onClick.RemoveAllListeners();
            
            /// 새 리스너 추가
            /// → 버튼 클릭 시 OnAppButtonClicked() 함수 호출
            appButton.onClick.AddListener(OnAppButtonClicked);
        }
        else
        {
            Debug.LogWarning("⚠️ appButton이 할당되지 않았습니다!");
        }
    }

    // ============================================================
    /// 【 OnAppButtonClicked() - 앱 버튼 클릭 이벤트 핸들러 】
    /// 
    /// 호출 시점: 사용자가 앱 아이콘 버튼을 클릭했을 때
    /// 
    /// 역할:
    /// - 클릭 로그 출력
    /// - GameManager.OnAppClicked() 호출
    /// - AppClick → Login 상태 전환
    /// 
    /// 흐름:
    /// 사용자 클릭
    ///   ↓
    /// OnAppButtonClicked() 호출
    ///   ↓
    /// GameManager.Instance.OnAppClicked()
    ///   ↓
    /// GameManager.ChangeState(GameState.Login)
    ///   ↓
    /// UIManager.ShowScreen(GameState.Login)
    ///   ↓
    /// LoginScreen 표시
    /// ============================================================
    private void OnAppButtonClicked()
    {
        Debug.Log("🖱️ 앱 버튼 클릭됨!");
        
        /// GameManager에 앱 클릭 알림
        /// → GameManager가 상태를 AppClick → Login으로 변경
        /// → 화면이 AppClickScreen → LoginScreen으로 전환
        GameManager.Instance.OnAppClicked();
    }
}

