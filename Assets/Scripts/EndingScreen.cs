using UnityEngine;
using UnityEngine.UI;

/// ============================================================
/// 【 EndingScreen - 엔딩 화면 (배터리 1% 도달 시) 】
/// 
/// 역할:
/// - 게임 종료 화면 표시
/// - 검은 배경 + 실루엣으로 분위기 연출
/// - 게임 메시지 표시
/// - 최종 통계 표시 (저장한 수, 하트, 사용 시간)
/// - 재시작 버튼 제공
/// 
/// 특징:
/// - 배터리가 1% 이하가 되었을 때 표시됨
/// - 검은 화면: 핸드폰 배터리 소진의 시각화
/// - 실루엣: 사용자의 형태만 보임
/// - 메시지: SNS 중독에 대한 성찰 유도
/// - 통계: 게임 중 누적된 행동 표시
/// 
/// 게임 흐름 끝:
/// [배터리 1% 도달]
///   ↓
/// GameManager.OnBatteryDepleted()
///   ↓
/// EndingScreen 표시 (이 화면)
///   ↓
/// 사용자가 재시작 버튼 클릭
///   ↓
/// 씬 재로드 → 게임 다시 시작
/// ============================================================

public class EndingScreen : MonoBehaviour
{
    /// ===== 【 UI 요소 참조 】 =====
    [SerializeField] private Image blackScreen;          // ⬛ 검은색 배경 (배터리 소진 표현)
    [SerializeField] private Image silhouetteImage;      // 👤 실루엣 이미지
    [SerializeField] private Text messageText;           // 📝 엔딩 메시지
    [SerializeField] private Text statisticsText;        // 📊 최종 통계
    [SerializeField] private Button restartButton;       // 🔄 재시작 버튼

    // ============================================================
    /// 【 Initialize() - 엔딩 화면 초기화 】
    /// 
    /// 호출 시점: UIManager.ShowScreen(GameState.Ending)에서 호출
    /// 
    /// 역할:
    /// 1. 검은 배경 설정 (배터리 소진 표현)
    /// 2. 실루엣 이미지 설정
    /// 3. 엔딩 메시지 표시
    /// 4. 최종 통계 표시
    /// 5. 재시작 버튼 이벤트 등록
    /// ============================================================
    public void Initialize()
    {
        Debug.Log("💀 엔딩 화면 초기화됨");

        /// ===== 【 Step 1: 검은 배경 설정 】 =====
        /// 배터리가 완전히 소진되었음을 시각화
        /// 검은색: RGB(0, 0, 0), 불투명도 100%
        if (blackScreen != null)
        {
            blackScreen.color = new Color(0f, 0f, 0f, 1f);
        }
        else
        {
            Debug.LogWarning("⚠️ blackScreen이 할당되지 않았습니다!");
        }

        /// ===== 【 Step 2: 실루엣 이미지 설정 】 =====
        /// 검은 배경 위에 실루엣만 보임
        /// 색상: 어두운 회색 RGB(75, 75, 75), 불투명도 80%
        /// 효과: 배경과 실루엣의 명도 차이로 형태 표현
        if (silhouetteImage != null)
        {
            silhouetteImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        }
        else
        {
            Debug.LogWarning("⚠️ silhouetteImage가 할당되지 않았습니다!");
        }

        /// ===== 【 Step 3: 엔딩 메시지 표시 】 =====
        /// SNS 중독에 대한 메시지
        /// 플레이어가 게임을 통해 느낀 "시간 낭비"를 언어화
        if (messageText != null)
        {
            messageText.text = "배터리가 모두 소모되었습니다...\n\n" +
                             "당신은 얼마나 많은 시간과 집중력을\n" +
                             "SNS에 소비했을까요?";
        }
        else
        {
            Debug.LogWarning("⚠️ messageText가 할당되지 않았습니다!");
        }

        /// ===== 【 Step 4: 최종 통계 표시 】 =====
        /// GameManager에서 게임 중 누적된 통계 조회
        GameManager gm = GameManager.Instance;
        if (statisticsText != null && gm != null)
        {
            /// 시간 변환: 초 → 분:초
            int minutes = (int)(gm.TotalPlayTime / 60f);      // 분 계산
            int seconds = (int)(gm.TotalPlayTime % 60f);      // 초 계산 (나머지)

            /// 최종 통계 텍스트 구성
            statisticsText.text = $"📊 최종 통계\n" +
                                 $"━━━━━━━━━━━\n" +
                                 $"저장한 수: {gm.SavedCount}개\n" +
                                 $"하트: {gm.HeartCount}개\n" +
                                 $"사용 시간: {minutes}분 {seconds}초\n" +
                                 $"━━━━━━━━━━━";
        }
        else
        {
            Debug.LogWarning("⚠️ statisticsText가 할당되지 않았거나 GameManager를 찾을 수 없습니다!");
        }

        /// ===== 【 Step 5: 재시작 버튼 설정 】 =====
        /// 게임을 다시 시작하기 위한 버튼
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
        else
        {
            Debug.LogWarning("⚠️ restartButton이 할당되지 않았습니다!");
        }
    }

    // ============================================================
    /// 【 OnRestartButtonClicked() - 재시작 버튼 클릭 】
    /// 
    /// 호출 시점: 엔딩 화면에서 "재시작" 버튼을 클릭했을 때
    /// 
    /// 역할:
    /// 1. 현재 씬 재로드
    /// 2. GameManager 싱글톤 초기화 (배터리, 통계 유지)
    /// 3. 게임 다시 시작
    /// 
    /// 주의:
    /// - DontDestroyOnLoad로 지정된 GameManager는 유지됨
    /// - 따라서 통계는 누적됨 (다시 하면 누적된 통계 + 새 게임)
    /// - 원한다면 통계 초기화 로직 추가 필요
    /// ============================================================
    private void OnRestartButtonClicked()
    {
        Debug.Log("🔄 게임 재시작!");
        
        /// SceneManager.LoadScene()으로 현재 씬 재로드
        /// → 모든 UI, 게임오브젝트 초기화
        /// → GameManager는 유지 (DontDestroyOnLoad)
        /// → 게임 처음부터 다시 시작
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}

