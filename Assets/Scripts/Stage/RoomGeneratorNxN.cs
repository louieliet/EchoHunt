using System.Collections.Generic;
using UnityEngine;

public class RoomGeneratorNxN : RoomGenerator
{
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject doorPrefab;

    public override int Complexity { get; } = 3;

    public override bool Evaluate(Vector2Int origin, List<Vector2Int> map, out Vector2Int[] consumed)
    {
        Debug.LogError("Doesnt work");
        Vector2Int[] corners = { origin, origin, origin, origin };
        Vector2Int[] steps = { Vector2Int.up + Vector2Int.right , Vector2Int.up + Vector2Int.left , Vector2Int.down + Vector2Int.right , Vector2Int.down + Vector2Int.left };
        
        for(int i = 0; i < steps.Length; i++)
        {
            bool WasExpansionSuccesful;
            int Iterations = 0;
            do {
                Iterations++;
                if(Iterations > 1000)
                {
                    Debug.Log("Square evaluation is broken");
                    break;
                }

                Debug.Log("Moving corner > " + corners[i] + " by steps > " + steps[i]);
                corners[i] += steps[i];
                WasExpansionSuccesful = SquareEvaluation(origin, corners[i], map, null);
                if (!WasExpansionSuccesful)
                {
                    Debug.Log("Expansion failed, roll back change");
                    corners[i] -= steps[i];
                }

            } while (WasExpansionSuccesful);
        }

        Vector2Int TopRightCorner = new Vector2Int(Mathf.Max(corners[0].x, corners[2].x), Mathf.Max(corners[0].y, corners[1].y));
        Vector2Int BottomLeftCorner = new Vector2Int(Mathf.Min(corners[1].x, corners[3].x), Mathf.Min(corners[2].y, corners[3].y));

        if (Vector2Int.Distance(TopRightCorner, BottomLeftCorner) <= 1)
        {
            Debug.Log("Cannot place square");
            consumed = null;
            return false;
        }

        List<Vector2Int> values = new();
        SquareEvaluation(TopRightCorner, BottomLeftCorner, map, values);
        consumed = values.ToArray();

        Debug.Log("Can yes place square");
        return true;
    }

    private bool SquareEvaluation(Vector2Int bottomCorner, Vector2Int topCorner, List<Vector2Int> map, List<Vector2Int> squared)
    {
        for (int y = bottomCorner.y; y < topCorner.y; y++)
        {
            for (int x = bottomCorner.x; x < topCorner.x; x++)
            {
                Vector2Int checkPosition = new Vector2Int(x, y);
                if (!map.Contains(checkPosition))
                {
                    return false; // No cabe si alguna casilla está ocupada
                }

                if(squared != null)
                    squared.Add(checkPosition);
            }
        }
        return true;
    }

    protected override void Generate()
    {
        Debug.LogError("Doesnt work");
        Debug.Log("NxN contains > " + roomTiles.Count + " elements");
        foreach (Vector2Int roomTile in roomTiles)
        {
            InstantiateAsset(floorPrefab, roomTile);
            // Obtenemos las posiciones adyacentes a esta losa
            List<Vector2Int> Adjacent = GetAdjacent(roomTile);

            // Iteramos sobre otras habitaciones adyacentes para colocar las puertas
            foreach (Vector2Int other in otherRooms)
            {
                // Calculamos la dirección de la puerta entre la losa actual y la habitación adyacente
                Vector2 doorDirection = ((Vector2)(other - roomTile)).normalized;

                // Si la losa y la habitación están adyacentes, colocamos una puerta
                InstantiateDirectionalAsset(doorPrefab, roomTile, doorDirection);
                Adjacent.Remove(other);  // Ya colocamos la puerta, la eliminamos de la lista
            }

            // Colocamos paredes solo en las losas que estén en los bordes del cuarto
            // Solo las losas adyacentes que no tienen puertas serán paredes
            foreach (Vector2Int wall in Adjacent)
            {
                // Calculamos la dirección de la pared
                Vector2 wallDirection = ((Vector2)(wall - roomTile)).normalized;

                // Instanciamos una pared en la dirección correspondiente
                InstantiateDirectionalAsset(wallPrefab, roomTile, wallDirection);
            }
        }

    }
}
