using UnityEngine;
using Photon.Pun;

public class SpatulaSlamSkill : MonoBehaviourPun
{
    public GameObject slamPrefab;

    public float cooldown = 8f;
    public float pivotHeight = 2.2f;

    public WeaponSwitcher weaponSwitcher;
    public int throwableSlotIndex = 0;

    public UnityEngine.UI.Image cooldownBar;

    private float cooldownTimer = 0f;

    void Update()
    {
        if (!photonView.IsMine) return;

        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        if (cooldownBar != null && CanUseSkill())
            cooldownBar.fillAmount = 1f - Mathf.Clamp01(cooldownTimer / cooldown);

        if (Input.GetKeyDown(KeyCode.Q) && cooldownTimer <= 0f && CanUseSkill())
        {
            CastSkill();
        }
    }

    bool CanUseSkill()
    {
        if (weaponSwitcher == null || weaponSwitcher.selectedWeapon != throwableSlotIndex)
            return false;
        if (RoomManager.instance.upgradingLevel != 1)
            return false;
        return true;
    }

    void CastSkill()
    {
        cooldownTimer = cooldown;

        Vector3 flatForward = transform.forward;
        flatForward.y = 0;
        flatForward.Normalize();

        Vector3 pivotPos = transform.position + Vector3.up * pivotHeight;
        Quaternion spawnRot = Quaternion.LookRotation(flatForward, Vector3.up);

        PhotonNetwork.Instantiate(slamPrefab.name, pivotPos, spawnRot);
    }
}