using UnityEngine;

public class WhiskSpin : MonoBehaviour
{
    public float spinSpeed = 720f;
    public float tiltAngle = 35f; // 회전축을 얼마나 기울일지 (0=프로펠러, 90=완전 눕힘)

    void Update()
    {
        // 기울어진 축을 기준으로 회전 (Y축을 X축 방향으로 tiltAngle만큼 기울인 축)
        Vector3 tiltedAxis = Quaternion.AngleAxis(tiltAngle, Vector3.forward) * Vector3.up;
        transform.Rotate(tiltedAxis, spinSpeed * Time.deltaTime, Space.World);
    }
}