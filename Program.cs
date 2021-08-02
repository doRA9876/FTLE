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
      int t = 510;
      string folderPath = string.Format("./data/ObsSphere");
      string outputPath = string.Format("./data/FTLE/ftle-{0}.txt", t);
      FTLE ftle = new FTLE(folderPath, 1000, -1);
      ftle.CalcFTLE(t);
      ftle.WriteFTLE(outputPath, t);
    }
  }
}
