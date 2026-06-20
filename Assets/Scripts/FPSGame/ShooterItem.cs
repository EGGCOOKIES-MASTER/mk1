using UnityEngine;
using UnityEngine.UI;

public class ShooterItem : MonoBehaviour
{
    public bool isSkull = false;
    public float riseSpeed = 300f;

    [Header("타겟 이미지 설정")]
    public Sprite badSprite;  // 빌런/해골 이미지 들어갈 칸
    public Sprite goodSprite; // 보너스/하트 이미지 들어갈 칸

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnShot);
        }
    }

    void Update()
    {
        if (ShooterGameManager.Instance != null && ShooterGameManager.Instance.IsFinished()) return;

        // 매 프레임 위로 상승
        transform.Translate(Vector3.up * riseSpeed * Time.deltaTime);

        // 화면 천장(Y: 650) 위로 완전히 벗어나면 자동 삭제
        if (transform.localPosition.y > 650f)
        {
            if (isSkull && ShooterGameManager.Instance != null)
            {
                ShooterGameManager.Instance.MissSkull(); // BAD를 놓치면 패널티 데미지
            }
            Destroy(gameObject);
        }
    }

    void OnShot()
    {
        if (ShooterGameManager.Instance == null || ShooterGameManager.Instance.IsFinished()) return;

        GameAudioManager.PlayGunshot();

        if (isSkull)
        {
            ShooterGameManager.Instance.AddScore(10); // BAD 맞추면 점수업!
        }
        else
        {
            ShooterGameManager.Instance.TakeDamage(); // GOOD 잘못 쏘면 감점/피격!
        }

        Destroy(gameObject);
    }
}
