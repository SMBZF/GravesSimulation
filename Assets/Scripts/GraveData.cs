using System.Collections.Generic;
using UnityEngine;

public class GraveData : MonoBehaviour
{
    public List<GameObject> offerings = new List<GameObject>();
    public string gravePrefabName;

    public bool allowOffering = true;
    public float offeringChance = 1f;

    private Dictionary<string, float> specialRadiusOverrides = new Dictionary<string, float>
    {
        { "coffin", 0.8f },
        { "gravestone-flat-open", 0.8f }
    };

    public bool TryRegisterOffering(GameObject offering)
    {
        if (!allowOffering || Random.value > offeringChance)
        {
            Destroy(offering);
            return false;
        }

        offerings.Add(offering);
        RepositionOfferings();
        return true;
    }

    public void RegisterOffering(GameObject offering)
    {
        offerings.Add(offering);
        RepositionOfferings();
    }

    public void RepositionOfferings()
    {
        float defaultRadius = 0.5f;
        float arcAngle = 120f;
        Vector3 basePos = transform.position;
        Vector3 forward = transform.forward;

        float radius = specialRadiusOverrides.TryGetValue(gravePrefabName, out float r) ? r : defaultRadius;

        int count = offerings.Count;
        for (int i = 0; i < count; i++)
        {
            float t = count > 1 ? i / (float)(count - 1) : 0.5f;
            float angle = Mathf.Lerp(-arcAngle / 2, arcAngle / 2, t);
            Quaternion rot = Quaternion.Euler(0, angle, 0);
            Vector3 offset = rot * (forward * radius);
            Vector3 extraBackOffset = -forward * 0.2f;

            Vector3 finalPos = basePos + offset + extraBackOffset;
            offerings[i].transform.position = finalPos;
        }
    }

    public void ClearOfferings()
    {
        foreach (var item in offerings)
        {
            if (item != null)
                Destroy(item);
        }
        offerings.Clear();
    }


#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.color = Color.cyan;
        string msg = allowOffering
            ? $"Offer Chance: {(offeringChance * 100f):F1}%"
            : "NO";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, msg);
    }
#endif
}
