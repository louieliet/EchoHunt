using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Restart()
    {
        var clock = FindObjectOfType<GameClock>();
        if (clock != null)
            clock.ResetClock();
        SceneManager.LoadScene(1);
    }
}
