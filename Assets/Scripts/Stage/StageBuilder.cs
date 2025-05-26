using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;

public class StageBuilder : MonoBehaviour
{
    public System.Action OnLevelBuild;

    public RoomGenerator[] RoomGenerators;

    public RoomGenerator LabGenerator;

    public static StageBuilder instance;

    [SerializeField] private Vector2Int StageDimensions = new Vector2Int(10, 10);
    [SerializeField] private int LabCount = 2;
    [SerializeField] private int CornerCount = 7;
    [SerializeField] private int NoiseCount = 15;
    [SerializeField] private float LevelScale = 3f;

    private HashSet<Vector2Int> StageBlob = new(); // El área generada a partir de la cual generaremos cuartos.
    private HashSet<RoomSchema> StageSchema = new();   // El lugar donde generaremos y guardaremos nuestros cuartos generados

    private List<Vector2Int> RandomSafePositions = new();
    private List<Vector2Int> RandomMazePositions = new();

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
        List<Vector2Int> CornerRooms = new();
        List<Vector2Int> Walls = new();

        HashSet<Vector2Int> LabGrid = new();

        // Fill a grid with valid spaces for lab colocation
        for (int x = 0; x < StageDimensions.x; x++)
        {
            for (int y = 0; y < StageDimensions.y; y++)
            {
                LabGrid.Add(new Vector2Int(x, y));
            }
        }

        HashSet<Vector2Int> LabSpaces = new(LabGrid);

        // Colocaremos generadores de laboratorios
        for (int i = 0; i < LabCount; i++)
        {
            RoomSchema sch = null;
            int iter = 0;
            do
            {
                // Obtener posicion aleatoria dentro del grid
                Vector2Int current = new Vector2Int(Random.Range(0, StageDimensions.x), Random.Range(0, StageDimensions.y));
                sch = LabGenerator.Evaluate(current, LabSpaces, LabGrid);
                iter++;
            }
            while (!sch.isValidSchema && iter < 10);

            if (sch.isValidSchema)
            {
                LabSpaces.ExceptWith(sch.roomTiles);
                StageSchema.Add(sch);

                CornerRooms.Add(sch.origin);
                StageBlob.UnionWith(sch.roomTiles);
            }            
        }

        // Añadir generadores de conexiones
        for(int i = 0; i < CornerCount; i++)
        {
            Vector2Int current = new Vector2Int(Random.Range(0, StageDimensions.x), Random.Range(0, StageDimensions.y));

            CornerRooms.Add(current);
            StageBlob.Add(current);
        }

        // Gusano
        Vector2Int wormGenerator = new Vector2Int(Random.Range(0, StageDimensions.x), Random.Range(0, StageDimensions.y));  // Posicion del gusano
        Vector2Int[] wormDirections = { Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left };    // Movimiento del gusano

        // Checar adyacencias
        Vector2Int[] noiseContiguity = { Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left, Vector2Int.up + Vector2Int.right, Vector2Int.up + Vector2Int.left, Vector2Int.down + Vector2Int.right, Vector2Int.down + Vector2Int.left };

        // Generar gusano que distorsiona puertas
        for (int i = 0; i < NoiseCount; i++)
        {
            // Checar si hay adyacencias en el ruido
            int adjacentNoise = 0;
            foreach(Vector2Int dir in noiseContiguity)
            {
                if (Walls.Contains(wormGenerator + dir))
                    adjacentNoise++;
            }

            // Si hay pocas adyacencias, anadir pared
            if(adjacentNoise <= 1)
                Walls.Add(wormGenerator);

            // Mover gusano
            wormGenerator += wormDirections[Random.Range(0, wormDirections.Length)];

            // Si el gusano se sale del mapa, resetear posicion
            if(wormGenerator.x < 0 || wormGenerator.x >= StageDimensions.x || wormGenerator.y < 0 || wormGenerator.y >= StageDimensions.y)
                wormGenerator = new Vector2Int(Random.Range(0, StageDimensions.x), Random.Range(0, StageDimensions.y));
        }

        // Connect all lab rooms among themselves
        for (int i = 0; i < CornerRooms.Count; i++)
        {
            for (int j = i + 1; j < CornerRooms.Count; j++)
            {
                List<Vector2Int> route = Pathfinding.FindRoute(Walls, CornerRooms[i], CornerRooms[j]);
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

        // Actualizar los cuartos de laboratorio de acuerdo al nuevo StageBlob
        foreach(RoomSchema room in StageSchema)
        {
            RandomSafePositions.AddRange(room.roomTiles);
            RandomSafePositions.Remove(room.origin);
            room.AutoGenerateAdjacent(StageBlob);
        }
    }

    private void GenerateStageSchema()
    {   
        HashSet<Vector2Int> PendingRooms = new(StageBlob);

        // Quitar de los cuartos pendientes los cuartos que ya han sido usados por los laboratorios
        foreach(RoomSchema prefabricated in StageSchema)
        {
            PendingRooms.ExceptWith(prefabricated.roomTiles);
        }

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
                    RandomMazePositions.AddRange(testSchema.roomTiles);

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

    public Vector3 GetRandomMazePosition()
    {
        int Index = Random.Range(0, RandomMazePositions.Count);
        Vector2Int RandomSpot = RandomMazePositions[Index];
        Vector3 RealPosition = new Vector3(RandomSpot.x, 0, RandomSpot.y) * LevelScale;

        RandomMazePositions.RemoveAt(Index);

        return RealPosition;
    }

    public Vector3 GetRandomSafePosition()
    {
        int Index = Random.Range(0, RandomSafePositions.Count);
        Vector2Int RandomSpot = RandomSafePositions[Index];
        Vector3 RealPosition = new Vector3(RandomSpot.x, 0, RandomSpot.y) * LevelScale;

        RandomSafePositions.RemoveAt(Index);

        return RealPosition;
    }

    IEnumerator ScaleCoroutine()
    {
        yield return new WaitForEndOfFrame();

        transform.localScale = Vector3.one * LevelScale;

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
