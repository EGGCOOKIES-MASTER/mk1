using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 엔딩 화면 매니저
/// 배터리가 1%가 되면 표시되는 화면
/// 검은 배경 + 어두운 실루엣으로 SNS 중독의 끝을 표현
/// </summary>
public class EndingManager : MonoBehaviour
{
    private GameObject screenGO;

    /// <summary>엔딩 화면 생성 및 표시</summary>
    public void Show()
    {
        // 기존 화면 정리
        if (screenGO != null)
            Destroy(screenGO);

        screenGO = new GameObject("EndingScreen");
        UIHelper.SetupFullScreenBackground(screenGO, Color.black);

        // 중앙에 실루엣 (어두운 원 - 사용자의 그림자)
        GameObject silhouetteGO = new GameObject("Silhouette");
        silhouetteGO.transform.SetParent(screenGO.transform);
        RectTransform silhouetteRect = silhouetteGO.AddComponent<RectTransform>();
        UIHelper.SetupRectTransform(silhouetteRect, new Vector2(0.5f, 0.5f), new Vector2(200, 300));

        Image silhouetteImage = silhouetteGO.AddComponent<Image>();
        silhouetteImage.color = new Color(0.3f, 0.3f, 0.3f);

        // 엔딩 메시지
        UIHelper.CreateText(screenGO, "EndingText",
            "배터리 1%\n당신의 핸드폰이 꺼졌습니다...", 20, Color.white,
            new Vector2(0.5f, 0.2f), new Vector2(300, 100));
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

