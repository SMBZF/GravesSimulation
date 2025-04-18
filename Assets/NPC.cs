using UnityEngine;
using System.Collections;

public class NPCRandomWander : MonoBehaviour
{
    [Header("移动范围设置")]
    public Transform centerPoint;  // 中心点（可以是空物体）
    public float wanderRadius = 5f;

    [Header("移动设置")]
    public float moveSpeed = 2f;
    public float stopDuration = 2f;

    private Vector3 targetPosition;
    private bool isMoving = false;

    void Start()
    {
        PickNewDestination();
    }

    void Update()
    {
        if (isMoving)
        {
            MoveTowardsTarget();
        }
    }

    void PickNewDestination()
    {
        // 随机一个范围内的目标点
        Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
        targetPosition = centerPoint.position + new Vector3(randomOffset.x, 0, randomOffset.y);

        isMoving = true;
    }

    void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // 到达目标点
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
            StartCoroutine(WaitAndMoveAgain());
        }
    }

    IEnumerator WaitAndMoveAgain()
    {
        yield return new WaitForSeconds(stopDuration);
        PickNewDestination();
    }

    // 可视化范围
    void OnDrawGizmosSelected()
    {
        if (centerPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(centerPoint.position, wanderRadius);
        }
    }
}
