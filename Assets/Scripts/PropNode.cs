using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropNode : MonoBehaviour
{
    // For the express purpose of randomizing prop rotation- leading to some nice variety.
    void Start()
    {
        GameObject child = transform.GetChild(0).gameObject;
        child.transform.rotation = new Quaternion(0f, Random.Range(-360f, 360f), 0f, 80f);
    }
}
