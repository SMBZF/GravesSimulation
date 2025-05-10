using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NightModeManager : MonoBehaviour
{
    [Header("幽灵生成设置")]
    public List<GameObject> ghostPrefabs;
    public float spawnHeight = 1.5f;

    [Header("墓园引用")]
    public GraveyardGenerator graveyardGenerator;

    [Header("字幕 UI 预制体")]
    public GameObject dialogCanvasPrefab;

    public void StartNight()
    {
        Debug.Log("[NightMode] 夜晚开始，等待访客离开...");
        StartCoroutine(WaitForVisitorsThenSpawnGhosts());
    }

    private IEnumerator WaitForVisitorsThenSpawnGhosts()
    {
        while (GameObject.FindGameObjectsWithTag("Visitor").Length > 0)
        {
            yield return new WaitForSeconds(0.5f); // 每0.5秒检测一次
        }

        Debug.Log("[NightMode] 所有访客已离开，开始生成幽灵");
        SpawnGhosts();
    }

    private void SpawnGhosts()
    {
        if (ghostPrefabs == null || ghostPrefabs.Count == 0)
        {
            Debug.LogError("未设置任何幽灵预制体，请在 Inspector 中配置 ghostPrefabs");
            return;
        }

        if (dialogCanvasPrefab == null)
        {
            Debug.LogError("未设置 dialogCanvasPrefab，请在 Inspector 中配置字幕预制体");
            return;
        }

        Rect bounds = graveyardGenerator.GetGraveyardBounds();
        GraveData[] graves = FindObjectsOfType<GraveData>();

        foreach (var grave in graves)
        {
            int offeringCount = grave.offerings.Count;
            bool shouldSpawn = false;

            if (offeringCount >= 3) shouldSpawn = true;
            else if (offeringCount == 2 && Random.value < 0.7f) shouldSpawn = true;
            else if (offeringCount == 1 && Random.value < 0.35f) shouldSpawn = true;

            if (shouldSpawn)
            {
                GameObject selectedGhost = ghostPrefabs[Random.Range(0, ghostPrefabs.Count)];

                Vector3 randomOffset = new Vector3(Random.Range(-0.1f, 0.1f), 0f, Random.Range(-0.1f, 0.1f));
                Vector3 spawnPos = grave.transform.position + randomOffset + Vector3.up * spawnHeight;

                GameObject ghost = Instantiate(selectedGhost, spawnPos, Quaternion.identity);

                GhostController gc = ghost.GetComponent<GhostController>();
                if (gc != null)
                {
                    gc.SetGraveTarget(grave.transform.position);
                    gc.SetTargetGrave(grave);
                    gc.SetWanderBounds(bounds);

                    GameObject dialog = Instantiate(dialogCanvasPrefab, ghost.transform);
                    FollowAndFaceCamera follower = dialog.GetComponent<FollowAndFaceCamera>();
                    if (follower != null)
                    {
                        follower.followTarget = ghost.transform;
                        gc.dialogFollower = follower;
                    }
                    else
                    {
                        Debug.LogWarning("生成的字幕 Canvas 缺少 FollowAndFaceCamera 脚本！");
                    }
                }

                Debug.Log($"[NightMode] 墓碑 {grave.name} 生成幽灵，贡品数 = {offeringCount}");
            }
        }
    }
}
