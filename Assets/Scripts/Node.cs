using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    // 1 is default. Editable in Editor.
    public int costToMove = 1;
    private Color selfColor;
    private MeshRenderer rnd;

    [HideInInspector]
    public int GScore = 0;

    // Used only by A*.
    [HideInInspector]
    public int HScore = 0;
    [HideInInspector]
    public int FScore = 0;

    // Connections related.
    [HideInInspector]
    public List<Node> connections = new List<Node>();
    [HideInInspector]
    public Node previous;

    private void Start()
    {
        rnd = gameObject.GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        selfColor = new Color(255, 255 - Mathf.Min(255, costToMove * 15), 255 - Mathf.Min(255, costToMove * 15));
        rnd.material.SetColor("_Color", selfColor);
    }

    private void OnDrawGizmos()
    {
        // Draw a wire-sphere roughly around the node.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), 0.2f);
    }

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