using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageBuilder : MonoBehaviour
{
    public Transform start;
    public Transform end;
    public Transform walls;

    private LineRenderer line;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        StartCoroutine(coroutine());
    }

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
    }
    /*
    private void Fractal(Vector3 origin, float direction, int depth)
    {
        if (depth > DivisionAmount) return;

        // Instantiate random room
        GameObject current = Instantiate(StagePrefabs[Random.Range(0, StagePrefabs.Length)], origin, Quaternion.identity);

        // Get available plugs
        Transform conncetions = current.transform.Find("Plugs");

        // Get the plug the new room is sprawling from
        Transform originNode = conncetions.transform.GetChild(Random.Range(0, conncetions.transform.childCount));

        // Set transform
        current.transform.rotation = Quaternion.Euler(0, direction - originNode.eulerAngles.y, 0);
        current.transform.Rotate(0, 180, 0);
        current.transform.position = origin - current.transform.rotation * originNode.localPosition;

        // Cancel the plug that was used to first set the room
        originNode.gameObject.SetActive(false);

        // Depth's been added by 1
        depth++;

        // For each plug that wasn't used, add another room.
        foreach (Transform connectionNode in conncetions)
        {
            if (connectionNode.gameObject.activeSelf)
                Fractal(connectionNode.position, connectionNode.eulerAngles.y, depth);
        }
    }
    */
}
