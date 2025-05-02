using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;

public class StageBuilder : MonoBehaviour
{
    public System.Action OnLevelBuild;

    public RoomGenerator[] RoomGenerators;

    public static StageBuilder instance;

    [SerializeField] private Vector2Int StageDimensions = new Vector2Int(10, 10);
    [SerializeField] private int LabCount = 5;
    [SerializeField] private int NoiseCount = 15;
    [SerializeField] private float LevelScale = 3f;

    private HashSet<Vector2Int> StageBlob = new(); // El área generada a partir de la cual generaremos cuartos.
    private HashSet<RoomSchema> StageSchema = new();   // El lugar donde generaremos y guardaremos nuestros cuartos generados

    private List<Vector2Int> RandomPositions;

    private NavMeshSurface navMeshSurface;

    void Start()
    {
        instance = this;
        navMeshSurface = GetComponent<NavMeshSurface>();

        // Ordenar por complejidad de los cuartos
        System.Array.Sort(RoomGenerators, (a, b) => b.Complexity.CompareTo(a.Complexity));

        GenerateStageBlob();
        GenerateStageSchema();
        ResolveDoorPlacement();
        GenerateStageGeometry();
    }

    private void GenerateStageBlob()
    {
        List<Vector2Int> BaseRooms = new();
        List<Vector2Int> Walls = new();

        // Para esta version de prueba, generar áreas al azar
        for (int i = 0; i < LabCount; i++)
        {
            Vector2Int current = new Vector2Int(Random.Range(0, StageDimensions.x), Random.Range(0, StageDimensions.y));
            BaseRooms.Add(current);
            StageBlob.Add(current);
        }

        // Para esta version de prueba, generar paredes al azar
        for (int i = 0; i < NoiseCount; i++)
        {
            Vector2Int current = new Vector2Int(Random.Range(0, StageDimensions.x), Random.Range(0, StageDimensions.y));
            Walls.Add(current);
        }

        // Connect all lab rooms among themselves
        for (int i = 0; i < BaseRooms.Count; i++)
        {
            for (int j = i + 1; j < BaseRooms.Count; j++)
            {
                List<Vector2Int> route = Pathfinding.FindRoute(Walls, BaseRooms[i], BaseRooms[j]);
                if (route == null)   // Si la conexion fue creada, añadir todos los cuartos al blob
                {
                    continue;
                }
                foreach (Vector2Int piece in route)
                {
                    if (!StageBlob.Contains(piece))
                        StageBlob.Add(piece);
                }
                
            }
        }
    }

    private void GenerateStageSchema()
    {   
        HashSet<Vector2Int> PendingRooms = new(StageBlob);

        while(PendingRooms.Count > 0)
        {
            Vector2Int room = PendingRooms.First();
            foreach (RoomGenerator gen in RoomGenerators)
            {
                RoomSchema testSchema = gen.Evaluate(room, PendingRooms, StageBlob);
                if(testSchema.isValidSchema)
                {
                    StageSchema.Add(testSchema);
                    PendingRooms.ExceptWith(testSchema.roomTiles);  // Delete consumed tiles

                    break;
                }
            }
        }
    }

    private void ResolveDoorPlacement()
    {
        List<RoomSchema> PendingSchemaList = new(StageSchema);

        for(int a = 0; a < PendingSchemaList.Count; a++)
        {
            RoomSchema SchemaA = PendingSchemaList[a];
            for(int b = a + 1; b < PendingSchemaList.Count; b++)
            {
                RoomSchema SchemaB = PendingSchemaList[b];

                if (AreRoomsAdjacent(SchemaA, SchemaB))
                    DiscardExcessDoors(SchemaA, SchemaB);
            }
        }
    }

    private void GenerateStageGeometry()
    {
        Queue<RoomSchema> PendingRooms = new(StageSchema);

        foreach (RoomSchema room in PendingRooms)
        {
            Vector3 Position = new Vector3(room.origin.x, 0, room.origin.y);
            RoomGenerator newRoom = (RoomGenerator)Instantiate(room.schemaGenerator, Position, Quaternion.identity);
            newRoom.SetGenerator(room);
            newRoom.transform.SetParent(transform);
        }

        StartCoroutine(ScaleCoroutine());
    }

    public Vector3 GetRandomPositionAtMaze()
    {
        int Index = Random.Range(0, RandomPositions.Count);
        Vector2Int RandomSpot = RandomPositions[Index];
        Vector3 RealPosition = new Vector3(RandomSpot.x, 0, RandomSpot.y) * LevelScale;

        RandomPositions.RemoveAt(Index);

        return RealPosition;
    }

    IEnumerator ScaleCoroutine()
    {
        yield return new WaitForEndOfFrame();

        transform.localScale = Vector3.one * LevelScale;

        RandomPositions = new(StageBlob);

        navMeshSurface.BuildNavMesh();
        OnLevelBuild?.Invoke();

        GameManager.StartGame();
    }

    private bool AreRoomsAdjacent(RoomSchema a, RoomSchema b)
    {
        foreach (Vector2Int adj in a.adjacentRooms)
        {
            // If roomtiles of B contains an element of A adjacency, we can say both rooms are adjacent.
            if (b.roomTiles.Contains(adj))
                return true;
        }

        return false;
    }

    private void DiscardExcessDoors(RoomSchema a, RoomSchema b)
    {
        // Values for A and values for B
        List<(Vector2Int, Vector2Int)> discardable = new();

        // Add all shared tiles
        foreach (Vector2Int room in a.roomTiles)
        {
            foreach(Vector2Int adj in a.adjacentRooms)
            {
                if (Vector2Int.Distance(room, adj) != 1) continue;

                if (b.roomTiles.Contains(adj))
                    discardable.Add((room, adj));
            }
        }

        // Get preferred door amount
        int doorAmount = Mathf.Max(a.doorAmount, b.doorAmount);

        // Get preferred direction
        RSHorOrientation horizontalOrientation = a.horizontalOrientation != RSHorOrientation.None ? a.horizontalOrientation : b.horizontalOrientation;
        RSVerOrientation verticalOrientation = a.verticalOrientation != RSVerOrientation.None ? a.verticalOrientation : b.verticalOrientation;

        // If no preferred direction was provided, set defaults
        if (horizontalOrientation == RSHorOrientation.None)
            horizontalOrientation = RSHorOrientation.West;
        if (verticalOrientation == RSVerOrientation.None)
            verticalOrientation = RSVerOrientation.North;

        // Sort items based on preferred direction
        discardable = discardable
            .OrderBy(pos => horizontalOrientation == RSHorOrientation.West ? pos.Item1.x : -pos.Item1.x)
            .ThenBy(pos => verticalOrientation == RSVerOrientation.North ? -pos.Item1.y : pos.Item1.y)
            .ToList();

        // Protect the selected doors
        int toProtect = Mathf.Min(doorAmount, discardable.Count);
        discardable.RemoveRange(0, toProtect);

        // Throw away unnecessary rooms and add as walls
        a.adjacentRooms.ExceptWith(discardable.Select(x => x.Item2));
        b.adjacentRooms.ExceptWith(discardable.Select(x => x.Item1));
    }
}
