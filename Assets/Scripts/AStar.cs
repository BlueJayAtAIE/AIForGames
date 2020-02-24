using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour
{
    private Node currentOn;
    private Vector3 CurrentVelocity;

    public Node destination;
    public float speed;

    [HideInInspector]
    public List<Node> finalPath = new List<Node>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightControl))
        {
            Path();
        }

        // Idealy I should just have a BasicSeek script component on the same object, and
        // just set the target to finalPath[0]. But I'm doing this just for now.
        if (finalPath.Count > 0)
        {
            Vector3 v = ((finalPath[0].gameObject.transform.position - transform.position) * speed).normalized;
            Vector3 force = v - CurrentVelocity;
            CurrentVelocity += force * Time.deltaTime;
            transform.position += CurrentVelocity * Time.deltaTime;
            //transform.rotation = Quaternion.LookRotation(v);
        }
    }

    private void Path()
    {
        NodeSpawner nManager = GameObject.Find("NodeGenerator").GetComponent<NodeSpawner>();
        foreach (var node in nManager.grid)
        {
            node.GScore = 0;
            node.HScore = 0;
            node.FScore = 0;
            node.previous = null;
        }

        List<Node> open = new List<Node>();
        List<Node> closed = new List<Node>();
        Node current = currentOn;

        open.Add(current);

        while (open.Count > 0)
        {
            // If the destination node is actually in our closed list...
            if (closed.Exists(check => destination == check))
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
                        n.GScore = current.GScore + n.costToMove;
                        n.HScore = (int)Mathf.Abs(current.transform.position.x + destination.transform.position.x) + (int)Mathf.Abs(current.transform.position.z + destination.transform.position.z);
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
        if (closed.Exists(check => destination == check))
        {
            // We've found the shortest path!
            // Clear out the old path.
            finalPath.Clear();
            Node temp = destination;
            finalPath.Insert(0, destination);

            // Add the previous of each node until we reach the start (start is null).
            while (temp.previous != null)
            {
                finalPath.Insert(0, temp.previous);
                temp = temp.previous;
            }

            // Remove the node we're currently on as we son't need to seek to
            // something we're already at.
            finalPath.Remove(currentOn);
        }
    }

    /// <summary>
    /// Sorts A given list of nodes by their G Score. Uses Insertion sort algorithm.
    /// </summary>
    private void SortNodes(ref List<Node> nList)
    {
        for (int i = 1; i < nList.Count; i++)
        {
            Node key = nList[i];

            int j = i - 1;

            while (j >= 0 && nList[j].FScore > key.FScore)
            {
                nList[j + 1] = nList[j];
                j = j - 1;
                nList[(j + 1)] = key;

                if (j > nList.Count) j = 0;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Node"))
        {
            Node collidedNode = collision.gameObject.GetComponent<Node>();
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

    private void OnDrawGizmos()
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
