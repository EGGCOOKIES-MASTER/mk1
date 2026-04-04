using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 릴스(피드) 화면 매니저
/// 선택된 테마의 릴스들을 수직으로 스크롤하며 볼 수 있음
/// 릴스 클릭 시: 30% 확률로 미니게임, 70% 확률로 통계 증가
/// </summary>
public class ReelsManager : MonoBehaviour
{
    private GameObject screenGO;
    private Text batteryText;
    private GameObject contentGO;
    
    private const int MINI_GAME_PROBABILITY = 30;
    private const int REEL_COUNT = 8;

    /// <summary>릴스 화면 생성 및 표시</summary>
    public void Show(string currentTheme, float batteryLevel, System.Action onBackClicked, System.Action onMiniGameSelected)
    {
        // 기존 화면 정리
        if (screenGO != null)
            Destroy(screenGO);

        screenGO = new GameObject("ReelsScreen");
        UIHelper.SetupFullScreenBackground(screenGO, Color.black);

        // 테마 표시
        UIHelper.CreateText(screenGO, "ThemeText", $"[{currentTheme}] 릴스", 20, Color.white,
            new Vector2(0.5f, 0.95f), new Vector2(200, 30));

        // 우상단에 배터리 표시
        GameObject batteryGO = new GameObject("BatteryText");
        batteryGO.transform.SetParent(screenGO.transform);
        batteryText = batteryGO.AddComponent<Text>();
        batteryText.text = $"{batteryLevel:F0}%";
        batteryText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        batteryText.fontSize = 24;
        batteryText.color = Color.white;
        RectTransform batteryRect = batteryGO.GetComponent<RectTransform>();
        batteryRect.anchorMin = new Vector2(1, 1);
        batteryRect.anchorMax = new Vector2(1, 1);
        batteryRect.anchoredPosition = new Vector2(-30, -30);
        batteryRect.sizeDelta = new Vector2(100, 50);

        // 좌상단에 돌아가기 버튼
        UIHelper.CreateButton(screenGO, "BackButton", 60, 50,
            new Vector2(0, 1), new Color(0.3f, 0.3f, 0.3f), "←", 25, onBackClicked,
            new Vector2(30, -30));

        // 스크롤 뷰 생성 (릴스 피드)
        UIHelper.CreateScrollView(screenGO, out contentGO);

        // 릴스 동적 생성
        for (int i = 0; i < REEL_COUNT; i++)
        {
            CreateReel(contentGO, i, onMiniGameSelected);
        }
    }

    /// <summary>개별 릴스 생성 (30% 확률로 미니게임, 70% 확률로 일반 릴스)</summary>
    private void CreateReel(GameObject parentGO, int index, System.Action onMiniGameSelected)
    {
        GameObject reelGO = new GameObject($"Reel_{index}");
        reelGO.transform.SetParent(parentGO.transform);
        
        RectTransform reelRect = reelGO.AddComponent<RectTransform>();
        reelRect.sizeDelta = new Vector2(0, 300);

        Image reelImage = reelGO.AddComponent<Image>();
        // 랜덤 색상의 릴스
        reelImage.color = new Color(Random.value, Random.value, Random.value);

        Button reelButton = reelGO.AddComponent<Button>();

        // 30% 확률로 미니게임 릴스 생성
        if (Random.value * 100 < MINI_GAME_PROBABILITY)
        {
            // 미니게임 릴스 표시
            UIHelper.CreateText(reelGO, "ReelText", "미니게임!", 30, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0, 0), Vector2.zero, true);

            reelButton.onClick.AddListener(onMiniGameSelected);
        }
    }

    /// <summary>배터리 표시 업데이트</summary>
    public void UpdateBattery(float batteryLevel)
    {
        if (batteryText != null)
        {
            batteryText.text = $"{batteryLevel:F0}%";
        }
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

