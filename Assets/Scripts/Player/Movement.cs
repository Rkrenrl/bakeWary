using UnityEngine.UI;
using Photon.Pun;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float walkSpeed = 8f;
    public float sprintSpeed = 14f;
    public float maxVelocityChange = 10f;

    [Space]
    public float airControl = 0.5f;
    
    [Space]
    public float jumpHeight = 30f;

    private Vector2 input;
    private Rigidbody rb;

    private bool sprinting;
    private bool jumping;
    private bool holdingFly;

    public bool grounded = false;

    [Header("Gauge (Float)")]
    public float maxGauge = 100f;
    public float currentGauge;
    public float gaugeDrainSpeed = 30f;
    public float gaugeRefillSpeed = 15f;
    public float floatSpeed = 8f;
    public float floatSpeedMultiplier = 1.3f;

    public Image floatingGuageBar;

    private bool floating;
    private bool wasFloating = false; // 상태 변화 감지용

    [Header("Float VFX")]
    public ParticleSystem floatVFX; // 발밑 파티클 (인스펙터에서 연결)

    private PhotonView pv;

    [Header("Animation")]
    public Animation handAnimation;
    public AnimationClip handWalkAnimation;
    public AnimationClip idleAnimation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentGauge = maxGauge;
        pv = GetComponent<PhotonView>();
        if (pv == null) pv = GetComponentInParent<PhotonView>();
    }

    void Update()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input.Normalize();

        sprinting = Input.GetButton("Sprint");
        jumping = Input.GetButton("Jump");
        holdingFly = Input.GetButton("Fly");

        floatingGuageBar.fillAmount = currentGauge / maxGauge;
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.CompareTag("Ground"))
        {
            grounded = true;
        }
    }

    void FixedUpdate()
    {
        floating = holdingFly && currentGauge > 0f;

        // 뜨는 상태가 바뀐 순간에만 RPC 전송 (매 프레임 X)
        if (floating != wasFloating)
        {
            wasFloating = floating;

            if (pv != null && pv.IsMine)
            {
                pv.RPC("SetFloatVFX", RpcTarget.All, floating);
            }
        }

        if (floating)
        {
            currentGauge -= gaugeDrainSpeed * Time.fixedDeltaTime;
        }
        else if (grounded)
        {
            currentGauge += gaugeRefillSpeed * Time.fixedDeltaTime;
        }

        currentGauge = Mathf.Clamp(currentGauge, 0f, maxGauge);

        if (floating)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, floatSpeed, rb.linearVelocity.z);

            if (input.magnitude > 0.5f)
            {
                float speed = (sprinting ? sprintSpeed : walkSpeed) * floatSpeedMultiplier;
                rb.AddForce(CalculateMovement(speed), ForceMode.VelocityChange);
            }

            grounded = false;
            return;
        }

        if(grounded)
        {
            if(jumping)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpHeight, rb.linearVelocity.z);
            }
            else if(input.magnitude > 0.5f)
            {
                handAnimation.clip = handWalkAnimation;
                if(Input.GetMouseButton(0)) handAnimation.Stop();
                else handAnimation.Play();
                
                rb.AddForce(CalculateMovement(sprinting ? sprintSpeed : walkSpeed), ForceMode.VelocityChange);   
            }
            else
            {
                handAnimation.clip = idleAnimation;
                if(Input.GetMouseButton(0)) handAnimation.Stop();
                else handAnimation.Play();
                
                var velocity1 = rb.linearVelocity;
                velocity1 = new Vector3(velocity1.x * 0.2f * Time.fixedDeltaTime, velocity1.y, velocity1.z * 0.2f * Time.fixedDeltaTime);
                rb.linearVelocity = velocity1;
            }
        }
        else
        {
            if(input.magnitude > 0.5f)
            {
                rb.AddForce(CalculateMovement(sprinting ? sprintSpeed * airControl : walkSpeed * airControl), ForceMode.VelocityChange);   
            }
            else
            {
                var velocity1 = rb.linearVelocity;
                velocity1 = new Vector3(velocity1.x * 0.2f * Time.fixedDeltaTime, velocity1.y, velocity1.z * 0.2f * Time.fixedDeltaTime);
                rb.linearVelocity = velocity1;
            }
        }

        grounded = false;
    }

    [PunRPC]
    public void SetFloatVFX(bool isFloating)
    {
        if (floatVFX == null) return;

        if (isFloating)
        {
            floatVFX.Play();
        }
        else
        {
            floatVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting); // 새 파티클 생성만 멈추고, 기존 파티클은 자연스럽게 사라짐
        }
    }

    Vector3 CalculateMovement(float _speed)
    {
        Vector3 targetVelocity = new Vector3(input.x, 0, input.y);
        targetVelocity = transform.TransformDirection(targetVelocity);
        targetVelocity *= _speed;

        Vector3 velocity = rb.linearVelocity;

        if(input.magnitude > 0.5f)
        {
            Vector3 velocityChange = targetVelocity - velocity;
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;
            return velocityChange;
        }
        else
            return new Vector3();
    }
}