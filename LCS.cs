using System;
using System.Linq;
using System.Numerics;

namespace Arihara.GuideSmoke
{
  public class LCS
  {
    private int lenX, lenY, lenZ;
    private float[,,] ftleField;
    private int[,,] lcsField;

    public LCS(float[,,] ftle)
    {
      lenX = ftle.GetLength(0);
      lenY = ftle.GetLength(1);
      lenZ = ftle.GetLength(2);
      this.ftleField = ftle;
      lcsField = new int[lenX, lenY, lenZ];
      NormalizeFTLE();
    }

    private void NormalizeFTLE()
    {
      float min = ftleField[0, 0, 0];
      float max = min;
      for (int ix = 0; ix < lenX; ix++)
      {
        for (int iy = 0; iy < lenY; iy++)
        {
          for (int iz = 0; iz < lenZ; iz++)
          {
            float num = ftleField[ix, iy, iz];
            if (num < min) min = num;
            if (max < num) max = num;
          }
        }
      }

      for (int ix = 0; ix < lenX; ix++)
      {
        for (int iy = 0; iy < lenY; iy++)
        {
          for (int iz = 0; iz < lenZ; iz++)
          {
            ftleField[ix, iy, iz] = (ftleField[ix, iy, iz] - min) / (max - min);
          }
        }
      }
    }

    public void LcsByThreshold(float threshold)
    {
      for (int ix = 0; ix < lenX; ix++)
      {
        for (int iy = 0; iy < lenY; iy++)
        {
          for (int iz = 0; iz < lenZ; iz++)
          {
            lcsField[ix, iy, iz] = (ftleField[ix, iy, iz] < threshold) ? 0 : 1;
          }
        }
      }
    }

    public void LcsByThreshold()
    {
      float median = CalcMedian();
      LcsByThreshold(median);
      Console.WriteLine("Used Median Value: {0}", median);
    }

    public void LcsByHessian()
    {
      for (int i = 0; i < 1; i++)
      {
        GaussianFilter2D();
      }
      ShowFTLE();
    }

    public void ShowFTLE()
    {
      for (int ix = 0; ix < lenX; ix++)
      {
        for (int iy = 0; iy < lenY; iy++)
        {
          for (int iz = 0; iz < lenZ; iz++)
          {
            Console.WriteLine("[{0} {1} {2}] : {3}", ix, iy, iz, ftleField[ix, iy, iz]);
          }
        }
      }
    }

    public void WriteLCS(string path, Vector3[,,] pos)
    {
      FileIO.WriteLCSFile(path, pos, lcsField, lenX, lenY, lenZ);
    }

    private float CalcMedian()
    {
      float[] arr = new float[lenX * lenY * lenZ];
      for (int ix = 0; ix < lenX; ix++)
      {
        for (int iy = 0; iy < lenY; iy++)
        {
          for (int iz = 0; iz < lenZ; iz++)
          {
            arr[ix * (lenY * lenZ) + iy * lenZ + iz] = ftleField[ix, iy, iz];
          }
        }
      }
      Array.Sort(arr);
      return arr[(lenX * lenY * lenZ) / 2];
    }

    private void GaussianFilter2D()
    {
      if (lenZ > 1) return;

      float[,] filter = {
        { 1f / 256,  4f / 256,  6f / 256,  4f / 256, 1f / 256 },
        { 4f / 256, 16f / 256, 24f / 256, 16f / 256, 4f / 256 },
        { 6f / 256, 24f / 256, 36f / 256, 24f / 256, 6f / 256 },
        { 4f / 256, 16f / 256, 24f / 256, 16f / 256, 4f / 256 },
        { 1f / 256,  4f / 256,  6f / 256,  4f / 256, 1f / 256 },
      };

      float[,,] tmp = new float[lenX, lenY, 1];

      for (int ix = 0; ix < lenX; ix++)
      {
        for (int iy = 0; iy < lenY; iy++)
        {
          for (int fx = -2; fx < 3; fx++)
          {
            for (int fy = -2; fy < 3; fy++)
            {
              tmp[ix, iy, 0] += filter[fx + 2, fy + 2] * GetCoordValue2D(ix + fx, iy + fy);
            }
          }
        }
      }
      ftleField = tmp;

      float GetCoordValue2D(int ix, int iy)
      {
        if ((ix * (ix - lenX)) >= 0 || (iy * (iy - lenY)) >= 0) return 0;
        else return ftleField[ix, iy, 0];
      }
    }
  }
}