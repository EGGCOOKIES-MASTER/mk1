using UnityEngine;

public class RunPlayer : MonoBehaviour
{
    public float jumpForce = 800f;   // 점프하는 힘
    public float gravity = 2000f;    // 아래로 떨어지는 중력 힘

    [Header("Ground Alignment")]
    [SerializeField] private RectTransform floor;
    [SerializeField] private float leftPadding = 80f;

    private float velocityY = 0f;    // Y축 현재 속도
    private float groundY;           // 시작 위치를 바닥 기준으로 사용
    private float floorTopY;
    private bool isGrounded = true;  // 바닥에 서 있는지 여부

    public float GroundY => groundY;
    public float FloorTopY => floorTopY;

    void Start()
    {
        AlignToFloor();
    }

    private void AlignToFloor()
    {
        RectTransform playerRect = transform as RectTransform;
        RectTransform parentRect = transform.parent as RectTransform;
        if (playerRect == null || parentRect == null)
        {
            groundY = transform.localPosition.y;
            floorTopY = groundY;
            return;
        }

        if (floor == null)
        {
            Transform floorTransform = transform.parent.Find("Floor");
            floor = floorTransform as RectTransform;
        }

        Canvas.ForceUpdateCanvases();

        if (floor != null)
        {
            Vector3[] floorCorners = new Vector3[4];
            floor.GetWorldCorners(floorCorners);
            floorTopY = parentRect.InverseTransformPoint(floorCorners[1]).y;
        }
        else
        {
            floorTopY = -parentRect.rect.height * 0.5f;
        }

        float playerBottomOffset = playerRect.rect.height * playerRect.pivot.y;
        groundY = floorTopY + playerBottomOffset;
        float playerX = -parentRect.rect.width * 0.5f + leftPadding + playerRect.rect.width * playerRect.pivot.x;
        transform.localPosition = new Vector3(playerX, groundY, 0f);
    }

    void Update()
    {
        // 1. 바닥에 있을 때 스페이스바나 마우스 클릭을 누르면 점프!
        if (isGrounded && WasJumpPressed())
        {
            velocityY = jumpForce;
            isGrounded = false;
        }

        // 2. 공중에 있을 때 중력 적용
        if (!isGrounded)
        {
            velocityY -= gravity * Time.deltaTime;
        }

        // 3. 위치 계산 및 반영
        transform.localPosition += Vector3.up * velocityY * Time.deltaTime;

        // 4. 바닥 착지 체크
        if (transform.localPosition.y <= groundY)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, groundY, 0f);
            velocityY = 0f;
            isGrounded = true;
        }
    }

    private bool WasJumpPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current?.spaceKey.wasPressedThisFrame == true ||
            UnityEngine.InputSystem.Pointer.current?.press.wasPressedThisFrame == true)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
#else
        return false;
#endif
    }

    // 장애물과 부딪혔을 때 처리 (충돌)
    // 장애물과 부딪혔을 때 처리
    public void ResetPosition()
    {
        Debug.Log("💥 악플 장애물에 부딪혔습니다!");

        // 매니저를 찾아 게임오버 시키기
        RunGameManager manager = FindFirstObjectByType<RunGameManager>();
        if (manager != null)
        {
            manager.GameOver();
        }
    }
}
