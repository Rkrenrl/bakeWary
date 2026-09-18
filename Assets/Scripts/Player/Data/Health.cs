using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class Health : MonoBehaviour
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

    [PunRPC]
    public void TakeDamage(int _damage)
    {
        health -= _damage;

        gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        healthText.text = health.ToString();

        if(health <= 0)
        {
            if(isLocalPlayer)
            {
                RoomManager.instance.SpawnPlayer();

                RoomManager.instance.deaths++;
                RoomManager.instance.SetHashes();
            }

            PhotonNetwork.Destroy(gameObject);
        }
    }
}
