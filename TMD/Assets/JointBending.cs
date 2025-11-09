using System.Security.Cryptography;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class JointBending : MonoBehaviour
{
    [Header("Base Shape")]
    public float width = 1f;
    public float height = 5f;
    [Range(1, 40)]
    public int verticalSegments = 10; // more segments = smoother bend

    [Header("Bend Control")]
    [Tooltip("How far (in world units) the TOP of the building is pushed sideways.")]
    public float bendAmount = 0f;

    Mesh _mesh;
    Vector3[] _baseVertices;

    void OnEnable()
    {
        CreateMesh();
        ApplyBend();
    }

    void OnValidate()
    {

        CreateMesh();
        ApplyBend();
    }

    void Update()
    {
        ApplyBend();
    }

    void CreateMesh()
    {
        if (_mesh == null)
        {
            _mesh = new Mesh();
            _mesh.name = "BuildingMesh";
            GetComponent<MeshFilter>().sharedMesh = _mesh;
        }

        int vertsPerColumn = 2; // bottom + top for each segment line
        int vertexCount = (verticalSegments + 1) * vertsPerColumn;
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int[] triangles = new int[verticalSegments * 6];

        float halfWidth = width * 0.5f;

        for (int i = 0; i <= verticalSegments; i++)
        {
            float t = (float)i / verticalSegments;   // 0..1 up the height
            float y = t * height;


            float xLeft = -halfWidth;
            float xRight = halfWidth;

            int baseIndex = i * vertsPerColumn;
            vertices[baseIndex + 0] = new Vector3(xLeft, y, 0f);  // left
            vertices[baseIndex + 1] = new Vector3(xRight, y, 0f); // right

            uvs[baseIndex + 0] = new Vector2(0f, t);
            uvs[baseIndex + 1] = new Vector2(1f, t);
        }

        int ti = 0;
        for (int i = 0; i < verticalSegments; i++)
        {
            int row = i * vertsPerColumn;

            int bl = row;             // bottom-left
            int br = row + 1;         // bottom-right
            int tl = row + 2;         // top-left
            int tr = row + 3;         // top-right

            triangles[ti++] = bl;
            triangles[ti++] = tl;
            triangles[ti++] = br;

            triangles[ti++] = br;
            triangles[ti++] = tl;
            triangles[ti++] = tr;
        }

        _mesh.Clear();
        _mesh.vertices = vertices;
        _mesh.uv = uvs;
        _mesh.triangles = triangles;
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        _baseVertices = _mesh.vertices;
    }

    void ApplyBend()
    {
        if (_mesh == null || _baseVertices == null || _baseVertices.Length == 0)
            return;

        Vector3[] deformed = new Vector3[_baseVertices.Length];

        float dir = Mathf.Sign(bendAmount);
        float target = Mathf.Abs(bendAmount);

        // If no bend: keep original
        if (target < 0.00001f)
        {
            for (int i = 0; i < _baseVertices.Length; i++)
                deformed[i] = _baseVertices[i];

            _mesh.vertices = deformed;
            _mesh.RecalculateBounds();
            return;
        }

        float R = SolveRadius(height, target);
        float halfWidth = width * 0.5f;

        for (int i = 0; i < _baseVertices.Length; i++)
        {
            Vector3 baseV = _baseVertices[i];

            // 0..1 up the building
            float t = Mathf.Clamp01(baseV.y / height);
            float s = t * height;
            float angle = s / R;
            float xCenter = dir * (R - R * Mathf.Cos(angle)); 
            float yCenter = R * Mathf.Sin(angle);

            Vector2 tangent = new Vector2(dir * Mathf.Sin(angle), Mathf.Cos(angle));
            Vector2 normal;
            if (dir >= 0f)
            {
                
                normal = new Vector2(tangent.y, -tangent.x);
            }
            else
            {
                
                normal = new Vector2(-tangent.y, tangent.x);
            }
            normal.Normalize();

            float normalizedX = (halfWidth > 0f) ? (baseV.x / halfWidth) : 0f;
            normalizedX = Mathf.Clamp(normalizedX, -1f, 1f);

            float x = xCenter + normalizedX * halfWidth * normal.x;
            float y = yCenter + normalizedX * halfWidth * normal.y;

            deformed[i] = new Vector3(x, y, 0f);
        }


        _mesh.vertices = deformed;
        _mesh.RecalculateBounds();
    }



    float SolveRadius(float height, float targetOffset)
    {
        targetOffset = Mathf.Abs(targetOffset);
        if (targetOffset < 0.00001f)
            return Mathf.Infinity;
        float R = (height * height) / (2f * targetOffset);

        for (int i = 0; i < 5; i++)
        {
            float theta = height / R;
            float cosT = Mathf.Cos(theta);
            float sinT = Mathf.Sin(theta);

            float f = R * (1f - cosT) - targetOffset;
            float df = 1f - cosT - theta * sinT;

            if (Mathf.Abs(df) < 1e-6f) break;
            R -= f / df;
        }

        return Mathf.Max(R, height * 0.25f);
    }



}
