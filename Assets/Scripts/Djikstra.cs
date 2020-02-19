using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Djikstra : MonoBehaviour
{
    List<Node> unvisited = new List<Node>();
    List<Node> visited = new List<Node>();

    Node current;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightControl))
        {
            Path();
        }
    }

    private void Path()
    {
        // Gather and mark all nodes as unvisited.
        unvisited.Clear();
        NodeSpawner nManager =  GameObject.Find("NodeGenerator").GetComponent<NodeSpawner>();
        foreach (var node in nManager.grid)
        {
            unvisited.Add(node);
        }
        // TODO, how to find what node we start on? Right now its 0,0-
        // but we can't always be sure of that...

        // Some progress was made here before i realized the reference on the handbook
        // and the powerpoint are following two different implementations...
    }
}
