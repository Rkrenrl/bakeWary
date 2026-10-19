using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlayerSetup : MonoBehaviourPun
{
    public Movement movement;
    public GameObject camera;
    public string nickname;
    public TextMeshPro nicknameText;
    public Transform TPweaponHolder;

    public Transform bodyModelRoot; // base 모델의 루트 (인스펙터에서 연결)

    private int lastUpgradingLevel = -1;

    public void IsLocalPlayer()
    {
        TPweaponHolder.gameObject.SetActive(false);
        movement.enabled = true;
        camera.SetActive(true);
        nicknameText.gameObject.SetActive(false);

        // 오직 "진짜 내 캐릭터"일 때만, 이 인스턴스에 한해서 레이어를 바꿈
        SetLayerRecursively(bodyModelRoot, LayerMask.NameToLayer("Player Model"));
    }

    void SetLayerRecursively(Transform root, int layer)
    {
        if (root == null) return;

        root.gameObject.layer = layer;
        foreach (Transform child in root)
        {
            SetLayerRecursively(child, layer);
        }
    }

    

    void Update()
    {
        int level = 1;
        if (photonView.Owner != null && photonView.Owner.CustomProperties.ContainsKey("upgradingLevel"))
        {
            level = (int)photonView.Owner.CustomProperties["upgradingLevel"];
        }

        if (level != lastUpgradingLevel)
        {
            lastUpgradingLevel = level;
            RefreshThrowableTier();
        }
    }

    void RefreshThrowableTier()
    {
        Transform throwableHolder = TPweaponHolder.GetChild(0); // Cube
        int index = Mathf.Clamp(lastUpgradingLevel - 1, 0, throwableHolder.childCount - 1);

        for (int i = 0; i < throwableHolder.childCount; i++)
        {
            throwableHolder.GetChild(i).gameObject.SetActive(i == index);
        }
    }

    [PunRPC]
    public void SetTPWeapon(int _weaponIndex)
    {
        foreach (Transform _weapon in TPweaponHolder)
            _weapon.gameObject.SetActive(false);

        TPweaponHolder.GetChild(_weaponIndex).gameObject.SetActive(true);

        if (_weaponIndex == 0)
            RefreshThrowableTier();
    }

    [PunRPC]
    public void SetNickname(string _name)
    {
        nickname = _name;
        nicknameText.text = nickname;
    }
}