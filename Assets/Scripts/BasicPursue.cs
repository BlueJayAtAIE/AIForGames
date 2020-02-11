using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicPursue : MonoBehaviour
{
    // GameObject which the agent seeks towards.
    public GameObject target;

    // Speed at which the agent moves.
    public float speed;
    //public Vector3 MaxVelocity;
    public Vector3 CurrentVelocity;

    private Rigidbody oRb;

    private void Start()
    {
        // Temp solution. Later will make a "SteeringAgent" 
        // class for these behaviours to derive from. That
        // will be what eventually holds CurrentVelocity.
        oRb = target.GetComponent<Rigidbody>();
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
        Vector3 v = ((target.transform.position + oRb.velocity - transform.position) * speed).normalized;
        Vector3 force = v.normalized * speed - CurrentVelocity;
        CurrentVelocity += force * Time.deltaTime;
        transform.position += CurrentVelocity * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(v);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, CurrentVelocity);
    }
}
