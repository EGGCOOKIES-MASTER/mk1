using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 알고리즘(테마 선택) 화면 매니저
/// 사용자가 보고 싶은 테마의 릴스를 선택할 수 있음 (음식, 운동, 패션, 여행, 음악)
/// </summary>
public class AlgorithmManager : MonoBehaviour
{
    private GameObject screenGO;

    /// <summary>테마 선택 화면 생성 및 표시</summary>
    public void Show(System.Action<string> onThemeSelected)
    {
        // 기존 화면 정리
        if (screenGO != null)
            Destroy(screenGO);

        screenGO = new GameObject("AlgorithmScreen");
        UIHelper.SetupFullScreenBackground(screenGO, new Color(0.95f, 0.95f, 0.95f));

        // 타이틀
        UIHelper.CreateText(screenGO, "Title", "테마 선택", 30, Color.black, 
            new Vector2(0.5f, 0.9f), new Vector2(300, 50));

        // 테마 버튼들 (각각 다른 색상)
        string[] themes = { "음식", "운동", "패션", "여행", "음악" };
        Color[] themeColors = { Color.yellow, Color.red, Color.magenta, 
                                new Color(0, 1, 1), new Color(1, 0.5f, 0) };

        for (int i = 0; i < themes.Length; i++)
        {
            CreateThemeButton(screenGO, themes[i], themeColors[i], i, onThemeSelected);
        }
    }

    /// <summary>테마 버튼 생성 (반복되는 코드를 메서드로 분리)</summary>
    private void CreateThemeButton(GameObject parentGO, string themeName, Color buttonColor, int index, System.Action<string> onThemeSelected)
    {
        float yPosition = 0.7f - (index * 0.12f);
        UIHelper.CreateButton(parentGO, $"ThemeButton_{themeName}", 200, 60,
            new Vector2(0.5f, yPosition), buttonColor, themeName, 20, 
            () => onThemeSelected(themeName));
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

