using UnityEngine;

public enum BodyType
{
    Char1_Thin,
#if !DEBUG_SAMPLE
    Char2_Fat,
    Char3_Tall,
    Char4_Short,
    Tang,
    Hulk,
#endif
    Count,
}

public static class Utils
{
    public const string NODE_PATH_LEFT_BALL = "Leftball";
    public const string NODE_PATH_RIGHT_BALL = "Rightball";

    public static void DrawPoint(string name, Vector3 position, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.transform.position = position;
        go.transform.localScale = Vector3.one / 10;
        go.GetComponent<Renderer>().material.color = color;
    }
}
