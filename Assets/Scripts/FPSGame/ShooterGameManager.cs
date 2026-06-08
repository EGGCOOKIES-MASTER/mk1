using UnityEngine;

public class ShooterGameManager : MonoBehaviour
{
    [Header("프리팹 연결")]
    public GameObject itemPrefab; // 아까 만든 TargetItem 프리팹
    public Transform canvasTransform; // 아이템이 생성될 Canvas

    [Header("게임 세팅")]
    public float spawnInterval = 0.8f; // 아이템이 리스폰되는 간격 (초)
    private float spawnTimer = 0f;

    private int score = 0;
    private int hp = 3;

    void Update()
    {
        // 주기적으로 하트나 해골을 생성
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnItem();
        }
    }

    void SpawnItem()
    {
        if (itemPrefab == null || canvasTransform == null) return;

        // 1. 아이템 생성 및 캔버스의 자식으로 설정
        GameObject newItem = Instantiate(itemPrefab, canvasTransform);

        // 🔥 [PC 가로화면 최적화 변경] 
        // 1920 해상도 기준, 좌우 넓은 범위(-800 ~ 800)에서 무작위로 나오게 설정합니다.
        float randomX = Random.Range(-800f, 800f);

        // 1080 해상도 기준, 화면 맨 밑바닥(-600 지점)에서 생성되어 떠오르게 합니다.
        newItem.transform.localPosition = new Vector3(randomX, -600f, 0f);

        // 3. 50% 확률로 해골 혹은 하트로 세팅하기
        ShooterItem shooterItem = newItem.GetComponent<ShooterItem>();
        if (shooterItem == null) shooterItem = newItem.AddComponent<ShooterItem>();

        TMPro.TextMeshProUGUI textComponent = newItem.GetComponentInChildren<TMPro.TextMeshProUGUI>();

        if (Random.value > 0.5f)
        {
            // 해골 대신 맞추어야 할 타겟 빌런 표시 (예: BAD 또는 X)
            shooterItem.isSkull = true;
            shooterItem.riseSpeed = Random.Range(200f, 400f);
            if (textComponent != null) textComponent.text = "BAD"; // 💀 대신 글자로!
        }
        else
        {
            // 하트 대신 쏘면 안 되는 보너스 표시 (예: GOOD 또는 O)
            shooterItem.isSkull = false;
            shooterItem.riseSpeed = Random.Range(150f, 300f);
            if (textComponent != null) textComponent.text = "GOOD"; // ❤️ 대신 글자로!
        }
    }

    // 점수 추가 함수
    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log($"🎯 현재 점수: {score}점");
    }

    // 하트를 잘못 쐈을 때 목숨 감소 함수
    public void TakeDamage()
    {
        hp--;
        Debug.Log($"❤️ 남은 목숨: {hp}");
        if (hp <= 0)
        {
            Debug.Log("💀 게임 오버! 하트를 너무 많이 쐈습니다.");
        }
    }

    // 해골을 놓치고 화면 밖으로 보냈을 때 실행할 규칙 (필요시 패널티 추가 가능)
    public void MissSkull()
    {
        Debug.Log("🏃‍♂️ 해골 빌런이 도망쳤습니다!");
    }
}