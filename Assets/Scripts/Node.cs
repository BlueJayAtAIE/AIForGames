using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public int costToMove = 1;

    [HideInInspector]
    public int GScore = 0;
    [HideInInspector]
    public List<Node> connections = new List<Node>();
    [HideInInspector]
    public Node previous;

    private void OnDrawGizmos()
    {
        // Draw a wire-sphere roughly around the node.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), 0.2f);

        // Foreach connection, draw a line from the transform of this to the transform of the connection.
        Gizmos.color = Color.blue;
        foreach (var node in connections)
        {
            //Gizmos.DrawLine(transform.position, node.gameObject.transform.position);
        }
    }
}