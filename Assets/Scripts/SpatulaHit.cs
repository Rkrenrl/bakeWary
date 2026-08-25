using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;

public class SpatulaHit : MonoBehaviourPun
{
    public int damage = 20;
    public bool hasHit = false; // 중복 판정 방지

    [Header("VFX")]
    public GameObject hitVFX;

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