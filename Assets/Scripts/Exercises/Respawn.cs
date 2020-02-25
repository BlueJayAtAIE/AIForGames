using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    private Vector3 origin = new Vector3(0, 1, 0);

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "KillPlane")
        {
            gameObject.transform.position = origin;
        }
    }
}
