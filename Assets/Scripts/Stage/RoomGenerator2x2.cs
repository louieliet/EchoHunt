using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomGenerator2x2 : RoomGenerator
{
    public GameObject[] wallPrefab;
    public GameObject[] doorPrefab;

    public override int Complexity { get; } = 2;

    public override RoomSchema Evaluate(Vector2Int origin, HashSet<Vector2Int> freeSpots, HashSet<Vector2Int> map)
    {
        // Esquemas que vamos a sumar a nuestro origen para probar validez
        Vector2Int[][] offsets = new Vector2Int[][]
        {
            // Top-Left
            new Vector2Int[] { Vector2Int.zero, Vector2Int.up, Vector2Int.left, Vector2Int.up + Vector2Int.left },
            // Top-Right
            new Vector2Int[] { Vector2Int.zero, Vector2Int.up, Vector2Int.right, Vector2Int.up + Vector2Int.right },
            // Bottom-Left
            new Vector2Int[] { Vector2Int.zero, Vector2Int.down, Vector2Int.left, Vector2Int.down + Vector2Int.left },
            // Bottom-Right
            new Vector2Int[] { Vector2Int.zero, Vector2Int.down, Vector2Int.right, Vector2Int.down + Vector2Int.right }
        };

        // Intentar cada uno de los esquemas con los offsets
        foreach (var offsetSet in offsets)
        {
            Vector2Int[] positions = offsetSet.Select(offset => origin + offset).ToArray();
            if (RoomSchema.CheckMapAvailability(positions, freeSpots))
            {
                RoomSchema schema = new RoomSchema(origin, positions);
                schema.AutoGenerateAdjacent(map);
                schema.MarkAsReady(this);
                return schema;
            }
        }

        return new RoomSchema(origin);
    }

    protected override void Generate()
    {
        List<Vector2Int> wallList = new(schema.BakeWalls());
        Vector2 CenteredPosition = Vector2.zero;

        foreach (Vector2Int roomPart in schema.roomTiles)
        {
            CenteredPosition += roomPart;
        }

        CenteredPosition /= schema.roomTiles.Count;
        transform.localPosition = new Vector3(CenteredPosition.x, 0, CenteredPosition.y);

        // For each room fragment
        foreach (Vector2Int roomPart in schema.roomTiles)
        {
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
