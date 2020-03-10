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

    private struct AnimVariables
    {
        public static string speed = "Speed";
        public static string fidget = "Fidget";
        public static string eat = "Eating";
        public static string dead = "Dead";
        public static string deadTransition = "DeathTrig";
    }

    #region General Variables
    private static SnirtSelector sManager; // To manage the UI.

    private Animator anim;

    private IBehavior behaviourTreeRoot;

    public bool amAlive = true; // Upon being toggled to false, the Snirt enters a "dead" state. Does not move, an now an obstacle.

    public GameObject predator;

    private Vector3 velocity;  // The current velocity of the Snirt. 
    public float speed = 4;
    private float waterSpeed;
    private float originalSpeed;
    public Vector3 velocityCap;  // A cap on the velocity to prevent extremely fast movement.

    [HideInInspector]
    public float hunger;  // Slowly decreses. Once you get to around 10, look for food.
    public float hungerMax = 100;  // Hunger cap- used to fill the hunger upon eating. Editable in inspector.

    [HideInInspector]
    public float stamina;  // Slowly decreses. Once you get 0, rest and restore stamina.
    public float staminaMax = 50;  // Stamina cap- used to fill the stamina upon resting. Editable in inspector.
    #endregion

    #region Pathfinding Variables
    private static GridSpawner gridManager;

    private NodeEX currentOn;  // The current node the Snirt is stepping on.
    private NodeEX destination; // The node the Snirt is attempting to path to.

    private List<NodeEX> finalPath = new List<NodeEX>();

    [HideInInspector]
    public string snirtName; // The name of the Snirt to display in the UI.
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
        anim = GetComponent<Animator>();

        gridManager = GameObject.FindGameObjectWithTag("NodeSpawner").GetComponent<GridSpawner>();

        sManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<SnirtSelector>();

        predator = GameObject.FindGameObjectWithTag("Enemy"); // TEMP

        behaviourTreeRoot = new Selector(                                                           // <ROOT>
                        new Sequence(                                                                   // [Starvation Check]
                            new StarvationCheck(gameObject),                                                // (Hunger at Complete 0?)
                            new Die(gameObject)),                                                           // (PERISH)
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
        waterSpeed = speed / 2;
        originalSpeed = speed;
        snirtName = RandomName();

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

    /// <summary>
    /// Silly function to generate a random (enough) string.
    /// </summary>
    private string RandomName()
    {
        string temp = "";
        int randy = Random.Range(1, 7);

        // Title
        switch (randy)
        {
            case 1:
            case 2:
            default:
                // Purposefully blank.
                break;
            case 3:
                temp += "Mr. ";
                break;
            case 4:
                temp += "Sir ";
                break;
            case 5:
                temp += "Mrs. ";
                break;
            case 6:
                temp += "Madame ";
                break;
        }

        // Name
        randy = Random.Range(1, 7);
        switch (randy)
        {
            default:
                temp += "Snirty";
                break;
            case 1:
                temp += "Chonk";
                break;
            case 2:
                temp += "Bumbo";
                break;
            case 3:
                temp += "Aloe";
                break;
            case 4:
                temp += "Speps";
                break;
            case 5:
                temp += "Grimble";
                break;
            case 6:
                temp += "Sir"; // Yes this is on purpose.
                break;
        }

        // Extra
        randy = Random.Range(1, 7);
        switch (randy)
        {
            default:
            case 1:
            case 2:
            case 3:
            case 4:
                // Purposefully blank.
                break;
            case 5:
                temp += " Jr.";
                break;
            case 6:
                temp += " Sr.";
                break;
        }

        return temp;
    }

    /// <summary>
    /// Sets a few variables needed for the Snirt to be considered dead by itself and the others.
    /// </summary>
    void CommitDie()
    {
        considerMe = false;
        gameObject.layer = 9; // Layer 9 is Obstacles
        anim.SetTrigger(AnimVariables.deadTransition);
        anim.SetBool(AnimVariables.dead, true);
        hunger = 0;
        stamina = 0;
        amAlive = false; // This is what will make it no longer go through the behaviour tree.
        velocity = Vector3.zero;
    }

    /// <summary>
    /// For calculating which movement-based animation to play.
    /// </summary>
    void DoAnimations()
    {
        // TODO
    }

    /// <summary>
    /// Subtracts a small amount from stamina and hunger.
    /// Used on all movement methods.
    /// </summary>
    void ModStats()
    {
        stamina -= Random.Range(0f, 0.1f);
        hunger -= Random.Range(0f, 0.1f);
    }

    #region Movement
    /// <summary>
    /// Move towards (Seek) the given target.
    /// </summary>
    public void MoveTowards(Vector3 target, float speedMod)
    {
        Vector3 v = ((target - transform.position) * (speed * speedMod)).normalized;
        Vector3 force = v - velocity;
        velocity += (force + Bound()) * Time.deltaTime;

        velocity = Clamp(velocity, -velocityCap, velocityCap);

        transform.position += velocity * (speed * speedMod) * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(new Vector3(velocity.x, 0, velocity.z));

        ModStats();
    }

    /// <summary>
    /// Run (Flee) from the given target.
    /// </summary>
    public void MoveFrom(Vector3 target, float speedMod)
    {
        Vector3 v = ((transform.position - target) * (speed * speedMod)).normalized;
        Vector3 force = v - velocity;
        velocity += (force + Bound()) * Time.deltaTime;

        velocity = Clamp(velocity, -velocityCap, velocityCap);

        transform.position += velocity * (speed * speedMod) * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(new Vector3(velocity.x, 0, velocity.z));

        ModStats();
    }

    /// <summary>
    /// Basic obstacle avoidance. 
    /// Requires any obstacles to be on the Obstacle Layer (9)!
    /// </summary>
    private Vector3 Avoid()
    {
        Vector3 temp = Vector3.zero;

        int layerMask = 9; // Layer 9 is Obstacles

        RaycastHit hit;
        // If you detect a collision on something matching the layermask...
        if (Physics.Raycast(transform.position, velocity, out hit, 3f, layerMask))
        {
            Debug.DrawRay(transform.position, velocity * hit.distance);
        }
        
        // Raycast velocity * a random small rotation (increments of 20)...
        // Keep going until you dont collide with anything.
        // Once you find a clear path, try to move towards where its clear.

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
        Vector3 ruleFive = Avoid();

        velocity = velocity + ((ruleOne + ruleTwo + ruleThree + ruleFour + ruleFive) * Time.deltaTime);

        velocity = Clamp(velocity, -velocityCap, velocityCap);

        transform.position = transform.position + velocity * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(new Vector3(velocity.x, 0, velocity.z));

        ModStats();
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
            if (Vector3.Distance(b.gameObject.transform.position, transform.position) < 1.3f)
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

        // Division by 4 here makes the desired velocity about 1/4th of the average.
        return (temp - velocity) / 4;
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

        return (temp - transform.position) / 50;
    }
    #endregion
    #endregion

    #region Behavior Tree Items
    #region Death
    class StarvationCheck : IBehavior
    {
        Snirt snirt;

        public StarvationCheck() { }
        public StarvationCheck(GameObject agent)
        {
            snirt = agent.GetComponent<Snirt>();
        }

        public BehaviorResult DoBehavior()
        {
            return snirt.hunger <= 0 ? BehaviorResult.SUCCESS : BehaviorResult.FAILURE;
        }
    }

    class Die : IBehavior
    {
        Snirt snirt;

        public Die() { }
        public Die(GameObject agent)
        {
            snirt = agent.GetComponent<Snirt>();
        }

        public BehaviorResult DoBehavior()
        {
            snirt.CommitDie();
            return BehaviorResult.SUCCESS;
        }
    }
    #endregion

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
            return snirt.hunger <= snirt.hungerMax / 3 ? BehaviorResult.SUCCESS : BehaviorResult.FAILURE;
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
            snirt.considerMe = false;

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
            // If you have a path, move towards the first node in the list.
            if (snirt.finalPath.Count > 0)
            {
                snirt.MoveTowards(snirt.finalPath[0].gameObject.transform.position, 1);
                return BehaviorResult.SUCCESS;
            }
            
            // Failsafe- if you no longer have a path, you cant move towards it.
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
            return BehaviorResult.SUCCESS;
        }
    }

    class Rest : IBehavior
    {
        private Snirt snirt;
        private int defaultLayer;

        public Rest() { }
        public Rest(GameObject agent)
        {
            snirt = agent.GetComponent<Snirt>();
            defaultLayer = snirt.gameObject.layer;
        }

        public BehaviorResult DoBehavior()
        {
            // Make the others disreguard you and recover stamina.
            snirt.considerMe = false;
            snirt.stamina += Random.Range(0.5f, 1f);
            snirt.gameObject.layer = 9; // Layer 9 is Obstacles.

            // If you've actually finished recovering set up for resuming wandering.
            if (snirt.stamina >= snirt.staminaMax)
            {
                snirt.gameObject.layer = defaultLayer;
                snirt.considerMe = true;
                snirt.RandomVelocity();
            }
            return BehaviorResult.SUCCESS;
        }
    }
    #endregion
    #endregion

    private void OnMouseDown()
    {
        sManager.SnirtStats(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Colliding with a predator (Enemy).
        if (collision.gameObject.CompareTag("Enemy"))
        {
            CommitDie();
        }

        // Colliding with the ground (Nodes).
        if (collision.gameObject.CompareTag("Node"))
        {
            NodeEX collidedNode = collision.gameObject.GetComponent<NodeEX>();

            // Change the currently on node to this one.
            currentOn = collidedNode;

            // If you're currently pathing, this manages the path.
            if (finalPath.Count > 0)
            {
                // If this node is the destination- the food- fill hunger and set up resuming wandering.
                if (collidedNode == destination)
                {
                    stamina = 0;
                    anim.ResetTrigger(AnimVariables.eat);
                    anim.SetTrigger(AnimVariables.eat);
                    hunger = hungerMax;
                    RandomVelocity();
                    considerMe = true;
                }

                // If the node is the next in line in the goal path and you hit, remove it.
                if (collidedNode == finalPath[0])
                {
                    finalPath.Remove(collidedNode);
                }
            }
        }

        // If they've collided with a water tile, they slow down to about half speed.
        if (collision.gameObject.layer == 4)
        {
            speed = waterSpeed;
        }
        else
        {
            speed = originalSpeed;
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