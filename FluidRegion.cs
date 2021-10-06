using System;

namespace Arihara.GuideSmoke
{
  class FluidRegion
  {
    float[,,] ftleField;
    float[][,,] densities;
    int lenX, lenY, lenZ;
    int startT, endT;

    #region Accessor
    public float[,,] FTLEField
    {
      get { return ftleField; }
    }
    #endregion

    public FluidRegion(ref float[,,] ftle, string densityFolderPath, int startT, int endT, int lenX, int lenY, int lenZ)
    {
      this.ftleField = ftle;
      this.lenX = lenX;
      this.lenY = lenY;
      this.lenZ = lenZ;
      this.startT = startT;
      this.endT = endT;
      densities = new float[(endT - startT) + 1][,,];
      for (int t = startT; t <= endT; t++)
      {
        string path = densityFolderPath + '/' + $"density-{t}.txt";
        if (!FileIO.LoadDensityFile(path, ref densities[t - startT])) Console.WriteLine("failed");
      }
    }

    public void Integrate()
    {
      bool[,,] region = new bool[lenX, lenY, lenZ];

      for (int t = startT; t <= endT; t++)
      {
        float[,,] density = densities[t - startT];
        for (int ix = 0; ix < lenX; ix++)
        {
          for (int iy = 0; iy < lenY; iy++)
          {
            for (int iz = 0; iz < lenZ; iz++)
            {
              if (density[ix, iy, iz] > 0) region[ix, iy, iz] = true;
            }
          }
        }
      }

      for (int ix = 0; ix < lenX; ix++)
      {
        for (int iy = 0; iy < lenY; iy++)
        {
          for (int iz = 0; iz < lenZ; iz++)
          {
            if (!region[ix,iy,iz]) ftleField[ix, iy, iz] = 0f;
          }
        }
      }
    }

    public void ShowDensity()
    {
      for (int t = startT; t <= endT; t++)
      {
        Console.WriteLine($"t:{t}");
        for (int ix = 0; ix < lenX; ix++)
        {
          for (int iy = 0; iy < lenY; iy++)
          {
            for (int iz = 0; iz < lenZ; iz++)
            {
              Console.WriteLine($"{ix} {iy} {iz}  {densities[t - startT][ix, iy, iz]}");
            }
          }
        }
      }
    }
  }
}