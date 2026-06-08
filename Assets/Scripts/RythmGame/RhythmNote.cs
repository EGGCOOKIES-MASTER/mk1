using UnityEngine;

public class RhythmNote : MonoBehaviour
{
    public float fallSpeed = 300f;
    private float deadZoneY;
    private RhythmLaneManager laneManager;

    public void Setup(float targetY)
    {
        deadZoneY = targetY - 150f;
        laneManager = FindFirstObjectByType<RhythmLaneManager>();
    }

    void Update()
    {
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        // 판정선을 넘어가서 놓쳤을 때 (MISS)
        if (transform.position.y <= deadZoneY)
        {
            if (laneManager != null)
            {
                laneManager.MissNote(); // 매니저에게 미스 신호 전송
            }
            DestroyNote();
        }
    }

    public void DestroyNote()
    {
        Destroy(gameObject);
    }
}