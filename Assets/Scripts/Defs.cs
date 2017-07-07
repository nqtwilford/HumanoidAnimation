using UnityEngine;

public enum BodyType
{
    Char1_Thin,
    Char2_Fat,
    Char3_Tall,
    Char4_Short,

    Count,
}

public static class Utils
{
    public static void DrawPoint(string name, Vector3 position, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.transform.position = position;
        go.transform.localScale = Vector3.one / 10;
        go.GetComponent<Renderer>().material.color = color;
    }
}
