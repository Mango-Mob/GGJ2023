using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class TerrainGenerator : MonoBehaviour
{
    Mesh mesh;
    
    public GameObject treePrefab;

    Vector3[] vertices;
    int[] triangles;
    public int TreeCount = 10;

    public int seed;
    public int terrainScale = 5;
    public int xSize = 20;
    public int zSize = 20;
    [Range(1, 10)]
    public int octaves = 3;
    public float noiseScale = 0.5f;
    public float islandSize = 3f;

    public AnimationCurve heightCurve;
    Vector2 origin;
    public bool regenerate = false;
    public bool regenerateFull = false;
    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        gameObject.AddComponent<MeshCollider>();
       
        transform.position = new Vector3(-xSize/2, 0, -zSize/2);
        Generate();
    }

    // Update is called once per frame
    void Update()
    {
        if( regenerate )
        {
            regenerate = false;
            regenerateFull = false;
            Generate();
        }else if(regenerateFull)
        {
            regenerateFull = false;
            seed = 0;
            Generate();
        }
    }

    void Generate()
    {
        if (seed == 0)
            seed = (int)UnityEngine.Random.Range(1, 10000000);

        origin = new Vector2(Mathf.Sqrt(seed), Mathf.Sqrt(seed));

        CreateShape();
        for (int i = 0; i < TreeCount; i++)
        {
            Vector2Int[] left = { new Vector2Int(0, 0), new Vector2Int(xSize, zSize / 2) };
            Vector2Int[] right = { new Vector2Int(0, zSize / 2), new Vector2Int(xSize, zSize) };

            GameObject.Instantiate(treePrefab, PlaceTree(left, right), Quaternion.identity);
        }
        UpdateMesh();
    }

    void CreateShape()
    {
        
        vertices = new Vector3[(1 + xSize) * (1 + zSize)];
        for(int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float xPos = x * (float)terrainScale / xSize;
                float zPos = z * (float)terrainScale / zSize;

                vertices[i] = new Vector3(xPos, heightCurve.Evaluate(NoiseFunction(xPos, zPos)), zPos);
                i++;
            }
        }

        triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;
        for (int i = 0, z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }
    private float NoiseFunction(float x, float z)
    {
        float a = 0, noiseSize = noiseScale, opacity = 1;
        float dist = Vector2.Distance(new Vector2(x, z), new Vector2(xSize, zSize)) / terrainScale;
        for (int o = 0; o < octaves; o++)
        {
            float xVal = (x / (noiseSize * terrainScale)) + origin.x;
            float zVal = (z / (noiseSize * terrainScale)) - origin.y;

            float y = noise.snoise(new float2(xVal, zVal));

            a += Mathf.InverseLerp(0, 1, y) / opacity;

            noiseSize /= 2f;
            opacity *= 2f;
        }

        return Mathf.Clamp(a -= FallOffMap(x, z, terrainScale), heightCurve[0].time, heightCurve[heightCurve.length - 1].time);
    }

    private float FallOffMap(float x, float z, float size)
    {
        float gradient = 1;

        gradient /= (x * z) / (size * size) * (1 - x / size) * (1 - z / size);
        gradient -= 16;
        gradient /= islandSize;

        return gradient;
    }
    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public Vector3 PlaceTree(Vector2Int[] left, Vector2Int[] right)
    {
        float left_average = GetAverageHeight(left[0], left[1]);
        float right_average = GetAverageHeight(right[0], right[1]);

        float min_height = heightCurve[0].value;
        float max_height = heightCurve[heightCurve.length - 1].value;

        float total = Mathf.Abs(min_height) + Mathf.Abs(max_height);

        float left_weight = (left_average + Mathf.Abs(min_height)) / total;
        float right_weight = (right_average + Mathf.Abs(min_height)) / total;


        if (UnityEngine.Random.Range(0, left_weight + right_weight) <= left_weight)
        {
            float left_size = Vector2Int.Distance(left[0], left[1]);
            if(left_size < 10)
            {   //Break
                Vector3 left_point = GetPointAt(left[0].x, left[0].y);
                Vector3 right_point = GetPointAt(left[1].x, left[1].y);
                return new Vector3(
                    Mathf.Lerp(left_point.x, right_point.x, UnityEngine.Random.Range(0, 1)), 
                    0, 
                    Mathf.Lerp(left_point.y, right_point.y, UnityEngine.Random.Range(0, 1))
                    ); 
            }
            //Select Left
            Vector2Int mid = (left[1] + left[0]) / 2;
            Vector2Int[] new_left = { new Vector2Int(left[0].x, left[0].y), new Vector2Int( mid.x, mid.y ) };
            Vector2Int[] new_right = { new Vector2Int(mid.x, mid.y), new Vector2Int(left[1].x, left[1].y) };
            return PlaceTree(new_left, new_right);
        }
        else
        {
            float right_size = Vector2Int.Distance(right[0], right[1]);
            if (right_size < 10)
            {   //Break
                Vector3 left_point = GetPointAt(right[0].x, right[0].y);
                Vector3 right_point = GetPointAt(right[1].x, right[1].y);
                return new Vector3(
                    Mathf.Lerp(left_point.x, right_point.x, UnityEngine.Random.Range(0, 1)),
                    0,
                    Mathf.Lerp(left_point.y, right_point.y, UnityEngine.Random.Range(0, 1))
                    );
            }
            //Select Right
            Vector2Int mid = (right[1] + right[0]) / 2;
            Vector2Int[] new_left = { new Vector2Int(right[0].x, right[0].y), new Vector2Int(mid.x, mid.y) };
            Vector2Int[] new_right = { new Vector2Int(mid.x, mid.y), new Vector2Int(right[1].x, right[1].y) };
            return PlaceTree(new_left, new_right);
        }
    }

    private Vector3 GetPointAt(int x, int y)
    {
        return vertices[y * zSize + x];
    }

    private float GetAverageHeight(Vector2Int a, Vector2Int b)
    {
        float total = 0;
        int count = 0;
        for (int x = a.x; x < b.x; x += (a.x <= b.x ? 1 : -1) )
        {
            for (int y = a.y; y < b.y; y += (a.y <= b.y ? 1 : -1))
            {
                total += GetPointAt(x, y).y;
                count++;
            }
        }

        return total / count;
    }

    private void OnDrawGizmosSelected()
    {
        if (vertices.Length == 0)
            return;

        Gizmos.color = Color.red;
        foreach (var vert in vertices)
        {
            Gizmos.DrawSphere(vert, 0.05f);
        }   
    }
}
