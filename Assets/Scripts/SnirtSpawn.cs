using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnirtSpawn : MonoBehaviour
{
    public GameObject[] snirtPrefabs;

    void Start()
    {
        int randy = Random.Range(0, snirtPrefabs.Length);

        Instantiate(snirtPrefabs[randy], new Vector3(transform.position.x, transform.position.y + 1.005f, transform.position.z), Quaternion.identity);
    }
}
