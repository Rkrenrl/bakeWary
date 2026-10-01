using Photon.Pun;
using UnityEngine;

public class PlayerBuff : MonoBehaviourPun
{
    [Header("Buff Values")]
    public float duration = 15f;
    public float damageReduction = 0.3f;
    public float speedMultiplier = 1.4f;
    public float floatSpeedMultiplier = 1.5f;

    [Header("VFX")]
    public GameObject auraVFX;
    public GameObject tornadoVFX;

    [Header("Tornado Damage")]
    public float tornadoRadius = 6f;
    public int tornadoTickDamage = 3;
    public float tornadoTickInterval = 0.5f;

    public bool IsBuffed { get; private set; }

    private float buffTimer = 0f;
    private float tickTimer = 0f;
    private bool wasTornadoActive = false; // 추가: 상태 변화 감지용

    [Header("Tornado Position")]
    public float tornadoGroundY = 0f; // 바닥 Y좌표 (레이캐스트로 자동 계산하도록 아래서 처리)
    public LayerMask groundMask;

    void Update()
    {
        if (!photonView.IsMine) return;
        if (!IsBuffed) return;

        buffTimer -= Time.deltaTime;
        if (buffTimer <= 0f)
        {
            photonView.RPC("SetBuff", RpcTarget.All, false);
            return;
        }

        Movement mv = GetComponent<Movement>();
        bool tornadoActive = mv != null && mv.IsFloating;

        if (tornadoActive != wasTornadoActive)
        {
            wasTornadoActive = tornadoActive;
            photonView.RPC("SetTornadoVFX", RpcTarget.All, tornadoActive);
        }

        if (tornadoActive)
        {
            // 매 프레임 바닥 Y를 구해서 X/Z만 플레이어를 따라가도록 위치 갱신
            UpdateTornadoPosition();

            tickTimer -= Time.deltaTime;
            if (tickTimer <= 0f)
            {
                tickTimer = tornadoTickInterval;
                DamageInTornado();
            }
        }
    }

    void UpdateTornadoPosition()
    {
        if (tornadoVFX == null) return;

        float groundY = transform.position.y; // 바닥을 못 찾으면 대체값

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 50f, groundMask))
        {
            groundY = hit.point.y;
        }

        Vector3 pos = transform.position;
        pos.y = groundY;

        tornadoVFX.transform.position = pos; // 위치는 로컬로 매 프레임 갱신 (RPC 필요 없음)
    }

    void DamageInTornado()
    {
        // 판정 중심은 tornadoVFX 위치(=바닥) 대신, 플레이어의 X/Z + 판정용 임의의 Y로 계산
        Vector3 center = tornadoVFX != null ? tornadoVFX.transform.position : transform.position;

        Collider[] hits = Physics.OverlapSphere(center, tornadoRadius * 10f); // 넉넉히 큰 반경으로 1차로 다 긁어옴 (Y 무시하고 수평만 볼 거라 세로로 긴 범위까지 커버)

        foreach (var col in hits)
        {
            if (!col.CompareTag("Player")) continue;

            PhotonView otherView = col.GetComponent<PhotonView>();
            if (otherView == null) continue;
            if (otherView.Owner.ActorNumber == photonView.Owner.ActorNumber) continue;

            // Y를 무시하고 수평(X/Z) 거리만으로 실제 판정
            Vector3 a = new Vector3(center.x, 0f, center.z);
            Vector3 b = new Vector3(col.transform.position.x, 0f, col.transform.position.z);
            float horizontalDist = Vector3.Distance(a, b);

            if (horizontalDist > tornadoRadius) continue; // 반경 밖이면 스킵

            Health health = col.GetComponent<Health>();
            if (health == null) continue;

            if (tornadoTickDamage >= health.health)
            {
                RoomManager.instance.kills++;
                RoomManager.instance.AddUpgradingXP(100);
                RoomManager.instance.SetHashes();
            }

            otherView.RPC("TakeDamage", RpcTarget.All, tornadoTickDamage);
        }
    }

    [PunRPC]
    public void SetBuff(bool on)
    {
        IsBuffed = on;
        buffTimer = on ? duration : 0f;

        if (auraVFX != null) auraVFX.SetActive(on);

        if (!on)
        {
            wasTornadoActive = false;
            if (tornadoVFX != null) tornadoVFX.SetActive(false);
        }
    }

    [PunRPC]
    public void SetTornadoVFX(bool on)
    {
        if (tornadoVFX != null) tornadoVFX.SetActive(on);
    }
}