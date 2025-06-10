using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[ExecuteInEditMode]
public class RoundedQuad : MonoBehaviour
{
    [Header("尺寸设置")]
    public float width = 2f;
    public float height = 1f;
    
    [Header("圆角设置")]
    [Range(0f, 1f)]
    public float cornerRadius = 0.2f;
    [Range(4, 32)]
    public int cornerSegments = 8; // 每个圆角的段数
    
    [Header("其他设置")]
    public bool generateOnStart = true;
    public bool autoUpdate = false; // 在编辑器中自动更新
    
    private MeshFilter meshFilter;
    private Mesh generatedMesh;
    
    void Start()
    {
        if (generateOnStart)
        {
            GenerateMesh();
        }
    }
    
    void OnValidate()
    {
        // 确保圆角半径不超过宽高的一半
        float maxRadius = Mathf.Min(width, height) * 0.5f;
        cornerRadius = Mathf.Clamp(cornerRadius, 0f, maxRadius);
        
        if (autoUpdate)
        {
            GenerateMesh();
        }
    }
    
    [ContextMenu("生成网格")]
    public void GenerateMesh()
    {
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();
        
        if (generatedMesh == null)
        {
            generatedMesh = new Mesh();
            generatedMesh.name = "RoundedRect";
        }
        
        CreateRoundedRectMesh();
        meshFilter.mesh = generatedMesh;
    }
    
    void CreateRoundedRectMesh()
    {
        // 计算实际参数
        float actualRadius = Mathf.Min(cornerRadius, Mathf.Min(width, height) * 0.5f);
        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;
        
        // 计算顶点数量
        int cornerVertexCount = cornerSegments; // 每个圆角的顶点数
        int totalVertices = 4 * cornerVertexCount + 1; // 四个圆角 + 中心点
        
        // 创建顶点和UV数组
        Vector3[] vertices = new Vector3[totalVertices];
        Vector2[] uvs = new Vector2[totalVertices];
        
        int vertexIndex = 0;
        
        // 添加中心顶点
        vertices[vertexIndex] = Vector3.zero;
        uvs[vertexIndex] = new Vector2(0.5f, 0.5f);
        vertexIndex++;
        
        // 四个圆角的中心点
        Vector3[] cornerCenters = new Vector3[]
        {
            new Vector3(halfWidth - actualRadius, halfHeight - actualRadius, 0),   // 右上
            new Vector3(-halfWidth + actualRadius, halfHeight - actualRadius, 0),  // 左上
            new Vector3(-halfWidth + actualRadius, -halfHeight + actualRadius, 0), // 左下
            new Vector3(halfWidth - actualRadius, -halfHeight + actualRadius, 0)   // 右下
        };
        
        // 每个圆角的起始角度
        float[] startAngles = { 0f, 90f, 180f, 270f };
        
        // 生成四个圆角的顶点
        for (int corner = 0; corner < 4; corner++)
        {
            Vector3 center = cornerCenters[corner];
            float startAngle = startAngles[corner] * Mathf.Deg2Rad;
            
            for (int i = 0; i < cornerVertexCount; i++)
            {
                float angle = startAngle + (i * 90f * Mathf.Deg2Rad) / (cornerVertexCount - 1);
                
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * actualRadius,
                    Mathf.Sin(angle) * actualRadius,
                    0
                );
                
                vertices[vertexIndex] = center + offset;
                
                // 计算UV坐标
                float u = (vertices[vertexIndex].x + halfWidth) / width;
                float v = (vertices[vertexIndex].y + halfHeight) / height;
                uvs[vertexIndex] = new Vector2(u, v);
                
                vertexIndex++;
            }
        }
        
        // 创建三角形
        int triangleCount = 4 * (cornerVertexCount - 1) * 3; // 每个圆角 (段数-1) 个三角形
        int[] triangles = new int[triangleCount];
        int triangleIndex = 0;
        
        // 为每个圆角创建三角形
        for (int corner = 0; corner < 4; corner++)
        {
            int cornerStartVertex = 1 + corner * cornerVertexCount;
            
            for (int i = 0; i < cornerVertexCount - 1; i++)
            {
                // 从中心点到圆角边缘的三角形
                triangles[triangleIndex] = 0; // 中心点
                triangles[triangleIndex + 1] = cornerStartVertex + i;
                triangles[triangleIndex + 2] = cornerStartVertex + i + 1;
                triangleIndex += 3;
            }
        }
        
        // 连接相邻圆角之间的区域
        ConnectCorners(ref triangles, ref triangleIndex, cornerVertexCount);
        
        // 应用到网格
        generatedMesh.Clear();
        generatedMesh.vertices = vertices;
        generatedMesh.triangles = triangles;
        generatedMesh.uv = uvs;
        generatedMesh.RecalculateNormals();
        generatedMesh.RecalculateBounds();
    }
    
    void ConnectCorners(ref int[] triangles, ref int triangleIndex, int cornerVertexCount)
    {
        // 连接四个圆角之间的直边区域
        for (int corner = 0; corner < 4; corner++)
        {
            int currentCornerStart = 1 + corner * cornerVertexCount;
            int nextCornerStart = 1 + ((corner + 1) % 4) * cornerVertexCount;
            
            int currentCornerEnd = currentCornerStart + cornerVertexCount - 1;
            int nextCornerEnd = nextCornerStart;
            
            // 检查是否需要扩展三角形数组
            if (triangleIndex + 2 >= triangles.Length)
            {
                System.Array.Resize(ref triangles, triangles.Length + 6);
            }
            
            // 连接相邻圆角
            triangles[triangleIndex] = 0; // 中心点
            triangles[triangleIndex + 1] = currentCornerEnd;
            triangles[triangleIndex + 2] = nextCornerEnd;
            triangleIndex += 3;
        }
    }
    
    // 公共方法用于运行时修改参数
    public void SetSize(float newWidth, float newHeight)
    {
        width = newWidth;
        height = newHeight;
        GenerateMesh();
    }
    
    public void SetCornerRadius(float newRadius)
    {
        cornerRadius = newRadius;
        GenerateMesh();
    }
    
    public void SetCornerSegments(int newSegments)
    {
        cornerSegments = Mathf.Clamp(newSegments, 4, 32);
        GenerateMesh();
    }
    
    void OnDestroy()
    {
        if (generatedMesh != null)
        {
            if (Application.isPlaying)
                Destroy(generatedMesh);
            else
                DestroyImmediate(generatedMesh);
        }
    }
}