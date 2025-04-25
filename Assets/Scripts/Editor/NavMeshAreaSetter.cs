#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class NavMeshAreaSetter
{
    public static void SetArea(GameObject obj, int areaIndex)
    {
        GameObjectUtility.SetNavMeshArea(obj, areaIndex);
    }
}
#endif
