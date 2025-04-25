using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class GraveyardGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 16;
    public int height = 9;
    [SerializeField] private float spacing = 1f;

    [Header("Prefabs")]
    [SerializeField] private GraveyardContentGenerator contentGenerator;
    public GameObject planePrefab;
    public List<FenceStyleSet> fenceStyles;

    [Header("Fence Style Selection")]
    public int selectedFenceIndex = 0;

    [Header("Tree Settings")]
    [SerializeField] private List<GameObject> springTrees;
    [SerializeField] private List<GameObject> summerTrees;
    [SerializeField] private List<GameObject> autumnTrees;
    [SerializeField] private List<GameObject> winterTrees;

    [Header("季节控制对象")]
    [SerializeField] private List<GameObject> springObjects;
    [SerializeField] private List<GameObject> summerObjects;
    [SerializeField] private List<GameObject> autumnObjects;
    [SerializeField] private List<GameObject> winterObjects;


    [Header("花朵设置")]
    [SerializeField] private List<GameObject> flowerPrefabs;
    [SerializeField, Range(0f, 1f)] public float flowerSpawnChance = 0.3f;
    [SerializeField] private int flowersPerCluster = 6;
    [SerializeField] private float flowerClusterRadius = 0.3f;

    [Range(0f, 1f)] public float treeSpawnChance = 0.3f;
    public enum Season { Spring, Summer, Autumn, Winter }
    public Season currentSeason = Season.Summer;

    [Header("Path Settings")]
    public GameObject stonePathPrefab;

    private Transform fenceParent;
    [Header("外围空地设置")]
    [SerializeField] private int gridOffset = 10;


    void Start()
    {
        GenerateGraveyard();
    }

    public void SpawnFlowersOnPlane(Transform plane, int clusterCount = 2, int flowersPerCluster = 5, float clusterRadius = 0.3f)
    {
        if (flowerPrefabs == null || flowerPrefabs.Count == 0) return;

        for (int i = 0; i < clusterCount; i++)
        {
            Vector3 basePos = plane.position + new Vector3(
                Random.Range(-0.4f, 0.4f),
                0f,
                Random.Range(-0.4f, 0.4f)
            );

            for (int j = 0; j < flowersPerCluster; j++)
            {
                GameObject prefab = flowerPrefabs[Random.Range(0, flowerPrefabs.Count)];
                Vector2 offset = Random.insideUnitCircle * clusterRadius;
                Vector3 flowerPos = basePos + new Vector3(offset.x, 0f, offset.y);
                flowerPos.y = 0.01f;

                GameObject flower = Instantiate(prefab, flowerPos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                // 不设置父级，保持在墓园根节点下
                flower.transform.SetParent(this.transform); // 或不设置父级


                float scale = Random.Range(0.02f, 0.04f);
                flower.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }

    public void GenerateGraveyard()
    {
        ClearPreviousGeneration();

        int totalWidth = width + gridOffset * 2;
        int totalHeight = height + gridOffset * 2;
        int xOffset = -gridOffset;
        int zOffset = -gridOffset;

        for (int x = 0; x < totalWidth; x++)
        {
            for (int z = 0; z < totalHeight; z++)
            {
                Vector3 pos = new Vector3((x + xOffset) * spacing, 0f, (z + zOffset) * spacing);
                GameObject plane = Instantiate(planePrefab, pos, Quaternion.identity, transform);
                plane.name = "Plane_" + x + "_" + z;
                plane.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
                plane.isStatic = true;

                // 随机生成部分 Plane 上的花丛
                if (flowerPrefabs != null && flowerPrefabs.Count > 0 && Random.value < flowerSpawnChance)
                {
                    SpawnFlowersOnPlane(plane.transform, 2, flowersPerCluster, flowerClusterRadius);
                }

                bool isInsideFence =
                    (x > gridOffset && x < width + gridOffset - 1 &&
                     z > gridOffset && z < height + gridOffset - 1);

                plane.layer = isInsideFence ? LayerMask.NameToLayer("NavOuter") : LayerMask.NameToLayer("NavOuter");

                bool isFenceArea =
                    (x >= gridOffset && x <= width + gridOffset - 1 &&
                     z >= gridOffset && z <= height + gridOffset - 1) &&
                    (x == gridOffset || x == width + gridOffset - 1 ||
                     z == gridOffset || z == height + gridOffset - 1);

                if (isFenceArea)
                    GenerateFenceAt(x, z, pos, plane.transform);
            }
        }

        FenceStyleSet set = fenceStyles[selectedFenceIndex];
        CreateFence(set.cornerPrefab, GetWorldPos(gridOffset, height + gridOffset - 1), Quaternion.Euler(0, 180, 0), GetPlaneTransform(gridOffset, height + gridOffset - 1), gridOffset, height + gridOffset - 1);
        CreateFence(set.cornerPrefab, GetWorldPos(width + gridOffset - 1, height + gridOffset - 1), Quaternion.Euler(0, -90, 0), GetPlaneTransform(width + gridOffset - 1, height + gridOffset - 1), width + gridOffset - 1, height + gridOffset - 1);
        CreateFence(set.cornerPrefab, GetWorldPos(gridOffset, gridOffset), Quaternion.Euler(0, 90, 0), GetPlaneTransform(gridOffset, gridOffset), gridOffset, gridOffset);
        CreateFence(set.cornerPrefab, GetWorldPos(width + gridOffset - 1, gridOffset), Quaternion.Euler(0, 0, 0), GetPlaneTransform(width + gridOffset - 1, gridOffset), width + gridOffset - 1, gridOffset);

        GenerateTreeBorder();

        int pathX = gridOffset + width / 2;
        int pathLength = Random.Range(2, height - 1);
        GenerateStonePath(pathX, pathLength);

        if (contentGenerator != null)
        {
            contentGenerator.SetupRange(width, height, spacing, gridOffset, pathX);
            contentGenerator.GenerateGraves();
        }

        GenerateSpawnAndExitPoints();
        BuildNavMeshSurfaces();
    }

    private void BuildNavMeshSurfaces()
    {
        Vector3 centerPos = GetWorldPos(gridOffset + width / 2, gridOffset + height / 2);

        GameObject ghostSurfaceObj = new GameObject("GhostNavMeshSurface");
        ghostSurfaceObj.transform.position = centerPos;
        ghostSurfaceObj.transform.SetParent(transform);

        NavMeshSurface ghostSurface = ghostSurfaceObj.AddComponent<NavMeshSurface>();
        ghostSurface.collectObjects = CollectObjects.All;
        ghostSurface.layerMask = LayerMask.GetMask("NavInner");
        ghostSurface.defaultArea = 3; // GhostArea
        ghostSurface.BuildNavMesh();

        GameObject visitorSurfaceObj = new GameObject("VisitorNavMeshSurface");
        visitorSurfaceObj.transform.position = centerPos;
        visitorSurfaceObj.transform.SetParent(transform);

        NavMeshSurface visitorSurface = visitorSurfaceObj.AddComponent<NavMeshSurface>();
        visitorSurface.collectObjects = CollectObjects.All;
        visitorSurface.layerMask = LayerMask.GetMask("NavInner", "NavOuter");
        visitorSurface.defaultArea = 0; // Walkable
        visitorSurface.BuildNavMesh();

        Debug.Log("[NavMesh] GhostNav + VisitorNav 烘焙完成");
    }

    private void GenerateFenceAt(int x, int z, Vector3 pos, Transform parentPlane)
    {
        FenceStyleSet set = fenceStyles[selectedFenceIndex];
        GameObject prefabToUse;

        bool isLeft = x == gridOffset;
        bool isRight = x == width + gridOffset - 1;
        bool isBottom = z == gridOffset;
        bool isTop = z == height + gridOffset - 1;
        int gateX = gridOffset + width / 2;

        bool isEdgeEnd =
            (isLeft && (z == gridOffset || z == height + gridOffset - 1)) ||
            (isRight && (z == gridOffset || z == height + gridOffset - 1)) ||
            (isTop && (x == gridOffset || x == width + gridOffset - 1)) ||
            (isBottom && (x == gridOffset || x == width + gridOffset - 1));

        if (isEdgeEnd || (isTop && x == gateX)) return;

        if (isTop && (x == gateX - 1 || x == gateX + 1))
        {
            prefabToUse = set.gateSidePrefab;
            Quaternion rot = Quaternion.Euler(0, 180, 0);
            GameObject fence = Instantiate(prefabToUse, pos + Vector3.up * 0.01f, rot);
            fence.name = "Fence_" + x + "_" + z;
            fence.transform.SetParent(parentPlane, true);

            if (x == gateX + 1)
            {
                Vector3 scale = fence.transform.localScale;
                scale.x *= -1;
                fence.transform.localScale = scale;
            }

            // 添加 NavMeshModifier 来忽略这个 gate prefab
            NavMeshModifier mod = fence.AddComponent<NavMeshModifier>();
            mod.overrideArea = true;
            mod.ignoreFromBuild = true;

            return;
        }


        prefabToUse = (Random.value < 0.9f) ? set.straightCommonPrefab : set.straightRarePrefab;

        Quaternion rotation = Quaternion.identity;
        if (isLeft) rotation = Quaternion.Euler(0, 90, 0);
        else if (isRight) rotation = Quaternion.Euler(0, -90, 0);
        else if (isBottom) rotation = Quaternion.Euler(0, 0, 0);
        else if (isTop) rotation = Quaternion.Euler(0, 180, 0);

        CreateFence(prefabToUse, pos, rotation, parentPlane, x, z);
    }

    private void CreateFence(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent, int x, int z)
    {
        GameObject fence = Instantiate(prefab, pos + Vector3.up * 0.01f, rot);
        fence.name = "Fence_" + x + "_" + z;
        fence.transform.SetParent(parent, true);

        // 如果是 gate prefab，则跳过 NavMeshObstacle 添加，只忽略导航构建
        if (prefab == fenceStyles[selectedFenceIndex].gateSidePrefab)
        {
            NavMeshModifier mod = fence.AddComponent<NavMeshModifier>();
            mod.overrideArea = true;
            mod.ignoreFromBuild = true;
            return;
        }

        // 其它围栏添加 NavMeshObstacle，阻挡角色穿墙
        if (fence.GetComponent<NavMeshObstacle>() == null)
        {
            NavMeshObstacle obstacle = fence.AddComponent<NavMeshObstacle>();
            obstacle.carving = true;
            obstacle.shape = NavMeshObstacleShape.Box;

            // 设置一个合理大小（建议你按实际 fence 模型微调）
            obstacle.size = new Vector3(0.1f, 0.1f, 0f);
            obstacle.center = new Vector3(0f, 0.1f, 0f);
        }
    }


    private void GenerateTreeBorder()
    {
        int minX = gridOffset;
        int maxX = width + gridOffset - 1;
        int minZ = gridOffset;
        int maxZ = height + gridOffset - 1;
        int gateX = gridOffset + width / 2;

        List<Vector2Int> borderPositions = new List<Vector2Int>();

        for (int x = minX - 1; x <= maxX + 1; x++)
        {
            if (x != gateX)
                borderPositions.Add(new Vector2Int(x, maxZ + 1));
            borderPositions.Add(new Vector2Int(x, minZ - 1));
        }

        for (int z = minZ; z <= maxZ; z++)
        {
            borderPositions.Add(new Vector2Int(minX - 1, z));
            borderPositions.Add(new Vector2Int(maxX + 1, z));
        }

        foreach (var pos in borderPositions)
        {
            if (Random.value < treeSpawnChance)
            {
                Vector3 worldPos = GetWorldPos(pos.x, pos.y);
                GameObject prefab = GetRandomTree();
                GameObject tree = Instantiate(prefab, worldPos, Quaternion.identity);
                tree.name = "Tree_" + pos.x + "_" + pos.y;

                float baseScale = Random.Range(0.3f, 0.7f);
                float yScale = Random.Range(baseScale, baseScale + 0.3f);
                tree.transform.localScale = new Vector3(baseScale, yScale, baseScale);
                tree.transform.position = new Vector3(tree.transform.position.x, 0f, tree.transform.position.z);
                tree.transform.SetParent(transform);
            }
        }
    }

    private void GenerateStonePath(int pathX, int length)
    {
        Vector3 posOut = GetWorldPos(pathX, height + gridOffset);
        Instantiate(stonePathPrefab, posOut, Quaternion.identity, transform).name = "StonePath_Outside";

        Vector3 posGate = GetWorldPos(pathX, height + gridOffset - 1);
        Instantiate(stonePathPrefab, posGate, Quaternion.identity, transform).name = "StonePath_Gate";

        for (int i = 1; i <= length; i++)
        {
            int z = height + gridOffset - 1 - i;
            if (z <= gridOffset) break;

            Vector3 pos = GetWorldPos(pathX, z);
            Instantiate(stonePathPrefab, pos, Quaternion.identity, transform).name = $"StonePath_{z}";
        }
    }

    public void ApplySeasonObjects()
    {
        void SetGroupActive(List<GameObject> list, bool active)
        {
            if (list == null) return;
            foreach (var go in list)
            {
                if (go != null) go.SetActive(active);
            }
        }

        SetGroupActive(springObjects, currentSeason == Season.Spring);
        SetGroupActive(summerObjects, currentSeason == Season.Summer);
        SetGroupActive(autumnObjects, currentSeason == Season.Autumn);
        SetGroupActive(winterObjects, currentSeason == Season.Winter);
    }


    private GameObject GetRandomTree()
    {
        List<GameObject> treeList = null;
        switch (currentSeason)
        {
            case Season.Spring: treeList = springTrees; break;
            case Season.Summer: treeList = summerTrees; break;
            case Season.Autumn: treeList = autumnTrees; break;
            case Season.Winter: treeList = winterTrees; break;
        }


        if (treeList == null || treeList.Count == 0)
        {
            Debug.LogWarning("No trees available for the current season.");
            return null;
        }

        return treeList[Random.Range(0, treeList.Count)];
    }

    private Vector3 GetWorldPos(int x, int z)
    {
        return new Vector3((x - gridOffset) * spacing, 0f, (z - gridOffset) * spacing);
    }

    private Transform GetPlaneTransform(int x, int z)
    {
        string name = $"Plane_{x}_{z}";
        Transform found = transform.Find(name);
        return found != null ? found : transform;
    }

    private void ClearPreviousGeneration()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        fenceParent = new GameObject("FenceGroup").transform;
        fenceParent.SetParent(transform);
    }

    public void SetFenceStyle(int index)
    {
        selectedFenceIndex = index;
        GenerateGraveyard();
    }
    public void SetSeasonToSpring()
    {
        currentSeason = Season.Spring;
        ApplySeasonDensity();        //
        ApplySeasonObjects();
        GenerateGraveyard();
    }

    public void SetSeasonToSummer()
    {
        currentSeason = Season.Summer;
        ApplySeasonDensity();
        ApplySeasonObjects();
        GenerateGraveyard();
    }

    public void SetSeasonToAutumn()
    {
        currentSeason = Season.Autumn;
        ApplySeasonDensity();
        ApplySeasonObjects();
        GenerateGraveyard();
    }

    public void SetSeasonToWinter()
    {
        currentSeason = Season.Winter;
        ApplySeasonDensity();
        ApplySeasonObjects();
        GenerateGraveyard();
    }


    private void GenerateSpawnAndExitPoints()
    {
        int pathX = gridOffset + width / 2;
        Vector3 spawnPos = GetWorldPos(pathX, height + gridOffset);
        Vector3 exitPos = GetWorldPos(pathX, height + gridOffset + 1);

        GameObject spawn = new GameObject("SpawnPoint");
        spawn.transform.position = spawnPos;
        spawn.transform.SetParent(transform);

        GameObject exit = new GameObject("ExitPoint");
        exit.transform.position = exitPos;
        exit.transform.SetParent(transform);
    }

    private void BuildNavMeshSurface()
    {
        Vector3 centerPos = GetWorldPos(gridOffset + width / 2, gridOffset + height / 2);
        GameObject navSurfaceObj = new GameObject("NavMeshSurface");
        navSurfaceObj.transform.position = centerPos;
        navSurfaceObj.transform.SetParent(transform);

        NavMeshSurface surface = navSurfaceObj.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.All;
        surface.BuildNavMesh();

        Debug.Log("[NavMesh] Build 完成后 NavMesh Triangles Count: " + NavMesh.CalculateTriangulation().indices.Length);
    }

    public Rect GetGraveyardBounds()
    {
        float minX = (gridOffset + 1) * spacing;
        float maxX = (gridOffset + width - 2) * spacing;

        float minZ = (gridOffset + 1) * spacing;
        float maxZ = (gridOffset + height - 2) * spacing;

        float widthInWorld = maxX - minX;
        float heightInWorld = maxZ - minZ;

        return new Rect(minX, minZ, widthInWorld, heightInWorld);
    }

    public void ApplySeasonDensity()
    {
        switch (currentSeason)
        {
            case Season.Spring:
                flowerSpawnChance = 0.23f; // 花朵很多
                treeSpawnChance = 0.5f;   // 树木少
                break;

            case Season.Summer:
                flowerSpawnChance = 0.12f; // 花朵少
                treeSpawnChance = 0.7f;   // 树木多
                break;

            case Season.Autumn:
                flowerSpawnChance = 0.08f; // 花少
                treeSpawnChance = 0.5f;   // 树也偏少
                break;

            case Season.Winter:
                flowerSpawnChance = 0.0f; // 没有花
                treeSpawnChance = 0.6f;   // 树很稀疏
                break;
        }

        Debug.Log($"[Season] 当前季节：{currentSeason}，花密度={flowerSpawnChance}，树密度={treeSpawnChance}");
    }
    public void SetFlowerSpawnChance(float value)
    {
        flowerSpawnChance = Mathf.Clamp(value, 0f, 0.23f);
    }


}
