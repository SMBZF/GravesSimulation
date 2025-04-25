using UnityEngine;

public class FollowAndFaceCamera : MonoBehaviour
{
    public Transform followTarget;
    public Vector3 offset = new Vector3(0, 2f, 0);

    [Header("仰角设置")]
    [Range(-45f, 45f)]
    public float tiltAngle = 10f; //可在 Inspector 中调整角度

    void LateUpdate()
    {
        if (followTarget == null || Camera.main == null) return;

        // 跟随位置
        transform.position = followTarget.position + offset;

        // 方向：摄像机 forward（水平投影）
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0f;
        if (cameraForward == Vector3.zero) cameraForward = Vector3.forward;

        // 构造基础朝向 + 修正方向 + 添加仰角
        Quaternion baseRotation = Quaternion.LookRotation(cameraForward) * Quaternion.Euler(0, 180f, 0);
        Quaternion tiltRotation = Quaternion.Euler(-tiltAngle, 0f, 0f); // 抬头一点点

        transform.rotation = baseRotation * tiltRotation;
    }
}
