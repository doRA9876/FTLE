using System;
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
      LcsByThreshold(CalcMedian());
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
      return 0;
    }
  }
}