using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;

struct MainCameraSettings
{
    public Vector3 position;
    public Quaternion rotation;
    public float nearClipPlane;
    public float farClipPlane;
    public float aspect;
};

[Serializable]
public struct CSM_Settings
{
    public int shadowMapResolution;
    [Range(0.01f, 1)]public List<float> splits;
}
public class CSM
{
    //public float[] splts = { 0.07f, 0.13f, 0.25f, 0.55f };
    //主相机视锥体
    private Vector3[] farCorners = new Vector3[4];
    private Vector3[] nearCorners = new Vector3[4];

    private Vector3[] f0_near = new Vector3[4], f0_far = new Vector3[4];
    private Vector3[] f1_near = new Vector3[4], f1_far = new Vector3[4];
    private Vector3[] f2_near = new Vector3[4], f2_far = new Vector3[4];
    private Vector3[] f3_near = new Vector3[4], f3_far = new Vector3[4];

    private Vector3[] box0 = new Vector3[8];
    private Vector3[] box1 = new Vector3[8];
    private Vector3[] box2 = new Vector3[8];
    private Vector3[] box3 = new Vector3[8];
    
    private MainCameraSettings settings;
    public CSM_Settings csm_settings;
    public CSM(CSM_Settings settings)
    {
        csm_settings = settings;
    }
    //Update更新分割视锥体
    public void Update(Camera camera, Vector3 lightDir)
    {
        //得到摄像机远近平面顶点
        //此时是视角空间
        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, farCorners);
        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, nearCorners);
        
        // // 视锥体顶点转世界坐标
        // for (int i = 0; i < 4; i++)
        // {
        //     //TransformVector，将vector从该transform的本地空间变换到世界空间
        //     //原理应该很简单，取transform三轴在世界空间的表示，组成列矩阵，然后转置
        //     //别忘了平移矩阵，就是position
        //     farCorners[i] = camera.transform.TransformVector(farCorners[i]) + camera.transform.position;
        //     nearCorners[i] = camera.transform.TransformVector(nearCorners[i]) + camera.transform.position;
        // }

        // 按照比例划分相机视锥体
        for(int i = 0; i < 4; i++)
        {
            Vector3 dir = farCorners[i] - nearCorners[i];

            f0_near[i] = nearCorners[i];
            f0_far[i] = f0_near[i] + dir * csm_settings.splits[0];

            f1_near[i] = f0_far[i];
            f1_far[i] = f1_near[i] + dir * csm_settings.splits[1];

            f2_near[i] = f1_far[i];
            f2_far[i] = f2_near[i] + dir * csm_settings.splits[2];

            f3_near[i] = f2_far[i];
            f3_far[i] = f3_near[i] + dir * csm_settings.splits[3];
        }

        // 计算包围盒
        box0 = LightSpaceAABB(f0_near, f0_far, lightDir);
        box1 = LightSpaceAABB(f1_near, f1_far, lightDir);
        box2 = LightSpaceAABB(f2_near, f2_far, lightDir);
        box3 = LightSpaceAABB(f3_near, f3_far, lightDir);

        Vector3 cameraPosition = camera.transform.position;
        for (int i = 0; i < 4; i++)
        {
            f0_near[i] = camera.transform.TransformVector(f0_near[i]) + cameraPosition;
            f0_far[i] = camera.transform.TransformVector(f0_far[i]) + cameraPosition;
            f1_near[i] = camera.transform.TransformVector(f1_near[i]) + cameraPosition;
            f1_far[i] = camera.transform.TransformVector(f1_far[i]) + cameraPosition;
            f2_near[i] = camera.transform.TransformVector(f2_near[i]) + cameraPosition;
            f2_far[i] = camera.transform.TransformVector(f2_far[i]) + cameraPosition;
            f3_near[i] = camera.transform.TransformVector(f3_near[i]) + cameraPosition;
            f3_far[i] = camera.transform.TransformVector(f3_far[i]) + cameraPosition;
        }
        
        for (int i = 0; i < 8; i++)
        {
            box0[i] = camera.transform.TransformVector(box0[i]) + cameraPosition;
            box1[i] = camera.transform.TransformVector(box1[i]) + cameraPosition;
            box2[i] = camera.transform.TransformVector(box2[i]) + cameraPosition;
            box3[i] = camera.transform.TransformVector(box3[i]) + cameraPosition;
        }
        if (camera.cameraType == CameraType.Preview)
        {
            //Debug.Log("center : " + (box0[0] + box0[7]) / 2);
            DrawFrustum(f0_near, f0_far, Color.blue);
            DrawAABB(box0, Color.blue);
        }
        if (camera.cameraType == CameraType.Preview)
        {
            DrawFrustum(f1_near, f1_far, Color.red);
            DrawAABB(box1, Color.red);
        }
        if (camera.cameraType == CameraType.Preview)
        {
            DrawFrustum(f2_near, f2_far, Color.green);
            DrawAABB(box2, Color.green);
        }
        if (camera.cameraType == CameraType.Preview)
        {
            DrawFrustum(f3_near, f3_far, Color.yellow);
            DrawAABB(box3, Color.yellow);
        }
    }
    
    Vector3[] LightSpaceAABB(Vector3[] nearCorners, Vector3[] farCorners, Vector3 lightDir)
    {
        //这里就是想让世界空间下的视椎体顶点变换到光源空间，旋转和光源一样
        //手动组装矩阵的方法
        Vector3 lightRight = Vector3.Cross(Vector3.up, lightDir);
        Vector3 lightUp = Vector3.Cross(lightDir, lightRight);
        Matrix4x4 toLightView = new Matrix4x4(lightRight, lightUp, lightDir, new Vector4(0, 0, 0, 1));
        Matrix4x4 toLightViewInv = toLightView.inverse;
        
        //这里LookAt计算的是world to light 的转置
        // Matrix4x4 toShadowViewInv = Matrix4x4.LookAt(Vector3.zero, lightDir, Vector3.up);
        // Matrix4x4 toShadowView = toShadowViewInv.inverse;
        
        // 视锥体顶点转光源方向
        // for(int i = 0; i < 4; i++)
        // {
        //     farCorners[i] = mulMatrix(toLightView, farCorners[i], 1.0f);
        //     nearCorners[i] = mulMatrix(toLightView, nearCorners[i], 1.0f);
        // }

        Vector2 farPoint_1 = (new Vector2(farCorners[0].x, farCorners[0].z) + new Vector2(farCorners[1].x, farCorners[1].z)) / 2;
        Vector2 farPoint_2 = (new Vector2(farCorners[2].x, farCorners[2].z) + new Vector2(farCorners[3].x, farCorners[3].z)) / 2;
        Vector2 nearPoint_1 = (new Vector2(nearCorners[0].x, nearCorners[0].z) + new Vector2(nearCorners[1].x, nearCorners[1].z)) / 2;
        Vector2 nearPoint_2 = (new Vector2(nearCorners[2].x, nearCorners[2].z) + new Vector2(nearCorners[3].x, nearCorners[3].z)) / 2;
        
        // s = (a + b + c + d) / 2
        // A = ((s - a)(s - b)(s - c)(s - d))^0.5
        // R = ((ac + bd)(ad + bc)(ab + cd))^0.5 / 4 / A
        float a = Vector2.Distance(farPoint_1, farPoint_2),
            b = Vector2.Distance(farPoint_1, nearPoint_1),
            c = Vector2.Distance(nearPoint_1, nearPoint_2),
            d = Vector2.Distance(farPoint_2, nearPoint_2);
        float s = (a + b + c + d) / 2;
        float A = Mathf.Pow((s - a) * (s - d) * (s - c) * (s - d), 0.5f);
        float raidus = Mathf.Pow((a * c + b * d) * (a * d + b * c) * (a * b + c * d), 0.5f) / 4 / A;
        
        // Debug.Log(raidus);
        
        Vector2 sphereCenter = Vector2.zero;
        ComputeSphere.CSM_Only_ComputeSphereIntersection(farPoint_1, nearPoint_1, raidus, out sphereCenter);
        Vector3 sphereCenter_V3 = new Vector3(sphereCenter.x, 0, sphereCenter.y);
        Vector3[] points =
        {
            new Vector3(sphereCenter.x - raidus, -raidus, sphereCenter.y - raidus), new Vector3(sphereCenter.x - raidus, -raidus, sphereCenter.y + raidus),
            new Vector3(sphereCenter.x - raidus, raidus, sphereCenter.y - raidus), new Vector3(sphereCenter.x - raidus, raidus, sphereCenter.y + raidus),
            new Vector3(sphereCenter.x + raidus, -raidus, sphereCenter.y - raidus), new Vector3(sphereCenter.x + raidus, -raidus, sphereCenter.y + raidus),
            new Vector3(sphereCenter.x + raidus, raidus, sphereCenter.y - raidus), new Vector3(sphereCenter.x + raidus, raidus, sphereCenter.y + raidus)
        };
        for (int i = 0; i < 8; i++)
        {
            points[i] = mulMatrix(toLightView, points[i] - sphereCenter_V3, 1) + sphereCenter_V3;
        }
        //好处就是在光源空间下，计算AABB盒很方便，只需要取xyz三轴的最大最小值，然后组装
        // 计算 AABB 包围盒
        // float[] x = new float[8];
        // float[] y = new float[8];
        // float[] z = new float[8];
        // for(int i = 0; i < 4; i++)
        // {
        //     x[i] = nearCorners[i].x; x[i+4] = farCorners[i].x;
        //     y[i] = nearCorners[i].y; y[i+4] = farCorners[i].y;
        //     z[i] = nearCorners[i].z; z[i+4] = farCorners[i].z;
        // }
        // float xmin=Mathf.Min(x), xmax=Mathf.Max(x);
        // float ymin=Mathf.Min(y), ymax=Mathf.Max(y);
        // float zmin=Mathf.Min(z), zmax=Mathf.Max(z);
        //
        // // 包围盒顶点转世界坐标
        // Vector3[] points = {
        //     new Vector3(xmin, ymin, zmin), new Vector3(xmin, ymin, zmax), new Vector3(xmin, ymax, zmin), new Vector3(xmin, ymax, zmax),
        //     new Vector3(xmax, ymin, zmin), new Vector3(xmax, ymin, zmax), new Vector3(xmax, ymax, zmin), new Vector3(xmax, ymax, zmax)
        // };
        
        // //乘以逆矩阵回去
        // for(int i = 0; i < 8; i++)
        //     points[i] = mulMatrix(toLightViewInv, points[i], 1.0f);
        // // 视锥体顶还原
        // for(int i = 0; i < 4; i++)
        // {
        //     farCorners[i] = mulMatrix(toLightViewInv, farCorners[i], 1.0f);
        //     nearCorners[i] = mulMatrix(toLightViewInv, nearCorners[i], 1.0f);
        // }
        
        return points;
    }
    
    public void ConfigCameraToShadowSpace(ref Camera camera, Vector3 lightDir, int level)
    {
        // 选择第 level 级视锥划分
        var box = new Vector3[8];
        // var f_near = new Vector3[4];
        // var f_far = new Vector3[4];
        if (level == 0)
        {
            box = box0;
            // f_near = f0_near;
            // f_far = f0_far;
        }
        if (level == 1)
        {
            box = box1;
            // f_near = f1_near;
            // f_far = f1_far;
        }
        if (level == 2)
        {
            box = box2; 
            // f_near = f2_near;
            // f_far = f2_far;
        }
        if (level == 3)
        {
            box = box3;
            // f_near = f3_near;
            // f_far = f3_far;
        }
        
        // 计算 Box 后面的 中点, 宽高比
        //Vector3 center = (box[2] + box[4]) / 2;
        
        //box 几何中心
        Vector3 center = (box[0] + box[7]) / 2;
        // 取box的宽高里长的那条作为深度图的边长
        // float w = Vector3.Magnitude(box[0] - box[4]);
        // float h = Vector3.Magnitude(box[0] - box[2]);
        //float len = Mathf.Max(w, h);
        // 造成旋转抖动的原因是摄像机旋转时，边长一直在变
        // 然后我们取长的对角线作为边长，这样只要摄像机参数和split不变就不会变
        //float len = Mathf.Max(Vector3.Magnitude(f_far[2] - f_near[0]), Vector3.Magnitude(f_far[2] - f_far[0]));
        
        // 通过分辨率算出每像素的大小
        //float disPerPix = len / csm_settings.shadowMapResolution;

        // Matrix4x4 toShadowViewInv = Matrix4x4.LookAt(Vector3.zero, lightDir, Vector3.up);
        // Matrix4x4 toShadowView = toShadowViewInv.inverse;

        //center = mulMatrix(toShadowView, center, 1.0f);
        // for (int i = 0; i < 3; i++)
        //     //center的位置对齐像素大小
        //     center[i] = Mathf.Floor(center[i] / disPerPix) * disPerPix;
        //center = mulMatrix(toShadowViewInv, center, 1.0f);
        
        float distance = Vector3.Magnitude(box[0] - box[1]);
        // 配置相机
        camera.transform.rotation = Quaternion.LookRotation(lightDir);
        camera.transform.position = center;
        camera.nearClipPlane = -distance / 2;
        camera.farClipPlane = distance / 2;
        camera.aspect = 1.0f;
        camera.orthographicSize = distance / 2;
    }
    
    // 保存相机参数, 更改为正交投影
    public void SaveMainCameraSettings(ref Camera camera)
    {
        settings.position = camera.transform.position;
        settings.rotation = camera.transform.rotation;
        settings.farClipPlane = camera.farClipPlane;
        settings.nearClipPlane = camera.nearClipPlane;
        settings.aspect = camera.aspect;
        camera.orthographic = true;
    }

    // 还原相机参数, 更改为透视投影
    public void RevertMainCameraSettings(ref Camera camera)
    {
        camera.transform.position = settings.position;
        camera.transform.rotation = settings.rotation;
        camera.farClipPlane = settings.farClipPlane;
        camera.nearClipPlane = settings.nearClipPlane;
        camera.aspect = settings.aspect;
        camera.orthographic = false;
    }
    
    // 画相机视锥体
    void DrawFrustum(Vector3[] nearCorners, Vector3[] farCorners, Color color)
    {
        for (int i = 0; i < 4; i++)
            Debug.DrawLine(nearCorners[i], farCorners[i], color);

        Debug.DrawLine(farCorners[0], farCorners[1], color);
        Debug.DrawLine(farCorners[0], farCorners[3], color);
        Debug.DrawLine(farCorners[2], farCorners[1], color);
        Debug.DrawLine(farCorners[2], farCorners[3], color);
        Debug.DrawLine(nearCorners[0], nearCorners[1], color);
        Debug.DrawLine(nearCorners[0], nearCorners[3], color);
        Debug.DrawLine(nearCorners[2], nearCorners[1], color);
        Debug.DrawLine(nearCorners[2], nearCorners[3], color);
    }

    // 画光源方向的 AABB 包围盒
    void DrawAABB(Vector3[] points, Color color)
    {
        // 画线
        Debug.DrawLine(points[0], points[1], color);
        Debug.DrawLine(points[0], points[2], color);
        Debug.DrawLine(points[0], points[4], color);

        Debug.DrawLine(points[6], points[2], color);
        Debug.DrawLine(points[6], points[7], color);
        Debug.DrawLine(points[6], points[4], color);

        Debug.DrawLine(points[5], points[1], color);
        Debug.DrawLine(points[5], points[7], color);
        Debug.DrawLine(points[5], points[4], color);

        Debug.DrawLine(points[3], points[1], color);
        Debug.DrawLine(points[3], points[2], color);
        Debug.DrawLine(points[3], points[7], color);
    }
    Vector3 mulMatrix(Matrix4x4 m, Vector3 v, float w)
    {
        Vector4 v4 = new Vector4(v.x, v.y, v.z, w);
        v4 = m * v4;
        return new Vector3(v4.x, v4.y, v4.z);
    }
}
