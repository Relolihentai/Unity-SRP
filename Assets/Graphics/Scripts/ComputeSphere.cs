using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ComputeSphere
{
    public static void CSM_Only_ComputeSphereCenter(Vector3 oneFarPoint, Vector3 oneNearPoint, out Vector3 sphereCenter, out float radius)
    {
        sphereCenter = Vector3.zero;
        float X0 = sphereCenter.x, Y0 = sphereCenter.y;
        float Px1 = oneFarPoint.x, Py1 = oneFarPoint.y, Pz1 = oneFarPoint.z;
        float Px2 = oneNearPoint.x, Py2 = oneNearPoint.y, Pz2 = oneNearPoint.z;
        float E0 = (Px1 - Px2) * (Px1 + Px2 - 2 * X0) + (Py1 - Py2) * (Py1 + Py2 - 2 * Y0);
        E0 /= Pz2 - Pz1;
        float Z0 = (Pz1 + Pz2 - E0) / 2;
        sphereCenter.z = Z0;
        radius = Vector3.Distance(sphereCenter, oneFarPoint);
    }
    public static void CSM_Only_ComputeSphereIntersection(Vector2 farPoint, Vector2 nearPoint, float radius, out Vector2 sphereCenter)
    {
        sphereCenter = Vector2.zero;
        float l1 = Mathf.Abs(farPoint.x);
        float offset_1 = Mathf.Pow(Mathf.Pow(radius, 2) - Mathf.Pow(l1, 2), 0.5f);
        float center_0 = farPoint.y - offset_1;
        float center_1 = farPoint.y + offset_1;

        float l2 = Mathf.Abs(nearPoint.x);
        float offset_2 = Mathf.Pow(Mathf.Pow(radius, 2) - Mathf.Pow(l2, 2), 0.5f);
        float center_2 = nearPoint.y - offset_2;
        float center_3 = nearPoint.y + offset_2;
        
        // Debug.Log("farPoint : " + farPoint);
        // Debug.Log("nearPoint : " + nearPoint);
        // Debug.Log("center_0 : " + center_0);
        // Debug.Log("center_1 : " + center_1);
        // Debug.Log("center_2 : " + center_2);
        // Debug.Log("center_3 : " + center_3);

        if (Mathf.Abs(center_0 - center_2) < 1e-1f) sphereCenter = new Vector2(0, center_0);
        else if (Mathf.Abs(center_0 - center_3) < 1e-1f) sphereCenter = new Vector2(0, center_0);
        else if (Mathf.Abs(center_1 - center_2) < 1e-1f) sphereCenter = new Vector2(0, center_1);
        else if (Mathf.Abs(center_1 - center_3) < 1e-1f) sphereCenter = new Vector2(0, center_1);
    }
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
