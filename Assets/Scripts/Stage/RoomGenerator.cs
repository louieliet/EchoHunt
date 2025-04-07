using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class RoomGenerator : MonoBehaviour
{
    protected List<Vector2Int> otherRooms = new();
    protected List<Vector2Int> roomTiles = new();

    public abstract int Complexity { get; }

    void Start()
    {
        Generate();
    }

    protected abstract void Generate();

    public abstract bool Evaluate(Vector2Int origin, List<Vector2Int> map, out Vector2Int[] consumed);

    public void SetGenerator(Vector2Int[] rooms, List<Vector2Int> map)
    {
        roomTiles.AddRange(rooms);

        HashSet<Vector2Int> filteredMap = new();    // Usamos hashset por que supuestamente es mas rapido

        foreach (Vector2Int tile in rooms)
        {
            foreach(Vector2Int adjacent in map)
            {
                if (Vector2Int.Distance(tile, adjacent) == 1 && !rooms.Contains(adjacent) && !filteredMap.Contains(adjacent))
                    filteredMap.Add(adjacent);
            }
        }

        otherRooms = filteredMap.ToList();
    }

    protected List<Vector2Int> GetAdjacent(Vector2Int position)
    {
        List<Vector2Int> adjacent = new();

        adjacent.Add(position + Vector2Int.up);
        adjacent.Add(position + Vector2Int.down);
        adjacent.Add(position + Vector2Int.left);
        adjacent.Add(position + Vector2Int.right);

        return adjacent;
    }

    protected GameObject InstantiateDirectionalAsset(GameObject prefab, Vector2 pos, Vector2 dir)
    {
        Vector3 realPosition = new Vector3(pos.x, 0, pos.y);
        Vector3 realDirection = new Vector3(dir.x, 0, dir.y);

        GameObject instantiated = Instantiate(prefab, realPosition, Quaternion.identity);

        instantiated.transform.forward = realDirection;
        instantiated.transform.SetParent(transform);

        return instantiated;
    }

    protected GameObject InstantiateAsset(GameObject prefab, Vector2 pos)
    {
        Vector3 realPosition = new Vector3(pos.x, 0, pos.y);

        GameObject instantiated = Instantiate(prefab, realPosition, Quaternion.identity);

        instantiated.transform.SetParent(transform);

        return instantiated;
    }
}
