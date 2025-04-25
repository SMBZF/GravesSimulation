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
    [Header("访客设置")]
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
            Debug.LogError("未找到 SpawnPoint 或 ExitPoint，请先生成墓园！");
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
        Debug.Log("[VisitorManager] 白天开始，访客生成启动");
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

        Debug.Log("[VisitorManager] 白天结束，访客生成停止");
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
                    Debug.LogWarning("未能获取有效访客类型，跳过本轮生成");
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
            Debug.LogError("[VisitorManager] 找不到 SpawnPoint 或 ExitPoint，请检查墓园生成！");
            return;
        }

        spawnPoint = spawn.transform;
        exitPoint = exit.transform;
    }


    VisitorType GetRandomVisitorType()
    {
        if (visitorTypes == null || visitorTypes.Count == 0)
        {
            Debug.LogWarning("访客类型未配置！");
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

    // VisitorManager.cs 添加这个
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
