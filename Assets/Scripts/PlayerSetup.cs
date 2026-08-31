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

    private int lastUpgradingLevel = -1;

    public void IsLocalPlayer()
    {
        TPweaponHolder.gameObject.SetActive(false);
        movement.enabled = true;
        camera.SetActive(true);
        nicknameText.gameObject.SetActive(false);
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