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
        string outputFTLE = string.Format("./data/FTLE/ftle-{0}.txt", t);
        string outputLCS = string.Format("./data/LCS/lcs-{0}.txt", t);
        string outputClassification = string.Format("./data/LCS/class-{0}.txt", t);
        LCS lcs = new LCS(ftle.GetFTLE(t), false);
        lcs.FtleClassificationByHessian(5, 0.01f);
        // lcs.LcsByThreshold();
        // lcs.WriteLCS(outputLCS, ftle.GetOriginalPos());
        lcs.WriteClasscification(outputClassification, ftle.GetOriginalPos());
        ftle.WriteFTLE(outputFTLE, t);
        Console.WriteLine("End FTLE Calculation");
      }
    }
  }
}
