using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("Animation")]
    public Animation anim;           // base 모델에 붙은 Animation 컴포넌트
    public AnimationClip idleClip;
    public AnimationClip runClip;

    [Header("Movement Reference")]
    public Rigidbody rb;             // 캐릭터 루트의 Rigidbody
    public float moveThreshold = 0.5f;
    public float fadeTime = 0.15f;

    private bool isRunning = false;

    void Start()
    {
        anim.AddClip(idleClip, "Idle");
        anim.AddClip(runClip, "Run");
        anim.Play("Idle");
    }

    void Update()
    {
        Vector3 flatVelocity = rb.linearVelocity;
        flatVelocity.y = 0f; // 점프/낙하/공중부양 중 수직 속도는 무시하고 수평 이동만 체크

        bool shouldRun = flatVelocity.magnitude > moveThreshold;

        if (shouldRun != isRunning)
        {
            isRunning = shouldRun;
            anim.CrossFade(isRunning ? "Run" : "Idle", fadeTime);
        }
    }
}