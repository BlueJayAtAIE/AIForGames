using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeEX : MonoBehaviour
{
    // 1 is default.
    public int costToMove = 1;

    // True is default. Toggle to false to make it unable to be walked on.
    public bool CanMoveThrough = true;

    [HideInInspector]
    public int GScore = 0;

    // Used only by A*.
    [HideInInspector]
    public int HScore = 0;
    [HideInInspector]
    public int FScore = 0;

    // Connections related.
    [HideInInspector]
    public List<NodeEX> connections = new List<NodeEX>();
    [HideInInspector]
    public Node previous;

    private void OnDrawGizmosSelected()
    {
        // Foreach connection, draw a line from the transform of this to the transform of the connection.
        Gizmos.color = Color.blue;
        foreach (var node in connections)
        {
            Gizmos.DrawLine(transform.position, node.gameObject.transform.position);
        }
    }
}