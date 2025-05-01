using UnityEngine;

public class GameOverBackgroudn : MonoBehaviour
{
    void Update()
    {
        transform.localScale = new Vector3(Random.value < 0.5f ? -1 : 1, Random.value < 0.5f ? -1 : 1, 1);
    }
}
