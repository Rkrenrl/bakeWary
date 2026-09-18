using Photon.Pun;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    public PhotonView playerSetupView;
    public int selectedWeapon = 0;

    void Start()
    {
        SelectWeapon();    
    }

    void Update()
    {
        int previousSelectedWeapon = selectedWeapon;

        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedWeapon = 0;
        }

        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedWeapon = 1;
        }
        
        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            selectedWeapon = 2;
        }

        if(Input.GetKeyDown(KeyCode.Alpha4))
        {
            selectedWeapon = 3;
        }

        if(Input.GetKeyDown(KeyCode.Alpha5))
        {
            selectedWeapon = 4;
        }

        if(Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if(selectedWeapon >= transform.childCount - 1)
            {
                selectedWeapon = transform.childCount - 1;
            }
            else
            {
                selectedWeapon += 1;
            }
        }

        if(Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if(selectedWeapon <= 0)
            {
                selectedWeapon = 0;
            }
            else
            {
                selectedWeapon -= 1;
            }
        }

        if(previousSelectedWeapon != selectedWeapon)
        {
            SelectWeapon();
        }

        
    }

    void SelectWeapon()
    {
        playerSetupView.RPC("SetTPWeapon", RpcTarget.AllBuffered, selectedWeapon);
        Movement movement = gameObject.transform.root.GetComponent<Movement>();

        if(selectedWeapon >= transform.childCount)
        {
            selectedWeapon = transform.childCount - 1;
        }

        if(selectedWeapon == 0)
        {
            
        }

        if(selectedWeapon == 1)
        {
            Knife knife = transform.GetChild(1).gameObject.GetComponent<Knife>();
            knife.GetComponent<Animation>().Stop();
            knife.GetComponent<Animation>().Play(knife.draw.name);
            knife.isPlaying = false;

            movement.walkSpeed = 17f;
            movement.sprintSpeed = 27f;
        }

        if(selectedWeapon == 2)
        {
            Weapon Ak = transform.GetChild(0).gameObject.GetComponent<Weapon>();
            Ak.animation.Stop();
            Ak.animation.Play(Ak.draw.name);

            movement.walkSpeed = 8f;
            movement.sprintSpeed = 14f;
        }
        

        int i = 0;

        foreach(Transform _weapon in transform)
        {
            if(i == selectedWeapon)
            {
                _weapon.gameObject.SetActive(true);
            }
            else
            {
                _weapon.gameObject.SetActive(false);
            }

            i++;
        }
    }
}
