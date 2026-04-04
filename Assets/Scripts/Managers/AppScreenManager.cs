using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 앱 클릭 화면 매니저
/// 게임 시작 시 표시되는 화면으로, SNS 앱 아이콘을 클릭하면 로그인 화면으로 이동
/// </summary>
public class AppScreenManager : MonoBehaviour
{
    private GameObject screenGO;

    /// <summary>앱 화면 생성 및 표시</summary>
    public void Show(System.Action onAppClicked)
    {
        // 기존 화면 정리
        if (screenGO != null)
            Destroy(screenGO);

        screenGO = new GameObject("AppScreen");
        UIHelper.SetupFullScreenBackground(screenGO, new Color(0.2f, 0.2f, 0.2f));

        // 앱 아이콘 (중앙에 위치)
        UIHelper.CreateButton(screenGO, "AppIcon", 150, 150, 
            new Vector2(0.5f, 0.5f), Color.blue, "SNS", 30, onAppClicked);
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

