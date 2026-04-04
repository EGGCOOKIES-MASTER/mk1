using UnityEngine;
using UnityEngine.UI;

// PostItem 클래스: SNS 피드의 개별 게시물을 관리하는 클래스
public class PostItem : MonoBehaviour
{
    // 게시물 텍스트 컴포넌트
    public Text postText;
    // 게시물 버튼 컴포넌트
    public Button postButton;
    // 미니게임 게시물인지 여부
    public bool isMiniGamePost;

    // 시작 시 버튼 클릭 리스너 추가
    private void Start()
    {
        postButton.onClick.AddListener(OnPostClicked);
    }

    // 게시물 설정 메서드
    public void SetPost(string text, bool miniGame)
    {
        postText.text = text;
        isMiniGamePost = miniGame;
    }

    // 게시물 클릭 시 호출되는 메서드
    private void OnPostClicked()
    {
        if (isMiniGamePost)
        {
            // 미니게임 UI 표시
            UIManager.Instance.ShowMiniGameUI();
        }
        else
        {
            // 일반 게시물 반응, 예를 들어 텍스트 변경
            Debug.Log("일반 게시물 클릭됨: " + postText.text);
        }
    }
}
