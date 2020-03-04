using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnirtSpawn : MonoBehaviour
{
    public GameObject snirtPrefab;

    void Start()
    {
        Instantiate(snirtPrefab, new Vector3(transform.position.x, transform.position.y + 1.005f, transform.position.z), Quaternion.identity);
    }
}
