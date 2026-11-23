using UnityEngine;

public class SpawnDetectorManager : MonoBehaviour
{
    public bool CanSpawn = false;

    public void OTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 3)
        {
            CanSpawn = false;
        }
        else
        {
            CanSpawn = true;
        }
    }

    public bool NearPlayer()
    {
        Vector3 playerPos = RoomManager.instance.localPlayerInstance.transform.position;
        Vector3 detectorPos = transform.position;
        playerPos.y = 0;
        detectorPos.y = 0;

        return Vector3.Distance(playerPos, detectorPos) <= 5; 
    }
}
