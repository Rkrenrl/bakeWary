using System.Collections;
using UnityEngine;
using Photon.Pun;

public class WhiskBuffSkill : MonoBehaviourPun
{
    public GameObject whiskMotionPrefab; // 머리 위에서 휘젓는 거대 휘핑기 (Resources)

    public float cooldown = 20f;
    public float castTime = 1.2f;   // 휘젓는 모션 시간
    public float spawnHeight = 3f;

    public WeaponSwitcher weaponSwitcher;
    public int throwableSlotIndex = 0;

    public UnityEngine.UI.Image cooldownBar;

    private float cooldownTimer = 0f;

    void Update()
    {
        if (!photonView.IsMine) return;

        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        if (cooldownBar != null && IsSkillRelevant())
            cooldownBar.fillAmount = 1f - Mathf.Clamp01(cooldownTimer / cooldown);

        if (Input.GetKeyDown(KeyCode.Q) && cooldownTimer <= 0f && CanUseSkill())
            StartCoroutine(CastRoutine());
    }

    bool IsSkillRelevant() => RoomManager.instance.upgradingLevel == 3;

    bool CanUseSkill()
    {
        if (weaponSwitcher == null || weaponSwitcher.selectedWeapon != throwableSlotIndex)
            return false;
        return RoomManager.instance.upgradingLevel == 3;
    }

    IEnumerator CastRoutine()
    {
        cooldownTimer = cooldown;

        Vector3 spawnPos = transform.position + Vector3.up * spawnHeight;
        GameObject whisk = PhotonNetwork.Instantiate(whiskMotionPrefab.name, spawnPos, Quaternion.identity);

        yield return new WaitForSeconds(castTime);

        if (whisk != null) PhotonNetwork.Destroy(whisk);

        PlayerBuff buff = GetComponent<PlayerBuff>();
        if (buff != null)
            buff.photonView.RPC("SetBuff", RpcTarget.All, true);
    }
}