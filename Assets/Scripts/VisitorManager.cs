using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using static Unity.VisualScripting.Antlr3.Runtime.Tree.TreeWizard;

[System.Serializable]
public class VisitorType
{
    public GameObject prefab;              // �ÿ�Ԥ���壨��� + ������
    public float speed = 3.5f;             // �����ٶ�
    public GameObject offeringPrefab;      // ���ݹ�Ʒ
    public float weight = 1f;              // ���ָ���Ȩ��
}

public class VisitorManager : MonoBehaviour
{
    [Header("�ÿ�����")]
    public List<VisitorType> visitorTypes; // ���ַÿ�����
    public float spawnDelay = 5f;          // ���ɼ��ʱ��
    public int maxVisitors = 3;            // ͬʱ���ÿ�����

    private Transform spawnPoint;
    private Transform exitPoint;
    private List<GameObject> activeVisitors = new List<GameObject>();

    void Start()
    {
        StartCoroutine(WaitAndStart());
    }

    IEnumerator WaitAndStart()
    {
        yield return new WaitForSeconds(0.2f); // �ȴ� 0.2 ����Ĺ԰������

        GameObject spawn = GameObject.Find("SpawnPoint");
        GameObject exit = GameObject.Find("ExitPoint");

        if (spawn == null || exit == null)
        {
            Debug.LogError("δ�ҵ� SpawnPoint �� ExitPoint����������Ĺ԰��");
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
            // ���������ķÿ�
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

        return visitorTypes[0]; // fallback
    }
}
