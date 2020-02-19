using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeSpawner : MonoBehaviour
{
    public GameObject nodePrefab;

    [HideInInspector]
    public Node[,] grid;
    public int x;
    public int y;

    void Start()
    {
        grid = new Node[x, y];

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                // Creates the node from the prefab, AND adds the node component to the grid array.
                grid[i, j] = Instantiate(nodePrefab, new Vector3(i, 0, j), Quaternion.identity).GetComponent<Node>();
            }
        }

        // Add the connections to the nodes. 
        // Do NOT add it if either "x" or "y" end up outside the array. This accounts for corners/edges.
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                if (i - 1 >= 0)
                {
                    grid[i, j].connections.Add(grid[i - 1, j]);
                }

                if (i + 1 < x)
                {
                    grid[i, j].connections.Add(grid[i + 1, j]);
                }

                if (j - 1 >= 0)
                {
                    grid[i, j].connections.Add(grid[i, j - 1]);
                }

                if (j + 1 < y)
                {
                    grid[i, j].connections.Add(grid[i, j + 1]);
                }
            }
        }
    }
}

// REFERENCE:
// [x - 1, y + 1]  [x, y + 1]  [x + 1, y + 1]
//   [x - 1, y]    [!ORIGIN!]    [x + 1, y]
// [x - 1, y - 1]  [x, y - 1]  [x + 1, y - 1]