using UnityEngine;

public class TimeScaleControl : MonoBehaviour
{
    public float timeScale;
    void Start()
    {
        timeScale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = timeScale;
        
    }
}
