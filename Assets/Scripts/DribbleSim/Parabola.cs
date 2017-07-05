using UnityEngine;

public static class Parabola
{
    public static Vector3 CalcV0Bounce1(Vector3 startPos, Vector3 endPos, float gravity, float time)
    {
        float h1 = startPos.y;
        float h2 = endPos.y;
        float g = -gravity, t = time;
        float C = (h1 + h2) / t - (g * t) / 2;
        float t1 = Mathf.Sqrt(2 * h1 / g + (C / g) * (C / g)) - C / g;
        float v0 = h1 / t1 - g * t1 / 2;

        Vector2 p1 = new Vector2(startPos.x, startPos.z);
        Vector2 p2 = new Vector2(endPos.x, endPos.z);
        Vector2 vh = (p2 - p1).magnitude / t * (p2 - p1).normalized;
        return new Vector3(vh.x, v0, vh.y);
    }

    public static void Update(ref Vector3 pos, ref Vector3 velocity, float gravity, float deltaTime)
    {
        pos += velocity * deltaTime;
        pos.y += gravity * deltaTime * deltaTime / 2;
        velocity.y += gravity * deltaTime;

        if (pos.y < 0)  // Hit ground
        {
            pos.y = -pos.y;
            velocity.y = -velocity.y;
        }
    }
}
