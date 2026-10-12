using UnityEngine;
using System.Collections;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

public class Knife : MonoBehaviour
{
    public Animation anim;
    public AnimationClip draw;
    public AnimationClip[] attack;
    public AnimationClip idle;
    public bool isPlaying = false;
    
    [Space]
    public Camera camera;
    
    [Space]
    public GameObject hitVFX;

    [Header("SFX")]
    public PlayerPhotonSoundManager playerPhotonSoundManager;

    [Space]
    public int damage = 75;

    int attackOrder = 0;

    
    bool nextInput = false;

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if(!isPlaying)
            {
                StartCoroutine(AttackMotion());
            }
            else
            {
                nextInput = true;
            }
        }
        if(!anim.isPlaying)
        {
            anim.Stop();
            anim.Play(idle.name);
        }
    }

    void DrawSFX()
    {
        playerPhotonSoundManager.PlayKnifeDrawSFX();
    }

    void Attack()
    {
        playerPhotonSoundManager.PlayKnifeSFX();

        Ray ray = new Ray(camera.transform.position, camera.transform.forward);

        RaycastHit hit;

        if(Physics.Raycast(ray.origin, ray.direction, out hit, 1f))
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
                        RoomManager.instance.AddTeamScore(RoomManager.instance.myTeam, 1);
                        RoomManager.instance.AddUpgradingXP(100); // PlayerStat.instance 대신 여기로
                        RoomManager.instance.SetHashes();
                        PhotonNetwork.LocalPlayer.AddScore(100);
                    }

                    hit.transform.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, damage); // 기존 그대로, 넉백 없음
                }
            }
        }
    }

   IEnumerator AttackMotion()
    {
        isPlaying = true;

        while (true)
        {
            nextInput = false;

            AnimationClip clip = attack[attackOrder];

            anim.Play(clip.name);

            // 현재 애니메이션 길이
            float length = anim[clip.name].length;

            // 애니메이션 재생 대기
            yield return new WaitForSeconds(length);

            // 다음 입력이 예약됐으면 콤보 진행
            if (nextInput)
            {
                attackOrder++;

                if (attackOrder >= attack.Length)
                {
                    attackOrder = 0;
                }

                continue;
            }

            // 0.5초 추가 입력 대기
            float timer = 0f;

            while (timer < 0.25f)
            {
                if (nextInput)
                {
                    attackOrder++;

                    if (attackOrder >= attack.Length)
                    {
                        attackOrder = 0;
                    }

                    goto NEXT;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            break;

        NEXT:
            continue;
        }

        attackOrder = 0;
        isPlaying = false;
    }
}
