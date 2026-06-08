using UnityEngine;

public class AvoidGameManager : MonoBehaviour
{
    [Header("오브젝트 연결")]
    public GameObject itemPrefab;
    public Transform spawnParent;

    [Header("게임 설정")]
    public float spawnInterval = 0.5f;

    // 흑화한 릴스 시청자들의 무작위 악플 리스트 😂
    private string[] badComments = {
        "어그로 ㄴㄴ",
        "노잼이네ㅋㅋㅋ",
        "이게 왜 추천에 뜸?",
        "팔로우 취소함",
        "주작이네",
        "할많하않...",
        "그만 좀 올려라",
        "이게 맞음??",
        "응 노인정~"
    };

    private float spawnTimer = 0f;
    private float canvasWidth = 1920f;
    private float spawnPositionY = 600f;

    void Start()
    {
        if (spawnParent == null)
        {
            spawnParent = FindFirstObjectByType<Canvas>().transform;
        }

        if (spawnParent != null)
        {
            canvasWidth = spawnParent.GetComponent<RectTransform>().rect.width;
        }
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnFallingItem();
            spawnTimer = 0f;
        }
    }

    void SpawnFallingItem()
    {
        if (itemPrefab == null || spawnParent == null) return;

        // 1. 화면 범위 내 랜덤 X축 위치 (글자 크기를 고려해 여백을 150 정도로 늘려줍니다)
        float minX = -(canvasWidth / 2f) + 150f;
        float maxX = (canvasWidth / 2f) - 150f;
        float randomX = Random.Range(minX, maxX);

        Vector3 spawnPosition = new Vector3(randomX, spawnPositionY, 0f);

        // 2. 아이템 오브젝트 생성 및 부모 설정
        GameObject newItem = Instantiate(itemPrefab, spawnParent);
        newItem.transform.localPosition = spawnPosition;

        // 3. [핵심] 무작위 악플을 골라서 텍스트 컴포넌트에 주입!
        FallingItem itemScript = newItem.GetComponent<FallingItem>();
        if (itemScript != null)
        {
            int randomIndex = Random.Range(0, badComments.Length);
            string selectedComment = badComments[randomIndex];

            // 악플이니까 경고 느낌의 빨간색 계열 글씨로 주입합니다.
            itemScript.SetText(selectedComment, Color.red);
        }
    }
}