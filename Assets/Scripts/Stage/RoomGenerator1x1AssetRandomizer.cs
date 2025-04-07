using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator1x1AssetRandomizer : RoomGenerator
{
    public GameObject[] wallPrefab;
    public GameObject[] doorPrefab;
    public GameObject[] decorationPrefab;
    public float DecorationProbability = 0.4f;

    public override int Complexity { get; } = 1;

    public override bool Evaluate(Vector2Int origin, List<Vector2Int> map, out Vector2Int[] consumed)
    {
        Vector2Int[] val = { origin };
        consumed = val;

        return true;
    }

    protected override void Generate()
    {
        Vector2Int ThisRoom = roomTiles[0];
        List<Vector2Int> Adjacent = GetAdjacent(ThisRoom);

        foreach(Vector2Int other in otherRooms)
        {
            Vector2 doorDirection = ((Vector2)(other - ThisRoom)).normalized;
            InstantiateDirectionalAsset(doorPrefab[Random.Range(0, doorPrefab.Length)], ThisRoom, doorDirection);
            Adjacent.Remove(other);
        }

        // Si no hay paredes, generemos una decoración
        if (Adjacent.Count == 0 && Random.value < DecorationProbability)
            InstantiateAsset(decorationPrefab[Random.Range(0, decorationPrefab.Length)], ThisRoom);

        foreach(Vector2Int wall in Adjacent)
        {
            Vector2 wallDirection = ((Vector2)(wall - ThisRoom)).normalized;
            InstantiateDirectionalAsset(wallPrefab[Random.Range(0, wallPrefab.Length)], ThisRoom, wallDirection);
        }
    }
}
