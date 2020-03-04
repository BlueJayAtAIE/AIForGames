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

    public float hunger;  // Slowly decreses. Once you get to around 10, look for food.
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

        behaviourTreeRoot = new Selector(                                                           // <ROOT>
                        new Sequence(                                                                   // [Predator Check]
                            new PredatorInRangeCheck(gameObject),                                           // (Can See Predator?)
                            new FleeFromPredator(gameObject)),                                              // (Flee)
                        new Sequence(                                                                   // [Food]
                            new LowFoodCheck(gameObject),                                                   // (Low on Food?)
                            new Selector(                                                                   // <Pathfinding>
                                new Sequence(                                                                   // [Path]
                                    new NeedPathCheck(gameObject),                                                 // (Need a Path?)
                                    new CreatePathToFood(gameObject, gridManager.foodNodes)),                      // (Create a Path)
                                new SeekAlongPathToFood(gameObject))),                                          // (Seek Along Path)
                        new Selector(                                                                   // <Wander>
                            new Sequence(                                                                   // [Move]
                                new StaminaCheck(gameObject),                                                   // (Stamina High Enough?)
                                new WanderFlock(gameObject)),                                                   // (Wander)
                            new Rest(gameObject)));                                                         // (Rest)

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

    /// <summary>
    /// Generate a new random velocity.
    /// </summary>
    void RandomVelocity()
    {
        velocity = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
    }

    #region Movement
    /// <summary>
    /// Move towards (Seek) the given target.
    /// </summary>
    public void MoveTowards(Vector3 target, float speedMod)
    {
        Vector3 v = ((target - transform.position) * (speed * speedMod)).normalized;
        Vector3 force = v - velocity;
        velocity += force * Time.deltaTime;

        velocity = Clamp(velocity, -velocityCap, velocityCap);

        transform.position += velocity * (speed * speedMod) * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(new Vector3(velocity.x, 0, velocity.z));
    }

    /// <summary>
    /// Run (Flee) from the given target.
    /// </summary>
    public void MoveFrom(Vector3 target, float speedMod)
    {
        Vector3 v = ((transform.position - target) * (speed * speedMod)).normalized;
        Vector3 force = v - velocity;
        velocity += force * Time.deltaTime;

        velocity = Clamp(velocity, -velocityCap, velocityCap);

        transform.position += velocity * (speed * speedMod) * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(new Vector3(velocity.x, 0, velocity.z));
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

    #region Flocking
    /// <summary>
    /// Add all the boid rules together to get the velocity, and transform the position.
    /// </summary>
    private void MoveFlock()
    {
        // Take force vectors for each of the rules.
        Vector3 ruleOne = separationMultiplier * Separation();
        Vector3 ruleTwo = alignmentMultiplier * Alignment();
        Vector3 ruleThree = cohesionMultiplier * Cohesion();
        Vector3 ruleFour = Bound();

        velocity = velocity + ((ruleOne + ruleTwo + ruleThree + ruleFour) * Time.deltaTime);

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
    #endregion
    #endregion

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
            snirt.MoveFrom(snirt.predator.transform.position, 2.5f);
            return BehaviorResult.SUCCESS;
        }
    }
    #endregion

    #region Food
    class LowFoodCheck : IBehavior
    {
        Snirt snirt;

        public LowFoodCheck() { }
        public LowFoodCheck(GameObject agent)
        {
            snirt = agent.GetComponent<Snirt>();
        }

        public BehaviorResult DoBehavior()
        {
            return snirt.hunger <= 10 ? BehaviorResult.SUCCESS : BehaviorResult.FAILURE;
        }
    }

    class NeedPathCheck : IBehavior
    {
        Snirt snirt;

        public NeedPathCheck() { }
        public NeedPathCheck(GameObject agent)
        {
            snirt = agent.GetComponent<Snirt>();
        }

        public BehaviorResult DoBehavior()
        {
            return snirt.finalPath.Count == 0 ? BehaviorResult.SUCCESS : BehaviorResult.FAILURE;
        }
    }

    class CreatePathToFood : IBehavior
    {
        private Snirt snirt;
        private static List<NodeEX> foodNodes;

        public CreatePathToFood() { }
        public CreatePathToFood(GameObject agent, List<NodeEX> foods)
        {
            snirt = agent.GetComponent<Snirt>();
            foodNodes = foods;
        }

        public BehaviorResult DoBehavior()
        {
            // Set a temp target holding the conents of the first index.
            NodeEX target = foodNodes[0];

            // "Quickly" compare distance, and set target to whatever is the lowest of the two.
            foreach (var n in foodNodes)
            {
                if (Vector3.Distance(snirt.transform.position, n.transform.position) < Vector3.Distance(snirt.transform.position, target.transform.position))
                {
                    target = n;
                }
            }

            // Use this closest target to preform A*.
            snirt.destination = target;

            List<NodeEX> open = new List<NodeEX>();
            List<NodeEX> closed = new List<NodeEX>();
            NodeEX current = snirt.currentOn;
            current.resetNode();

            open.Add(current);

            while (open.Count > 0)
            {
                // If the destination node is actually in our closed list...
                if (closed.Exists(check => snirt.destination == check))
                {
                    break;
                }

                // Set current to open's first, then remove it from open, adding it to closed.
                current = open[0];
                open.RemoveAt(0);
                closed.Add(current);

                foreach (var n in current.connections)
                {
                    // If the node doesn't already exist in the closed list...
                    if (!closed.Exists(check => n == check))
                    {
                        // If it's not already in open, add it, and calculate the G Score/previous.
                        if (!open.Exists(check => n == check))
                        {
                            // Reset values to blank here to prevent wiping the whole board- which takes way too long.
                            n.resetNode();
                            n.GScore = current.GScore + n.costToMove;
                            n.HScore = (int)Mathf.Abs(current.transform.position.x + snirt.destination.transform.position.x) + (int)Mathf.Abs(current.transform.position.z + snirt.destination.transform.position.z);
                            n.FScore = n.GScore + n.HScore;
                            n.previous = current;
                            open.Add(n);
                        }
                        else
                        {
                            // If it IS in open, and the new cost is lower...
                            if (n.GScore > current.GScore + n.costToMove)
                            {
                                // Calculate the G Score and previous connection.
                                n.GScore = current.GScore + n.costToMove;
                                n.previous = current;
                            }
                        }
                    }
                }

                SortNodes(ref open);
            }

            // This will be hit when the while ends naturally or when it hits the break.
            if (closed.Exists(check => snirt.destination == check))
            {
                // We've found the shortest path!
                // Clear out the old path.
                snirt.finalPath.Clear();
                NodeEX temp = snirt.destination;
                snirt.finalPath.Insert(0, snirt.destination);

                // Add the previous of each node until we reach the start (start is null).
                while (temp.previous != null)
                {
                    snirt.finalPath.Insert(0, temp.previous);
                    temp = temp.previous;
                }

                // Remove the node we're currently on as we don't need to seek to
                // something we're already at.
                snirt.finalPath.Remove(snirt.currentOn);

                return BehaviorResult.SUCCESS;
            }

            // If you never found a path- this will fail.
            return BehaviorResult.FAILURE;
        }

        /// <summary>
        /// Sorts A given list of nodes by their F Score. Uses Insertion sort algorithm.
        /// </summary>
        private void SortNodes(ref List<NodeEX> nList)
        {
            for (int i = 1; i < nList.Count; i++)
            {
                NodeEX key = nList[i];

                int j = i - 1;

                while (j >= 0 && nList[j].FScore > key.FScore)
                {
                    nList[j + 1] = nList[j];
                    j = j - 1;
                    nList[j + 1] = key;

                    if (j > nList.Count) j = 0;
                }
            }
        }
    }

    class SeekAlongPathToFood : IBehavior
    {
        Snirt snirt;

        public SeekAlongPathToFood() { }
        public SeekAlongPathToFood(GameObject agent)
        {
            snirt = agent.GetComponent<Snirt>();
        }

        public BehaviorResult DoBehavior()
        {
            if (snirt.finalPath.Count > 0)
            {
                snirt.MoveFrom(snirt.finalPath[0].gameObject.transform.position, 1);
                return BehaviorResult.SUCCESS;
            }
            
            return BehaviorResult.FAILURE;
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
            snirt.MoveFlock();
            snirt.stamina -= Random.Range(0f, 0.1f);
            snirt.hunger -= Random.Range(0f, 0.1f);
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Node"))
        {
            NodeEX collidedNode = collision.gameObject.GetComponent<NodeEX>();
            currentOn = collidedNode;

            if (finalPath.Count > 0)
            {
                if (collidedNode == destination)
                {
                    hunger = hungerMax;
                }

                if (collidedNode == finalPath[0])
                {
                    finalPath.Remove(collidedNode);
                }
            }
        }
    }

    // Just in-case the Snirt idles on one node.
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Node"))
        {
            NodeEX collidedNode = collision.gameObject.GetComponent<NodeEX>();
            currentOn = collidedNode;

            if (finalPath.Count > 0)
            {
                if (collidedNode == finalPath[0])
                {
                    finalPath.Remove(collidedNode);
                }
            }
        }
    }

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

    private void OnDrawGizmosSelected()
    {
        // Gizmo for the current destination node.
        if (destination != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(new Vector3(destination.gameObject.transform.position.x, destination.gameObject.transform.position.y + 0.5f, destination.gameObject.transform.position.z), 0.2f);
        }

        foreach (var n in finalPath)
        {
            // Gizmo for highlighting nodes in the final path.
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(new Vector3(n.gameObject.transform.position.x, n.gameObject.transform.position.y + 0.5f, n.gameObject.transform.position.z), 0.2f);

            // Gizmo for drawing the path through the final path.
            Gizmos.color = Color.black;
            if (n.previous != null)
            {
                Gizmos.DrawLine(n.gameObject.transform.position, n.previous.gameObject.transform.position);
            }
        }

        // Gizmo for the current destination node.
        if (currentOn != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(new Vector3(currentOn.gameObject.transform.position.x, currentOn.gameObject.transform.position.y + 0.5f, currentOn.gameObject.transform.position.z), 0.2f);
        }
    }
}