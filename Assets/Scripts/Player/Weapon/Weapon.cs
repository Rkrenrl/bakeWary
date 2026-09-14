using Photon.Pun;
using Photon.Pun.UtilityScripts;
using TMPro;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int damage;

    public Camera camera;

    public float fireRate;

    [Header("VFX")]
    public GameObject hitVFX;

    float nextFire;

    [Header("Ammo")]
    public int mag = 150;
    public int ammo = 30;
    public int magAmmo = 30;

    [Header("SFX")]
    public int shootSFXIndex = 0;
    public PlayerPhotonSoundManager playerPhotonSoundManager;

    [Header("UI")]
    public TextMeshProUGUI magText;
    public TextMeshProUGUI ammoText;

    [Header("Animation")]
    public Animation animation;
    public AnimationClip reload;
    public AnimationClip draw;

    [Header("Reciol Settings")]
    // [Range(0,1)]
    // public float recoilPercent = 0.3f;

    [Range(0,2)]
    public float recoverPercent = 0.7f;

    [Space]
    public float recoilUp = 1f;
    public float recoilBack = 0f;

    Vector3 originalPosition;
    Vector3 recoilVelocity = Vector3.zero;

    float recoilLength;
    float recoverLength;

    bool recoiling;
    bool recovering;

    void Start()
    {
        magText.text = mag.ToString();
        ammoText.text = ammo + "/" + magAmmo;

        originalPosition = transform.localPosition;

        recoilLength = 0;
        recoverLength = 1 / fireRate * recoverPercent;
    }

    void Update()
    {
        if(nextFire > 0)
            nextFire -= Time.deltaTime;

        if(Input.GetButton("Fire1") && nextFire <= 0 && ammo > 0 && animation.isPlaying == false)
        {
            nextFire = 1 / fireRate;

            ammo--;

            magText.text = mag.ToString();
            ammoText.text = ammo + "/" + magAmmo;

            Fire();
        }

        if(Input.GetKeyDown(KeyCode.R) && mag > 0 && ammo != magAmmo)
        {
            ReloadAnim();
        }

        if(recoiling)
        {
            Recoil();
        }

        if(recovering)
        {
            Recovering();
        }
    }

    void ReloadAnim()
    {
        animation.Play(reload.name);
    }

    void Reload()
    {
        if(mag > 0)
        {
            if(mag < magAmmo)
            {
                ammo = mag;
                mag = 0;
            }
            else
            {
                mag -= magAmmo - ammo;
                ammo = magAmmo;
            }
        }

        magText.text = mag.ToString();
        ammoText.text = ammo + "/" + magAmmo;
    }

    void Fire()
    {
        recoiling = true;
        recovering = false;

        playerPhotonSoundManager.PlayShootSFX(shootSFXIndex);

        Ray ray = new Ray(camera.transform.position , camera.transform.forward);

        RaycastHit hit;

        if(Physics.Raycast(ray.origin, ray.direction, out hit, 100f))
        {
            if(hit.transform.position.x != camera.transform.position.x && 
                hit.transform.position.z != camera.transform.position.z)
            {
                PhotonNetwork.Instantiate(hitVFX.name, hit.point, Quaternion.identity);

                if(hit.transform.gameObject.GetComponent<Health>())
                {
                    PhotonNetwork.LocalPlayer.AddScore(damage);

                    if(damage >= hit.transform.gameObject.GetComponent<Health>().health)
                    {
                        RoomManager.instance.kills++;
                        RoomManager.instance.SetHashes();

                        PhotonNetwork.LocalPlayer.AddScore(100);
                    }

                    hit.transform.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, damage);
                }
            }
        }
    }

    void Recoil()
    {
        Vector3 finalPosition = new Vector3(originalPosition.x , originalPosition.y + recoilUp, originalPosition.z - recoilBack);

        transform.localPosition = Vector3.SmoothDamp(transform.localPosition , finalPosition , ref recoilVelocity , recoilLength);
        
        if(transform.localPosition == finalPosition)
        {
            recoiling = false;
            recovering = true;
        }
    }

    void Recovering()
    {
        Vector3 finalPosition = originalPosition;
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition , finalPosition , ref recoilVelocity , recoverLength);
        
        if(transform.localPosition == finalPosition)
        {
            recoiling = false;
            recovering = false;
        }
    }
}
