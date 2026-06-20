using UnityEngine;
using UnityEngine.UI;

public class AvoidPlayer : MonoBehaviour
{
    [Header("플레이어 표정 관리")]
    [SerializeField] private Image playerImage;
    [SerializeField] private Sprite smileSprite;
    [SerializeField] private Sprite sadSprite;
    [SerializeField] private float sadExpressionDuration = 0.6f;

    public float moveSpeed = 700f;

    [Header("충돌 판정 크기 (기본값 추천)")]
    public float collisionWidth = 80f;
    public float collisionHeight = 60f;

    private RectTransform rectTransform;
    private Canvas rootCanvas;
    private Coroutine expressionCoroutine;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
        if (playerImage == null) playerImage = GetComponent<Image>();
    }

    void Start()
    {
        ShowSmile();
    }

    void Update()
    {
        // 1. 키보드 A, D / 방향키 입력 처리
        float moveInput = 0f;
        if (UnityEngine.InputSystem.Keyboard.current?.aKey.isPressed == true ||
            UnityEngine.InputSystem.Keyboard.current?.leftArrowKey.isPressed == true)
        {
            moveInput = -1f;
        }
        else if (UnityEngine.InputSystem.Keyboard.current?.dKey.isPressed == true ||
                 UnityEngine.InputSystem.Keyboard.current?.rightArrowKey.isPressed == true)
        {
            moveInput = 1f;
        }

        // 마우스 클릭(터치) 이동 지원
        if (UnityEngine.InputSystem.Pointer.current?.press.isPressed == true)
        {
            float mouseX = UnityEngine.InputSystem.Pointer.current.position.ReadValue().x;
            if (mouseX < Screen.width / 2f) moveInput = -1f;
            else moveInput = 1f;
        }

        transform.Translate(Vector3.right * moveInput * moveSpeed * Time.deltaTime);

        // 2. 화면 가로 밖으로 도망 방지
        if (rootCanvas != null)
        {
            float canvasWidth = rootCanvas.GetComponent<RectTransform>().rect.width;
            float halfWidth = rectTransform.rect.width / 2f;
            Vector3 currentPos = transform.localPosition;

            float minX = -(canvasWidth / 2f) + halfWidth;
            float maxX = (canvasWidth / 2f) - halfWidth;

            currentPos.x = Mathf.Clamp(currentPos.x, minX, maxX);
            transform.localPosition = currentPos;
        }

        // 3. 실시간 악플 충돌 체크
        CheckCollisions();
    }

    void CheckCollisions()
    {
        // 게임이 종료되었다면 충돌 계산 중단
        if (AvoidGameManager.Instance != null && AvoidGameManager.Instance.IsFinished()) return;

        FallingItem[] activeItems = FindObjectsByType<FallingItem>(FindObjectsSortMode.None);

        foreach (FallingItem item in activeItems)
        {
            if (item == null) continue;

            RectTransform itemRect = item.GetComponent<RectTransform>();
            if (itemRect == null) continue;

            // UI 픽셀 정밀 거리 계산
            float distanceX = Mathf.Abs(rectTransform.anchoredPosition.x - itemRect.anchoredPosition.x);
            float distanceY = Mathf.Abs(rectTransform.anchoredPosition.y - itemRect.anchoredPosition.y);

            // 가로 세로 범위 내에 겹치면 부딪힌 것!
            if (distanceX <= collisionWidth && distanceY <= collisionHeight)
            {
                HandleCollision(item);
                break;
            }
        }
    }

    void HandleCollision(FallingItem item)
    {
        if (item.isBadItem)
        {
            ShowSadExpression();

            // 매니저에게 목숨이 깎였다고 알림
            if (AvoidGameManager.Instance != null)
            {
                AvoidGameManager.Instance.PlayerHit();
            }
        }

        Destroy(item.gameObject);
    }

    void ShowSadExpression()
    {
        if (playerImage == null || sadSprite == null) return;
        playerImage.sprite = sadSprite;

        if (expressionCoroutine != null) StopCoroutine(expressionCoroutine);
        expressionCoroutine = StartCoroutine(RestoreSmileAfterDelay());
    }

    System.Collections.IEnumerator RestoreSmileAfterDelay()
    {
        yield return new WaitForSeconds(sadExpressionDuration);
        ShowSmile();
        expressionCoroutine = null;
    }

    void ShowSmile()
    {
        if (playerImage != null && smileSprite != null) playerImage.sprite = smileSprite;
    }
}