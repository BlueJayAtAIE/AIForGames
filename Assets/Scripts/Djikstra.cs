using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Djikstra : MonoBehaviour
{
    private List<Node> nodeList = new List<Node>();
    private Node current;

    public Node destination;

    [HideInInspector]
    public List<Node> finalPath = new List<Node>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightControl))
        {
            Path();
            // TODO, seek along the final path.
        }
    }

    private void Path()
    {
        // Gather and mark all nodes as unvisited.
        nodeList.Clear();
        NodeSpawner nManager = GameObject.Find("NodeGenerator").GetComponent<NodeSpawner>();
        foreach (var node in nManager.grid)
        {
            nodeList.Add(node);
        }

        List<Node> open = new List<Node>();
        List<Node> closed = new List<Node>();
        // TODO, how to find what node we start on? Right now its 0,0-
        // but we can't always be sure of that...

        open.Add(nodeList[0]);

        while (open.Count > 0)
        {
            // If the destination node is actually in our closed list...
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

            while (j >= 0 && nList[j].GScore > key.GScore)
            {
                nList[j + 1] = nList[j];
                j =- 1;
                nList[j + 1] = key;

                if (j > nList.Count) j = 0;
            }
        }
    }

    private void OnDrawGizmos()
    {
        foreach (var n in finalPath)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(new Vector3(n.gameObject.transform.position.x, n.gameObject.transform.position.y + 0.5f, n.gameObject.transform.position.z), 0.2f);

            Gizmos.color = Color.black;
            if (n.previous != null)
            {
                Gizmos.DrawLine(n.gameObject.transform.position, n.previous.gameObject.transform.position);
            }

        }

        if (destination != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(new Vector3(destination.gameObject.transform.position.x, destination.gameObject.transform.position.y + 0.5f, destination.gameObject.transform.position.z), 0.2f);
        }
    }
}
