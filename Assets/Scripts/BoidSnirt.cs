using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidSnirt : MonoBehaviour
{
    [System.Serializable]
    public struct Constraints
    {
        public int repelForce;
        public Vector3 Min;
        public Vector3 Max;
    }

    // List of Boids that influence this one.
    private List<BoidSnirt> neighborhoodL;

    // The current velocity of the boid. 
    // This will be set to random numbers upon Start.
    public Vector3 velocity;

    public float speed = 4;

    // Used for controlling the overall flock behavior.
    public int separationMultiplier = 1;  // Set to -1 to cause collisions.
    public int alignmentMultiplier = 1;  // Set to -1 to cause chaos.
    public int cohesionMultiplier = 1;  // Set to -1 to cause flock-splitting.

    // Editable in-editor.
    public Constraints boundingPositionBox;

    void Start()
    {
        neighborhoodL = new List<BoidSnirt>();
        //neighborhood = FindObjectsOfType<Boid>();

        velocity = new Vector3(Random.Range(-1f, 2f), 0, Random.Range(-3f, 2f));
    }

    void Update()
    {
        Move();
    }

    /// <summary>
    /// Add all the rules together to get the velocity, and transform the position.
    /// </summary>
    private void Move()
    {
        // Take force vectors for each of the rules.
        Vector3 ruleOne = separationMultiplier * Separation();
        Vector3 ruleTwo = alignmentMultiplier * Alignment();
        Vector3 ruleThree = cohesionMultiplier * Cohesion();
        Vector3 ruleFour = Bound();

        velocity = velocity + ((ruleOne + ruleTwo + ruleThree + ruleFour) * Time.deltaTime);

        // DEBUG ONLY
        //velocity = new Vector3(1, 1, 1);

        transform.position = transform.position + velocity * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(new Vector3(velocity.x, 0, velocity.z));
    }

    /// <summary>
    /// Calculates the Separation force.
    /// This is the SUM position of all the neighbors.
    /// </summary>
    private Vector3 Separation()
    {
        Vector3 temp = Vector3.zero;

        // Here we see if the position is within a certain threshhold.
        // If it is, add a repelling force.
        foreach (var b in neighborhoodL)
        {
            if (Vector3.Distance(b.gameObject.transform.position, transform.position) < 3)
            {
                temp = temp - (b.gameObject.transform.position - transform.position);
            }
        }

        return temp;
    }

    /// <summary>
    /// Calculates Alignment force.
    /// This is the average velocity of all the neighbors.
    /// </summary>
    private Vector3 Alignment()
    {
        Vector3 temp = Vector3.zero;

        // Add the velocities of all the neighbors together...
        foreach (var b in neighborhoodL)
        {
            temp = temp + b.velocity;
        }

        // ... then divide by the count (taking the average).
        // Max prevents division by 0.
        temp = temp / Mathf.Max(1, neighborhoodL.Count);

        // Division by 8 here makesthe desired velocity about 1/8th of the average.
        return (temp - velocity) / 8;
    }

    /// <summary>
    /// Calculates Cohesion force.
    /// This is the AVERAGE positions of all the neighbors.
    /// </summary>
    private Vector3 Cohesion()
    {
        Vector3 temp = Vector3.zero;

        // Add the positions of all the neighbors together...
        foreach (var b in neighborhoodL)
        {
            temp = temp + b.gameObject.transform.position;
        }

        // ... then divide by the count (taking the average).
        // Max prevents division by 0.
        temp = temp / Mathf.Max(1, neighborhoodL.Count);

        // Division by 100 here moves it about 1% of the way to the average.
        return (temp - transform.position) / 100;
    }

    /// <summary>
    /// Adds force to keep the boids within user-specified bounds.
    /// </summary>
    private Vector3 Bound()
    {
        Vector3 temp = Vector3.zero;

        // Checking X
        if (transform.position.x < boundingPositionBox.Min.x)
        {
            temp.x = boundingPositionBox.repelForce;
        }
        else if (transform.position.x > boundingPositionBox.Max.x)
        {
            temp.x = -boundingPositionBox.repelForce;
        }

        // Checking Y
        if (transform.position.y < boundingPositionBox.Min.y)
        {
            temp.y = boundingPositionBox.repelForce;
        }
        else if (transform.position.y > boundingPositionBox.Max.y)
        {
            temp.y = -boundingPositionBox.repelForce;
        }

        // Checking Z
        if (transform.position.z < boundingPositionBox.Min.z)
        {
            temp.z = boundingPositionBox.repelForce;
        }
        else if (transform.position.z > boundingPositionBox.Max.z)
        {
            temp.z = -boundingPositionBox.repelForce;
        }

        return temp;
    }

    /// <summary>
    /// Basic obstacle avoidance. 
    /// Requires any obstacles to be on the Obstacle Layer (9)!
    /// </summary>
    private Vector3 Avoid()
    {
        Vector3 temp = Vector3.zero;

        // raycast along velocity
        // if you detect a collision on something matching the layermask,
        // raycast velocity * a random small rotation
        // keep going until you dont collide with anything
        // once you find a clear path, try to move towards where its clear.

        return temp;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Boid"))
        {
            neighborhoodL.Add(other.GetComponent<BoidSnirt>());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Boid"))
        {
            neighborhoodL.Remove(other.GetComponent<BoidSnirt>());
        }
    }
}
