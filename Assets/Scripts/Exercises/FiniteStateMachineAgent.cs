using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FiniteStateMachineAgent : MonoBehaviour
{
    public enum States { PATROL, SEEK, FLEE }

    private States state;
    public Text printState;

    public Transform[] waypoints;
    private int currentPatrolIndex;

    public Transform fleeTarget;
    public Transform seekTarget;

    public float speed = 4;
    public float awarenessRadius = 5;
    private Vector3 CurrentVelocity = new Vector3(0, 0, 0);

    void Start()
    {
        currentPatrolIndex = 0;
        printState.text = state.ToString();
    }

    void Update()
    {
        if (Vector3.Distance(gameObject.transform.position, fleeTarget.transform.position) < awarenessRadius)
        {
            if (state != States.FLEE)
            {
                ChangeState(States.FLEE);
            }
        }
        else if (Vector3.Distance(gameObject.transform.position, seekTarget.transform.position) < awarenessRadius)
        {
            if (state != States.SEEK)
            {
                ChangeState(States.SEEK);
            }
        }
        else if (Vector3.Distance(gameObject.transform.position, seekTarget.transform.position) > awarenessRadius && Vector3.Distance(gameObject.transform.position, seekTarget.transform.position) > awarenessRadius)
        {
            if (state != States.PATROL)
            {
                ChangeState(States.PATROL);
            }
        }

        switch (state)
        {
            case States.PATROL:
                Patrol();
                break;
            case States.SEEK:
                Seek();
                break;
            case States.FLEE:
                Flee();
                break;
            default:
                Debug.Log("Invalid state! How did you even do this?");
                break;
        }
    }

    private void ChangeState(States newState)
    {
        // Exit the last state.
        switch (state)
        {
            case States.PATROL:
                OnPatrolExit();
                break;
            case States.SEEK:
                OnSeekExit();
                break;
            case States.FLEE:
                OnFleeExit();
                break;
            default:
                Debug.Log("Invalid state! How did you even do this?");
                break;
        }

        // Update the state to the new one.
        state = newState;

        // Enter the new state.
        switch (state)
        {
            case States.PATROL:
                OnPatrolEnter();
                break;
            case States.SEEK:
                OnSeekEnter();
                break;
            case States.FLEE:
                OnFleeEnter();
                break;
            default:
                Debug.Log("Invalid state! How did you even do this?");
                break;
        }

        printState.text = state.ToString();
    }

    private void OnPatrolEnter()
    {
        Debug.Log("Patrol Enter.");
    }

    private void Patrol()
    {
        Vector3 target = waypoints[currentPatrolIndex].position;

        if (Vector3.Distance(gameObject.transform.position, target) < 1)
        {
            currentPatrolIndex++;

            if (currentPatrolIndex > waypoints.Length - 1) currentPatrolIndex = 0;
        }

        Vector3 v = ((target - gameObject.transform.position) * speed).normalized;
        Vector3 force = v - CurrentVelocity;
        CurrentVelocity += force * Time.deltaTime;
        gameObject.transform.position += CurrentVelocity * speed * Time.deltaTime;
    }

    private void OnPatrolExit()
    {
        Debug.Log("Patrol Exit.");
    }

    private void OnSeekEnter()
    {
        Debug.Log("Seek Enter.");
    }

    private void Seek()
    {
        Vector3 target = seekTarget.transform.position;

        Vector3 v = ((target - gameObject.transform.position) * speed).normalized;
        Vector3 force = v - CurrentVelocity;
        CurrentVelocity += force * Time.deltaTime;
        gameObject.transform.position += CurrentVelocity * speed * Time.deltaTime;
    }

    private void OnSeekExit()
    {
        // Empty
    }

    private void OnFleeEnter()
    {
        // Empty
    }

    private void Flee()
    {
        Vector3 target = fleeTarget.transform.position;

        Vector3 v = ((gameObject.transform.position - target) * speed).normalized;
        Vector3 force = v - CurrentVelocity;
        CurrentVelocity += force * Time.deltaTime;
        gameObject.transform.position += CurrentVelocity * speed * Time.deltaTime;
    }

    private void OnFleeExit()
    {
        // Empty
    }
}
