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

    [Range(0, 1)]
    public float timeValue = 0f;

    void Update()
    {
        if (directionalLight != null)
        {
            // 光强插值
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, GetDayCurveValue(timeValue));
            directionalLight.intensity = intensity;

            // 太阳轨迹（方位+仰角）
            float sunAngleY = Mathf.Lerp(90f, 350f, timeValue); // 90(东) → 450(再回东)
            float sunAngleX = GetSunElevationAngle(timeValue);  // 仰角变化（早晚低、中午高）
            directionalLight.transform.rotation = Quaternion.Euler(sunAngleX, sunAngleY, 0f);
        }

        if (skyboxMaterial != null)
        {
            // Skybox曝光插值
            float exposure = Mathf.Lerp(minExposure, maxExposure, GetDayCurveValue(timeValue));
            skyboxMaterial.SetFloat("_Exposure", exposure);
        }
    }

    // 仰角随时间变化曲线
    float GetSunElevationAngle(float t)
    {
        // 从 -10° (黎明) → 90° (正午) → -10° (傍晚)
        return Mathf.Lerp(-3f, 75f, Mathf.Sin(t * Mathf.PI));
    }

    // 光强/曝光变化曲线 (0→1→0)
    float GetDayCurveValue(float t)
    {
        return Mathf.Clamp01(Mathf.Sin(t * Mathf.PI));
    }
}
