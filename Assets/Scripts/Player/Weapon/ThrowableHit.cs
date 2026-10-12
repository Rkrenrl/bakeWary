using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;

public class ThrowableHit : MonoBehaviourPun
{
    public int damage = 20;
    public bool hasHit = false; // 중복 판정 방지

    [Header("VFX")]
    public GameObject hitVFX;

    [Header("Homing (버프 시)")]
    public GameObject homingVFX;      // 작은 소용돌이 (프리팹 자식, 기본 꺼둠)
    public float homingRange = 12f;   // 이 거리 안에 들어오면 추적 시작
    public float homingStrength = 2.5f; // 추적 강도 (낮을수록 약하게 휘어짐)

    private bool isHoming = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        object[] data = photonView.InstantiationData;
        if (data != null && data.Length > 0)
            isHoming = (bool)data[0];

        if (homingVFX != null) homingVFX.SetActive(isHoming);
    }

    void Update()
    {
        if(gameObject.transform.position.y < -5)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        if (!isHoming || rb == null) return;
        if (!photonView.IsMine) return;

        Transform target = FindNearestEnemy();
        if (target == null) return;

        Vector3 toTarget = (target.position - transform.position).normalized;
        Vector3 newDir = Vector3.Slerp(rb.linearVelocity.normalized, toTarget, homingStrength * Time.fixedDeltaTime);

        rb.linearVelocity = newDir * rb.linearVelocity.magnitude; // 속도 크기는 유지, 방향만 살짝 보정
    }

    Transform FindNearestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, homingRange);
        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (var col in hits)
        {
            if (!col.CompareTag("Player")) continue;

            PhotonView otherView = col.GetComponent<PhotonView>();
            if (otherView == null) continue;
            if (otherView.Owner.ActorNumber == photonView.Owner.ActorNumber) continue;

            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = col.transform;
            }
        }

        return nearest;
    }


    void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        if (hasHit) return;

        if (other.CompareTag("Player"))
        {
            PhotonView otherView = other.GetComponent<PhotonView>();

            if (otherView.Owner.ActorNumber == photonView.Owner.ActorNumber)
                return; // 자기 자신은 무시

            if (other.GetComponent<Health>())
            {
                hasHit = true;

                PhotonNetwork.LocalPlayer.AddScore(damage);

                if(damage >= other.GetComponent<Health>().health)
                {
                    RoomManager.instance.kills++;
                    RoomManager.instance.AddTeamScore(RoomManager.instance.myTeam, 1);
                    RoomManager.instance.AddUpgradingXP(100); // PlayerStat.instance 대신 여기로
                    RoomManager.instance.SetHashes();
                    PhotonNetwork.LocalPlayer.AddScore(100);
                }

                otherView.RPC("TakeDamage", RpcTarget.All, damage); // 기존 그대로, 넉백 없음

                PhotonNetwork.Instantiate(hitVFX.name, transform.position, Quaternion.identity);
                PhotonNetwork.Destroy(gameObject);
            }
        }
        else // 벽, 바닥 등 플레이어가 아닌 모든 것
        {
            hasHit = true;

            PhotonNetwork.Instantiate(hitVFX.name, transform.position, Quaternion.identity);
            PhotonNetwork.Destroy(gameObject);
        }
    }
}