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
            // ��ǿ��ֵ
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, GetDayCurveValue(timeValue));
            directionalLight.intensity = intensity;

            // ̫���켣����λ+���ǣ�
            float sunAngleY = Mathf.Lerp(90f, 350f, timeValue); // 90(��) �� 450(�ٻض�)
            float sunAngleX = GetSunElevationAngle(timeValue);  // ���Ǳ仯������͡�����ߣ�
            directionalLight.transform.rotation = Quaternion.Euler(sunAngleX, sunAngleY, 0f);
        }

        if (skyboxMaterial != null)
        {
            // Skybox�ع��ֵ
            float exposure = Mathf.Lerp(minExposure, maxExposure, GetDayCurveValue(timeValue));
            skyboxMaterial.SetFloat("_Exposure", exposure);
        }
    }

    // ������ʱ��仯����
    float GetSunElevationAngle(float t)
    {
        // �� -10�� (����) �� 90�� (����) �� -10�� (����)
        return Mathf.Lerp(-3f, 75f, Mathf.Sin(t * Mathf.PI));
    }

    // ��ǿ/�ع�仯���� (0��1��0)
    float GetDayCurveValue(float t)
    {
        return Mathf.Clamp01(Mathf.Sin(t * Mathf.PI));
    }
}
