using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolALT : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 4;

    [HideInInspector]
    public Transform target;

    IBehavior behaviorTreeRoot;

    void Start()
    {
        target = waypoints[0];

        behaviorTreeRoot = new Selector(new IBehavior[]{
                                            new Sequence(new IBehavior[]{
                                                 new WaypointReached(target, gameObject),
                                                 new GetNewWaypoint(waypoints, gameObject)}),
                                            new MoveTowardsWaypoint(target, gameObject, speed)});
    }

    void Update()
    {
        behaviorTreeRoot.DoBehavior();
    }

    class WaypointReached : IBehavior
    {
        private Transform target;
        private PatrolALT agent;

        public WaypointReached() { }
        
        public WaypointReached(Transform target, GameObject agent)
        {
            this.target = target;
            this.agent = agent.GetComponent<PatrolALT>();
        }

        public BehaviorResult DoBehavior()
        {
            target = agent.target;
            return Vector3.Distance(agent.transform.position, target.transform.position) < 1 ? BehaviorResult.SUCCESS : BehaviorResult.FAILURE;
        }
    }    
    
    class GetNewWaypoint : IBehavior
    {
        private PatrolALT agent;
        private Transform[] waypoints;
        private int current;

        public GetNewWaypoint() { }

        public GetNewWaypoint(Transform[] waypoints, GameObject agent)
        {
            this.waypoints = waypoints;
            this.agent = agent.GetComponent<PatrolALT>();
        }

        public BehaviorResult DoBehavior()
        {
            current++;

            if (current > waypoints.Length - 1)
            {
                current = 0;
            }

            agent.target = waypoints[current];

            return BehaviorResult.SUCCESS;
        }
    }    
    
    class MoveTowardsWaypoint : IBehavior
    {
        private Vector3 CurrentVelocity = new Vector3(0, 0, 0);
        private Transform target;
        private PatrolALT agent;

        private float speed;

        public MoveTowardsWaypoint() { }

        public MoveTowardsWaypoint(Transform target, GameObject agent, float speed)
        {
            this.target = target;
            this.agent = agent.GetComponent<PatrolALT>();
            this.speed = speed;
        }

        public BehaviorResult DoBehavior()
        {
            target = agent.target;
            Vector3 v = ((target.transform.position - agent.gameObject.transform.position) * speed).normalized;
            Vector3 force = v - CurrentVelocity;
            CurrentVelocity += force * Time.deltaTime;
            agent.gameObject.transform.position += CurrentVelocity * speed * Time.deltaTime;
            return BehaviorResult.SUCCESS;
        }
    }
}
