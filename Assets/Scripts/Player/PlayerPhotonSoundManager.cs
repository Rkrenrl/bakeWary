using UnityEngine;
using Photon.Pun;

public class PlayerPhotonSoundManager : MonoBehaviour
{
    public AudioSource footstepSource;
    public AudioClip footstepSFX;

    public AudioSource gunShootSource;
    public AudioClip[] allGunShootSFX;

    public AudioSource knifeSource;
    public AudioClip knifeSFX;

    public AudioSource knifeDrawSource;
    public AudioClip knifeDrawSFX;

    public void PlayFootstepSFX()
    {
        GetComponent<PhotonView>().RPC("PlayFootstepSFX_RPC", RpcTarget.All);
    }

    [PunRPC]
    public void PlayFootstepSFX_RPC()
    {
        footstepSource.clip = footstepSFX;

        footstepSource.pitch = Random.Range(0.7f, 1.2f);
        footstepSource.volume = Random.Range(1.2f, 1.8f);

        footstepSource.Play(); 
    }

    public void PlayShootSFX(int index)
    {
        GetComponent<PhotonView>().RPC("PlayShootSFX_RPC",RpcTarget.All,index);
    }

    [PunRPC]
    public void PlayShootSFX_RPC(int index)
    {
        gunShootSource.clip = allGunShootSFX[index];

        gunShootSource.pitch = Random.Range(0.7f, 1.2f);
        gunShootSource.volume = Random.Range(0.8f, 0.9f);

        gunShootSource.Play(); 
    }

    public void PlayKnifeSFX()
    {
        GetComponent<PhotonView>().RPC("PlayKnifeSFX_RPC", RpcTarget.All);
    }

    [PunRPC]
    public void PlayKnifeSFX_RPC()
    {
        knifeSource.clip = knifeSFX;

        knifeSource.pitch = Random.Range(0.7f, 1.2f);
        knifeSource.volume = Random.Range(0.5f, 0.75f);

        knifeSource.Play(); 
    }

    public void PlayKnifeDrawSFX()
    {
        GetComponent<PhotonView>().RPC("PlayKnifeDrawSFX_RPC", RpcTarget.All);
    }
    
    [PunRPC]
    public void PlayKnifeDrawSFX_RPC()
    {
        knifeDrawSource.clip = knifeDrawSFX;

        knifeDrawSource.volume = 0.5f;

        knifeDrawSource.Play();
    }
}
