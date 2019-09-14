using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class VoxTree
{
    public VoxTree[] subtrees;

    public int voxelValue = 0;

    public int xCenter;
    public int yCenter;
    public int zCenter;
    public int xBase;
    public int yBase;
    public int zBase;
    public int scale;

    public VoxTree(int x, int y, int z, int scale)
    {
        this.subtrees = new VoxTree[8];
        this.xCenter = x + scale / 2;
        this.yCenter = y + scale / 2;
        this.zCenter = z + scale / 2;
        this.xBase = x;
        this.yBase = y;
        this.zBase = z;
        this.scale = scale;
    }

    public VoxTree(int x, int y, int z, int scale, int value)
    {
        this.subtrees = new VoxTree[8];
        for (int i = 0; i < 8; i++)
        {
            this.subtrees[i] = null;
        }
        this.xCenter = x + scale / 2;
        this.yCenter = y + scale / 2;
        this.zCenter = z + scale / 2;
        this.xBase = x;
        this.yBase = y;
        this.zBase = z;
        this.scale = scale;
        this.voxelValue = value;
    }

    public int getVoxel(int x, int y, int z)
    {
        if (this.scale == 1)
        {
            return this.voxelValue;
        }

        VoxTree subtree = this.subtrees[this.getSubtreeIndex(x, y, z)];

        if (subtree != null)
        {
            return subtree.getVoxel(x, y, z);
        }

        return this.voxelValue;
    }

    public int GetX(int index)
    {
        return (index & 4) == 0 ? this.xBase : this.xCenter;
    }
    public int GetY(int index)
    {
        return (index & 2) == 0 ? this.yBase : this.yCenter;
    }
    public int GetZ(int index)
    {
        return (index & 1) == 0 ? this.zBase : this.zCenter;
    }

    public void setVoxel(int x, int y, int z, int value)
    {
        if (x < this.xBase)
        {
            return;
        }
        else if (x >= this.xBase + this.scale)
        {
            return;
        }
        if (y < this.yBase)
        {
            return;
        }
        else if (y >= this.yBase + this.scale)
        {
            return;
        }
        if (z < this.zBase)
        {
            return;
        }
        else if (z >= this.zBase + this.scale)
        {
            return;
        }

        if (this.scale == 1)
        {
            this.voxelValue = value;
            return;
        }

        int index = this.getSubtreeIndex(x, y, z);
        if (this.subtrees[index] == null)
        {
            this.subtrees[index] = this.newSubtree(index, this.voxelValue);
        }
        this.subtrees[index].setVoxel(x, y, z, value);
    }

    public VoxTree newSubtree(int index, int value)
    {
        return new VoxTree(this.GetX(index), this.GetY(index), this.GetZ(index), this.scale / 2, value);
    }

    public VoxTree GetSubtree(int index)
    {
        return this.subtrees[index];
    }

    public bool setVoxelBox(int x, int y, int z, int xScale, int yScale, int zScale, int value)
    {
        int x2 = x + xScale - 1;
        int y2 = y + yScale - 1;
        int z2 = z + zScale - 1;

        // Make sure box intersects this tree:
        if (x > this.xBase + this.scale - 1 || this.xBase > x2 ||
            y > this.yBase + this.scale - 1 || this.yBase > y2 ||
            z > this.zBase + this.scale - 1 || this.zBase > z2)
        {
            return false;
        }

        // Check if box engulfs this entire tree:
        if ((x <= this.xBase) && (this.scale + this.xBase < x + xScale) &&
            (y <= this.yBase) && (this.scale + this.yBase < y + yScale) &&
            (z <= this.zBase) && (this.scale + this.zBase < z + zScale))
        {
            for (int i = 0; i < 8; i++)
            {
                this.subtrees[i] = null;
            }
            this.voxelValue = value;
            return true;
        }

        for (int i = 0; i < 8; i++)
        {
            this.subtreeSetVoxelBox(i, x, y, z, xScale, yScale, zScale, value);
        }

        return true;
    }

    bool subtreeSetVoxelBox(int index, int x, int y, int z, int xScale, int yScale, int zScale, int value)
    {
        if (this.subtrees[index] == null)
        {
            this.subtrees[index] = this.newSubtree(index, this.voxelValue);
            if (this.subtrees[index].setVoxelBox(x, y, z, xScale, yScale, zScale, value))
            {
                return true;
            }
            else
            {
                this.subtrees[index] = null;
                return false;
            }
        }
        return this.subtrees[index].setVoxelBox(x, y, z, xScale, yScale, zScale, value);
    }

    public bool SphereIntersectsCube(int cx, int cy, int cz, int cScale, int sx, int sy, int sz, float radius)
    {
        float dist = radius * radius;
        if (sx < cx)
        {
            dist -= Mathf.Pow(sx - cx, 2f);
        }
        else if (sx > (cx + cScale))
        {
            dist -= Mathf.Pow(sx - (cx + cScale), 2f);
        }
        if (sy < cy)
        {
            dist -= Mathf.Pow(sy - cy, 2f);
        }
        else if (sy > (cy + cScale))
        {
            dist -= Mathf.Pow(sy - (cy + cScale), 2f);
        }
        if (sz < cz)
        {
            dist -= Mathf.Pow(sz - cz, 2f);
        }
        else if (sz > (cz + cScale))
        {
            dist -= Mathf.Pow(sz - (cz + cScale), 2f);
        }
        return (dist > 0);
    }

    public bool SphereHitsTree(int x, int y, int z, float radius)
    {
        return this.SphereIntersectsCube(this.xBase, this.yBase, this.zBase, this.scale - 1, x, y, z, radius);
    }

    public float Distance(int x, int y, int z, int x2, int y2, int z2)
    {
        return Mathf.Sqrt(Mathf.Pow(x - x2, 2f) + Mathf.Pow(y - y2, 2f) + Mathf.Pow(z - z2, 2f));
    }

    public bool setVoxelSphere(int x, int y, int z, float radius, int value)
    {
        if (!this.SphereHitsTree(x, y, z, radius))
        {
            return false;
        }

        if (this.scale == 1)
        {
            this.voxelValue = value;
            return true;
        }


        int xTest, yTest, zTest;
        if (x <= this.xCenter)
        {
            xTest = this.xBase + this.scale - 1;
        }
        else
        {
            xTest = this.xBase;
        }
        if (y <= this.yCenter)
        {
            yTest = this.yBase + this.scale - 1;
        }
        else
        {
            yTest = this.yBase;
        }
        if (z <= this.zCenter)
        {
            zTest = this.zBase + this.scale - 1;
        }
        else
        {
            zTest = this.zBase;
        }

        if (Distance(x, y, z, xTest, yTest, zTest) <= radius)
        {
            for (int i = 0; i < 8; i++)
            {
                this.subtrees[i] = null;
            }
            this.voxelValue = value;
            return true;
        }/* */

        for (int i = 0; i < 8; i++)
        {
            this.subtreeSetVoxelSphere(i, x, y, z, radius, value);
        }

        return true;
    }

    public bool subtreeSetVoxelSphere(int index, int x, int y, int z, float radius, int value)
    {
        if (this.subtrees[index] == null)
        {
            this.subtrees[index] = this.newSubtree(index, this.voxelValue);
            if (this.subtrees[index].setVoxelSphere(x, y, z, radius, value))
            {
                return true;
            }
            else
            {
                this.subtrees[index] = null;
                return false;
            }
        }
        return this.subtrees[index].setVoxelSphere(x, y, z, radius, value);
    }

    public int GetValue()
    {
        return voxelValue;
    }

    public bool Collapse()
    {
        if (this.scale == 1)
        {
            return true;
        }
        bool canCollapse = true;
        // Collapse all collapsible subtrees
        for (int i = 0; i < 8; i++)
        {
            if (this.subtrees[i] != null)
            {
                // Keep collapsing subtrees, even if the root can't be collapsed
                canCollapse = canCollapse && this.subtrees[i].Collapse();
            }
        }
        if (!canCollapse)
        {
            return false;
        }
        // All subtrees are collapsed, but perhaps not all to the same value
        int collapseValue = this.subtrees[0] == null ? this.voxelValue : this.subtrees[0].GetValue();
        for (int i = 1; i < 8; i++)
        {
            int newCollapseValue;
            if (this.subtrees[i] != null)
            {
                newCollapseValue = this.subtrees[i].GetValue();
            }
            else
            {
                newCollapseValue = this.voxelValue;
            }
            if (collapseValue != newCollapseValue)
            {
                return false;
            }
        }
        // All subtrees are collapsed to the same value
        this.voxelValue = collapseValue;
        for (int i = 0; i < 8; i++)
        {
            this.subtrees[i] = null;
        }

        return true;
    }


    public int getSubtreeIndex(int x, int y, int z)
    {
        int subtreeIndex = 0;
        if (x >= xCenter)
        {
            subtreeIndex += 4;
        }
        if (y >= yCenter)
        {
            subtreeIndex += 2;
        }
        if (z >= zCenter)
        {
            subtreeIndex += 1;
        }
        return subtreeIndex;
    }

    public void fillVoxelArray(int[][][] voxels, int xOffset = 0, int yOffset = 0, int zOffset = 0, bool limit = false)
    {
        if ((this.scale == 1) && (!limit))
        {
            voxels[this.xBase - xOffset][this.yBase - yOffset][this.zBase - zOffset] = voxelValue;
        }
        else if (this.scale == 1)
        {
            if (this.xBase - xOffset >= voxels.Length)
            {
                return;
            }
            if (this.yBase - yOffset >= voxels.Length)
            {
                return;
            }
            if (this.zBase - zOffset >= voxels.Length)
            {
                return;
            }
            voxels[this.xBase - xOffset][this.yBase - yOffset][this.zBase - zOffset] = voxelValue;
        }
        int subscale = this.scale / 2;
        for (int i = 0; i < 8; i++)
        {
            if (this.subtrees[i] == null)
            {
                int xLow = this.GetX(i);
                int yLow = this.GetY(i);
                int zLow = this.GetZ(i);


                if (limit)
                {
                    for (int x = xLow; (x < xLow + subscale) && (x < voxels.Length); x++)
                    {
                        for (int y = yLow; (y < yLow + subscale) && (y < voxels.Length); y++)
                        {
                            for (int z = zLow; (z < zLow + subscale) && (z < voxels.Length); z++)
                            {
                                voxels[x - xOffset][y - yOffset][z - zOffset] = this.voxelValue;
                            }
                        }
                    }
                }
                else
                {
                    for (int x = xLow; x < (xLow + subscale); x++)
                    {
                        for (int y = yLow; y < yLow + subscale; y++)
                        {
                            for (int z = zLow; z < zLow + subscale; z++)
                            {
                                voxels[x - xOffset][y - yOffset][z - zOffset] = this.voxelValue;
                            }
                        }
                    }
                }
            }
            else
            {
                this.subtrees[i].fillVoxelArray(voxels, xOffset, yOffset, zOffset, limit);
            }
        }
    }
}
