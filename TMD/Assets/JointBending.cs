using System.Security.Cryptography;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class JointBending : MonoBehaviour
{
    [Header("Base Shape")]
    public double width = 1;
    public double height = 5;

    [Range(1, 400)]
    public int verticalSegments = 100;

    [Header("Bend Control")]
    [Tooltip("How far (in world units) the TOP of the building is pushed sideways.")]
    public double bendAmount = 0;

    [Header("Visual Floors")]
    [Tooltip("Real-world height (meters) of one floor.")]
    public float floorHeightMeters = 3f;

    public float metersPerWorldUnit = 10f;

    [Header("Offset")]
    [Range(0f, 0.9f)]
    public float topBlankFraction = 0.1f;

    [Header("Bending Safety Cutoff")]
    public float bendCutoff = 1000f;

    bool bendingPaused = false;

    public Material buildingMaterial;

    double _lastWidth;
    double _lastHeight;

    Mesh _mesh;
    Vector3[] _baseVertices;
    Vector2[] _baseUVs;

    public Vector3 tmdPoint = Vector3.zero;

    void OnEnable()
    {
        _lastWidth = width;
        _lastHeight = height;

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

        if (!Mathf.Approximately((float)_lastWidth, (float)width) ||
        !Mathf.Approximately((float)_lastHeight, (float)height))
        {
            _lastWidth = width;
            _lastHeight = height;

            CreateMesh();   // rebuild vertices/UVs for new height/width
        }
        UpdateMaterialFloors();
        ApplyBend();
    }

    public void CreateMesh()
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

        double halfWidth = width * 0.5d;
        float heightMeters = (float)(height * metersPerWorldUnit);

        float floors = (floorHeightMeters > 0f)
            ? heightMeters / floorHeightMeters
            : 1f;

        for (int i = 0; i <= verticalSegments; i++)
        {
            double t = (double)i / verticalSegments;   // 0..1 up the height
            double y = t * height;


            double xLeft = -halfWidth;
            double xRight = halfWidth;

            int baseIndex = i * vertsPerColumn;

            vertices[baseIndex + 0] = new Vector3((float)-halfWidth, (float)y, 0f);
            vertices[baseIndex + 1] = new Vector3((float)+halfWidth, (float)y, 0f);

            float v = (float)(t * floors);
            uvs[baseIndex + 0] = new Vector2(0f, v);
            uvs[baseIndex + 1] = new Vector2(1f, v);
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

        UpdateMaterialFloors();

        _baseVertices = _mesh.vertices;
        _baseUVs = _mesh.uv;

    }

    void ApplyBend()
    {
        if (_mesh == null || _baseVertices == null || _baseVertices.Length == 0)
            return;

        Vector3[] deformed = new Vector3[_baseVertices.Length];

        double dir = Mathf.Sign((float)bendAmount);
        double target = Mathf.Abs((float)bendAmount);

        if (bendCutoff > 0f && target > bendCutoff)
        {
            target = bendCutoff;
        }

        // If no bend: keep original
        if (target < 0.00001f)
        {
            for (int i = 0; i < _baseVertices.Length; i++)
                deformed[i] = _baseVertices[i];

            _mesh.vertices = deformed;
            _mesh.uv = _baseUVs;
            _mesh.RecalculateBounds();
            return;
        }

        double R = SolveRadius(height, target);
        double halfWidth = width * 0.5f;

        for (int i = 0; i < _baseVertices.Length; i++)
        {
            Vector3 baseV = _baseVertices[i];

            // 0..1 up the building
            double t = Mathf.Clamp01((float)(baseV.y / height));
            double s = t * height;
            double angle = s / R;
            double xCenter = dir * (R - R * Mathf.Cos((float)angle)); 
            double yCenter = R * Mathf.Sin((float)angle);

            Vector2 tangent = new Vector2((float)(dir * Mathf.Sin((float)angle)), Mathf.Cos((float)angle));
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

            double normalizedX = (halfWidth > 0f) ? (baseV.x / halfWidth) : 0f;
            normalizedX = Mathf.Clamp((float)normalizedX, -1f, 1f);

            double x = xCenter + normalizedX * halfWidth * normal.x;
            double y = yCenter + normalizedX * halfWidth * normal.y;

            deformed[i] = new Vector3((float)x, (float)y, 0f);
            if (i == 75) { 
                tmdPoint = deformed[i];
            }
			if (i == 76)
			{
				tmdPoint = (tmdPoint+deformed[i])/2;
			}
		}

        _mesh.vertices = deformed;
        _mesh.uv = _baseUVs;
        _mesh.RecalculateBounds();
    }



    double SolveRadius(double height, double targetOffset)
    {
        targetOffset = Mathf.Abs((float)targetOffset);
        if (targetOffset < 0.00001f)
            return Mathf.Infinity;
        double R = (height * height) / (2f * targetOffset);

        for (int i = 0; i < 5; i++)
        {
            double theta = height / R;
            double cosT = Mathf.Cos((float)theta);
            double sinT = Mathf.Sin((float)theta);

            double f = R * (1f - cosT) - targetOffset;
            double df = 1f - cosT - theta * sinT;

            if (Mathf.Abs((float)df) < 1e-6f) break;
            R -= f / df;
        }

        return Mathf.Max((float)R, (float)height * 0.25f);
    }

    void UpdateMaterialFloors()
    {
        if (buildingMaterial == null)
            return;

        float heightMeters = (float)(height * metersPerWorldUnit);
        float floors = (floorHeightMeters > 0f)
            ? heightMeters / floorHeightMeters
            : 1f;

        buildingMaterial.SetFloat("_Floors", floors);
        buildingMaterial.SetFloat("_TopBlankFraction", topBlankFraction);
    }


}
