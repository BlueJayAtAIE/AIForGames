using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YeetOnClick : MonoBehaviour
{
    public GameObject toYeet;

    private bool isActive = true;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isActive = !isActive;
            toYeet.SetActive(!isActive);
        }
    }
}
