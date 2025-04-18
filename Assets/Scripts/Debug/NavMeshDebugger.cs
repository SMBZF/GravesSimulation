using UnityEngine;
using UnityEngine.AI;

public class NavMeshDebugger : MonoBehaviour
{
    void OnDrawGizmos()
    {
        var triangulation = NavMesh.CalculateTriangulation();

        Gizmos.color = Color.cyan;

        for (int i = 0; i < triangulation.indices.Length; i += 3)
        {
            Vector3 v0 = triangulation.vertices[triangulation.indices[i]];
            Vector3 v1 = triangulation.vertices[triangulation.indices[i + 1]];
            Vector3 v2 = triangulation.vertices[triangulation.indices[i + 2]];

            Gizmos.DrawLine(v0, v1);
            Gizmos.DrawLine(v1, v2);
            Gizmos.DrawLine(v2, v0);
        }
    }
}
