using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

[System.Serializable]
public class VisitorType
{
    public GameObject prefab;
    public float speed = 3.5f;
    public GameObject offeringPrefab;
    public float weight = 1f;
}

public class VisitorManager : MonoBehaviour
{
    [Header("�ÿ�����")]
    public List<VisitorType> visitorTypes;
    public float spawnDelay = 5f;
    public int maxVisitors = 3;

    private Transform spawnPoint;
    private Transform exitPoint;
    private List<GameObject> activeVisitors = new List<GameObject>();

    private Coroutine spawnCoroutine;
    private bool isSpawning = false;

    void Awake()
    {
        StartCoroutine(WaitAndCachePoints());
    }

    IEnumerator WaitAndCachePoints()
    {
        yield return new WaitForSeconds(0.2f);

        GameObject spawn = GameObject.Find("SpawnPoint");
        GameObject exit = GameObject.Find("ExitPoint");

        if (spawn == null || exit == null)
        {
            Debug.LogError("δ�ҵ� SpawnPoint �� ExitPoint����������Ĺ԰��");
            yield break;
        }

        spawnPoint = spawn.transform;
        exitPoint = exit.transform;
    }

    public void StartDay()
    {
        if (isSpawning) return;

        isSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnVisitorLoop());
        Debug.Log("[VisitorManager] ���쿪ʼ���ÿ���������");
    }

    public void EndDay()
    {
        if (!isSpawning) return;

        isSpawning = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        Debug.Log("[VisitorManager] ����������ÿ�����ֹͣ");
    }

    IEnumerator SpawnVisitorLoop()
    {
        while (isSpawning)
        {
            activeVisitors.RemoveAll(v => v == null);

            if (activeVisitors.Count < maxVisitors)
            {
                VisitorType type = GetRandomVisitorType();
                if (type == null)
                {
                    Debug.LogWarning("δ�ܻ�ȡ��Ч�ÿ����ͣ�������������");
                    yield return new WaitForSeconds(spawnDelay);
                    continue;
                }


                GameObject newVisitor = Instantiate(type.prefab, spawnPoint.position, Quaternion.identity);
                newVisitor.transform.rotation = Quaternion.LookRotation(-spawnPoint.forward);
                activeVisitors.Add(newVisitor);

                DayVisitorAgent agent = newVisitor.GetComponent<DayVisitorAgent>();
                if (agent != null)
                {
                    agent.spawnPoint = spawnPoint;
                    agent.exitPoint = exitPoint;
                    agent.offeringPrefab = type.offeringPrefab;
                }

                NavMeshAgent nav = newVisitor.GetComponent<NavMeshAgent>();
                if (nav != null)
                {
                    nav.speed = type.speed;
                }
            }

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    public void RefreshPoints()
    {
        GameObject spawn = GameObject.Find("SpawnPoint");
        GameObject exit = GameObject.Find("ExitPoint");

        if (spawn == null || exit == null)
        {
            Debug.LogError("[VisitorManager] �Ҳ��� SpawnPoint �� ExitPoint������Ĺ԰���ɣ�");
            return;
        }

        spawnPoint = spawn.transform;
        exitPoint = exit.transform;
    }


    VisitorType GetRandomVisitorType()
    {
        if (visitorTypes == null || visitorTypes.Count == 0)
        {
            Debug.LogWarning("�ÿ�����δ���ã�");
            return null;
        }

        float totalWeight = 0f;
        foreach (var v in visitorTypes)
            totalWeight += v.weight;

        float rand = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var v in visitorTypes)
        {
            cumulative += v.weight;
            if (rand <= cumulative)
                return v;
        }

        return visitorTypes[0];
    }

    // VisitorManager.cs ������
    public void ForceAllVisitorsToExit()
    {
        GameObject[] visitors = GameObject.FindGameObjectsWithTag("Visitor");
        foreach (var visitor in visitors)
        {
            DayVisitorAgent agent = visitor.GetComponent<DayVisitorAgent>();
            if (agent != null)
            {
                agent.GoToExitImmediately();
            }
        }
    }


}
