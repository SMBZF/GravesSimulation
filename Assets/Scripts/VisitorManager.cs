using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using static Unity.VisualScripting.Antlr3.Runtime.Tree.TreeWizard;

[System.Serializable]
public class VisitorType
{
    public GameObject prefab;              // 访客预制体（外观 + 动作）
    public float speed = 3.5f;             // 行走速度
    public GameObject offeringPrefab;      // 祭拜供品
    public float weight = 1f;              // 出现概率权重
}

public class VisitorManager : MonoBehaviour
{
    [Header("访客设置")]
    public List<VisitorType> visitorTypes; // 多种访客配置
    public float spawnDelay = 5f;          // 生成间隔时间
    public int maxVisitors = 3;            // 同时最多访客数量

    private Transform spawnPoint;
    private Transform exitPoint;
    private List<GameObject> activeVisitors = new List<GameObject>();

    void Start()
    {
        StartCoroutine(WaitAndStart());
    }

    IEnumerator WaitAndStart()
    {
        yield return new WaitForSeconds(0.2f); // 等待 0.2 秒让墓园先生成

        GameObject spawn = GameObject.Find("SpawnPoint");
        GameObject exit = GameObject.Find("ExitPoint");

        if (spawn == null || exit == null)
        {
            Debug.LogError("未找到 SpawnPoint 或 ExitPoint，请先生成墓园！");
            yield break;
        }

        spawnPoint = spawn.transform;
        exitPoint = exit.transform;

        StartCoroutine(SpawnVisitorLoop());
    }


    IEnumerator SpawnVisitorLoop()
    {
        while (true)
        {
            // 清理死亡的访客
            activeVisitors.RemoveAll(v => v == null);

            if (activeVisitors.Count < maxVisitors)
            {
                VisitorType type = GetRandomVisitorType();
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

        return visitorTypes[0]; // fallback
    }
}
