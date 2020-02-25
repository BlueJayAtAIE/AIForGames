using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicFlee : MonoBehaviour
{
    // GameObject which the agent seeks towards.
    public GameObject target;

    // Speed at which the agent moves.
    public float speed;
    public Vector3 CurrentVelocity;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        HandbookMethod();
    }

    /// <summary>
    /// Uses the Method outline provided by the ADGP Handbook.
    /// </summary>
    private void HandbookMethod()
    {
        Vector3 v = ((transform.position - target.transform.position) * speed).normalized;
        Vector3 force = v - CurrentVelocity;
        CurrentVelocity += force * Time.deltaTime;
        transform.position += CurrentVelocity * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(v);
    }
}
