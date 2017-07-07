using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class TestRepeat : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        float a = -3.14159f;
        float b = 0;

        Stopwatch watch = Stopwatch.StartNew();
        for (int i = 0; i < 1000000; ++i)
        {
            b = a % 1f;
        }
        watch.Stop();
        Debug.Log(b);
        Debug.Log(watch.ElapsedMilliseconds);

        watch.Reset();
        watch.Start();
        for (int i = 0; i < 1000000; ++i)
        {
            b = Mathf.Repeat(a, 1f);
        }
        watch.Stop();
        Debug.Log(b);
        Debug.Log(watch.ElapsedMilliseconds);

        watch.Reset();
        watch.Start();
        for (int i = 0; i < 1000000; ++i)
        {
            b = a % 1f;
        }
        watch.Stop();
        Debug.Log(b);
        Debug.Log(watch.ElapsedMilliseconds);
    }
}
