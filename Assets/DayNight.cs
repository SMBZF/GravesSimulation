using UnityEngine;

public class DayNightController : MonoBehaviour
{
    [Header("Light Settings")]
    public Light directionalLight;
    public float minIntensity = 0.05f;
    public float maxIntensity = 2f;

    [Header("Skybox Settings")]
    public Material skyboxMaterial;
    public float minExposure = 0.26f;
    public float maxExposure = 1f;

    [Header("Time Control")]
    [Range(0, 1)]
    public float timeValue = 0f;
    public float cycleSpeed = 0.01f;

    [Header("Day/Night Range")]
    [Range(0f, 1f)] public float dayStart = 0.15f;
    [Range(0f, 1f)] public float dayEnd = 0.85f;

    private bool isDay = false;
    private bool wasDay = false;

    [Header("System References")]
    public VisitorManager visitorManager;
    public NightModeManager nightModeManager;

    [Header("Switch")]
    public bool isPaused = true;

    void Awake()
    {
        timeValue = dayStart - 0.005f; // 启动时设置略低于白天起点
    }

    void Update()
    {
        // 时间推进（如未暂停）
        if (!isPaused)
        {
            timeValue += Time.deltaTime * cycleSpeed;

            // ✅ 时间到达1后重置为0，实现循环
            if (timeValue > 1f)
                timeValue = 0f;
        }

        // ☀️ 更新太阳光照强度和角度
        if (directionalLight != null)
        {
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, GetDayCurveValue(timeValue));
            directionalLight.intensity = intensity;

            float sunAngleY = Mathf.Lerp(90f, 450f, timeValue); // 水平方向旋转
            float sunAngleX = GetSunElevationAngle(timeValue);  // 高度角
            directionalLight.transform.rotation = Quaternion.Euler(sunAngleX, sunAngleY, 0f);
        }

        // 更新天空盒曝光度
        if (skyboxMaterial != null)
        {
            float exposure = Mathf.Lerp(minExposure, maxExposure, GetDayCurveValue(timeValue));
            skyboxMaterial.SetFloat("_Exposure", exposure);
        }

        // 🌞🌙 白天夜晚状态判断切换
        wasDay = isDay;
        isDay = (timeValue >= dayStart && timeValue <= dayEnd);

        if (isDay != wasDay)
        {
            if (isDay)
            {
                Debug.Log("[DayNight] 进入白天");
                visitorManager?.StartDay();
                ClearAllGhosts();
            }
            else
            {
                Debug.Log("[DayNight] 进入夜晚");
                visitorManager?.EndDay();
                nightModeManager?.StartNight();
            }
        }
    }

    void ClearAllGhosts()
    {
        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        Debug.Log($"[DayNight] 清除幽灵数量：{ghosts.Length}");

        foreach (GameObject ghost in ghosts)
        {
            GhostController controller = ghost.GetComponent<GhostController>();
            if (controller != null)
            {
                controller.TriggerVanish();
            }
            else
            {
                Destroy(ghost);
            }
        }
    }

    float GetSunElevationAngle(float t)
    {
        return Mathf.Lerp(-3f, 75f, Mathf.Sin(t * Mathf.PI)); // 模拟太阳高度
    }

    float GetDayCurveValue(float t)
    {
        return Mathf.Clamp01(Mathf.Sin(t * Mathf.PI)); // 用正弦模拟一天强度变化
    }
}
