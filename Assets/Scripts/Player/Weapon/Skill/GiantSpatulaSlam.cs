using System.Collections;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;

public class GiantSpatulaSlam : MonoBehaviourPun
{
    [Header("Swing Setup")]
    public Transform bladeVisual; // 위치/회전은 프리팹에서 직접 세팅해둔 그대로 사용 (건드리지 않음)
    public float swingArcAngle = 160f;

    [Header("Timing")]
    public float delayBeforeSwing = 0.25f;
    public float swingDuration = 0.35f;
    public float destroyDelay = 1.5f;

    [Header("Damage")]
    public int damage = 40;
    public float hitRadius = 3.5f;

    [Header("VFX")]
    public GameObject impactVFX;

    private Quaternion impactRot;
    private bool hasDamaged = false;

    void Start()
    {
        // 생성 시 넘겨받은 회전(캐릭터가 보던 방향)을 그대로 "완성 자세"로 사용
        // → 프리팹에서 세팅해둔 bladeVisual의 자세가 곧 이 순간의 모습이 됨
        impactRot = transform.rotation;

        Vector3 flatForward = impactRot * Vector3.forward;
        Vector3 rightAxis = Vector3.Cross(Vector3.up, flatForward).normalized;
        if (rightAxis.sqrMagnitude < 0.0001f) rightAxis = impactRot * Vector3.right;

        // 시작 자세: 완성 자세에서 같은 축으로 뒤로 젖혀놓기만 하면 됨
        transform.rotation = Quaternion.AngleAxis(-swingArcAngle, rightAxis) * impactRot;

        StartCoroutine(SlamRoutine());
    }

    IEnumerator SlamRoutine()
    {
        yield return new WaitForSeconds(delayBeforeSwing);

        Quaternion startRot = transform.rotation;
        float elapsed = 0f;

        while (elapsed < swingDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / swingDuration;
            t = t * t; // 채찍처럼 끝에서 가속

            transform.rotation = Quaternion.Slerp(startRot, impactRot, t);
            yield return null;
        }

        transform.rotation = impactRot;
        OnImpact();

        yield return new WaitForSeconds(destroyDelay);

        if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }

    void OnImpact()
    {
        // 데미지 판정 중심도 "실제로 모델이 최종적으로 위치한 지점"을 그대로 사용
        // → 시각적으로 보이는 위치와 데미지 판정 위치가 항상 정확히 일치
        Vector3 hitCenter = bladeVisual != null ? bladeVisual.position : transform.position;

        if (impactVFX != null)
            Instantiate(impactVFX, hitCenter, Quaternion.identity);

        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null) audio.Play();

        if (hasDamaged) return;
        if (!photonView.IsMine) return;

        hasDamaged = true;

        Collider[] hits = Physics.OverlapSphere(hitCenter, hitRadius);

        foreach (var col in hits)
        {
            if (!col.CompareTag("Player")) continue;

            PhotonView otherView = col.GetComponent<PhotonView>();
            if (otherView == null) continue;
            if (otherView.Owner.ActorNumber == photonView.Owner.ActorNumber) continue;

            Health health = col.GetComponent<Health>();
            if (health == null) continue;

            PhotonNetwork.LocalPlayer.AddScore(damage);

            otherView.RPC("TakeDamage", RpcTarget.All, damage, photonView.Owner.ActorNumber);
        }
    }
    
    void OnDrawGizmos()
    {
        if (bladeVisual == null) return;

        Vector3 hitCenter = bladeVisual.position;

        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // 반투명 빨간색
        Gizmos.DrawSphere(hitCenter, hitRadius);
    }
}