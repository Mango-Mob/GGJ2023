using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class TerrainGenerator : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    public int seed;
    public int terrainScale = 5;
    public int xSize = 20;
    public int zSize = 20;
    [Range(1, 10)]
    public int octaves = 3;
    public float noiseScale = 0.5f;

    public AnimationCurve heightCurve;
    Vector2 origin;
    public bool regenerate = false;
    public bool regenerateFull = false;
    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

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

        for (int o = 0; o < octaves; o++)
        {
            float xVal = (x / (noiseSize * terrainScale)) + origin.x;
            float zVal = (z / (noiseSize * terrainScale)) - origin.y;

            float y = noise.snoise(new float2(xVal, zVal));

            a += Mathf.InverseLerp(0, 1, y) / opacity;

            noiseSize /= 2f;
            opacity *= 2f;
        }

        return a;
    }
    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }
}
