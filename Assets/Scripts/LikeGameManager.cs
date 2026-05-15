using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Video;

public enum GameState { Playing, Win, Lose }

public class LikeGameManager : MonoBehaviour
{
    [Header("UI Objects")]
    public GameObject inGameUI;     // 게임 중 화면 (Slider, Like 버튼을 넣은 부모 오브젝트)
    public Slider ratioSlider;
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;

    [Header("Video")]
    public VideoPlayer reelsVideo;

    [Header("Game Settings")]
    public float winValue = 100f;
    public float currentLikes = 30f;
    public float dislikeSpeed = 15f; // 난이도 조절: 싫어요 속도
    public float likePower = 10f;    // 난이도 조절: 클릭 파워

    public GameState state = GameState.Playing;

    void Start()
    {
        // 초기 설정
        if (resultPanel != null) resultPanel.SetActive(false);
        if (inGameUI != null) inGameUI.SetActive(true);
    }

    void Update()
    {
        if (state != GameState.Playing) return;

        currentLikes -= dislikeSpeed * Time.deltaTime;
        currentLikes = Mathf.Clamp(currentLikes, 0, winValue);

        if (ratioSlider != null)
            ratioSlider.value = currentLikes / winValue;

        if (currentLikes >= winValue) GameWin();
        else if (currentLikes <= 0) GameLose();
    }

    public void OnLikeButtonClick()
    {
        if (state != GameState.Playing) return;
        currentLikes += likePower;
    }

    void GameWin()
    {
        state = GameState.Win;
        ShowResult("VICTORY!"); // 영어로 변경
    }

    void GameLose()
    {
        state = GameState.Lose;
        ShowResult("DEFEATED..."); // 영어로 변경
    }

    void ShowResult(string msg)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            if (inGameUI != null) inGameUI.SetActive(false);

            // 결과창 뜰 때 영상과 소리 모두 제어
            if (reelsVideo != null)
            {
                reelsVideo.Pause();       // 영상 일시정지
                reelsVideo.SetDirectAudioMute(0, true); // 소리 음소거 (트랙 0번)
            }

            if (resultText != null) resultText.text = msg;
        }
    }

    public void RestartGame() 
    { 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }

    public void GoHome()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndReelsSessionAndDrain("manual_exit");
            GameManager.Instance.SetStateWithoutUI(GameManager.GameState.Algorithm);
        }

        SceneManager.LoadScene("MainScene");
    }
}