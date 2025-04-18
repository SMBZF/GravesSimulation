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
    [SerializeField] private List<GameObject> summerTrees;
    [SerializeField] private List<GameObject> autumnTrees;
    [SerializeField] private List<GameObject> winterTrees;

    [Range(0f, 1f)] public float treeSpawnChance = 0.3f;
    public enum Season { Summer, Autumn, Winter }
    public Season currentSeason = Season.Summer;

    [Header("Path Settings")]
    public GameObject stonePathPrefab;

    private Transform fenceParent;
    private const int gridOffset = 10;

    void Start()
    {
        GenerateGraveyard();
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

                bool isFenceArea =
                    (x >= gridOffset && x <= width + gridOffset - 1 &&
                     z >= gridOffset && z <= height + gridOffset - 1) &&
                    (x == gridOffset || x == width + gridOffset - 1 ||
                     z == gridOffset || z == height + gridOffset - 1);

                if (isFenceArea)
                {
                    GenerateFenceAt(x, z, pos, plane.transform);
                }
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
        BuildNavMeshSurface();
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

    private GameObject GetRandomTree()
    {
        List<GameObject> treeList = null;
        switch (currentSeason)
        {
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

    public void SetSeasonToSummer()
    {
        currentSeason = Season.Summer;
        GenerateGraveyard();
    }

    public void SetSeasonToAutumn()
    {
        currentSeason = Season.Autumn;
        GenerateGraveyard();
    }

    public void SetSeasonToWinter()
    {
        currentSeason = Season.Winter;
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
}
