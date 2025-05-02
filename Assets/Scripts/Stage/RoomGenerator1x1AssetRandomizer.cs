using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomGenerator1x1AssetRandomizer : RoomGenerator
{
    public GameObject[] wallPrefab;
    public GameObject[] doorPrefab;
    public GameObject[] decorationPrefab;
    public float DecorationProbability = 0.4f;

    public override int Complexity { get; } = 1;

    public override RoomSchema Evaluate(Vector2Int origin, HashSet<Vector2Int> freeSpots, HashSet<Vector2Int> map)
    {
        Vector2Int[] consumed = { origin };

        RoomSchema newSchema = new RoomSchema(origin, consumed);
        newSchema.AutoGenerateAdjacent(map);

        newSchema.MarkAsReady(this);

        return newSchema;
    }

    protected override void Generate()
    {
        Vector2Int ThisRoom = schema.roomTiles.First();
        List<Vector2Int> wallList = new(schema.BakeWalls());

        foreach (Vector2Int other in schema.adjacentRooms)
        {
            Vector2 doorDirection = ((Vector2)(other - ThisRoom)).normalized;
            InstantiateDirectionalAsset(doorPrefab[Random.Range(0, doorPrefab.Length)], ThisRoom, doorDirection);
        }

        // Si no hay paredes, generemos una decoración
        if (schema.adjacentRooms.Count == 0 && Random.value < DecorationProbability)
            InstantiateAsset(decorationPrefab[Random.Range(0, decorationPrefab.Length)], ThisRoom);

        foreach(Vector2Int wall in wallList)
        {
            Vector2 wallDirection = ((Vector2)(wall - ThisRoom)).normalized;
            InstantiateDirectionalAsset(wallPrefab[Random.Range(0, wallPrefab.Length)], ThisRoom, wallDirection);
        }
    }
}
