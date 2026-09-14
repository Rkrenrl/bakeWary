using UnityEngine;
using Photon.Pun;

public class RollingPinRollSkill : MonoBehaviourPun
{
    public GameObject rollPrefab;

    public float cooldown = 10f;
    public float spawnBackOffset = 2f;   // 플레이어 뒤로 얼마나 떨어진 지점에 생성할지
    public float spawnHeight = 1.5f;     // 플레이어 기준 얼마나 위에 생성할지

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
        if (RoomManager.instance.upgradingLevel != 2) // 밀대는 레벨 2
            return false;
        return true;
    }

    void CastSkill()
    {
        cooldownTimer = cooldown;

        Vector3 flatForward = transform.forward;
        flatForward.y = 0f;
        flatForward.Normalize();

        Vector3 spawnPos = transform.position - flatForward * spawnBackOffset + Vector3.up * spawnHeight;
        Quaternion spawnRot = Quaternion.LookRotation(flatForward, Vector3.up);

        // 초기 진행 방향을 생성 시점에 모든 클라이언트에 동일하게 전달
        object[] data = new object[] { flatForward };

        PhotonNetwork.Instantiate(rollPrefab.name, spawnPos, spawnRot, 0, data);
    }
}