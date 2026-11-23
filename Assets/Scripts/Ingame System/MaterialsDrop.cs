using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class MaterialsDrop : MonoBehaviour
{
    public List<GameObject> materialsPrefab;
    public GameObject SpawnDetector;

    void Update()
    {
        
    }

    void DetectorSpawn()
    {
        Vector3 detectorSpawnPosition = 
            new Vector3(Random.Range(-60, 170), 0, Random.Range(-60, 300));
        
        PhotonNetwork.Instantiate(materialsPrefab[Random.Range(0,materialsPrefab.Count)].name, detectorSpawnPosition, Quaternion.identity);

        
    }
}
