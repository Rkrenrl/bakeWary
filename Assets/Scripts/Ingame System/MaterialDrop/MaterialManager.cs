using UnityEngine;

public class MaterialManager : MonoBehaviour
{
    public bool isGround = false;

    void Update()
    {
        if(!isGround)
            transform.position -= new Vector3(0, -2 * Time.deltaTime, 0);
    }

    public void OnTriggerEnter(Collider other)
    {
        isGround = other.gameObject.layer == 7;
    }
}
