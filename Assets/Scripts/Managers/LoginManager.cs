using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 로그인 화면 매니저
/// 왼쪽: 사용 통계 (저장 수, 하트, 사용 시간)
/// 오른쪽: 프로필 + 로그인 버튼
/// </summary>
public class LoginManager : MonoBehaviour
{
    private GameObject screenGO;

    /// <summary>로그인 화면 생성 및 표시</summary>
    public void Show(int savedCount, int heartsPressed, int usedMinutes, System.Action onLoginClicked)
    {
        // 기존 화면 정리
        if (screenGO != null)
            Destroy(screenGO);

        screenGO = new GameObject("LoginScreen");
        UIHelper.SetupFullScreenBackground(screenGO, Color.white);

        // ========== 왼쪽: 통계 영역 ==========
        GameObject statsGO = new GameObject("Stats");
        statsGO.transform.SetParent(screenGO.transform);
        RectTransform statsRect = statsGO.AddComponent<RectTransform>();
        statsRect.anchorMin = Vector2.zero;
        statsRect.anchorMax = new Vector2(0.5f, 1);
        UIHelper.SetupRectTransform(statsRect);

        Image statsImage = statsGO.AddComponent<Image>();
        statsImage.color = new Color(0.9f, 0.9f, 0.9f);

        // 통계 텍스트 (저장 수, 하트, 사용 시간 표시)
        UIHelper.CreateText(statsGO, "StatsTitle", 
            $"내 활동\n\n저장한 수: {savedCount}\n하트 누른 수: {heartsPressed}\n사용한 시간: {usedMinutes}분",
            18, Color.black, new Vector2(0.25f, 0.5f), new Vector2(200, 250));

        // ========== 오른쪽: 프로필 영역 ==========
        GameObject profileGO = new GameObject("Profile");
        profileGO.transform.SetParent(screenGO.transform);
        RectTransform profileRect = profileGO.AddComponent<RectTransform>();
        profileRect.anchorMin = new Vector2(0.5f, 0);
        profileRect.anchorMax = Vector2.one;
        UIHelper.SetupRectTransform(profileRect);

        Image profileImage = profileGO.AddComponent<Image>();
        profileImage.color = new Color(0.8f, 0.8f, 0.8f);

        // 프로필 원
        GameObject profileCircleGO = new GameObject("ProfileCircle");
        profileCircleGO.transform.SetParent(profileGO.transform);
        RectTransform profileCircleRect = profileCircleGO.AddComponent<RectTransform>();
        UIHelper.SetupRectTransform(profileCircleRect, new Vector2(0.5f, 0.7f), new Vector2(100, 100));

        Image profileCircleImage = profileCircleGO.AddComponent<Image>();
        profileCircleImage.color = Color.cyan;

        // 로그인 버튼
        UIHelper.CreateButton(profileGO, "LoginButton", 150, 50, 
            new Vector2(0.5f, 0.2f), Color.green, "로그인", 20, onLoginClicked);
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

