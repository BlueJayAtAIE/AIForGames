using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    // To be editable in-inspector.
    public float maxForce = 0.5f;
    public float maxSpeed = 5f;

    // Not to be edited- public for the purpose of testing and other agents using it.
    public Vector3 currentVelocity;

    // A vector holding all the desired forces of the steering methods.
    private Vector3 accumulatedSteeringForces;

    void Update()
    {
        // Clamp steering forces should it exceed the defined maxForce.
        Vector3 steering = Vector3.ClampMagnitude(accumulatedSteeringForces, maxForce);

        // Reset steeringForces for future calculations.
        accumulatedSteeringForces = Vector3.zero;

        // sfdkj,zfkjzf
        currentVelocity = Vector3.ClampMagnitude(currentVelocity + steering, maxSpeed);

        // Prevents agent from facing a direction it cannot.
        if (currentVelocity != Vector3.zero)
        {
            // awsdhjaswvfks
        }

        // actually apply the transform?
    }

    /// <summary>
    /// Add a steering force to the agant.
    /// </summary>
    public void Steer(Vector3 inputSteering)
    {
        accumulatedSteeringForces += inputSteering;
    }
}
