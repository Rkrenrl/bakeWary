using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using TMPro;
using UnityEngine.UI;

public class Health : MonoBehaviourPun
{
    public float maxHealth = 100f;
    public float health;
    public bool isLocalPlayer;

    [Header("UI")]
    public Image healthBar;
    public TextMeshProUGUI healthText;

    [Header("KD")]
    public int kill = 0;
    public int death = 0;

    void Update()
    {
        healthBar.fillAmount = health / maxHealth;
    }

    private bool isDead = false;

    [PunRPC]
    public void TakeDamage(int _damage, int _attackerActorNumber)
    {
        if (isDead) return; // 이미 죽은 뒤에 도착한 중복 피해는 무시

        PlayerBuff buff = GetComponent<PlayerBuff>();
        if (buff != null && buff.IsBuffed)
            _damage = Mathf.RoundToInt(_damage * (1f - buff.damageReduction));

        health -= _damage;

        gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        healthText.text = health.ToString();
        healthBar.fillAmount = (float)health / 100;

        if (health <= 0)
        {
            isDead = true;

            // 소유자(죽은 사람 본인) 클라이언트에서만 실행
            if (isLocalPlayer)
            {
                RoomManager.instance.deaths++;
                RoomManager.instance.SetHashes();

                // 킬러에게 "네가 죽였다"고 한 번만 알림
                Photon.Realtime.Player killer = PhotonNetwork.CurrentRoom.GetPlayer(_attackerActorNumber);
                if (killer != null)
                    photonView.RPC("AwardKill", killer);

                RoomManager.instance.SpawnPlayer();
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    [PunRPC]
    public void AwardKill()
    {
        // 킬러 본인 클라이언트에서만 실행됨
        RoomManager.instance.kills++;
        RoomManager.instance.AddTeamScore(RoomManager.instance.myTeam, 1);
        RoomManager.instance.AddUpgradingXP(100);
        RoomManager.instance.SetHashes();
        PhotonNetwork.LocalPlayer.AddScore(100);
    }
}
