using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator1x1 : RoomGenerator
{
    public GameObject wallPrefab;
    public GameObject doorPrefab;

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
            InstantiateDirectionalAsset(doorPrefab, ThisRoom, doorDirection);
            Adjacent.Remove(other);
        }

        foreach(Vector2Int wall in Adjacent)
        {
            Vector2 doorDirection = ((Vector2)(wall - ThisRoom)).normalized;
            InstantiateDirectionalAsset(wallPrefab, ThisRoom, doorDirection);
        }
    }
}
