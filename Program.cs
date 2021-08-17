using System;
using System.Numerics;


/*
Using MAC Grid
*/

namespace Arihara.GuideSmoke
{
  class Program
  {
    static void Main(string[] args)
    {
      string folderPath = string.Format("./data/ObsSphere_64x64x1");
      FTLE ftle = new FTLE(folderPath, 1000, -1);
      for (int t = 200; t < 601; t += 50)
      {
        Console.WriteLine("Start FTLE Calculation : t = {0}", t);
        ftle.CalcFTLE(t);
        // ftle.ShowFTLE(t);
        string outputPath = string.Format("./data/FTLE/ftle-{0}.txt", t);
        ftle.WriteFTLE(outputPath, t);
        Console.WriteLine("End FTLE Calculation");
      }
    }
  }
}
