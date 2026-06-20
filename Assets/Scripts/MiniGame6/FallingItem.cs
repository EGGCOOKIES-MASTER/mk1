using UnityEngine;
using TMPro;

public class FallingItem : MonoBehaviour
{
    public float fallSpeed = 500f; // 떨어지는 속도
    public bool isBadItem = true;

    [Header("텍스트 컴포넌트 연결")]
    public TextMeshProUGUI commentText;

    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // 매니저가 무작위 대사를 주입할 때 사용할 함수
    public void SetText(string newText, Color textColor)
    {
        if (commentText != null)
        {
            commentText.text = newText;
            commentText.color = textColor;
        }
    }

    void Update()
    {
        // 아래 방향으로 떨어지기
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        // 화면 바닥 Y축 밑으로 내려가면 자동 삭제
        if (rectTransform != null && rectTransform.anchoredPosition.y <= -700f)
        {
            Destroy(gameObject);
        }
    }
}