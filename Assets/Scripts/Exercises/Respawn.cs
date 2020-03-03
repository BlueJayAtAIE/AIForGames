using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    private Vector3 origin;

    private void Start()
    {
        origin = gameObject.transform.position;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("KillPlane"))
        {
            gameObject.transform.position = origin;
        }
    }
}
