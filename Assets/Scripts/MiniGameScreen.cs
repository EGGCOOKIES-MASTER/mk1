using UnityEngine;
using UnityEngine.UI;

/// ============================================================
/// 【 MiniGameScreen - 미니게임 화면 (임시) 】
/// 
/// 역할:
/// - 미니게임 화면 표시 (현재는 임시 화면)
/// - 5가지 미니게임 중 랜덤 선택
/// - 완료/스킵 버튼으로 미니게임 종료
/// - 배터리 감소 트리거
/// 
/// 특징:
/// - 완료 또는 스킵 버튼 클릭 시 동일하게 배터리 -10%
/// - 게임 시간 +5초 추가
/// - 하트 +1 증가
/// - 나중에 실제 미니게임 로직으로 교체 가능
/// 
/// 나중에 할 것:
/// - LikeButtonGame.cs (좋아요 연타)
/// - TimingGame.cs (타이밍 맞추기)
/// - YabanwaiGame.cs (야바위)
/// - QuizGame.cs (퀴즈)
/// - HiddenPictureGame.cs (숨은그림찾기)
/// ============================================================

public class MiniGameScreen : MonoBehaviour
{
    /// ===== 【 UI 요소 참조 】 =====
    [SerializeField] private Text titleText;             // 🎮 미니게임 제목
    [SerializeField] private Text instructionText;       // 📝 미니게임 설명/지시사항
    [SerializeField] private Button completeButton;      // ✅ 완료 버튼
    [SerializeField] private Button skipButton;          // ⏭️ 스킵 버튼

    /// ===== 【 미니게임 정보 】 =====
    /// currentMiniGame: 현재 선택된 미니게임 이름
    /// playtime: 이 미니게임의 플레이 시간 (초)
    private string currentMiniGame = "좋아요 연타";
    private float playtime = 0f;

    // ============================================================
    /// 【 Initialize() - 미니게임 화면 초기화 】
    /// 
    /// 호출 시점: UIManager.ShowScreen(GameState.MiniGame)에서 호출
    /// 
    /// 역할:
    /// 1. 5가지 게임 중 랜덤 선택
    /// 2. 게임 제목 설정
    /// 3. 게임 설명 설정
    /// 4. 완료/스킵 버튼 이벤트 등록
    /// 5. 플레이타임 초기화
    /// ============================================================
    public void Initialize()
    {
        Debug.Log("🎮 미니게임 화면 초기화됨");

        /// 5가지 게임 중 랜덤 선택
        SelectRandomMiniGame();

        /// 게임 제목 설정
        if (titleText != null)
        {
            titleText.text = $"⭐ {currentMiniGame}";
        }
        else
        {
            Debug.LogWarning("⚠️ titleText가 할당되지 않았습니다!");
        }

        /// 게임 설명 설정
        /// 현재는 임시 설명, 나중에 각 게임별 설명으로 변경
        if (instructionText != null)
        {
            instructionText.text = "미니게임을 플레이해보세요!\n(지금은 임시 화면입니다)";
        }
        else
        {
            Debug.LogWarning("⚠️ instructionText가 할당되지 않았습니다!");
        }

        /// 완료 버튼 설정
        if (completeButton != null)
        {
            completeButton.onClick.RemoveAllListeners();
            completeButton.onClick.AddListener(OnCompleteButtonClicked);
        }
        else
        {
            Debug.LogWarning("⚠️ completeButton이 할당되지 않았습니다!");
        }

        /// 스킵 버튼 설정
        /// 스킵해도 배터리는 소모됨 (게임의 핵심 메시지)
        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(OnSkipButtonClicked);
        }
        else
        {
            Debug.LogWarning("⚠️ skipButton이 할당되지 않았습니다!");
        }

        playtime = 0f;
    }

    // ============================================================
    /// 【 SelectRandomMiniGame() - 미니게임 랜덤 선택 】
    /// 
    /// 역할:
    /// - 5가지 미니게임 중 하나를 랜덤으로 선택
    /// - currentMiniGame 변수에 저장
    /// 
    /// 5가지 미니게임:
    /// 1. 좋아요 연타 ❤️ - 버튼을 빠르게 클릭
    /// 2. 타이밍 맞추기 ⏱️ - 정확한 시간에 버튼 클릭
    /// 3. 야바위 ✋ - 숨겨진 것 맞추기
    /// 4. 퀴즈 🧠 - 문제 푸는 게임
    /// 5. 숨은그림찾기 🔍 - 다른 그림 찾기
    /// ============================================================
    private void SelectRandomMiniGame()
    {
        /// 5가지 미니게임 목록
        string[] miniGames = new string[]
        {
            "좋아요 연타",      // 1번 - 버튼 연타
            "타이밍 맞추기",    // 2번 - 정확한 타이밍
            "야바위",           // 3번 - 추측 게임
            "퀴즈",             // 4번 - 객관식 문제
            "숨은그림찾기"      // 5번 - 차이점 찾기
        };

        /// Random.Range(0, 5)는 0~4 사이의 정수 반환
        currentMiniGame = miniGames[Random.Range(0, miniGames.Length)];
        Debug.Log($"🎮 선택된 미니게임: {currentMiniGame}");
    }

    // ============================================================
    /// 【 OnCompleteButtonClicked() - 완료 버튼 클릭 】
    /// 
    /// 호출 시점: "완료" 버튼을 클릭했을 때
    /// 
    /// 역할:
    /// 1. 미니게임 완료 로그 출력
    /// 2. 게임 시간 추가 (+5초)
    /// 3. 하트 수 증가 (+1)
    /// 4. GameManager.OnMiniGameComplete() 호출
    /// 5. 배터리 -10% + 엔딩 여부 판단
    /// ============================================================
    private void OnCompleteButtonClicked()
    {
        Debug.Log($"✅ 미니게임 '{currentMiniGame}' 완료!");
        
        /// 게임 시간 추가 (예: 5초)
        GameManager.Instance.AddPlayTime(5f);

        /// 하트 증가 (미니게임 완료)
        GameManager.Instance.IncreaseHeartCount();

        /// 미니게임 완료 알림
        /// → GameManager에서:
        ///   - 배터리 -10%
        ///   - 배터리 확인 (1% 이하면 엔딩, 아니면 릴스로 복귀)
        GameManager.Instance.OnMiniGameComplete();
    }

    // ============================================================
    /// 【 OnSkipButtonClicked() - 스킵 버튼 클릭 】
    /// 
    /// 호출 시점: "스킵" 버튼을 클릭했을 때
    /// 
    /// 역할:
    /// - 미니게임을 건너뜀
    /// - 하지만 배터리는 여전히 -10% 소모
    /// 
    /// 게임 설계:
    /// - "스킵해도 배터리가 소모된다" 
    /// - = SNS 중독의 메시지:
    ///   "어차피 시간이 낭비된다"
    /// ============================================================
    private void OnSkipButtonClicked()
    {
        Debug.Log($"⏭️ 미니게임 '{currentMiniGame}' 스킵!");

        /// 스킵해도 배터리는 소모됨 (게임의 핵심 메시지)
        /// 따라서 완료와 동일한 결과
        GameManager.Instance.OnMiniGameComplete();
    }
}

