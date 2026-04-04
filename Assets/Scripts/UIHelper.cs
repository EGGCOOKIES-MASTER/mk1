using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 공용 UI 생성 헬퍼 클래스
/// 모든 화면에서 사용되는 버튼, 텍스트 등을 일관되게 생성
/// </summary>
public static class UIHelper
{
    // ================================ RectTransform 설정 ================================
    /// <summary>RectTransform 기본값 설정 (여백 제거)</summary>
    public static void SetupRectTransform(RectTransform rect, Vector2 anchorPos = default, Vector2 sizeDelta = default)
    {
        if (anchorPos != default)
        {
            rect.anchorMin = anchorPos;
            rect.anchorMax = anchorPos;
            rect.sizeDelta = sizeDelta;
        }
        
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    // ================================ 배경 설정 ================================
    /// <summary>전체 화면 배경 설정</summary>
    public static void SetupFullScreenBackground(GameObject screenGO, Color bgColor)
    {
        RectTransform rectTransform = screenGO.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        SetupRectTransform(rectTransform);

        Image bgImage = screenGO.AddComponent<Image>();
        bgImage.color = bgColor;
    }

    // ================================ 버튼 생성 ================================
    /// <summary>버튼 생성 (색상, 텍스트, 클릭 이벤트 포함)</summary>
    public static GameObject CreateButton(GameObject parentGO, string buttonName, float width, float height,
        Vector2 anchorPosition, Color buttonColor, string buttonText, int fontSize,
        UnityEngine.Events.UnityAction onClickAction, Vector2 offsetPosition = default)
    {
        GameObject buttonGO = new GameObject(buttonName);
        buttonGO.transform.SetParent(parentGO.transform);

        Button button = buttonGO.AddComponent<Button>();
        button.onClick.AddListener(onClickAction);

        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = buttonColor;

        RectTransform buttonRect = buttonGO.AddComponent<RectTransform>();
        buttonRect.anchorMin = anchorPosition;
        buttonRect.anchorMax = anchorPosition;
        buttonRect.sizeDelta = new Vector2(width, height);
        
        if (offsetPosition != default)
            buttonRect.anchoredPosition = offsetPosition;

        CreateText(buttonGO, "ButtonText", buttonText, fontSize, Color.black,
            Vector2.zero, Vector2.zero, Vector2.zero, true);

        return buttonGO;
    }

    // ================================ 텍스트 생성 ================================
    /// <summary>텍스트 생성 (fillParent = true면 부모를 가득 채움)</summary>
    public static void CreateText(GameObject parentGO, string textName, string textContent, int fontSize,
        Color textColor, Vector2 anchorPos, Vector2 sizeDelta, Vector2 offsetPos = default, bool fillParent = false)
    {
        GameObject textGO = new GameObject(textName);
        textGO.transform.SetParent(parentGO.transform);

        Text text = textGO.AddComponent<Text>();
        text.text = textContent;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.color = textColor;
        text.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        
        if (fillParent)
        {
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
        else
        {
            textRect.anchorMin = anchorPos;
            textRect.anchorMax = anchorPos;
            textRect.sizeDelta = sizeDelta;
            
            if (offsetPos != default)
                textRect.anchoredPosition = offsetPos;
        }
    }

    // ================================ 스크롤 뷰 생성 ================================
    /// <summary>스크롤 뷰 생성 (피드 용)</summary>
    public static GameObject CreateScrollView(GameObject parentGO, out GameObject contentGO)
    {
        GameObject scrollViewGO = new GameObject("ScrollView");
        scrollViewGO.transform.SetParent(parentGO.transform);
        
        ScrollRect scrollRect = scrollViewGO.AddComponent<ScrollRect>();
        Image scrollImage = scrollViewGO.AddComponent<Image>();
        scrollImage.color = Color.black;
        
        RectTransform scrollRectTransform = scrollViewGO.AddComponent<RectTransform>();
        scrollRectTransform.anchorMin = Vector2.zero;
        scrollRectTransform.anchorMax = Vector2.one;
        scrollRectTransform.offsetMin = new Vector2(0, 50);
        scrollRectTransform.offsetMax = new Vector2(0, -50);

        // 뷰포트
        GameObject viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollViewGO.transform);
        RectTransform viewportRect = viewportGO.AddComponent<RectTransform>();
        SetupRectTransform(viewportRect);
        viewportGO.AddComponent<Mask>();
        scrollRect.viewport = viewportRect;

        // 콘텐츠
        contentGO = new GameObject("Content");
        contentGO.transform.SetParent(viewportGO.transform);
        RectTransform contentRect = contentGO.AddComponent<RectTransform>();
        SetupRectTransform(contentRect);
        contentRect.sizeDelta = new Vector2(0, 1200);
        
        VerticalLayoutGroup vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.childControlHeight = false;
        vlg.childForceExpandHeight = false;
        scrollRect.content = contentRect;

        return scrollViewGO;
    }
}

