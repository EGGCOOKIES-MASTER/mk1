using UnityEngine;

public class RunPlayer : MonoBehaviour
{
    public float jumpForce = 800f;   // 점프하는 힘
    public float gravity = 2000f;    // 아래로 떨어지는 중력 힘

    private float velocityY = 0f;    // Y축 현재 속도
    private float groundY = -180f; // 방금 인스펙터에 수정한 Pos Y 값과 똑같이 맞춰야 점프 후 제자리에 착지합니다!   // 발바닥 착지 기준점 (플레이어 이미지 중심점 위치)
    private bool isGrounded = true;  // 바닥에 서 있는지 여부

    void Update()
    {
        // 1. 바닥에 있을 때 스페이스바나 마우스 클릭을 누르면 점프!
        if (isGrounded && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
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