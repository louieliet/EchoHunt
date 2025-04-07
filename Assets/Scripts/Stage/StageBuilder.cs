using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StageBuilder : MonoBehaviour
{
    public RoomGenerator[] RoomGenerators;

    [SerializeField] private Vector2Int StageDimensions = new Vector2Int(10, 10);
    [SerializeField] private int LabCount = 5;
    [SerializeField] private int NoiseCount = 15;
    [SerializeField] private float LevelScale = 3f;
    private List<Vector2Int> StageBlob = new(); // El área generada a partir de la cual generaremos cuartos.

    void Start()
    {
        GenerateStageBlob();
        GenerateLevelGeometry();
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
                    Debug.Log("Failed to create route");
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

    private void GenerateLevelGeometry()
    {   
        // Ordenar por complejidad de los cuartos
        System.Array.Sort(RoomGenerators, (a, b) => b.Complexity.CompareTo(a.Complexity));
        HashSet<Vector2Int> PendingRooms = new(StageBlob);

        while(PendingRooms.Count > 0)
        {
            Vector2Int room = PendingRooms.First();
            foreach (RoomGenerator gen in RoomGenerators)
            {
                Vector2Int[] consumed;
                if(gen.Evaluate(room, PendingRooms.ToList(), out consumed))
                {
                    Vector3 Position = new Vector3(room.x, 0, room.y);

                    RoomGenerator newRoom = (RoomGenerator)Instantiate(gen, Position, Quaternion.identity);
                    newRoom.SetGenerator(consumed, StageBlob);
                    newRoom.transform.SetParent(transform);

                    foreach(Vector2Int c in consumed)
                    {
                        PendingRooms.Remove(c);
                    }
                }
            }
        }

        StartCoroutine(ScaleCoroutine());
    }

    IEnumerator ScaleCoroutine()
    {
        yield return null;
        transform.localScale = Vector3.one * LevelScale;
    }

    /*
    IEnumerator coroutine()
    {
        yield return new WaitForSeconds(1f);
        List<Vector2Int> wallsVec = new();
        foreach(Transform w in walls)
        {
            wallsVec.Add(new Vector2Int(Mathf.RoundToInt(w.position.x), Mathf.RoundToInt(w.position.z)));
        }
        List<Vector2Int> route = Pathfinding.FindRoute(wallsVec, new Vector2Int(Mathf.RoundToInt(start.position.x), Mathf.RoundToInt(start.position.z)), new Vector2Int(Mathf.RoundToInt(end.position.x), Mathf.RoundToInt(end.position.z)));
        if (route != null)
        {
            Vector3[] realpos = new Vector3[route.Count];
            for (int i = 0; i < route.Count; i++)
            {
                realpos[i].x = route[i].x;
                realpos[i].z = route[i].y;
            }
            line.positionCount = route.Count;
            line.SetPositions(realpos);
        }

        StartCoroutine(coroutine());
    }*/
}
