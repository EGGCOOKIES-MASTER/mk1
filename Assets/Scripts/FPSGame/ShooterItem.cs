using UnityEngine;
using UnityEngine.UI;

public class ShooterItem : MonoBehaviour
{
    public bool isSkull = false; // true면 해골(점수), false면 하트(함정)
    public float riseSpeed = 300f; // 위로 떠오르는 속도

    private Button button;
    private ShooterGameManager gameManager;

    void Start()
    {
        button = GetComponent<Button>();
        gameManager = FindFirstObjectByType<ShooterGameManager>();

        // 버튼을 클릭(사격)했을 때 실행될 함수 연결
        if (button != null)
        {
            button.onClick.AddListener(OnShot);
        }
    }

    void Update()
    {
        // 매 프레임마다 위(Vector3.up)로 이동
        transform.Translate(Vector3.up * riseSpeed * Time.deltaTime);

        // 화면 꼭대기 위로 완전히 벗어나면 자동 삭제
        if (transform.localPosition.y > 1100f)
        {
            // 해골을 못 맞추고 놓쳤을 때의 패널티 처리 (필요시)
            if (isSkull && gameManager != null)
            {
                gameManager.MissSkull();
            }
            Destroy(gameObject);
        }
    }

    // 이 아이템을 총으로 쐈을 때 (클릭했을 때)
    void OnShot()
    {
        if (gameManager == null) return;

        if (isSkull)
        {
            // 해골을 맞추면 점수 획득!
            gameManager.AddScore(10);
            Debug.Log("💀 해골 격추! +10점");
        }
        else
        {
            // 실수로 하트를 맞추면 데미지!
            gameManager.TakeDamage();
            Debug.Log("💥 앗! 하트를 쐈습니다! 데미지!");
        }

        // 총에 맞았으므로 화면에서 즉시 파괴
        Destroy(gameObject);
    }
}