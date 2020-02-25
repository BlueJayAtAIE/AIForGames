using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicSeek : MonoBehaviour
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
        Vector3 v = ((target.transform.position - transform.position) * speed).normalized;
        Vector3 force = v - CurrentVelocity;
        CurrentVelocity += force * Time.deltaTime;
        transform.position += CurrentVelocity * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(v);
    }

    /// <summary>
    /// Uses Unity's Vector3.Move towards.
    /// </summary>
    private void MoveTowardsMethod()
    {
        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, CurrentVelocity);
    }
}
