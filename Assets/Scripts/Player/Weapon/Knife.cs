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

    [Header("Slash VFX")]
    public GameObject slashVFX;                           // Resources 폴더의 프리팹
    public float slashDistance = 1f;                      // 카메라 앞 몇 m에 띄울지
    public float[] slashRollAngles = { 10f, -10f,};   // 콤보 순서별 베는 각도

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

        SpawnSlashVFX();

        Ray ray = new Ray(camera.transform.position, camera.transform.forward);

        RaycastHit hit;

        if(Physics.Raycast(ray.origin, ray.direction, out hit, 2f))
        {
            if(hit.transform.position.x != camera.transform.position.x && 
                hit.transform.position.z != camera.transform.position.z)
            {
                PhotonNetwork.Instantiate(hitVFX.name, hit.point, Quaternion.identity);

                if(hit.transform.gameObject.GetComponent<Health>())
                {
                    PhotonNetwork.LocalPlayer.AddScore(damage);

                    hit.transform.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, damage, PhotonNetwork.LocalPlayer.ActorNumber);
                }
            }
        }
    }

    void SpawnSlashVFX()
    {
        if (slashVFX == null) return;

        Transform cam = camera.transform;
        Vector3 pos = cam.position + cam.forward * slashDistance - new Vector3(0,0.3f,0);

        // 콤보 순서마다 비스듬한 각도를 달리해서 매번 다른 방향으로 베는 느낌을 줌
        float roll = slashRollAngles.Length > 0
            ? slashRollAngles[attackOrder % slashRollAngles.Length]
            : 0f;

        Quaternion rot = cam.rotation * Quaternion.Euler(90, 180, roll);

        PhotonNetwork.Instantiate(slashVFX.name, pos, rot);
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
