using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomGenerator3x3 : RoomGenerator
{
    public GameObject[] wallPrefab;
    public GameObject[] doorPrefab;

    public override int Complexity { get; } = 3;

    public override RoomSchema Evaluate(Vector2Int origin, HashSet<Vector2Int> freeSpots, HashSet<Vector2Int> map)
    {
        Vector2Int[] tiles = { origin, 
            // Adjacent 4
            origin + Vector2Int.up,
            origin + Vector2Int.down,
            origin + Vector2Int.left,
            origin + Vector2Int.right,
            // Adjacent D
            origin + Vector2Int.up + Vector2Int.left,
            origin + Vector2Int.up + Vector2Int.right,
            origin + Vector2Int.down + Vector2Int.left,
            origin + Vector2Int.down + Vector2Int.right,
        };

        RoomSchema newSchema = new RoomSchema(origin, tiles);
        newSchema.AutoGenerateAdjacent(map);

        if (!RoomSchema.CheckMapAvailability(tiles, freeSpots))
            return newSchema;

        newSchema.MarkAsReady(this);

        return newSchema;
    }

    protected override void Generate()
    {
        List<Vector2Int> wallList = new(schema.BakeWalls());

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
