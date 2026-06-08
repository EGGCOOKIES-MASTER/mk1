using UnityEngine;
using TMPro;
using System.Collections;

public class JudgmentEffect : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    // 외부에서 판정 글자와 색상을 넘겨주며 호출하는 함수
    public void ShowText(string message, Color textColor)
    {
        textMesh.text = message;
        textMesh.color = textColor;

        // 이미 작동 중인 효과가 있다면 끄고 새로 시작
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutRoutine());
    }

    IEnumerator FadeOutRoutine()
    {
        float duration = 0.5f; // 효과 지속 시간 (0.5초)
        float timer = 0f;

        Vector3 startPos = Vector3.zero; // 정중앙 시작
        transform.localPosition = startPos;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // 1. 위로 슬며시 올라가는 연출
            transform.localPosition = startPos + new Vector3(0, progress * 50f, 0);

            // 2. 서서히 투명해지는 연출 (Fade Out)
            Color c = textMesh.color;
            c.a = Mathf.Lerp(1f, 0f, progress);
            textMesh.color = c;

            yield return null;
        }

        textMesh.text = ""; // 완전히 사라지면 텍스트 비우기
    }
}