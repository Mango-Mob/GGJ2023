using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;

public class TerrainGenerator : MonoBehaviour
{
    Mesh mesh;
    
    public GameObject treePrefab;
    public GameObject playerPlatform;

    Vector3[] vertices;
    int[] triangles;
    public int TreeCount = 10;

    public int seed;

    [Range(1, 500)]
    public int terrainScale = 5;

    [Range(1, 500)]
    public int cellSize = 20;

    [Range(1, 10)]
    public int octaves = 3;

    [Range(0f, 1f)]
    public float noiseScale = 0.5f;

    [Range(1, 300)]
    public int islandSize = 120;

    [Range(1, 500)]
    public int flattenerRange = 1;

    [Range(0f, 1f)]
    public float flattenerInfluence = 0.5f;

    public AnimationCurve heightCurve;
    Vector2 origin;

    [Range(1, 20)]
    public int midDivision = 16;

    [Range(0f, 1f)]
    public float landHeight;

    [Header("Debug")]
    public bool regenerate = false;
    public bool regenerateFull = false;
    public bool showFallOff = false;
    public bool showFlattener = false;
    public bool showPropMask = false;

    private PropFactory factory;
    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        gameObject.AddComponent<MeshCollider>();
        factory = gameObject.AddComponent<PropFactory>();
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
        }
        else if(regenerateFull)
        {
            regenerateFull = false;
            seed = 0;
            Generate();
        }
        else if (showFallOff)
        {
            showFallOff = false;
            ShowFalloff();
        }
        else if (showFlattener)
        {
            showFlattener = false;
            ShowFlattener();
        }
        else if (showPropMask)
        {
            showPropMask = false;
            ShowPropMask();
        }
    }

    void Generate()
    {
        if (seed == 0)
            seed = (int)UnityEngine.Random.Range(1, 10000000);

        UnityEngine.Random.InitState(seed);

        factory.Clear();

        origin = new Vector2(Mathf.Sqrt(seed), Mathf.Sqrt(seed));
        
        factory.SpawnPlatform(playerPlatform, new Vector3(
            UnityEngine.Random.Range(0, terrainScale),
            heightCurve.Evaluate(UnityEngine.Random.Range(landHeight, heightCurve[heightCurve.length - 1].time)),
            UnityEngine.Random.Range(0, terrainScale)
            ));
        
        CreateShape(NoiseFunction);

        UpdateMesh();
        
        for (int i = 0; i < TreeCount; i++)
        {
            Vector2Int[] left = { new Vector2Int(0, 0), new Vector2Int(cellSize, cellSize / 2) };
            Vector2Int[] right = { new Vector2Int(0, cellSize / 2), new Vector2Int(cellSize, cellSize) };

            factory.SpawnTree(treePrefab, PlaceTree(left, right));
        }

        transform.position = new Vector3(-terrainScale / 2, 0, -terrainScale / 2);
    }
    void ShowFalloff()
    {
        if (seed == 0)
            seed = (int)UnityEngine.Random.Range(1, 10000000);

        UnityEngine.Random.InitState(seed);
        factory.Clear();

        origin = new Vector2(Mathf.Sqrt(seed), Mathf.Sqrt(seed));

        CreateShape(FallOffMap);
        UpdateMesh();

        transform.position = new Vector3(-terrainScale / 2, 0, -terrainScale / 2);
    }
    void ShowFlattener()
    {
        if (seed == 0)
            seed = (int)UnityEngine.Random.Range(1, 10000000);

        UnityEngine.Random.InitState(seed);
        factory.Clear();

        origin = new Vector2(Mathf.Sqrt(seed), Mathf.Sqrt(seed));

        CreateShape(Flattener);
        UpdateMesh();

        transform.position = new Vector3(-terrainScale / 2, 0, -terrainScale / 2);
    }
    private void ShowPropMask()
    {
        if (seed == 0)
            seed = (int)UnityEngine.Random.Range(1, 10000000);

        UnityEngine.Random.InitState(seed);
        factory.Clear();
        origin = new Vector2(Mathf.Sqrt(seed), Mathf.Sqrt(seed));

        factory.SpawnPlatform(playerPlatform, new Vector3(
            UnityEngine.Random.Range(0, terrainScale),
            heightCurve.Evaluate(UnityEngine.Random.Range(landHeight, heightCurve[heightCurve.length - 1].time)),
            UnityEngine.Random.Range(0, terrainScale)
            ));

        CreateShape(PropMask);
        UpdateMesh();

        transform.position = new Vector3(-terrainScale / 2, 0, -terrainScale / 2);
    }

    #region Terrain
    void CreateShape( Func<float, float, float> NoiseFunction )
    {
        
        vertices = new Vector3[(1 + cellSize) * (1 + cellSize)];
        for(int i = 0, z = 0; z <= cellSize; z++)
        {
            for (int x = 0; x <= cellSize; x++, i++)
            {
                float xPos = x * (float)terrainScale / cellSize;
                float zPos = z * (float)terrainScale / cellSize;

                vertices[i] = new Vector3(xPos, heightCurve.Evaluate(NoiseFunction(xPos, zPos)), zPos);
            }
        }

        triangles = new int[cellSize * cellSize * 6];

        int vert = 0;
        int tris = 0;
        for (int z = 0; z < cellSize; z++)
        {
            for (int x = 0; x < cellSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + cellSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + cellSize + 1;
                triangles[tris + 5] = vert + cellSize + 2;

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

        a -= FallOffMap(x, z);

        a = Mathf.Clamp(a, heightCurve[0].time, heightCurve[heightCurve.length - 1].time);

        float ideal = Flattener(x, z);
        return Mathf.Lerp(a, ideal, flattenerInfluence);
    }

    private float FallOffMap(float x, float z)
    {
        float gradient = 1;

        gradient /= (x * z) / (terrainScale * terrainScale) * (1 - x / terrainScale) * (1 - z / terrainScale);
        gradient -= midDivision;
        gradient /= islandSize;

        return gradient;
    }
    
    private float Flattener(float x, float z)
    {
        Vector2 mid_point = new Vector2(terrainScale / 2, terrainScale / 2);
        Vector2 this_point = new Vector2(x, z);

        float distance = GetAsProportion(Vector2.Distance(mid_point, this_point), 0, flattenerRange);

        return heightCurve[heightCurve.length - 1].time - Mathf.Clamp(distance, heightCurve[0].time, heightCurve[heightCurve.length - 1].time);
    }

    private float PropMask(float x, float z)
    {
        float height = 0;
        float count = 0;
        foreach (var prop in factory.propList)
        {
            if (prop.GetComponent<Collider>().bounds.Contains(this.transform.localToWorldMatrix * new Vector3(x, heightCurve.Evaluate(landHeight), z)))
            {
                height += prop.transform.position.y;
                count++;
            }
        }

        return (count > 1) ? height / count : height;
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }



    #endregion

    #region Props
    public Vector3 PlaceTree(Vector2Int[] left, Vector2Int[] right, bool is_hSplit = true)
    {
        float max_left, max_right;

        float left_average = GetAverageHeight(left[0], left[1], out max_left);
        float right_average = GetAverageHeight(right[0], right[1], out max_right);

        float left_weight = GetAsProportion(left_average, heightCurve[0].value, heightCurve[heightCurve.length - 1].value);
        float left_max_weight = GetAsProportion(max_left, heightCurve[0].value, heightCurve[heightCurve.length - 1].value);
        float right_weight = GetAsProportion(right_average, heightCurve[0].value, heightCurve[heightCurve.length - 1].value);
        float right_max_weight = GetAsProportion(max_right, heightCurve[0].value, heightCurve[heightCurve.length - 1].value);

        left_weight *= (left_max_weight) / (left_max_weight + right_max_weight);
        right_weight *= (right_max_weight) / (left_max_weight + right_max_weight);

        float result = UnityEngine.Random.Range(0, left_weight + right_weight);
        if ( result <= left_weight)
        {
            float left_size = Vector2Int.Distance(left[0], left[1]);
            if(left_size < 10)
            {   //Break
                Vector3 left_point = GetPointAt(left[0].x, left[0].y);
                Vector3 right_point = GetPointAt(left[1].x, left[1].y);
                return new Vector3(
                    Mathf.Lerp(left_point.x, right_point.x, UnityEngine.Random.Range(0, 1)),
                    Mathf.Lerp(left_point.y, right_point.y, 0.5f), 
                    Mathf.Lerp(left_point.z, right_point.z, UnityEngine.Random.Range(0, 1))
                    ); 
            }
            //Select Left
            Vector2Int[] split = (is_hSplit) ? hSplit(left) : vSplit(left);
            Vector2Int[] new_left = { split[0], split[1] };
            Vector2Int[] new_right = { split[2], split[3] };
            return PlaceTree(new_left, new_right, !is_hSplit);
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
                    Mathf.Lerp(left_point.z, right_point.z, UnityEngine.Random.Range(0, 1))
                    );
            }
            //Select Right
            Vector2Int[] split = (is_hSplit) ? hSplit(right) : vSplit(right);
            Vector2Int[] new_left = { split[0], split[1] };
            Vector2Int[] new_right = { split[2], split[3] };
            return PlaceTree(new_left, new_right, !is_hSplit);
        }
    }
    #endregion

    #region Utility
    private float GetAsProportion(float value, float min, float max)
    {
        return (value - min) / (max - min);
    }
    private Vector2Int[] hSplit(Vector2Int[] side)
    {
        Vector2Int mid = (side[1] + side[0]) / 2;
        int w = side[1].x - side[0].x;
        int h = side[1].y - side[0].y;

        Vector4[] rect = { 
            new Vector4(mid.x, mid.y - h/4, w, h/2),
            new Vector4(mid.x, mid.y + h/4, w, h/2)
        };

        return new Vector2Int[] {
            new Vector2Int((int)(rect[0].x - rect[0].z/2), (int)(rect[0].y - rect[0].w/2)),
            new Vector2Int((int)(rect[0].x + rect[0].z/2), (int)(rect[0].y + rect[0].w/2)),
            new Vector2Int((int)(rect[1].x - rect[0].z/2), (int)(rect[1].y - rect[1].w/2)),
            new Vector2Int((int)(rect[1].x + rect[0].z/2), (int)(rect[1].y + rect[1].w/2)),
        };
    }

    private Vector2Int[] vSplit(Vector2Int[] side)
    {
        Vector2Int mid = (side[1] + side[0]) / 2;
        int w = side[1].x - side[0].x;
        int h = side[1].y - side[0].y;

        Vector4[] rect = { //Convert to x, y, w, h
            new Vector4(mid.x - w/4, mid.y, w/2, h),
            new Vector4(mid.x + w/4, mid.y, w/2, h)
        };

        return new Vector2Int[] {
            new Vector2Int((int)(rect[0].x - rect[0].z/2), (int)(rect[0].y - rect[0].w/2)),
            new Vector2Int((int)(rect[0].x + rect[0].z/2), (int)(rect[0].y + rect[0].w/2)),
            new Vector2Int((int)(rect[1].x - rect[0].z/2), (int)(rect[1].y - rect[1].w/2)),
            new Vector2Int((int)(rect[1].x + rect[0].z/2), (int)(rect[1].y + rect[1].w/2)),
        };
    }

    private Vector3 GetPointAt(int x, int y)
    {
        return vertices[y * cellSize + x];
    }

    private float GetAverageHeight(Vector2Int a, Vector2Int b, out float max_height)
    {
        max_height = float.MinValue;

        float total = 0;
        int count = 0;
        for (int x = a.x; x < b.x; x += (a.x <= b.x ? 1 : -1) )
        {
            for (int y = a.y; y < b.y; y += (a.y <= b.y ? 1 : -1))
            {
                float curr_height  = GetPointAt(x, y).y;

                if(max_height < curr_height)
                {
                    max_height = curr_height;
                }
                total += curr_height;
                count++;
            }
        }

        return total / count;
    }

    #endregion
    private void OnDrawGizmosSelected()
    {
        if (Application.isEditor)
            return;

        Gizmos.color = Color.red;
        foreach (var vert in vertices)
        {
            Gizmos.DrawSphere(vert, 0.05f);
        }   
    }
}
