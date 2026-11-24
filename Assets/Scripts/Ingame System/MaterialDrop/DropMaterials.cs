using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

public class DropMaterials : MonoBehaviour
{
    public List<GameObject> materialsPrefab;
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
            new Vector3(Random.Range(-60, 170), 90, Random.Range(-60, 300));
        
        detectorPrefab.transform.position = detectingSpawnPosition;
    }

    void DropMaterial()
    {
        GameObject drop = PhotonNetwork.Instantiate
            (materialsPrefab[Random.Range(0, materialsPrefab.Count)].name,
            detectorPrefab.transform.position, Quaternion.identity);

        while(!drop.GetComponent<MaterialManager>().isGround)
        {
            detectorPrefab.transform.GetChild(0).gameObject.SetActive(true);
        }
        detectorPrefab.transform.GetChild(0).gameObject.SetActive(false);
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
            
            DropMaterial();

            MoveDetector();
        }
    }
}
