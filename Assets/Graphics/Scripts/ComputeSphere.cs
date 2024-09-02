using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ComputeSphere
{
    public static void ComputeSphereIntersection(Vector2 point_1, Vector2 point_2, float radius, out Vector2 intersection_1, out Vector2 intersection_2)
    {
        float Xa = point_1.x, Ya = point_1.y;
        float Xb = point_2.x, Yb = point_2.y;
        float Xa2 = Mathf.Pow(Xa, 2), Ya2 = Mathf.Pow(Ya, 2);
        float Xb2 = Mathf.Pow(Xb, 2), Yb2 = Mathf.Pow(Yb, 2);

        float C1 = (Xa2 - Xb2 + Ya2 - Yb2) / 2.0f / (Xa - Xb);
        float C2 = (Ya - Yb) / (Xa - Xb);

        float a = Mathf.Pow(C2, 2.0f) + 1.0f;
        float b = -2.0f * C1 * C2 + 2.0f * Xa * C2 - 2.0f * Ya;
        float c = Xa2 + Ya2 - 2 * Xa * C1 + Mathf.Pow(C1, 2) - Mathf.Pow(radius, 2);

        float delta = Mathf.Pow(b, 2) - 4 * a * c;
        intersection_1.y = (-b - Mathf.Pow(delta, 0.5f)) / 2.0f * a;
        intersection_1.x = C1 - C2 * intersection_1.y;
        
        intersection_2.y = (-b + Mathf.Pow(delta, 0.5f)) / 2.0f * a;
        intersection_2.x = C1 - C2 * intersection_2.y;
    }
}
