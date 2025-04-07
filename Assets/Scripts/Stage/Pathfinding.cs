using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class Pathfinding : MonoBehaviour
{
    static int maxIter = 1000;
    ///<summary>
    ///In your map include walls as points in an array, assuming a grid-like structure
    ///</summary>
    public static List<Vector2Int> FindRoute(List<Vector2Int> walls, Vector2Int origin, Vector2Int destination)
    {
        int Iterations = 0;

        // Debug.Log("Start routing: Connecting > " + origin + " > with > " + destination);
        PathfindingNode start = new PathfindingNode(origin);
        start.SetParams(0, GetDistance(origin, destination));
        start.SetParent(null);

        List<PathfindingNode> openList = new();    // Nodos candidatos
        List<PathfindingNode> closedList = new();  // Nodos bien evaluados

        openList.Add(start);

        while (openList.Count > 0 && Iterations < maxIter)
        {
            Iterations++;

            PathfindingNode currentNode = openList.Min(); // Get node of lowest F value
            // Debug.Log("Starting at node > " + currentNode.GetPosition());

            if (currentNode == destination)
                return BuildRoute(currentNode); // Ha terminado el algoritmo

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            // Debug.Log("Check neighbors!");
            PathfindingNode[] currentNeighbors = currentNode.GetNeighbors();
            foreach(PathfindingNode n in currentNeighbors)
            {
                // Debug.Log("Evaluating neighbor > " + n.GetPosition());
                if (closedList.Contains(n))
                {  // Saltarse nodos que ya evaluamos
                    // Debug.Log("Already evaluated from openlist");
                    continue;
                }

                if (walls.Contains(n.GetPosition()))
                {
                    // Debug.Log("Hit wall");
                    continue;
                }

                PathfindingNode existingNode = openList.Find(node => node.GetPosition() == n.GetPosition());
                int newCost = currentNode.GetCost() + 1; // We move through the grid one step at a time 

                // Debug.Log("Is node in list? > "+(existingNode == null));
                if (existingNode == null)
                {
                    // Debug.Log("Added to open list");
                    existingNode = n;
                    openList.Add(n);
                }
                else if(newCost > n.GetCost())
                {
                    // Debug.Log("Skipped because this node is already reached from a better path");
                    continue;   // Saltarse si este camino no es el mas eficiente
                }

                // Debug.Log("Node data updated");
                existingNode.SetParent(currentNode);
                existingNode.SetParams(newCost, GetDistance(n.GetPosition(), destination));
            }
        }
        //if (Iterations < maxIter)
            // Debug.Log("Max Iterations reached");
        // Debug.Log("Failed to find a path");
        return null;    // El algoritmo ha fallado
    }

    private static List<Vector2Int> BuildRoute(PathfindingNode node)
    {
        // Debug.Log("Building return path!");
        List<Vector2Int> finalpos = new();
        PathfindingNode c = node;

        while (c != null)
        {
            finalpos.Add(c.GetPosition());
            c = c.GetParent();
        }

        return finalpos;
    }

    private static int GetDistance(Vector2Int point, Vector2Int destination)
    {
        // Distancia manhattan
        int dX = Mathf.Abs(destination.x - point.x);
        int dY = Mathf.Abs(destination.y - point.y);

        return dX + dY;
    }

    private class PathfindingNode : IComparable<PathfindingNode>
    {
        Vector2Int position;
        int g; // Distance to start
        int h; // Heuristic (distance to end)
        int f; // Total score
        PathfindingNode parent;

        public PathfindingNode(Vector2Int position) // Create node with position
        {
            this.position = position;
            g = 0;
            h = 0;
            f = 0;
            parent = null;
        }

        public void SetParams(int G, int H)
        {
            if(G != 0) // No actualizar si el costo ha sido 0 (Se sobreentiende que por algun motivo quieres actualizar los parametros pero no el costo)
                g = G;  

            h = H;  // Heuristic value
            f = g + h;  // Score
        }

        public PathfindingNode GetParent()
        {
            return parent;
        }

        public Vector2Int GetPosition()
        {
            return position;
        }

        public int GetCost()
        {
            return g;
        }

        public void SetParent(PathfindingNode parent)
        {
            this.parent = parent;
        }

        // Obtiene los vecinos adyacentes-4 para una casilla, generando nuevas referencias.
        // No checa si cruza paredes o si ya se tenia un nodo en la misma posición. Eso lo tendrás que manejar tu.
        public PathfindingNode[] GetNeighbors()
        {
            PathfindingNode[] neighbors = new PathfindingNode[4];

            neighbors[0] = new PathfindingNode(position + Vector2Int.up);
            neighbors[1] = new PathfindingNode(position + Vector2Int.down);
            neighbors[2] = new PathfindingNode(position + Vector2Int.left);
            neighbors[3] = new PathfindingNode(position + Vector2Int.right);

            foreach (PathfindingNode neighbor in neighbors)
            {
                neighbor.SetParent(this);
            }

            return neighbors;
        }

        // Comparar puntuación del nodo F (No se si si lo usa el compilador al final o no)
        public int CompareTo(PathfindingNode other)
        {
            return this.f.CompareTo(other.f);
        }

        // Sobrecargar operadores
        public static bool operator ==(PathfindingNode node, PathfindingNode node2)
        {
            if (ReferenceEquals(node, node2))
                return true;

            if (node is null || node2 is null)
                return false;

            return node.position == node2.position;
        }

        public static bool operator !=(PathfindingNode node, PathfindingNode node2)
        {
            return !(node == node2);
        }

        public static bool operator ==(PathfindingNode node, Vector2Int vec)
        {
            if (node is null)
                return false;

            return node.position == vec;
        }

        public static bool operator !=(PathfindingNode node, Vector2Int vec)
        {
            return !(node == vec);  // Llamamos al operador == ya implementado
        }

        public override bool Equals(object other)
        {
            if (other is not PathfindingNode) return false;
            return this.position == ((PathfindingNode)other).position;
        }

        public override int GetHashCode()
        {
            return position.GetHashCode();
        }
    }
}
