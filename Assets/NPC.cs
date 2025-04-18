using UnityEngine;
using System.Collections;

public class NPCRandomWander : MonoBehaviour
{
    [Header("�ƶ���Χ����")]
    public Transform centerPoint;  // ���ĵ㣨�����ǿ����壩
    public float wanderRadius = 5f;

    [Header("�ƶ�����")]
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
        // ���һ����Χ�ڵ�Ŀ���
        Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
        targetPosition = centerPoint.position + new Vector3(randomOffset.x, 0, randomOffset.y);

        isMoving = true;
    }

    void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // ����Ŀ���
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

    // ���ӻ���Χ
    void OnDrawGizmosSelected()
    {
        if (centerPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(centerPoint.position, wanderRadius);
        }
    }
}
