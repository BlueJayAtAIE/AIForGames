using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [System.Serializable]
    public struct Constraints
    {
        public int Xmin, Xmax, Ymin, Ymax, Zmin, Zmax;
    }

    // List of Boids that influence this one.
    private List<Boid> neighborhoodL;
    //private Boid[] neighborhood;

    // The current velocity of the boid. 
    // This will be set to random numbers upon Start.
    public Vector3 velocity;

    // Used for controlling the overall flock behavior.
    public int separationMultiplier = 1;  // Set to -1 to cause collisions.
    public int alignmentMultiplier = 1;  // Set to -1 to cause chaos.
    public int cohesionMultiplier = 1;  // Set to -1 to cause flock-splitting.

    // Editable in-editor.
    public Constraints boundingPositionBox;

    void Start()
    {
        neighborhoodL = new List<Boid>();
        //neighborhood = FindObjectsOfType<Boid>();

        velocity = new Vector3(Random.Range(-1f, 2f), Random.Range(-2f, 2f), Random.Range(-3f, 2f));
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

        // Add each of them toegther for your final velocity.
        // Not certain which of these two should be used- yield different results.
        velocity = velocity + ((ruleOne + ruleTwo + ruleThree + ruleFour) * Time.deltaTime);
        //velocity = velocity + ruleOne + ruleTwo + ruleThree * Time.deltaTime;

        // DEBUG ONLY
        //velocity = new Vector3(1, 1, 1);

        transform.position = transform.position + velocity * Time.deltaTime;
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
            if (Vector3.Distance(b.gameObject.transform.position, transform.position) < 1)
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
    /// Adds force to keep them within user-specified bounds.
    /// </summary>
    private Vector3 Bound()
    {
        Vector3 temp = Vector3.zero;

        // Checking X
        if (transform.position.x < boundingPositionBox.Xmin)
        {
            temp.x = 10;
        }
        else if (transform.position.x > boundingPositionBox.Xmax)
        {
            temp.x = -10;
        }

        // Checking Y
        if (transform.position.y < boundingPositionBox.Ymin)
        {
            temp.y = 10;
        }
        else if (transform.position.y > boundingPositionBox.Ymax)
        {
            temp.y = -10;
        }

        // Checking Z
        if (transform.position.z < boundingPositionBox.Zmin)
        {
            temp.z = 10;
        }
        else if (transform.position.z > boundingPositionBox.Zmax)
        {
            temp.z = -10;
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
        //Debug.Log("I'm hit!");
        if (other.gameObject.tag == "Boid")
        {
            neighborhoodL.Add(other.GetComponent<Boid>());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Boid")
        {
            // This SHOULD go through and only get rid of the
            // Boid in the list that matches the Boid who just left.
            neighborhoodL.Remove(other.GetComponent<Boid>());
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, velocity);
    }
}
