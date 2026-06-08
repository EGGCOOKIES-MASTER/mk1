using UnityEngine;
using TMPro; // 텍스트 제어를 위해 추가

public class FallingItem : MonoBehaviour
{
    public float fallSpeed = 400f;
    public bool isBadItem = true;

    [Header("텍스트 컴포넌트 연결")]
    public TextMeshProUGUI commentText; // 프리팹 안의 CommentText를 연결할 칸

    private float destroyY = -600f;

    // 외부(Manager)에서 악플 대사를 넘겨받아 글자를 바꿔주는 함수
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
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        if (transform.localPosition.y <= destroyY)
        {
            Destroy(gameObject);
        }
    }
}