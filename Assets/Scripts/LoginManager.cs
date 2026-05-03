using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환을 위해 필수!

public class LoginManager : MonoBehaviour
{
    [Header("설정: 이동할 씬 이름")]
    [Tooltip("유니티 Build Settings에 등록된 메인 씬 이름을 정확히 입력하세요.")]
    public string mainSceneName = "MainScene";

    // 로그인 버튼에 연결할 함수
    public void OnLoginButtonClick()
    {
        // 1. 버튼이 눌렸는지 콘솔창(Console)에서 바로 확인 가능
        Debug.Log("로그인 버튼 클릭됨! " + mainSceneName + " 씬으로 이동을 시도합니다.");

        // 2. 씬 이름이 비어있는지 체크 (실수 방지)
        if (!string.IsNullOrEmpty(mainSceneName))
        {
            SceneManager.LoadScene(mainSceneName);
        }
        else
        {
            Debug.LogError("이동할 씬 이름이 설정되지 않았습니다! Inspector에서 이름을 확인하세요.");
        }
    }
}