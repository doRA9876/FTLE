using System;

namespace Arihara.GuideSmoke
{
  class FluidRegion
  {
    float[,,] ftleField;
    float[][,,] densities;
    int lenX, lenY, lenZ;
    int startT, endT;

    public FluidRegion(ref float[,,] ftle, string densityFolderPath, int startT, int endT, int lenX, int lenY, int lenZ)
    {
      this.ftleField = ftle;
      this.lenX = lenX;
      this.lenY = lenY;
      this.lenZ = lenZ;
      this.startT = startT;
      this.endT = endT;
      for (int t = startT; t <= endT; t++)
      {
        densities = new float[(endT - startT) + 1][,,];
        string path = densityFolderPath + '/' + $"density-{t}.txt";
        if (!FileIO.LoadDensityFile(path, ref densities[t - startT])) Console.WriteLine("failed");
      }
    }

    public void ShowDensity()
    {
      for (int t = startT; t <= endT; t++)
      {
        Console.WriteLine($"t:{t}");
        Console.WriteLine(densities[t - startT][0, 0, 0]);
      }
    }
  }
}