using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snirt : MonoBehaviour
{
    [System.Serializable]
    public struct Constraints
    {
        public int repelForce;
        public Vector3 Min;
        public Vector3 Max;
    }

    #region General Variables
    private IBehavior behaviourTreeRoot;

    public bool amAlive = true; // Upon being toggled to false, the Snirt enters a "dead" state. Does not move, an now an obstacle.

    public GameObject predator;

    private Vector3 velocity;  // The current velocity of the Snirt. 
    public float speed = 4;
    public Vector3 velocityCap;  // A cap on the velocity to prevent extremely fast movement.

    private float hunger;  // Slowly decreses. Once you get to around 10, look for food.
    public float hungerMax = 50;  // Hunger cap- used to fill the hunger upon eating. Editable in inspector.

    private float stamina;  // Slowly decreses. Once you get 0, rest and restore stamina.
    public float staminaMax = 50;  // Stamina cap- used to fill the stamina upon resting. Editable in inspector.
    #endregion

    #region Pathfinding Variables
    private static GridSpawner gridManager;

    private NodeEX currentOn;  // The current node the Snirt is stepping on.
    private NodeEX destination; // The node the Snirt is attempting to path to.

    private List<NodeEX> finalPath = new List<NodeEX>();
    #endregion

    #region Flocking Variables
    private List<Snirt> neighborhoodL = new List<Snirt>();  // List of Boids that influence this one when flocking.
    [HideInInspector]
    public bool considerMe = true;  // Used for determining if the Snirt should be added to the neighborhood.

    // Used for controlling the overall flock behavior.
    private int separationMultiplier = 1;  // Set to -1 to cause collisions.
    private int alignmentMultiplier = 1;  // Set to -1 to cause chaos.
    private int cohesionMultiplier = 1;  // Set to -1 to cause flock-splitting.

    public Constraints boundingPositionBox;  // Used to enter the coords to bound the Snirts to.
    #endregion

    void Start()
    {
        gridManager = GameObject.FindGameObjectWithTag("NodeSpawner").GetComponent<GridSpawner>();

        predator = GameObject.FindGameObjectWithTag("Enemy"); // TEMP

        behaviourTreeRoot = new Selector(                        // <ROOT>
                        new Sequence(                               // [Predator Check]
                            new PredatorInRangeCheck(gameObject),       // (Can See Predator?)
                            new FleeFromPredator(gameObject)),          // (Flee)
                        new Sequence(                               // [Food]
                            new LowFoodCheck(),                         // (Low on Food?)
                            new Selector(                               // <Pathfinding>
                                new Sequence(                               // [Path]
                                    new NeedPathCheck(),                        // (Need a Path?)
                                    new CreatePathToFood()),                    // (Create a Path)
                                new SeekAlongPathToFood())),                // (Seek Along Path)
                        new Selector(                               // <Wander>
                            new Sequence(                               // [Move]
                                new StaminaCheck(gameObject),               // (Stamina High Enough?)
                                new WanderFlock(gameObject)),               // (Wander)
                            new Rest(gameObject)));                     // (Rest)

        hunger = Random.Range(hungerMax - 20, hungerMax);
        stamina = Random.Range(staminaMax - 20, staminaMax);

        RandomVelocity();
    }

    void Update()
    {
        if (amAlive)
        {
            behaviourTreeRoot.DoBehavior();
        }
    }

    void RandomVelocity()
    {
        velocity = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
    }

    #region Behavior Tree Items
    #region Enemy Check
    class PredatorInRangeCheck : IBehavior
    {
        Snirt snirt;

        public PredatorInRangeCheck() { }
        public PredatorInRangeCheck(GameObject agent)
        {
            snirt = agent.GetComponent<Snirt>();
        }

        public BehaviorResult DoBehavior()
        {
            // TODO maybe improve this instead of having a hard coded distance check
            return Vector3.Distance(snirt.transform.position, snirt.predator.transform.position) < 5 ? BehaviorResult.SUCCESS : BehaviorResult.FAILURE;
        }
    }

    class FleeFromPredator : IBehavior
    {
        Snirt snirt;

        public FleeFromPredator() { }
        public FleeFromPredator(GameObject agent)
        {
            snirt = agent.GetComponent<Snirt>();
        }

        public BehaviorResult DoBehavior()
        {
            Vector3 v = ((snirt.transform.position - snirt.predator.transform.position) * snirt.speed * 2.5f).normalized;
            Vector3 force = v - snirt.velocity;
            snirt.velocity += (force + snirt.Bound()) * Time.deltaTime;
            snirt.transform.position += snirt.velocity * (snirt.speed * 2.5f) * Time.deltaTime;
            snirt.transform.rotation = Quaternion.LookRotation(new Vector3(snirt.velocity.x, 0, snirt.velocity.z));
            return BehaviorResult.SUCCESS;
        }
    }
    #endregion

    #region Food
    class LowFoodCheck : IBehavior
    {
        public BehaviorResult DoBehavior()
        {
            // TODO
            return BehaviorResult.FAILURE;
        }
    }

    class NeedPathCheck : IBehavior
    {
        public BehaviorResult DoBehavior()
        {
            // TODO
            return BehaviorResult.FAILURE;
        }
    }

    class CreatePathToFood : IBehavior
    {
        Snirt snirt;
        List<NodeEX> foodNodes;

        public CreatePathToFood() { }
        public CreatePathToFood(GameObject agent, List<NodeEX> nodes)
        {
            snirt = agent.GetComponent<Snirt>();
            foodNodes = nodes;
        }

        public BehaviorResult DoBehavior()
        {
            // Set a temp target holding the conents of the first index.
            NodeEX target = foodNodes[0];

            // Quickly calculate the Manhattan Distance, and set target to whatever is the lowest of the two.
            foreach (var n in foodNodes)
            {
                if (Mathf.Abs(snirt.transform.position.x + n.transform.position.x) + Mathf.Abs(snirt.transform.position.z + n.transform.position.z) < Mathf.Abs(snirt.transform.position.x + target.transform.position.x) + Mathf.Abs(snirt.transform.position.z + target.transform.position.z))
                {
                    target = n;
                }
            }

            // Use this closest target to preform A*.
            // TODO

            return BehaviorResult.SUCCESS;
        }
    }

    class SeekAlongPathToFood : IBehavior
    {
        public BehaviorResult DoBehavior()
        {
            // TODO
            return BehaviorResult.SUCCESS;
        }
    }
    #endregion

    #region Wander
    class StaminaCheck : IBehavior
    {
        private Snirt snirt;

        public StaminaCheck() { }
        public StaminaCheck(GameObject agent)
        {
            snirt = agent.GetComponent<Snirt>();
        }

        public BehaviorResult DoBehavior()
        {
            return snirt.stamina > 0 && snirt.considerMe ? BehaviorResult.SUCCESS : BehaviorResult.FAILURE;
        }
    }

    class WanderFlock : IBehavior
    {
        private Snirt snirt;

        public WanderFlock() { }
        public WanderFlock(GameObject agent)
        {
            snirt = agent.GetComponent<Snirt>();
        }

        public BehaviorResult DoBehavior()
        {
            snirt.FlockMove();
            snirt.stamina -= Random.Range(0f, 0.1f);
            return BehaviorResult.SUCCESS;
        }
    }

    class Rest : IBehavior
    {
        private Snirt snirt;

        public Rest() { }
        public Rest(GameObject agent)
        {
            snirt = agent.GetComponent<Snirt>();
        }

        public BehaviorResult DoBehavior()
        {
            snirt.considerMe = false;
            snirt.stamina += Random.Range(0.5f, 1f);
            if (snirt.stamina >= snirt.staminaMax)
            {
                snirt.considerMe = true;
                snirt.RandomVelocity();
            }
            return BehaviorResult.SUCCESS;
        }
    }
    #endregion
    #endregion

    #region Flocking
    /// <summary>
    /// Add all the boid rules together to get the velocity, and transform the position.
    /// </summary>
    private void FlockMove()
    {
        // Take force vectors for each of the rules.
        Vector3 ruleOne = separationMultiplier * Separation();
        Vector3 ruleTwo = alignmentMultiplier * Alignment();
        Vector3 ruleThree = cohesionMultiplier * Cohesion();
        Vector3 ruleFour = Bound();

        velocity = velocity + ((ruleOne + ruleTwo + ruleThree + ruleFour) * Time.deltaTime);

        // DEBUG ONLY
        //velocity = new Vector3(1, 1, 1);

        velocity = Clamp(velocity, -velocityCap, velocityCap);

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
        // The ConsiderMe check isn't applied to Separation because they should still maintain distance even while resting.
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
        int neighborCount = 0;

        // Add the velocities of all the neighbors together...
        foreach (var b in neighborhoodL)
        {
            if (b.considerMe)
            {
                temp = temp + b.velocity;
                neighborCount++;
            }
        }

        // ... then divide by the count (taking the average).
        // Max prevents division by 0.
        temp = temp / Mathf.Max(1, neighborCount);

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
        int neighborCount = 0;


        // Add the positions of all the neighbors together...
        foreach (var b in neighborhoodL)
        {
            if (b.considerMe)
            {
                temp = temp + b.gameObject.transform.position;
                neighborCount++;
            }
        }

        // ... then divide by the count (taking the average).
        // Max prevents division by 0.
        temp = temp / Mathf.Max(1, neighborCount);

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

        // TODO

        return temp;
    }

    private Vector3 Clamp(Vector3 a, Vector3 min, Vector3 max)
    {
        Vector3 temp = new Vector3();
        temp.x = Mathf.Clamp(a.x, min.x, max.x);
        // Y is 0 here to prevent a phenomenon I call Group Bouncing.
        temp.y = 0;
        temp.z = Mathf.Clamp(a.z, min.z, max.z);
        return temp;
    }
    #endregion

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Boid"))
        {
            neighborhoodL.Add(other.GetComponent<Snirt>());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Boid"))
        {
            neighborhoodL.Remove(other.GetComponent<Snirt>());
        }
    }
}