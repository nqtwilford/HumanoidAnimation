using UnityEngine;
using UnityEditor;

public class ParabolaTest : EditorWindow
{
    const float GRAVITY = -9.8f;

    GameObject startPos;
    GameObject endPos;
    float totalTime;
    Vector3 v0;

    Vector3 curPos;
    Vector3 curVel;
    float curTime;
    bool simulating;
    double startSimulatingTime;
    double lastTime;
    GameObject goCurPos;

    [MenuItem("Test/Parabola Test")]
    public static void ShowWindow()
    {
        GetWindow<ParabolaTest>();
    }

    private void OnEnable()
    {
        EditorApplication.update += Update;
    }

    private void OnDisable()
    {
        EditorApplication.update -= Update;
    }

    private void OnGUI()
    {
        startPos = EditorGUILayout.ObjectField("Start Position:", startPos, typeof(GameObject), true) as GameObject;
        endPos = EditorGUILayout.ObjectField("End Position:", endPos, typeof(GameObject), true) as GameObject;

        totalTime = EditorGUILayout.FloatField("Total Time:", totalTime);
        if (GUILayout.Button("Calc V0"))
            CalcV0();
        if (totalTime != 0f)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Simulate"))
                Simulate();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            EditorGUILayout.Slider(curTime, 0, totalTime);
            GUILayout.EndHorizontal();
        }
    }

    void CalcV0()
    {
        if (startPos == null || endPos == null)
            return;
        v0 = Parabola.CalcV0Bounce1(startPos.transform.position, endPos.transform.position, GRAVITY, totalTime);
        startPos.transform.rotation = Quaternion.LookRotation(v0);
    }

    void Simulate()
    {
        curPos = startPos.transform.position;
        curVel = v0;
        curTime = 0f;
        simulating = true;
        startSimulatingTime = EditorApplication.timeSinceStartup;
        lastTime = EditorApplication.timeSinceStartup;
        if (!goCurPos)
        {
            goCurPos = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            goCurPos.transform.localScale = Vector3.one / 10;
            goCurPos.transform.position = curPos;
        }
    }

    void StopSimulate()
    {
        simulating = false;
    }

    private void Update()
    {
        if (simulating)
        {
            float deltaTime = (float)(EditorApplication.timeSinceStartup - lastTime);
            curTime = (float)(EditorApplication.timeSinceStartup - startSimulatingTime);
            lastTime = EditorApplication.timeSinceStartup;
            Parabola.Update(ref curPos, ref curVel, GRAVITY, deltaTime);
            goCurPos.transform.position = curPos;

            if (curTime >= totalTime)
                StopSimulate();
        }
    }
}
