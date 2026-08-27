using UnityEngine;
using TMPro;
using Photon.Pun;
using System.Collections;

public class PlayerSetup : MonoBehaviour
{
    public Movement movement;

    public GameObject camera;

    public string nickname;

    public TextMeshPro nicknameText;

    public Transform TPweaponHolder;

    public void IsLocalPlayer()
    {
        TPweaponHolder.gameObject.SetActive(false);

        movement.enabled = true;
        camera.SetActive(true);

        nicknameText.gameObject.SetActive(false);
    }

    [PunRPC]
    public void SetTPWeapon(int _weaponIndex)
    {
        foreach(Transform _weapon in TPweaponHolder)
        {
            _weapon.gameObject.SetActive(false);
        }

        TPweaponHolder.GetChild(_weaponIndex).gameObject.SetActive(true);
        if(_weaponIndex == 0)
        {
            int index = Mathf.Clamp(RoomManager.instance.upgradingLevel - 1, 0, 4);
            TPweaponHolder.GetChild(_weaponIndex).GetChild(index - 1).gameObject.SetActive(true);
            if(index > 1)
                TPweaponHolder.GetChild(_weaponIndex).GetChild(index - 2).gameObject.SetActive(false);
        }
    }

    [PunRPC]
    public void SetNickname(string _name)
    {
        nickname = _name;

        nicknameText.text = nickname;
    }

    
}
