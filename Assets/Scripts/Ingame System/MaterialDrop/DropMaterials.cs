using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

public class DropMaterials : MonoBehaviour
{
    List<GameObject> materialsPrefab;
    public GameObject SpawnDetector;

    GameObject detectorPrefab;

    void Start()
    {
        detectorPrefab = PhotonNetwork.Instantiate(SpawnDetector.name, new Vector3(Random.Range(-60, 170), 0, Random.Range(-60, 300)), Quaternion.identity);

        StartCoroutine(DropCycle());
    }


    void MoveDetector()
    {
        Vector3 detectingSpawnPosition = 
            new Vector3(Random.Range(-60, 170), 0, Random.Range(-60, 300));
        
        detectorPrefab.transform.position = detectingSpawnPosition;
    }

    IEnumerator DropCycle()
    {
        var cooldown = new WaitForSeconds(30f);

        while(true)
        {
            yield return cooldown;
            
            SpawnDetectorManager SDM = detectorPrefab.GetComponent<SpawnDetectorManager>();
            while(!(SDM.CanSpawn || !SDM.NearPlayer()))
            {
                MoveDetector();
            }
            
            
        }
    }
}
