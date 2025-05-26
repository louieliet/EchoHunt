using UnityEngine;

public class dancezombie : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<Animator>().SetTrigger("Capture");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
