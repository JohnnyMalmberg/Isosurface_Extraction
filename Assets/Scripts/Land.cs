using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public static class WorldGen
{
    public static float PerlinNoise3D(float x, float y, float z)
    {
        int seed = 0;
        float noiseScale = 0.1f;
        x *= noiseScale;
        y *= noiseScale;
        z *= noiseScale;
        x += seed;
        y += seed;
        z += seed;
        float xy = Mathf.PerlinNoise(x, y);
        float xz = Mathf.PerlinNoise(x, z);
        float yz = Mathf.PerlinNoise(y, z);
        float yx = Mathf.PerlinNoise(y, x);
        float zx = Mathf.PerlinNoise(z, x);
        float zy = Mathf.PerlinNoise(z, y);
        return (xy + xz + yz + yx + zx + zy) / 6f;
    }

    public static int TerrainVoxel(int x, int y, int z)
    {
        return -1;
        //return Mathf.Tan(x * 0.1f) + Mathf.Sin(y * 0.1f) + Mathf.Cos(z * 0.1f) > 0f ? 1 : -1;
        //return (0.3f - ((float)(y) / 20f) * (WorldGen.PerlinNoise3D(x, y, z) - 0.2f)) > 0f ? 1 : -1;
        //return (WorldGen.PerlinNoise3D(x, y, z) - 0.55f) > 0f ? 1 : -1;
    }
}

public class Land : MonoBehaviour
{
    Chunk[][][] chunks;

    public Chunk chunkPrefab;

    public int chunkResolution = 32;

    public int numChunks = 3;
    int numChunksCubed;

    public bool collapse = false;

    VoxTree[][][] voxelsT;

    void Awake()
    {
        numChunksCubed = numChunks * numChunks * numChunks;

        voxelsT = new VoxTree[numChunks][][];
        chunks = new Chunk[numChunks][][];
        for (int x = 0; x < numChunks; x++)
        {
            voxelsT[x] = new VoxTree[numChunks][];
            chunks[x] = new Chunk[numChunks][];
            for (int y = 0; y < numChunks; y++)
            {
                voxelsT[x][y] = new VoxTree[numChunks];
                chunks[x][y] = new Chunk[numChunks];
                for (int z = 0; z < numChunks; z++)
                {
                    chunks[x][y][z] = Instantiate(chunkPrefab);
                    chunks[x][y][z].SetResolution(chunkResolution);
                    voxelsT[x][y][z] = new VoxTree(x * (chunkResolution), y * (chunkResolution), z * (chunkResolution), chunkResolution);
                    chunks[x][y][z].SetPosition(x * (chunkResolution), y * (chunkResolution), z * (chunkResolution));
                }
            }
        }

        // This should be put into a function
        for (int i = 0; i < chunkResolution * numChunks; i++)
        {
            for (int j = 0; j < chunkResolution * numChunks; j++)
            {
                for (int k = 0; k < chunkResolution * numChunks; k++)
                {
                    voxelsT[i / chunkResolution][j / chunkResolution][k / chunkResolution].setVoxel(i, j, k, WorldGen.TerrainVoxel(i, j, k));
                }
            }
        }

        for (int x = 0; x < numChunks; x++)
        {
            for (int y = 0; y < numChunks; y++)
            {
                for (int z = 0; z < numChunks; z++)
                {
                    voxelsT[x][y][z].setVoxelBox(20, 20, 20, 50, 50, 50, 1);
                }
            }
        }


        // TODO: refactor this
        for (int i = 0; i < numChunks; i++)
        {
            for (int j = 0; j < numChunks; j++)
            {
                for (int k = 0; k < numChunks; k++)
                {
                    chunks[i][j][k].UpdateVoxels(voxelsT[i][j][k]);
                    if (i > 0)
                    {
                        chunks[i - 1][j][k].UpdateVoxels(voxelsT[i][j][k], true);
                        if (j > 0)
                        {
                            chunks[i - 1][j - 1][k].UpdateVoxels(voxelsT[i][j][k], true);
                            if (k > 0)
                            {
                                chunks[i - 1][j - 1][k - 1].UpdateVoxels(voxelsT[i][j][k], true);
                            }
                        }
                    }

                    if (j > 0)
                    {
                        chunks[i][j - 1][k].UpdateVoxels(voxelsT[i][j][k], true);
                        if (k > 0)
                        {
                            chunks[i][j - 1][k - 1].UpdateVoxels(voxelsT[i][j][k], true);
                        }
                    }

                    if (k > 0)
                    {
                        chunks[i][j][k - 1].UpdateVoxels(voxelsT[i][j][k], true);
                        if (i > 0)
                        {
                            chunks[i - 1][j][k - 1].UpdateVoxels(voxelsT[i][j][k], true);
                        }
                    }
                }
            }
        }

        for (int i = 0; i < numChunks; i++)
        {
            for (int j = 0; j < numChunks; j++)
            {
                for (int k = 0; k < numChunks; k++)
                {
                    chunks[i][j][k].UpdateVoxels(voxelsT[i][j][k]);
                    chunks[i][j][k].UpdateMesh();
                }
            }
        }


    }

    struct coords
    {
        public int i;
        public int j;
        public int k;
        public coords(int i, int j, int k)
        {
            this.i = i;
            this.j = j;
            this.k = k;
        }
    }

    void UpdateChunk(object c)
    {
        coords c2 = (coords)c;
        int i = c2.i;
        int j = c2.j;
        int k = c2.k;
        chunks[i][j][k].UpdateVoxels(voxelsT[i][j][k]);
        //chunks[i][j][k].UpdateMesh();
    }

}
