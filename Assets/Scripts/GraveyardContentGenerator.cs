using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class GraveSet
{
    public GameObject graveBasePrefab;
    public List<GameObject> tombstoneVariants;
    public List<float> tombstoneVariantWeights;
}

public class GraveyardContentGenerator : MonoBehaviour
{
    [Header("普通墓碑集合")]
    [SerializeField] private List<GraveSet> normalGraveSets;
    [SerializeField] private List<float> normalGraveSetWeights;

    [Header("特殊墓碑集合（Z轴最小排）")]
    [SerializeField] private List<GraveSet> specialGraveSets;
    [SerializeField] private List<float> specialGraveSetWeights;

    [Header("装饰物（Z轴最小排，间隔生成）")]
    [SerializeField] private List<GameObject> decorationPrefabs;
    [SerializeField] private List<float> decorationWeights;

    private int internalStartX;
    private int internalEndX;
    private int internalStartZ;
    private int internalEndZ;
    private float spacing = 1f;
    private int pathX;
    private int offset; // 新增

    public void SetupRange(int width, int height, float spacing, int offset, int pathX)
    {
        internalStartX = offset + 1;
        internalEndX = offset + width - 2;
        internalStartZ = offset + 1;
        internalEndZ = offset + height - 2;
        this.spacing = spacing;
        this.pathX = pathX;
        this.offset = offset; // 保存 offset

        Debug.Log($"[SetupRange] Set internal range: X({internalStartX}-{internalEndX}) Z({internalStartZ}-{internalEndZ})");
    }

    public void GenerateGraves()
    {
        Debug.Log("[GenerateGraves] Generating graves...");
        Clear();

        int count = 0;

        for (int z = internalStartZ; z <= internalEndZ; z++)
        {
            if (z == internalStartZ)
            {
                for (int x = internalStartX; x <= internalEndX; x++)
                {
                    if (x == pathX) continue;

                    Vector3 pos = new Vector3((x - offset) * spacing, 0f, (z - offset) * spacing);

                    if (x % 2 == 0)
                    {
                        GraveSet set = GetWeightedRandom(specialGraveSets, specialGraveSetWeights);
                        if (set != null && set.graveBasePrefab != null)
                        {
                            GameObject grave = Instantiate(set.graveBasePrefab, pos, Quaternion.identity, transform);
                            grave.name = $"SpecialGrave_{x}_{z}";
                            count++;

                            if (set.tombstoneVariants.Count > 0)
                            {
                                GameObject tombstone = GetWeightedRandom(set.tombstoneVariants, set.tombstoneVariantWeights);
                                if (tombstone != null)
                                {
                                    Vector3 offsetPos = pos;
                                    GameObject ts = Instantiate(tombstone, offsetPos, Quaternion.identity, transform);
                                    ts.name = $"SpecialTombstone_{x}_{z}";
                                    ts.tag = "Grave";

                                    GraveData gd = ts.GetComponent<GraveData>();
                                    if (gd == null)
                                        gd = ts.AddComponent<GraveData>();
                                    gd.gravePrefabName = tombstone.name;

                                    if (gd.gravePrefabName == "gravestone-broken" || gd.gravePrefabName == "gravestone-debris")
                                        gd.offeringChance = 0.3f;
                                    else if (gd.gravePrefabName == "shovel-dirt")
                                        gd.allowOffering = false;

                                    if (!ts.TryGetComponent<NavMeshObstacle>(out _))
                                    {
                                        var obs = ts.AddComponent<NavMeshObstacle>();
                                        obs.carving = true;
                                        obs.shape = NavMeshObstacleShape.Box;
                                        obs.size = new Vector3(0.1f, 0.5f, 0.1f);
                                        obs.center = new Vector3(0f, 0.5f, 0f);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        GameObject decor = GetWeightedRandom(decorationPrefabs, decorationWeights);
                        if (decor != null)
                        {
                            GameObject deco = Instantiate(decor, pos, Quaternion.identity, transform);
                            deco.name = $"Decoration_{x}_{z}";
                            count++;
                        }
                    }
                }

                continue;
            }

            if (z % 2 == 0) continue;

            for (int x = internalStartX; x <= internalEndX; x++)
            {
                if (x == pathX) continue;
                if ((x + z) % 2 != 0) continue;

                Vector3 pos = new Vector3((x - offset) * spacing, 0f, (z - offset) * spacing);
                GraveSet set = GetWeightedRandom(normalGraveSets, normalGraveSetWeights);
                if (set != null && set.graveBasePrefab != null)
                {
                    GameObject grave = Instantiate(set.graveBasePrefab, pos, Quaternion.identity, transform);
                    grave.name = $"Grave_{x}_{z}";
                    count++;

                    if (set.tombstoneVariants.Count > 0)
                    {
                        GameObject tombstone = GetWeightedRandom(set.tombstoneVariants, set.tombstoneVariantWeights);
                        if (tombstone != null)
                        {
                            Vector3 offsetPos = pos + grave.transform.forward * 0.5f;
                            GameObject ts = Instantiate(tombstone, offsetPos, Quaternion.identity, transform);
                            ts.name = $"Tombstone_{x}_{z}";
                            ts.tag = "Grave";

                            GraveData gd = ts.GetComponent<GraveData>();
                            if (gd == null)
                                gd = ts.AddComponent<GraveData>();
                            gd.gravePrefabName = tombstone.name;

                            if (gd.gravePrefabName == "gravestone-broken" || gd.gravePrefabName == "gravestone-debris")
                                gd.offeringChance = 0.3f;
                            else if (gd.gravePrefabName == "shovel-dirt")
                                gd.allowOffering = false;

                            if (!ts.TryGetComponent<NavMeshObstacle>(out _))
                            {
                                var obs = ts.AddComponent<NavMeshObstacle>();
                                obs.carving = true;
                                obs.shape = NavMeshObstacleShape.Box;
                                obs.size = new Vector3(0.1f, 0.5f, 0.1f);
                                obs.center = new Vector3(0f, 0.5f, 0f);
                            }
                        }
                    }
                }
            }
        }

        Debug.Log($"[GenerateGraves] Total graves created: {count}");
    }

    public void Clear()
    {
        int removed = transform.childCount;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        Debug.Log($"[Clear] Cleared {removed} graves.");
    }

    private T GetWeightedRandom<T>(List<T> list, List<float> weights)
    {
        if (list.Count != weights.Count || list.Count == 0)
            return default;

        float total = 0f;
        foreach (float w in weights)
            total += w;

        float rand = Random.Range(0f, total);
        float cumulative = 0f;

        for (int i = 0; i < list.Count; i++)
        {
            cumulative += weights[i];
            if (rand <= cumulative)
                return list[i];
        }

        return default;
    }
}
