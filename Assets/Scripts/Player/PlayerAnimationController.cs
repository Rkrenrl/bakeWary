using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("Animation")]
    public Animation anim;
    public AnimationClip idleClip;
    public AnimationClip runClip;

    [Header("Movement Reference")]
    public float moveThreshold = 0.5f;
    public float fadeTime = 0.15f;

    private bool isRunning = false;
    private Vector3 lastPosition;

    void Start()
    {
        anim.cullingType = AnimationCullingType.AlwaysAnimate;

        Debug.Log($"[Anim] idleClip={idleClip}, runClip={runClip}"); // 추가

        anim.AddClip(idleClip, "Idle");
        anim.AddClip(runClip, "Run");
        anim.Play("Idle");

        lastPosition = transform.position;
    }

    void Update()
    {
        Vector3 delta = transform.position - lastPosition;
        delta.y = 0f;

        float currentSpeed = delta.magnitude / Time.deltaTime;
        lastPosition = transform.position;

        bool shouldRun = currentSpeed > moveThreshold;

        if (shouldRun != isRunning)
        {
            isRunning = shouldRun;
            Debug.Log($"[Anim] 전환 시도: {(isRunning ? "Run" : "Idle")}"); // 추가
            anim.CrossFade(isRunning ? "Run" : "Idle", fadeTime);
        }
    }
}