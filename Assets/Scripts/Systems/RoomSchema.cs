using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class RoomSchema
{
    public bool isValidSchema { get; private set; }
    public RoomGenerator schemaGenerator { get; private set; }

    public Vector2Int origin { get; private set; }
    public HashSet<Vector2Int> roomTiles { get; private set; }
    public HashSet<Vector2Int> adjacentRooms { get; private set; }

    public int doorAmount { get; private set; }

    public RSHorOrientation horizontalOrientation = RSHorOrientation.None;
    public RSVerOrientation verticalOrientation = RSVerOrientation.None;

    // Empty constructor
    public RoomSchema(Vector2Int origin)
    {
        this.origin = origin;

        schemaGenerator = null;
        isValidSchema = false;
    }

    // Only using tiles constructor
    public RoomSchema(Vector2Int origin, IEnumerable<Vector2Int> usingRooms)
    {
        this.origin = origin;

        schemaGenerator = null;
        isValidSchema = false;

        roomTiles = new(usingRooms);
    }

    // Using and adjacent constructor
    public RoomSchema(Vector2Int origin, IEnumerable<Vector2Int> usingRooms, IEnumerable<Vector2Int> adjacentRooms)
    {
        this.origin = origin;

        schemaGenerator = null;
        isValidSchema = false;

        roomTiles = new(usingRooms);
        this.adjacentRooms = new(adjacentRooms);
    }

    public void AutoGenerateAdjacent(IEnumerable<Vector2Int> map)
    {
        adjacentRooms = new(GetAdjacent(roomTiles, map));
    }

    public void SetUsingTiles(IEnumerable<Vector2Int> positions)
    {
        roomTiles = new(positions);
    }

    public void SetDoorAmount(int amount)
    {
        doorAmount = amount;
    }

    public void MarkAsReady(RoomGenerator signature)
    {
        if (roomTiles == null) return;
        if (adjacentRooms == null) return;

        if (doorAmount == 0) doorAmount = 1;

        isValidSchema = true;
        schemaGenerator = signature;
    }

    public IEnumerable<Vector2Int> BakeWalls()
    {
        HashSet<Vector2Int> walls = new();
        HashSet<Vector2Int> bin = new();

        foreach (Vector2Int position in roomTiles)
        {
            walls.Add(position + Vector2Int.up);
            walls.Add(position + Vector2Int.down);
            walls.Add(position + Vector2Int.left);
            walls.Add(position + Vector2Int.right);
        }

        foreach(Vector2Int wall in walls)
        {
            if (adjacentRooms.Contains(wall) || roomTiles.Contains(wall))
                bin.Add(wall);
        }

        walls.ExceptWith(bin);

        return walls;
    }

    public static bool CheckMapAvailability(IEnumerable<Vector2Int> positions, IEnumerable<Vector2Int> map)
    {
        HashSet<Vector2Int> mapSet = new(map);

        foreach(Vector2Int position in positions)
        {
            if (!mapSet.Contains(position))
                return false;
        }

        return true;
    }

    public static IEnumerable<Vector2Int> GetAdjacent(IEnumerable<Vector2Int> positions, IEnumerable<Vector2Int> map)
    {
        HashSet<Vector2Int> positionSet = new(positions);
        HashSet<Vector2Int> mapSet = new(map);

        HashSet<Vector2Int> adjacent = new();
        HashSet<Vector2Int> bin = new();

        foreach (Vector2Int position in positions)
        {
            adjacent.Add(position + Vector2Int.up);
            adjacent.Add(position + Vector2Int.down);
            adjacent.Add(position + Vector2Int.left);
            adjacent.Add(position + Vector2Int.right);
        }

        foreach (Vector2Int adj in adjacent)
        {
            if (!mapSet.Contains(adj) || positionSet.Contains(adj)) // If a position is not part of the map OR the position is part of the room, remove it
                bin.Add(adj);
        }

        adjacent.ExceptWith(bin);

        return adjacent;
    }
}

public enum RSVerOrientation
{
    None = 0,
    North = 1,
    South = -1
}

public enum RSHorOrientation
{
    None = 0,
    West = -1,
    East = 1
}