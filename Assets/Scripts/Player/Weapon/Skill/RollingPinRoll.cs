using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;

public class RollingPinRoll : MonoBehaviourPun
{
    private enum RollState { Falling, Rolling, Broken }

    [Header("Visual")]
    public Transform rollVisual;
    public float radius = 1.5f;

    [Header("Movement")]
    public float speed = 12f;
    public int maxBounces = 5;
    public float maxDistance = 40f;
    public LayerMask wallMask;
    public LayerMask groundMask; // 바닥 레이어 (낙하 착지 지점 계산용)

    [Header("Falling")]
    public float gravity = 25f; // 낙하 가속도

    [Header("Damage")]
    public float baseDamage = 60f;
    public float damageFalloffPerDistance = 1.2f;
    public float minDamage = 10f;
    public float hitRadius = 2f;

    [Header("VFX")]
    public GameObject breakVFX;
    public GameObject landVFX; // 착지 순간 이펙트 (선택, 비워둬도 됨)

    private Vector3 direction;
    private float distanceTraveled = 0f;
    private int bounceCount = 0;
    private HashSet<int> hitPlayers = new HashSet<int>();

    private RollState state;
    private float fallVelocity = 0f;
    private float restY; // 바닥에 얹혔을 때의 목표 Y좌표

    void Start()
    {
        object[] data = photonView.InstantiationData;
        direction = ((Vector3)data[0]).normalized;

        float groundY = transform.position.y;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 500f, groundMask))
        {
            groundY = hit.point.y;
        }

        restY = groundY + radius;

        if (transform.position.y > restY + 0.05f)
        {
            state = RollState.Falling;
        }
        else
        {
            Vector3 pos = transform.position;
            pos.y = restY;
            transform.position = pos;
            state = RollState.Rolling;
        }
    }

    void FixedUpdate()
    {
        if (state == RollState.Broken) return;

        if (state == RollState.Falling)
        {
            DoFall();
            return;
        }

        DoRoll();
    }

    void DoFall()
    {
        fallVelocity += gravity * Time.fixedDeltaTime;

        Vector3 pos = transform.position;
        pos.y -= fallVelocity * Time.fixedDeltaTime;

        if (pos.y <= restY)
        {
            pos.y = restY;
            state = RollState.Rolling;

            if (landVFX != null)
                Instantiate(landVFX, pos, Quaternion.identity);
        }

        transform.position = pos;

        CheckPlayerHit(); // 낙하 중에도 판정
    }

    void DoRoll()
    {
        float moveStep = speed * Time.fixedDeltaTime;

        if (Physics.SphereCast(transform.position, radius * 0.9f, direction, out RaycastHit hit, moveStep + 0.2f, wallMask))
        {
            direction = Vector3.Reflect(direction, hit.normal);
            direction.y = 0f;
            direction.Normalize();

            bounceCount++;

            if (bounceCount >= maxBounces)
            {
                Break();
                return;
            }
        }

        transform.position += direction * moveStep;
        distanceTraveled += moveStep;

        if (rollVisual != null)
        {
            Vector3 rollAxis = Vector3.Cross(Vector3.up, direction);
            float angle = (moveStep / radius) * Mathf.Rad2Deg;
            rollVisual.Rotate(rollAxis, angle, Space.World);
        }

        CheckPlayerHit();

        if (distanceTraveled >= maxDistance)
        {
            Break();
        }
    }

    void CheckPlayerHit()
    {
        if (!photonView.IsMine) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, hitRadius);

        foreach (var col in hits)
        {
            if (!col.CompareTag("Player")) continue;

            PhotonView otherView = col.GetComponent<PhotonView>();
            if (otherView == null) continue;
            if (otherView.Owner.ActorNumber == photonView.Owner.ActorNumber) continue;
            if (hitPlayers.Contains(otherView.Owner.ActorNumber)) continue;

            Health health = col.GetComponent<Health>();
            if (health == null) continue;

            hitPlayers.Add(otherView.Owner.ActorNumber);

            float rawDamage = baseDamage - distanceTraveled * damageFalloffPerDistance;
            int damage = Mathf.RoundToInt(Mathf.Max(rawDamage, minDamage));

            PhotonNetwork.LocalPlayer.AddScore(damage);

            if (damage >= health.health)
            {
                RoomManager.instance.kills++;
                RoomManager.instance.AddUpgradingXP(100);
                RoomManager.instance.SetHashes();
                PhotonNetwork.LocalPlayer.AddScore(100);
            }

            otherView.RPC("TakeDamage", RpcTarget.All, damage);
        }
    }

    void Break()
    {
        if (state == RollState.Broken) return;
        state = RollState.Broken;

        if (breakVFX != null)
            Instantiate(breakVFX, transform.position, Quaternion.identity);

        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}