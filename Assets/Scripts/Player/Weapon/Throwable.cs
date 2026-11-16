using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Throwable : MonoBehaviourPun
{
    [Header("Object")]
    public Camera camera;
    public GameObject hand;

    [Header("THrowables Prefab")]
    GameObject currentPrefab;
    public List<GameObject> ThrowablePrefabs;

    [Header("Value")]
    public Transform throwPoint; // 무기가 나갈 위치 (보통 손이나 카메라 앞)
    public float throwForce = 15f;
    public float rotationSpeed = 10f;

    void Update()
    {
        int upgradingLevel = 1;

        if (photonView.Owner != null && photonView.Owner.CustomProperties.ContainsKey("upgradingLevel"))
        {
            upgradingLevel = (int)photonView.Owner.CustomProperties["upgradingLevel"];
        }

        int index = Mathf.Clamp(upgradingLevel - 1, 0, ThrowablePrefabs.Count - 1);
        currentPrefab = ThrowablePrefabs[index];

        for (int i = 0; i < transform.GetChild(0).childCount - 2; i++)
        {
            transform.GetChild(0).GetChild(i).gameObject.SetActive(i == index);
        }
    }

    public void ThrowWeapon()
    {
        if (!photonView.IsMine) return;

        Vector3 spawnPos = throwPoint.position;
        Quaternion spawnRot = Quaternion.LookRotation(transform.forward);

        PlayerBuff buff = GetComponentInParent<PlayerBuff>();
        bool homing = buff != null && buff.IsBuffed;

        object[] data = new object[] { homing };

        GameObject currentThrowable = PhotonNetwork.Instantiate(currentPrefab.name, spawnPos, spawnRot, 0, data);

        Rigidbody rb = currentThrowable.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.angularVelocity = currentThrowable.transform.right * rotationSpeed;
            rb.AddForce(camera.transform.forward * throwForce, ForceMode.VelocityChange);
        }
    }

    [Header("ThrowForce Control")]
    [SerializeField] private Vector3 localOffset = new Vector3(0, 0, 1f); // 카메라 기준 원하는 위치
    [SerializeField] private Vector3 localEulerOffset = Vector3.zero;      // 카메라 기준 원하는 회전
    [SerializeField] private float rotateSpeed = 0.75f;
    [SerializeField] private float maxAngle = 45f;

    private float currentAngle = 0f;
 
    void LateUpdate() // 카메라가 이번 프레임에 회전을 끝낸 뒤에 실행됨
    {
        if (Input.GetMouseButton(0))
        {
            currentAngle -= rotateSpeed;
            currentAngle = Mathf.Clamp(currentAngle, -maxAngle, 0f);

            throwForce += 1.25f;
            throwForce = Mathf.Clamp(throwForce, 15, 100);
        }
        else
        {
            currentAngle = 0f;
        }
        if(Input.GetMouseButtonUp(0))
        {
            ThrowWeapon();
            throwForce = 15;
        }

        Transform cam = camera.transform;

        // 매 프레임 카메라 기준으로 위치/회전을 강제로 고정
        hand.transform.position = cam.TransformPoint(localOffset);
        hand.transform.rotation = cam.rotation * Quaternion.Euler(localEulerOffset) * Quaternion.Euler(currentAngle, 0, 0);
    }
    
}