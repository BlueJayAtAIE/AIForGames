using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    // Assign in-editor.
    public GameObject[] nodePrefabs;
    public int x;
    public int y;

    [HideInInspector]
    public NodeEX[,] grid;
    [HideInInspector]
    public List<NodeEX> foodNodes;

    void Start()
    {
        grid = new NodeEX[x, y];
        foodNodes = new List<NodeEX>();

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                int randy = Random.Range(0, 100);
                int spawn;

                if (randy < 45) spawn = 0;       // Grass 1 (0-44)
                else if (randy < 75) spawn = 1;  // Grass 2 (45-74)
                else if (randy < 76) spawn = 2;  // Berry (75)
                else if (randy < 98) spawn = 3;  // Water (76-97)
                else if (randy < 99) spawn = 4;  // Snirt Spawner (98)
                else spawn = 5;                  // Tree (99)

                // Creates the node from the prefab, AND adds the node component to the grid array.
                grid[i, j] = Instantiate(nodePrefabs[spawn], new Vector3(i, 0, j), Quaternion.identity).GetComponent<NodeEX>();
                
                // If the node was a berry bush (spawn case 2) also add it to the food list.
                if (spawn == 2)
                {
                    foodNodes.Add(grid[i, j]);
                }
            }
        }

        // Add the connections to the nodes.
        // Do NOT add it if either "x" or "y" end up outside the array. This accounts for corners/edges.
        // Do NOT add it if you cannot move through it either.
        // Nodes which cannot be moved through add no connections to themselves.
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                if (grid[i, j].CanMoveThrough)
                {
                    if (i - 1 >= 0 && grid[i - 1, j].CanMoveThrough)
                    {
                        grid[i, j].connections.Add(grid[i - 1, j]);
                    }

                    if (i + 1 < x && grid[i + 1, j].CanMoveThrough)
                    {
                        grid[i, j].connections.Add(grid[i + 1, j]);
                    }

                    if (j - 1 >= 0 && grid[i, j - 1].CanMoveThrough)
                    {
                        grid[i, j].connections.Add(grid[i, j - 1]);
                    }

                    if (j + 1 < y && grid[i, j + 1].CanMoveThrough)
                    {
                        grid[i, j].connections.Add(grid[i, j + 1]);
                    }
                }
            }
        }
    }
}

// REFERENCE:
// [x - 1, y + 1]  [x, y + 1]  [x + 1, y + 1]
//   [x - 1, y]    [!ORIGIN!]    [x + 1, y]
// [x - 1, y - 1]  [x, y - 1]  [x + 1, y - 1]