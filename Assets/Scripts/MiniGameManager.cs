using UnityEngine;
using UnityEngine.UI;

// MiniGameManager 클래스: 미니게임(좋아요 연타)을 관리하는 싱글톤 클래스
public class MiniGameManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static MiniGameManager Instance;

    // 좋아요 버튼
    public Button likeButton;
    // 좋아요 카운트 텍스트
    public Text likeCountText;
    // 타이머 텍스트
    public Text timerText;
    // 필요한 좋아요 수
    public int requiredLikes = 10;
    // 현재 좋아요 수
    private int currentLikes = 0;
    // 남은 시간
    private float timeLeft = 5f; // 5초

    // 싱글톤 초기화
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 시작 시 버튼 리스너 추가 및 미니게임 초기화
    private void Start()
    {
        likeButton.onClick.AddListener(OnLikeClicked);
        ResetMiniGame();
    }

    // 매 프레임마다 타이머 업데이트
    private void Update()
    {
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            timerText.text = Mathf.CeilToInt(timeLeft) + "초";
            if (timeLeft <= 0)
            {
                EndMiniGame();
            }
        }
    }

    // 좋아요 버튼 클릭 시 호출
    private void OnLikeClicked()
    {
        currentLikes++;
        likeCountText.text = currentLikes + "/" + requiredLikes;
        if (currentLikes >= requiredLikes)
        {
            EndMiniGame();
        }
    }

    // 미니게임 초기화
    private void ResetMiniGame()
    {
        currentLikes = 0;
        timeLeft = 5f;
        likeCountText.text = "0/" + requiredLikes;
        timerText.text = "5초";
    }

    // 미니게임 종료
    private void EndMiniGame()
    {
        // SNS로 돌아가기
        UIManager.Instance.ReturnToSNS();
        ResetMiniGame();
    }
}
