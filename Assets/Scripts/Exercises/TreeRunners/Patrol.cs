using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed;

    [HideInInspector]
    public Transform target;

    IDecision decisionTreeRoot;

    void Start()
    {
        target = waypoints[0];
        decisionTreeRoot = new WaypointReached(target, gameObject, // Additional Variables.
                                new GetWaypoint(gameObject, waypoints, 0), // True Branch
                                new MoveTowardsPatrolpoint(target, gameObject, speed)); // False Branch
    }

    void Update()
    {
        IDecision currentDecision = decisionTreeRoot;
        while (currentDecision != null)
        {
            currentDecision = currentDecision.MakeDecision();
        }
    }
}

public class WaypointReached : IDecision
{
    private Transform target;
    private GameObject agent;
    private IDecision trueBranch;
    private IDecision falseBranch;

    public WaypointReached() { }

    public WaypointReached(Transform target, GameObject agent, IDecision trueBranch, IDecision falseBranch) 
    {
        this.target = target;
        this.agent = agent;
        this.trueBranch = trueBranch;
        this.falseBranch = falseBranch;
    }

    public IDecision MakeDecision()
    {
        target = agent.GetComponent<Patrol>().target;
        return Vector3.Distance(agent.transform.position, target.transform.position) < 1 ? trueBranch : falseBranch;
    }
}

public class GetWaypoint : IDecision
{
    private GameObject agent;
    private Transform[] waypoints;
    private int current;

    public GetWaypoint() { }

    public GetWaypoint(GameObject agent, Transform[] waypoints, int current) 
    {
        this.agent = agent;
        this.waypoints = waypoints;
        this.current = current;
    }

    public IDecision MakeDecision()
    {
        current++;

        if (current > waypoints.Length - 1)
        {
            current = 0;
        }

        agent.GetComponent<Patrol>().target = waypoints[current];

        return null;
    }
}

public class MoveTowardsPatrolpoint : IDecision
{
    private Vector3 CurrentVelocity = new Vector3(0, 0, 0);
    private Transform target;
    private GameObject agent;
    private float speed;

    public MoveTowardsPatrolpoint() { }

    public MoveTowardsPatrolpoint(Transform target, GameObject agent, float speed)
    {
        this.target = target;
        this.agent = agent;
        this.speed = speed;
    }

    public IDecision MakeDecision()
    {
        target = agent.GetComponent<Patrol>().target;
        Vector3 v = ((target.transform.position - agent.transform.position) * speed).normalized;
        Vector3 force = v - CurrentVelocity;
        CurrentVelocity += force * Time.deltaTime;
        agent.transform.position += CurrentVelocity * speed * Time.deltaTime;

        return null;
    }
}
