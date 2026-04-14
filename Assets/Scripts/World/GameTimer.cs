using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance { get; private set; }

    private float startTime;
    private bool isRunning;
    public float FinalTime { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        startTime = Time.time;
        isRunning = true;
        FinalTime = 0f;
    }

    public void StopTimer()
    {
        if (!isRunning)
        {
            return;
        }

        FinalTime = Time.time - startTime;
        isRunning = false;
    }

    public float GetCurrentTime()
    {
        if (isRunning)
        {
            return Time.time - startTime;
        }

        return FinalTime;
    }
}