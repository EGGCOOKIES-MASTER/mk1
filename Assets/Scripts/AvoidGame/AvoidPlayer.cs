using UnityEngine;
using UnityEngine.UI; // 하트 이미지를 제어하기 위해 반드시 필요합니다!

public class AvoidPlayer : MonoBehaviour
{
    [Header("Player Expression")]
    [SerializeField] private Image playerImage;
    [SerializeField] private Sprite smileSprite;
    [SerializeField] private Sprite sadSprite;
    [SerializeField] private float sadExpressionDuration = 0.8f;

    public float moveSpeed = 500f; // 캐릭터 이동 속도
    public float collisionDistance = 60f; // 충돌로 인정할 거리

    [Header("하트 UI 연결")]
    public Image[] heartImages; // 인스펙터에서 하트 3개를 넣을 바구니!
    private int currentHp;

    private RectTransform rectTransform;
    private Canvas rootCanvas;
    private Coroutine expressionCoroutine;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
        if (playerImage == null)
        {
            playerImage = GetComponent<Image>();
        }
    }

    void Start()
    {
        ShowSmile();

        // 게임 시작 시, 연결된 하트 이미지의 개수만큼 체력을 설정합니다.
        if (heartImages != null && heartImages.Length > 0)
        {
            currentHp = heartImages.Length;
        }
        else
        {
            currentHp = 3; // 만약 연결을 안 했다면 기본값 3개
        }
    }

    void Update()
    {
        // 1. 키보드 및 마우스 입력 이동 처리
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

        if (UnityEngine.InputSystem.Pointer.current?.press.isPressed == true)
        {
            float mouseX = UnityEngine.InputSystem.Pointer.current.position.ReadValue().x;
            if (mouseX < Screen.width / 2f) moveInput = -1f;
            else moveInput = 1f;
        }

        transform.Translate(Vector3.right * moveInput * moveSpeed * Time.deltaTime);

        // 2. 화면 밖 탈출 방지
        if (rootCanvas != null)
        {
            float canvasWidth = rootCanvas.GetComponent<RectTransform>().rect.width;
            float halfWidth = rectTransform.rect.width / 2f;
            Vector3 currentPos = transform.localPosition;

            float minX = -(canvasWidth / 2f) + halfWidth;
            float maxX = (canvasWidth / 2f) - halfWidth;

            if (rectTransform.anchorMin.x == 0 && rectTransform.anchorMax.x == 0)
            {
                minX = 0f + halfWidth;
                maxX = canvasWidth - halfWidth;
            }
            else if (rectTransform.anchorMin.x == 0.5f && rectTransform.anchorMax.x == 0.5f)
            {
                minX = -(canvasWidth / 2f) + halfWidth;
                maxX = (canvasWidth / 2f) - halfWidth;
            }

            currentPos.x = Mathf.Clamp(currentPos.x, minX, maxX);
            transform.localPosition = currentPos;
        }

        // 3. 실시간 악플 충돌 체크
        CheckCollisions();
    }

    void CheckCollisions()
    {
        FallingItem[] activeItems = FindObjectsByType<FallingItem>(FindObjectsSortMode.None);

        foreach (FallingItem item in activeItems)
        {
            float distance = Vector3.Distance(transform.localPosition, item.transform.localPosition);

            if (distance <= collisionDistance)
            {
                HandleCollision(item);
            }
        }
    }

    void HandleCollision(FallingItem item)
    {
        if (item.isBadItem)
        {
            Debug.Log("💥 악플 적중! 데미지를 입었습니다!");
            TakeDamage(); // 악플에 맞으면 목숨 깎기 함수 실행!
        }

        Destroy(item.gameObject);
    }

    void TakeDamage()
    {
        if (currentHp <= 0) return;

        // 체력을 1 깎음
        currentHp--;
        ShowSadExpression();

        // 체력이 깎인 순서에 해당하는 하트 오브젝트를 꺼버림
        if (heartImages != null && currentHp < heartImages.Length && heartImages[currentHp] != null)
        {
            heartImages[currentHp].gameObject.SetActive(false);
        }

        // 목숨이 모두 소진되었다면?
        if (currentHp <= 0)
        {
            Debug.Log("💀 모든 하트 소진! 게임 오버!");
        }
    }
    void ShowSadExpression()
    {
        if (playerImage == null || sadSprite == null) return;

        playerImage.sprite = sadSprite;

        if (expressionCoroutine != null)
        {
            StopCoroutine(expressionCoroutine);
        }

        if (currentHp > 0)
        {
            expressionCoroutine = StartCoroutine(RestoreSmileAfterDelay());
        }
    }

    System.Collections.IEnumerator RestoreSmileAfterDelay()
    {
        yield return new WaitForSeconds(sadExpressionDuration);
        ShowSmile();
        expressionCoroutine = null;
    }

    void ShowSmile()
    {
        if (playerImage != null && smileSprite != null)
        {
            playerImage.sprite = smileSprite;
        }
    }
} // 클래스를 닫는 마지막 중괄호
