using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class DayVisitorAgent : MonoBehaviour
{
    public enum State
    {
        Idle,
        SelectGrave,
        NavigateToGrave,
        Offer,
        Exit
    }

    public State currentState = State.Idle;

    public float idleTime = 1.5f;
    public GameObject offeringPrefab;
    public float offeringOffset = -0.5f;
    public Transform spawnPoint;
    public Transform exitPoint;
    public float stopDistance = 0.3f;

    private NavMeshAgent agent;
    private Transform targetGrave;
    private DayVisitorAnimatorController animatorController;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animatorController = GetComponent<DayVisitorAnimatorController>();

        Debug.Log("[VisitorAgent] 出生位置: " + transform.position);

        if (!agent.isOnNavMesh)
        {
            Debug.LogError("[VisitorAgent] 不在 NavMesh 上！请检查 SpawnPoint 是否正确！");
            Destroy(gameObject);
            return;
        }

        StartCoroutine(StateMachine());
    }

    IEnumerator StateMachine()
    {
        yield return new WaitForSeconds(idleTime);
        ChangeState(State.SelectGrave);

        while (true)
        {
            switch (currentState)
            {
                case State.SelectGrave:
                    animatorController?.SetWalking(false);
                    FindRandomGrave();
                    break;

                case State.NavigateToGrave:
                    if (targetGrave != null)
                    {
                        agent.isStopped = false;
                        agent.SetDestination(targetGrave.position);
                        animatorController?.SetWalking(true);

                        yield return new WaitUntil(() =>
                            !agent.pathPending && agent.remainingDistance <= stopDistance);

                        agent.isStopped = true;
                        animatorController?.SetWalking(false);
                        ChangeState(State.Offer);
                    }
                    break;

                case State.Offer:
                    animatorController?.SetWalking(false);
                    animatorController?.PlayOfferingAnimation();

                    StartCoroutine(SpawnOffering());
                    yield return new WaitForSeconds(1.5f);
                    ChangeState(State.Exit);
                    break;

                case State.Exit:
                    if (exitPoint != null)
                    {
                        agent.isStopped = false;
                        agent.SetDestination(exitPoint.position);
                        animatorController?.SetWalking(true);

                        yield return new WaitUntil(() =>
                            !agent.pathPending && agent.remainingDistance <= stopDistance);

                        animatorController?.SetWalking(false);
                        Destroy(gameObject);
                    }
                    yield break;
            }

            yield return null;
        }
    }

    void ChangeState(State newState)
    {
        currentState = newState;
    }

    void FindRandomGrave()
    {
        GameObject[] graves = GameObject.FindGameObjectsWithTag("Grave");
        List<Transform> candidates = new List<Transform>();
        List<float> weights = new List<float>();

        foreach (GameObject grave in graves)
        {
            GraveData data = grave.GetComponent<GraveData>();
            if (data == null || !data.allowOffering) continue;

            // 使用 offeringChance 作为权重，低几率墓碑不容易被选中
            candidates.Add(grave.transform);
            weights.Add(data.offeringChance);
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning("[Visitor] 没有可供奉的墓碑！");
            return;
        }

        // 进行加权随机选择
        float totalWeight = 0f;
        foreach (float w in weights) totalWeight += w;

        float rand = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < candidates.Count; i++)
        {
            cumulative += weights[i];
            if (rand <= cumulative)
            {
                targetGrave = candidates[i];
                ChangeState(State.NavigateToGrave);
                Debug.Log($"[Visitor] 选中墓碑: {targetGrave.name}");
                return;
            }
        }
    }


    IEnumerator SpawnOffering()
    {
        yield return new WaitForSeconds(0.5f); // 延迟 0.5 秒再供奉

        if (offeringPrefab != null && targetGrave != null)
        {
            GraveData graveData = targetGrave.GetComponent<GraveData>();

            if (graveData != null && graveData.allowOffering)
            {
                if (Random.value <= graveData.offeringChance)
                {
                    Vector3 forwardOffset = -targetGrave.forward * offeringOffset;
                    Vector3 pos = targetGrave.position + forwardOffset;

                    GameObject offering = Instantiate(offeringPrefab, pos, Quaternion.identity);
                    graveData.RegisterOffering(offering);
                    Debug.Log($"[Visitor] 成功供奉到 {graveData.gravePrefabName}");
                }
                else
                {
                    Debug.Log($"[Visitor] 被墓碑 {graveData.gravePrefabName} 拒绝（几率）");
                }
            }
            else
            {
                Debug.Log($"[Visitor] 墓碑 {targetGrave.name} 不允许供奉");
            }
        }
    }



}
