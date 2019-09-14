using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    public bool DEBUG_GRID = false;
    public bool DEBUG_VERTICES = false;
    public bool DEBUG_VOXELS = false;

    Mesh chunkMesh;
    MeshCollider meshCollider;

    List<Vector3> vertices;
    List<int> triangles;
    List<Color> colors;

    int[][][] voxels;
    VoxTree voxelsT;

    int resolution;

    public float smoothing = 1f;

    public bool chunky = false;

    int x;
    int y;
    int z;


    void Awake()
    {

    }

    public void SetResolution(int resolution)
    {
        GetComponent<MeshFilter>().mesh = chunkMesh = new Mesh();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        chunkMesh.name = "Chunk Mesh";
        vertices = new List<Vector3>();
        triangles = new List<int>();
        colors = new List<Color>();

        this.resolution = resolution + 2;
        this.voxels = new int[this.resolution][][];

        for (int i = 0; i < this.resolution; i++)
        {
            this.voxels[i] = new int[this.resolution][];
            for (int j = 0; j < this.resolution; j++)
            {
                this.voxels[i][j] = new int[this.resolution];
            }
        }
    }

    public void SetPosition(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.transform.position = new Vector3(x, y, z);
    }

    void UpdatePhysics()
    {
        meshCollider.sharedMesh = chunkMesh;
    }

    bool edgeThroughSurface(int voxel1, int voxel2)
    {
        return (voxel1 <= 0) ^ (voxel2 <= 0);
    }

    Vector3 getSurface(int x, int y, int z, int x2, int y2, int z2, int prefetched1, int prefetched2)
    {
        float scaled1 = 0.5f, scaled2 = 0.5f;
        return new Vector3(x, y, z) * scaled1 + new Vector3(x2, y2, z2) * scaled2;
    }

    public void UpdateVoxels(VoxTree voxTree, bool limit = false)
    {
        voxTree.fillVoxelArray(this.voxels, this.x, this.y, this.z, limit);
    }

    public void UpdateMesh()
    {
        chunkMesh.Clear();
        vertices.Clear();
        triangles.Clear();

        for (int x = 0; x < voxels.Length - 1; x++)
        {
            for (int y = 0; y < voxels[x].Length - 1; y++)
            {
                for (int z = 0; z < voxels[x][y].Length - 1; z++)
                {
                    int edgesWithin = 0;
                    Vector3 centerPosition = new Vector3(0, 0, 0);
                    int voxelxyz = (int)voxels[x][y][z];
                    int voxelXyz = (int)voxels[x + 1][y][z];
                    int voxelxYz = (int)voxels[x][y + 1][z];
                    int voxelxyZ = (int)voxels[x][y][z + 1];
                    int voxelXYz = (int)voxels[x + 1][y + 1][z];
                    int voxelXyZ = (int)voxels[x + 1][y][z + 1];
                    int voxelxYZ = (int)voxels[x][y + 1][z + 1];
                    int voxelXYZ = (int)voxels[x + 1][y + 1][z + 1];
                    // start
                    if (edgeThroughSurface(voxelxyz, voxelXyz))
                    {
                        edgesWithin++;
                        centerPosition += getSurface(x, y, z, x + 1, y, z, voxelxyz, voxelXyz);
                    }
                    if (edgeThroughSurface(voxelxyz, voxelxYz))
                    {
                        edgesWithin++;
                        centerPosition += getSurface(x, y, z, x, y + 1, z, voxelxyz, voxelxYz);
                    }
                    if (edgeThroughSurface(voxelxyz, voxelxyZ))
                    {
                        edgesWithin++;
                        centerPosition += getSurface(x, y, z, x, y, z + 1, voxelxyz, voxelxyZ);
                    }
                    // x+1
                    if (edgeThroughSurface(voxelXyz, voxelXYz))
                    {
                        edgesWithin++;
                        centerPosition += getSurface(x + 1, y, z, x + 1, y + 1, z, voxelXyz, voxelXYz);
                    }
                    if (edgeThroughSurface(voxelXyz, voxelXyZ))
                    {
                        edgesWithin++;
                        centerPosition += getSurface(x + 1, y, z, x + 1, y, z + 1, voxelXyz, voxelXyZ);
                    }
                    // y+1
                    if (edgeThroughSurface(voxelxYz, voxelXYz))
                    {
                        edgesWithin++;
                        centerPosition += getSurface(x, y + 1, z, x + 1, y + 1, z, voxelxYz, voxelXYz);
                    }
                    if (edgeThroughSurface(voxelxYz, voxelxYZ))
                    {
                        edgesWithin++;
                        centerPosition += getSurface(x, y + 1, z, x, y + 1, z + 1, voxelxYz, voxelxYZ);
                    }
                    // z+1
                    if (edgeThroughSurface(voxelxyZ, voxelXyZ))
                    {
                        edgesWithin++;
                        centerPosition += getSurface(x, y, z + 1, x + 1, y, z + 1, voxelxyZ, voxelXyZ);
                    }
                    if (edgeThroughSurface(voxelxyZ, voxelxYZ))
                    {
                        edgesWithin++;
                        centerPosition += getSurface(x, y, z + 1, x, y + 1, z + 1, voxelxyZ, voxelxYZ);
                    }
                    // all +1
                    if (edgeThroughSurface(voxelXYZ, voxelxYZ))
                    {
                        edgesWithin++;
                        centerPosition += getSurface(x + 1, y + 1, z + 1, x, y + 1, z + 1, voxelXYZ, voxelxYZ);
                    }
                    if (edgeThroughSurface(voxelXYZ, voxelXyZ))
                    {
                        edgesWithin++;
                        centerPosition += getSurface(x + 1, y + 1, z + 1, x + 1, y, z + 1, voxelXYZ, voxelXyZ);
                    }
                    if (edgeThroughSurface(voxelXYZ, voxelXYz))
                    {
                        edgesWithin++;
                        centerPosition += getSurface(x + 1, y + 1, z + 1, x + 1, y + 1, z, voxelXYZ, voxelXYz);
                    }


                    if (edgesWithin == 0 || chunky)
                    {
                        vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
                    }
                    else
                    {
                        vertices.Add(centerPosition / edgesWithin);
                    }
                    //colors.Add(Color.white);
                }
            }
        }


        for (int x = 1; x < voxels.Length - 1; x++)
        {
            for (int y = 1; y < voxels[x].Length - 1; y++)
            {
                for (int z = 1; z < voxels[x][y].Length - 1; z++)
                {
                    int voxel1 = (int)voxels[x][y][z];
                    int voxel2 = (int)voxels[x + 1][y][z];
                    int vIndex = vertexIndex(x, y, z);
                    if (edgeThroughSurface(voxel1, voxel2))
                    {
                        //if (voxels[x][y][z] < 0f)
                        if (voxel1 < 0)
                        {
                            triangles.Add(vIndex);
                            triangles.Add(vIndex - 1);
                            triangles.Add(vIndex - this.resolution + 1);
                            triangles.Add(vIndex - this.resolution + 1);
                            triangles.Add(vIndex - 1);
                            triangles.Add(vIndex - this.resolution);
                        }
                        else
                        {
                            triangles.Add(vIndex - this.resolution);
                            triangles.Add(vIndex - 1);
                            triangles.Add(vIndex - this.resolution + 1);
                            triangles.Add(vIndex - this.resolution + 1);
                            triangles.Add(vIndex - 1);
                            triangles.Add(vIndex);
                        }
                    }
                    voxel2 = (int)voxels[x][y + 1][z];
                    if (edgeThroughSurface(voxel1, voxel2))
                    {
                        //if (voxels[x][y][z] >= 0f)
                        if (voxel1 >= 0)
                        {
                            triangles.Add(vIndex);
                            triangles.Add(vIndex - 1);
                            triangles.Add(vIndex - (resolution - 1) * (resolution - 1));
                            triangles.Add(vIndex - (resolution - 1) * (resolution - 1));
                            triangles.Add(vIndex - 1);
                            triangles.Add(vIndex - (resolution - 1) * (resolution - 1) - 1);
                        }
                        else
                        {
                            triangles.Add(vIndex - (resolution - 1) * (resolution - 1) - 1);
                            triangles.Add(vIndex - 1);
                            triangles.Add(vIndex - (resolution - 1) * (resolution - 1));
                            triangles.Add(vIndex - (resolution - 1) * (resolution - 1));
                            triangles.Add(vIndex - 1);
                            triangles.Add(vIndex);
                        }
                    }
                    voxel2 = (int)voxels[x][y][z + 1];
                    if (edgeThroughSurface(voxel1, voxel2))
                    {
                        //if (voxels[x][y][z] >= 0f)
                        if (voxel1 >= 0)
                        {
                            triangles.Add(vIndex);
                            triangles.Add(vIndex - (resolution - 1) * (resolution - 1));
                            triangles.Add(vIndex - this.resolution + 1);
                            triangles.Add(vIndex - this.resolution + 1);
                            triangles.Add(vIndex - (resolution - 1) * (resolution - 1));
                            triangles.Add(vIndex - (resolution - 1) * (resolution - 1) - this.resolution + 1);
                        }
                        else
                        {
                            triangles.Add(vIndex - (resolution - 1) * (resolution - 1) - this.resolution + 1);
                            triangles.Add(vIndex - (resolution - 1) * (resolution - 1));
                            triangles.Add(vIndex - this.resolution + 1);
                            triangles.Add(vIndex - this.resolution + 1);
                            triangles.Add(vIndex - (resolution - 1) * (resolution - 1));
                            triangles.Add(vIndex);
                        }
                    }
                }
            }
        }



        chunkMesh.vertices = vertices.ToArray();
        //chunkMesh.colors = colors.ToArray();
        chunkMesh.triangles = triangles.ToArray();
        chunkMesh.RecalculateNormals();
        UpdatePhysics();
    }

    int vertexIndex(int x, int y, int z)
    {
        return (x * (resolution - 1) * (resolution - 1)) + (y * (resolution - 1)) + z;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDrawGizmos()
    {
        if (DEBUG_GRID)
        {
            Gizmos.color = new Color(1, 0, 0, 0.25f);
            for (int i = 0; i < this.resolution; i++)
            {
                for (int j = 0; j < this.resolution; j++)
                {
                    Gizmos.DrawLine(new Vector3(i + this.x, j + this.y, this.z), new Vector3(i + this.x, j + this.y, this.z + this.resolution - 1));
                    Gizmos.DrawLine(new Vector3(this.x, j + this.y, i + this.z), new Vector3(this.x + this.resolution - 1, j + this.y, i + this.z));
                    Gizmos.DrawLine(new Vector3(i + this.x, this.y, j + this.z), new Vector3(i + this.x, this.y + this.resolution - 1, j + this.z));
                }
            }
        }
        if (DEBUG_VERTICES)
        {
            Gizmos.color = Color.red;
            if (vertices != null)
            {
                foreach (Vector3 vertex in vertices)
                {
                    Gizmos.DrawCube(vertex, new Vector3(0.2f, 0.2f, 0.2f));
                }
            }
        }
        if (DEBUG_VOXELS)
        {
            for (int x = 0; x < this.resolution; x++)
            {
                for (int y = 0; y < this.resolution; y++)
                {
                    for (int z = 0; z < this.resolution; z++)
                    {
                        Gizmos.color = new Color(voxels[x][y][z], voxels[x][y][z], voxels[x][y][z]);
                        Gizmos.DrawSphere(new Vector3(x, y, z), 0.1f);
                    }
                }
            }
        }
    }
}
