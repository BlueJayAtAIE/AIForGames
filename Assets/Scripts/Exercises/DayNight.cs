using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNight : MonoBehaviour
{
    public float speed;

    void Update()
    {
        gameObject.transform.Rotate(new Vector3(1, 0, 0), speed * Time.deltaTime);
    }
}
