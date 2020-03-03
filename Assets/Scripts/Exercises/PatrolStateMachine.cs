using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolStateMachine : MonoBehaviour
{
    IFiniteState[] states;

    public Transform[] waypoints;

    public Transform fleeTarget;
    public Transform seekTarget;

    public float speed = 4;
    public float awarenessRadius = 5;
    private Vector3 CurrentVelocity = new Vector3(0, 0, 0);

    void Start()
    {
        states = new IFiniteState[] { new Patrol(waypoints, gameObject, speed), new Seek(), new Flee() };
    }

    void Update()
    {
        
    }

    class Patrol : IFiniteState
    {
        private GameObject agent;
        private float speed;
        private Transform[] waypoints;
        private int currentPatrolIndex;
        private Vector3 CurrentVelocity = new Vector3(0, 0, 0);

        public Patrol() { }

        public Patrol(Transform[] patrolRoute, GameObject agent, float agentSpeed)
        {
            waypoints = patrolRoute;
            this.agent = agent;
            speed = agentSpeed;
            currentPatrolIndex = 0;
        }

        public void EnterState()
        {
            Debug.Log("Patrol Enter.");
        }

        public void RunState()
        {
            Vector3 target = waypoints[currentPatrolIndex].position;

            if (Vector3.Distance(agent.transform.position, target) < 1)
            {
                currentPatrolIndex++;

                if (currentPatrolIndex > waypoints.Length - 1) currentPatrolIndex = 0;
            }

            Vector3 v = ((target - agent.transform.position) * speed).normalized;
            Vector3 force = v - CurrentVelocity;
            CurrentVelocity += force * Time.deltaTime;
            agent.transform.position += CurrentVelocity * speed * Time.deltaTime;
        }

        public void ExitState()
        {
            Debug.Log("Patrol Exit.");
        }


        public void AddCondition(IStateTransition transition)
        {

        }

        public void RemoveCondition(IStateTransition transition)
        {

        }


        public IFiniteState ChangeState()
        {
            throw new System.NotImplementedException();
        }
    }

    class Seek : IFiniteState
    {
        public void EnterState()
        {

        }

        public void RunState()
        {

        }

        public void ExitState()
        {

        }


        public void AddCondition(IStateTransition transition)
        {

        }

        public void RemoveCondition(IStateTransition transition)
        {
        }


        public IFiniteState ChangeState()
        {
            throw new System.NotImplementedException();
        }
    }

    class Flee : IFiniteState
    {
        public void EnterState()
        {
        }

        public void RunState()
        {
        }

        public void ExitState()
        {
        }


        public void AddCondition(IStateTransition transition)
        {
        }

        public void RemoveCondition(IStateTransition transition)
        {
        }


        public IFiniteState ChangeState()
        {
            throw new System.NotImplementedException();
        }
    }
}
