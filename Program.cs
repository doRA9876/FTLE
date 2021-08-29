using System;
using System.IO;
using System.Numerics;


/*
Using MAC Grid
*/

namespace Arihara.GuideSmoke
{
  class Program
  {
    static string MATLAB_FTLE_Path = "D:/Projects/MATLAB/FTLE/results";
    static string MATLAB_LCS_Path = "D:/Projects/MATLAB/LCS/results";
    static void Main(string[] args)
    {
      // Sample();
      SampleClear();
      // CalcDataset();
    }

    static void SampleClear()
    {
      // string folderPath = string.Format("./data/ConstVelField_128x128x1");
      string folderPath = string.Format("./data/ObsSphere_64x64x1_VC");
      ClearFTLE cFTLE = new ClearFTLE(folderPath, 1000, -1);

      for (int t = 250; t < 651; t += 10)
      {
        Console.WriteLine("Start FTLE Calculation : t = {0}", t);
        cFTLE.CalcFTLE(t);
        // ftle.ShowFTLE(t);
        string outputFTLE = MATLAB_FTLE_Path + '/' + string.Format("ftle-{0}.txt", t);
        string outputLCS = MATLAB_LCS_Path + '/' + string.Format("lcs-{0}.txt", t);
        string outputClassification = string.Format("./data/LCS/class-{0}.txt", t);
        LCS lcs = new LCS(cFTLE.GetFTLE(t), false);
        lcs.LcsByHessian(5, 0.01f, 0.5f, false, 20);
        // lcs.LcsByThreshold();
        lcs.WriteLCS(outputLCS, cFTLE.GetOriginalPos());
        // lcs.WriteFTLE(outputFTLE, cFTLE.GetOriginalPos());
        // lcs.WriteClasscification(outputClassification, cFTLE.GetOriginalPos());
        cFTLE.WriteFTLE(outputFTLE, t);
        Console.WriteLine("End FTLE Calculation");
      }
    }

    static void Sample()
    {
      string folderPath = string.Format("./data/ConstVelField_128x128x1");
      // string folderPath = string.Format("./data/ObsSphere_64x64x1");
      FTLE ftle = new FTLE(folderPath, 1000, 1);
      bool isNormalize = true;
      bool isSkeletonize = false;

      for (int t = 250; t < 651; t += 10)
      {
        Console.WriteLine("Start FTLE Calculation : t = {0}", t);
        ftle.CalcFTLE(t);
        // ftle.ShowFTLE(t);
        string outputFTLE = MATLAB_FTLE_Path + '/' + string.Format("ftle-{0}.txt", t);
        string outputLCS = MATLAB_LCS_Path + '/' + string.Format("lcs-{0}.txt", t);
        string outputClassification = string.Format("./data/LCS/class-{0}.txt", t);
        LCS lcs = new LCS(ftle.GetFTLE(t), isNormalize);
        lcs.LcsByHessian(0, 0.01f, 0.3f, isSkeletonize, 5);
        // lcs.LcsByThreshold(0.5f);
        lcs.WriteLCS(outputLCS, ftle.GetOriginalPos());
        // lcs.WriteFTLE(outputFTLE, ftle.GetOriginalPos());
        // lcs.WriteClasscification(outputClassification, ftle.GetOriginalPos());
        ftle.WriteFTLE(outputFTLE, t);
        Console.WriteLine("End FTLE Calculation");
      }
    }

    static void CalcDataset()
    {
      for (int num = 1; num <= 1000; num++)
      {
        Console.WriteLine("Start No.{0} Calculation", num);

        string folderPath = string.Format("./data/DataSet/DataSet/{0}/velocity", num);
        FTLE ftle = new FTLE(folderPath, 500, -1); //calculation backward
        for (int t = 50; t < 500; t += 50)
        {
          Console.WriteLine("Start FTLE Calculation : t = {0}", t);
          string dirFTLE = string.Format("./data/DataSet/FTLE/{0}", num);
          string dirLCS = string.Format("./data/DataSet/LCS/{0}", num);
          Directory.CreateDirectory(dirFTLE);
          Directory.CreateDirectory(dirLCS);
          ftle.CalcFTLE(t);
          string outputFTLE = dirFTLE + "/" + string.Format("ftle-{0}.txt", t);
          string outputLCS = dirLCS + "/" + string.Format("lcs-{0}.txt", t);
          LCS lcs = new LCS(ftle.GetFTLE(t), false);
          lcs.LcsByHessian(5, 0.01f, 0.5f, true, 20);
          lcs.WriteLCS(outputLCS, ftle.GetOriginalPos());
          ftle.WriteFTLE(outputFTLE, t);
          Console.WriteLine("End FTLE Calculation");
        }
        Console.WriteLine("End No.{0} Calculation", num);
      }
    }
  }
}
