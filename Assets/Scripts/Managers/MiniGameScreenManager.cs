using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 미니게임 화면 매니저
/// 클릭 버튼을 눌러 미니게임을 완료
/// 완료 시 배터리 2% 소모, 사용 시간 +5분
/// </summary>
public class MiniGameScreenManager : MonoBehaviour
{
    private GameObject screenGO;

    /// <summary>미니게임 화면 생성 및 표시</summary>
    public void Show(System.Action onMiniGameComplete)
    {
        // 기존 화면 정리
        if (screenGO != null)
            Destroy(screenGO);

        screenGO = new GameObject("MiniGameScreen");
        UIHelper.SetupFullScreenBackground(screenGO, Color.yellow);

        // 미니게임 설명 텍스트
        UIHelper.CreateText(screenGO, "MiniGameText", 
            "타이핑 게임\n아무 버튼이나 누르세요!", 25, Color.black,
            new Vector2(0.5f, 0.6f), new Vector2(300, 150));

        // 클릭 버튼
        UIHelper.CreateButton(screenGO, "ClickButton", 200, 100,
            new Vector2(0.5f, 0.3f), Color.blue, "클릭!", 25, onMiniGameComplete);
    }

    /// <summary>화면 정리</summary>
    public void Hide()
    {
        if (screenGO != null)
        {
            Destroy(screenGO);
            screenGO = null;
        }
    }
}

