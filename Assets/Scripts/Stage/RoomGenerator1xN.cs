using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class RoomGenerator1xN : RoomGenerator
{
    public GameObject[] wallPrefab;
    public GameObject[] doorPrefab;
    public GameObject floorPrefab;

    public override int Complexity { get; } = 2;

    public override RoomSchema Evaluate(Vector2Int origin, HashSet<Vector2Int> freeSpots, HashSet<Vector2Int> map)
    {
        // direcciones que vamos a sumar a nuestro origen para probar validez
        Vector2Int[] rayDirections = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        int BestDirectionLength = 0;
        Vector2Int BestDirection = Vector2Int.zero;

        // Intentar cada uno de los direcciones con los offsets
        foreach (var direction in rayDirections)
        {
            Vector2Int testingTile = origin;
            int testingLength = 0;

            while (freeSpots.Contains(testingTile))
            {
                testingLength++;
                testingTile += direction;
            }

            if(testingLength > BestDirectionLength)
            {
                BestDirectionLength = testingLength;
                BestDirection = direction;
            }
        }

        if(BestDirectionLength <= 1)
            return new RoomSchema(origin);

        Vector2Int[] tiles = new Vector2Int[BestDirectionLength + 1];
        Vector2Int savingTile = origin;
        tiles[0] = origin;

        for (int i = 0; i < BestDirectionLength; i++)
        {
            tiles[i+1] = savingTile;
            savingTile += BestDirection;
        }

        RoomSchema schema = new RoomSchema(origin, tiles);

        schema.AutoGenerateAdjacent(map);
        schema.MarkAsReady(this);
        return schema;
    }

    protected override void Generate()
    {
        List<Vector2Int> wallList = new(schema.BakeWalls());

        // For each room fragment
        foreach (Vector2Int roomPart in schema.roomTiles)
        {
            InstantiateAsset(floorPrefab, roomPart);

            // Check all adjacents
            foreach (Vector2Int other in schema.adjacentRooms)
            {
                if (Vector2Int.Distance(roomPart, other) != 1) continue;    // If adjacent distance isn't 1, then it's not adjacent to this tile

                Vector2 doorDirection = ((Vector2)(other - roomPart)).normalized;
                InstantiateDirectionalAsset(doorPrefab[Random.Range(0, doorPrefab.Length)], roomPart, doorDirection);
            }

            foreach (Vector2Int wall in wallList)
            {
                if (Vector2Int.Distance(roomPart, wall) != 1) continue;    // If adjacent distance isn't 1, then it's not adjacent to this tile

                Vector2 doorDirection = ((Vector2)(wall - roomPart)).normalized;
                InstantiateDirectionalAsset(wallPrefab[Random.Range(0, wallPrefab.Length)], roomPart, doorDirection);
            }
        }
    }
}
