using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float speed = 40;
    Vector3 velocity = Vector3.zero;
    float yaw;
    float pitch;

    void Start()
    {
        
    }

    void Update()
    {
        // While the right mouse is held down...
        if (Input.GetMouseButton(1))
        {
            yaw += speed * Input.GetAxis("Mouse X");
            pitch -= speed * Input.GetAxis("Mouse Y");

            transform.eulerAngles = new Vector3(pitch, yaw, 0);
        }

        // If shift is being held, increase the speed.
        float modedSpeed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? speed * 4 : speed;

        // Move with arrows or WASD.
        transform.position += (transform.forward * Input.GetAxisRaw("Vertical")) * modedSpeed * Time.deltaTime;
        transform.position += (transform.right * Input.GetAxisRaw("Horizontal")) * modedSpeed * Time.deltaTime;
    }
}
