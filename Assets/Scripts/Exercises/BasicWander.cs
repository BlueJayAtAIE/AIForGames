using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicWander : MonoBehaviour
{
    // Randomly selected transform.
    private Vector3 target;

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
        // Target is defined here rather than assigned by a GameObject.
        // it needs: to be a point at the end of a defined radius around the agent,
        // a "jitter" amount,
        // and a distance from the current position.

        // TODO: fix all of this, none of which actually works.
        Vector3 jitter = new Vector3(Random.Range(0, 9), 0, Random.Range(0, 8));
        target = (Random.onUnitSphere + jitter).normalized;
        target += new Vector3(10, 0, 10) + transform.position;

        Vector3 v = ((target - transform.position) * speed).normalized;
        Vector3 force = v - CurrentVelocity;
        CurrentVelocity += force * Time.deltaTime;
        transform.position += CurrentVelocity * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(v);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(target, 1);
    }
}
