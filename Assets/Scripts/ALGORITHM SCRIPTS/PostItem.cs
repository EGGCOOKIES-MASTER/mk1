using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/*
 PostItem

 - 역할: 그리드 셀에 포스트를 표시하고 클릭 처리
 - 간단함: 단순 제목 표시 + 클릭시 미니게임 시작
 - contentImage: 색상 표시(실제 이미지 로드는 나중)
 */
public class PostItem : MonoBehaviour, IPointerClickHandler {
    [Header("UI References (assign in Inspector)")]
    public Image contentImage;       // 그리드 셀 배경 이미지
    public Text titleText;           // 포스트 제목 (옵션)

    // 바인딩된 데이터 레퍼런스
    private PostData data;

    /// <summary>
    /// 간단한 바인딩: 제목만 표시
    /// </summary>
    public void Bind(PostData post) {
        data = post;
        if (titleText != null) titleText.text = post.title ?? "";

        // 고유한 색상 생성(시각적 구분용)
        if (contentImage != null) {
            int hash = Mathf.Abs((post.id ?? "").GetHashCode());
            float v = (hash % 100) / 100f;
            contentImage.color = Color.Lerp(Color.gray, Color.black, v);
        }
    }

    /// <summary>
    /// 클릭시 미니게임 시작
    /// </summary>
    public void OnPointerClick(PointerEventData eventData) {
        Debug.Log($"Post clicked: {data?.id ?? "unknown"}");
        if (GameManager.Instance != null) {
            GameManager.Instance.OnMiniGameStart();
        }
    }
}

