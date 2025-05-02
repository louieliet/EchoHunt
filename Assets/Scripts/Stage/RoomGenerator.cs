using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class RoomGenerator : MonoBehaviour
{
    protected RoomSchema schema;

    public abstract int Complexity { get; }

    void Start()
    {
        Generate();
    }

    protected abstract void Generate();

    public abstract RoomSchema Evaluate(Vector2Int origin, HashSet<Vector2Int> freeSpots, HashSet<Vector2Int> map);

    public void SetGenerator(RoomSchema schema)
    {
        this.schema = schema;
        /*
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

        otherRooms = filteredMap.ToList();*/
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
