using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.AI;

public class GhostController : MonoBehaviour
{
    public enum State
    {
        Idle,
        Wander,
        ReceiveOffering,
        TalkWithGhost,
        Vanish
    }

    public State currentState = State.Idle;

    [Header("行为参数")]
    public float idleDuration = 2f;
    public float wanderRadius = 3f;
    public float wanderSpeed = 1.5f;
    public float returnSpeed = 1f;
    public float floatHeight = 1.5f;
    public float offeringApproachDistance = 1.2f;

    [Header("交谈设置")]
    public float talkDistance = 2.5f;
    public float talkDuration = 3f;
    public float talkChance = 0.3f;
    public float talkApproachDistance = 1.5f;
    public AudioClip[] talkSounds;
    private AudioSource audioSource;

    [Header("淡出设置")]
    public float fadeDuration = 1.5f;
    public float appearStart = -0.01f;
    public float appearEnd = 0.8f;

    [Header("字幕控制器")]
    public FollowAndFaceCamera dialogFollower;
    public string[] offeringLines;
    public string[] talkLines;

    [Header("转向设置")]
    public float turnSpeed = 3f;

    private Vector3 originGrave;
    private bool hasGrave = false;
    private Rect wanderBounds;
    private Renderer cachedRenderer;
    private Material ghostMaterial;
    private GraveData targetGrave;
    private NavMeshAgent agent;
    private Transform currentTalkTarget;
    private Vector3 wanderCenter;
    private bool hasReceivedOffering = false;

    void Start()
    {
        cachedRenderer = GetComponentInChildren<Renderer>();
        if (cachedRenderer != null)
            ghostMaterial = cachedRenderer.material;

        audioSource = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = wanderSpeed;
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        wanderCenter = originGrave;
        StartCoroutine(StateMachine());
    }

    void LateUpdate()
    {
        if (agent != null)
        {
            Vector3 pos = transform.position;
            pos.y = floatHeight;
            transform.position = pos;
        }
    }

    public void SetGraveTarget(Vector3 gravePos)
    {
        originGrave = gravePos;
        hasGrave = true;
        wanderCenter = gravePos;
    }

    public void SetWanderBounds(Rect bounds)
    {
        wanderBounds = bounds;
    }

    public void SetTargetGrave(GraveData grave)
    {
        targetGrave = grave;
    }

    IEnumerator StateMachine()
    {
        while (true)
        {
            switch (currentState)
            {
                case State.Idle:
                    float wait = Random.Range(idleDuration * 0.5f, idleDuration * 1.5f);
                    yield return new WaitForSeconds(wait);

                    if (!hasReceivedOffering && targetGrave != null && targetGrave.offerings.Count > 0)
                    {
                        ChangeState(State.ReceiveOffering);
                        break;
                    }

                    if (TryStartTalk()) break;

                    ChangeState(State.Wander);
                    break;

                case State.Wander:
                    Vector3 sampleTarget;
                    bool foundWander = NavMesh.SamplePosition(
                        wanderCenter + Random.insideUnitSphere * wanderRadius,
                        out NavMeshHit hitWander,
                        wanderRadius,
                        agent.areaMask);

                    if (foundWander)
                    {
                        sampleTarget = hitWander.position;

                        Vector3 dir = (sampleTarget - transform.position).normalized;
                        Quaternion targetRot = Quaternion.LookRotation(dir);
                        while (Quaternion.Angle(transform.rotation, targetRot) > 1f)
                        {
                            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed);
                            yield return null;
                        }

                        agent.SetDestination(sampleTarget);

                        while (!agent.pathPending && agent.remainingDistance > agent.stoppingDistance)
                        {
                            Vector3 lookTarget = agent.steeringTarget;
                            lookTarget.y = transform.position.y;
                            SmoothLookAt(lookTarget);
                            yield return null;
                        }
                    }

                    ChangeState(State.Idle);
                    break;

                case State.ReceiveOffering:
                    Vector3 dirToGrave = (originGrave - transform.position).normalized;
                    Vector3 approachPos = originGrave - dirToGrave * 0.7f;

                    if (NavMesh.SamplePosition(approachPos, out NavMeshHit hitApproach, 1f, agent.areaMask))
                    {
                        agent.SetDestination(hitApproach.position);

                        while (!agent.pathPending && agent.remainingDistance > 0.1f)
                        {
                            Vector3 lookTarget = agent.steeringTarget;
                            lookTarget.y = transform.position.y;
                            SmoothLookAt(lookTarget);
                            yield return null;
                        }
                    }

                    SmoothLookAt(originGrave);

                    if (targetGrave != null)
                    {
                        ShowDialog(GetRandomLine(offeringLines));
                        targetGrave.ClearOfferings();
                    }

                    yield return new WaitForSeconds(2f);
                    HideDialog();

                    wanderCenter = transform.position;
                    hasReceivedOffering = true;
                    ChangeState(State.Idle);
                    break;

                case State.TalkWithGhost:
                    if (currentTalkTarget != null)
                    {
                        float dist = Vector3.Distance(transform.position, currentTalkTarget.position);
                        if (dist > talkApproachDistance)
                        {
                            // 先平滑转向对方
                            Vector3 dir = (currentTalkTarget.position - transform.position).normalized;
                            Quaternion targetRot = Quaternion.LookRotation(dir);
                            while (Quaternion.Angle(transform.rotation, targetRot) > 1f)
                            {
                                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
                                yield return null;
                            }

                            // 然后再前往对方位置
                            agent.SetDestination(currentTalkTarget.position);

                            while (agent.remainingDistance > talkApproachDistance)
                            {
                                SmoothLookAt(currentTalkTarget.position);
                                yield return null;
                            }
                            agent.ResetPath();
                        }

                        SmoothLookAt(currentTalkTarget.position);
                    }


                    ShowDialog(GetRandomLine(talkLines));
                    if (talkSounds != null && talkSounds.Length > 0 && audioSource != null)
                        audioSource.PlayOneShot(talkSounds[Random.Range(0, talkSounds.Length)]);

                    yield return new WaitForSeconds(talkDuration);
                    HideDialog();
                    ChangeState(State.Idle);
                    break;

                case State.Vanish:
                    if (hasGrave)
                        yield return StartCoroutine(MoveBackToGrave());

                    yield return StartCoroutine(FadeOutAndDestroy());
                    yield break;
            }

            yield return null;
        }
    }

    bool TryStartTalk()
    {
        GhostController[] ghosts = FindObjectsOfType<GhostController>();
        foreach (var ghost in ghosts)
        {
            if (ghost != this &&
                ghost.currentState == State.Idle &&
                Vector3.Distance(transform.position, ghost.transform.position) < talkDistance)
            {
                if (Random.value < talkChance)
                {
                    currentTalkTarget = ghost.transform;
                    ghost.currentTalkTarget = this.transform;

                    SmoothLookAt(ghost.transform.position);
                    ghost.SmoothLookAt(transform.position);

                    ghost.ForceTalkWithMe(this.transform);
                    ChangeState(State.TalkWithGhost);
                    return true;
                }
            }
        }
        return false;
    }

    public void ForceTalkWithMe(Transform talker)
    {
        if (currentState == State.Idle || currentState == State.Wander)
        {
            currentTalkTarget = talker;
            StopAllCoroutines();
            ChangeState(State.TalkWithGhost);
            StartCoroutine(StateMachine());
        }
    }

    void ShowDialog(string line)
    {
        if (dialogFollower != null)
        {
            dialogFollower.gameObject.SetActive(true);
            Text text = dialogFollower.GetComponentInChildren<Text>();
            if (text != null) text.text = line;
        }
    }

    void HideDialog()
    {
        if (dialogFollower != null)
            dialogFollower.gameObject.SetActive(false);
    }

    void ChangeState(State newState)
    {
        currentState = newState;
    }

    public void TriggerVanish()
    {
        if (currentState != State.Vanish)
        {
            StopAllCoroutines();
            ChangeState(State.Vanish);
            StartCoroutine(StateMachine());
        }
    }

    IEnumerator MoveBackToGrave()
    {
        Vector3 target = originGrave;
        agent.SetDestination(target);

        while (!agent.pathPending && agent.remainingDistance > agent.stoppingDistance)
        {
            Vector3 lookTarget = agent.steeringTarget;
            lookTarget.y = transform.position.y;
            SmoothLookAt(lookTarget);
            yield return null;
        }
    }

    IEnumerator FadeOutAndDestroy()
    {
        float t = 0f;

        if (ghostMaterial == null)
        {
            Destroy(gameObject);
            yield break;
        }

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float appearValue = Mathf.Lerp(appearStart, appearEnd, t / fadeDuration);

            if (ghostMaterial.HasProperty("_appear"))
                ghostMaterial.SetFloat("_appear", appearValue);

            yield return null;
        }

        if (dialogFollower != null)
            Destroy(dialogFollower.gameObject);

        Destroy(gameObject);
    }

    public void ForceDestroy()
    {
        if (dialogFollower != null)
            Destroy(dialogFollower.gameObject);

        Destroy(gameObject);
    }

    string GetRandomLine(string[] lines)
    {
        if (lines == null || lines.Length == 0) return "";
        return lines[Random.Range(0, lines.Length)];
    }

    public void SmoothLookAt(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
        }
    }
}
