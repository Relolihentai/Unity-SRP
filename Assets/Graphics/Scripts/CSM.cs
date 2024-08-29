using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;
using Quaternion = UnityEngine.Quaternion;

struct MainCameraSettings
{
    public Vector3 position;
    public Quaternion rotation;
    public float nearClipPlane;
    public float farClipPlane;
    public float aspect;
};
public class CSM
{
    public float[] splts = { 0.07f, 0.13f, 0.25f, 0.55f };
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
    
    MainCameraSettings settings;
    //Update更新分割视锥体
    public void Update(Camera camera, Vector3 lightDir)
    {
        //得到摄像机远近平面顶点
        //此时是视角空间
        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, farCorners);
        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, nearCorners);
        
        // 视锥体顶点转世界坐标
        for (int i = 0; i < 4; i++)
        {
            //TransformVector，将vector从该transform的本地空间变换到世界空间
            //原理应该很简单，取transform三轴在世界空间的表示，组成列矩阵，然后转置
            //别忘了平移矩阵，就是position
            farCorners[i] = camera.transform.TransformVector(farCorners[i]) + camera.transform.position;
            nearCorners[i] = camera.transform.TransformVector(nearCorners[i]) + camera.transform.position;
        }

        // 按照比例划分相机视锥体
        for(int i = 0; i < 4; i++)
        {
            Vector3 dir = farCorners[i] - nearCorners[i];

            f0_near[i] = nearCorners[i];
            f0_far[i] = f0_near[i] + dir * splts[0];

            f1_near[i] = f0_far[i];
            f1_far[i] = f1_near[i] + dir * splts[1];

            f2_near[i] = f1_far[i];
            f2_far[i] = f2_near[i] + dir * splts[2];

            f3_near[i] = f2_far[i];
            f3_far[i] = f3_near[i] + dir * splts[3];
        }

        // 计算包围盒
        box0 = LightSpaceAABB(f0_near, f0_far, lightDir);
        // DrawFrustum(f0_near, f0_far, Color.blue);
        // DrawAABB(box0, Color.blue);
        box1 = LightSpaceAABB(f1_near, f1_far, lightDir);
        // DrawFrustum(f1_near, f1_far, Color.red);
        // DrawAABB(box1, Color.red);
        box2 = LightSpaceAABB(f2_near, f2_far, lightDir);
        // DrawFrustum(f2_near, f2_far, Color.green);
        // DrawAABB(box2, Color.green);
        box3 = LightSpaceAABB(f3_near, f3_far, lightDir);
        // DrawFrustum(f3_near, f3_far, Color.yellow);
        // DrawAABB(box3, Color.yellow);
    }
    
    Vector3[] LightSpaceAABB(Vector3[] nearCorners, Vector3[] farCorners, Vector3 lightDir)
    {
        //这里就是想让世界空间下的视椎体顶点变换到光源空间，旋转和光源一样
        //手动组装矩阵的方法
        // Vector3 lightRight = Vector3.Cross(Vector3.up, lightDir);
        // Vector3 lightUp = Vector3.Cross(lightDir, lightRight);
        // Matrix4x4 toLightView = new Matrix4x4(lightRight, lightUp, lightDir, new Vector4(0, 0, 0, 1));
        
        //这里LookAt计算的是world to light 的转置
        Matrix4x4 toShadowViewInv = Matrix4x4.LookAt(Vector3.zero, lightDir, Vector3.up);
        Matrix4x4 toShadowView = toShadowViewInv.inverse;

        // 视锥体顶点转光源方向
        for(int i = 0; i < 4; i++)
        {
            farCorners[i] = mulMatrix(toShadowView, farCorners[i], 1.0f);
            nearCorners[i] = mulMatrix(toShadowView, nearCorners[i], 1.0f);
        }

        //好处就是在光源空间下，计算AABB盒很方便，只需要取xyz三轴的最大最小值，然后组装
        // 计算 AABB 包围盒
        float[] x = new float[8];
        float[] y = new float[8];
        float[] z = new float[8];
        for(int i = 0; i < 4; i++)
        {
            x[i] = nearCorners[i].x; x[i+4] = farCorners[i].x;
            y[i] = nearCorners[i].y; y[i+4] = farCorners[i].y;
            z[i] = nearCorners[i].z; z[i+4] = farCorners[i].z;
        }
        float xmin=Mathf.Min(x), xmax=Mathf.Max(x);
        float ymin=Mathf.Min(y), ymax=Mathf.Max(y);
        float zmin=Mathf.Min(z), zmax=Mathf.Max(z);

        // 包围盒顶点转世界坐标
        Vector3[] points = {
            new Vector3(xmin, ymin, zmin), new Vector3(xmin, ymin, zmax), new Vector3(xmin, ymax, zmin), new Vector3(xmin, ymax, zmax),
            new Vector3(xmax, ymin, zmin), new Vector3(xmax, ymin, zmax), new Vector3(xmax, ymax, zmin), new Vector3(xmax, ymax, zmax)
        };
        
        //乘以逆矩阵回去
        for(int i = 0; i < 8; i++)
            points[i] = mulMatrix(toShadowViewInv, points[i], 1.0f);

        // 视锥体顶还原
        for(int i = 0; i < 4; i++)
        {
            farCorners[i] = mulMatrix(toShadowViewInv, farCorners[i], 1.0f);
            nearCorners[i] = mulMatrix(toShadowViewInv, nearCorners[i], 1.0f);
        }

        return points;
    }
    
    public void ConfigCameraToShadowSpace(ref Camera camera, Vector3 lightDir, int level, float distance)
    {
        // 选择第 level 级视锥划分
        var box = new Vector3[8];
        if(level == 0) box = box0; if(level == 1) box = box1; 
        if(level == 2) box = box2; if(level == 3) box = box3;

        // 计算 Box 中点, 宽高比
        Vector3 center = (box[3] + box[4]) / 2; 
        float w = Vector3.Magnitude(box[0] - box[4]);
        float h = Vector3.Magnitude(box[0] - box[2]);

        // 配置相机
        camera.transform.rotation = Quaternion.LookRotation(lightDir);
        camera.transform.position = center; 
        camera.nearClipPlane = -distance;
        camera.farClipPlane = distance;
        camera.aspect = w / h;
        camera.orthographicSize = h * 0.5f;
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
