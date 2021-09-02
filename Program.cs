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
      // SampleClear();
      InteractiveSystem();
      // CalcDataset();
    }

    static void SampleClear()
    {
      // string folderPath = string.Format("./data/noVC_256x256x1/velocity");
      string folderPath = string.Format("./data/Buoyancy_noVC_256x256x1/velocity");
      ClearFTLE cFTLE = new ClearFTLE(folderPath, 1000, -1, 256);
      bool isNormalize = false;
      bool isSkeletonize = true;

      for (int t = 100; t < 951; t += 50)
      {
        Console.WriteLine("Start FTLE Calculation : t = {0}", t);
        cFTLE.CalcFTLE(t);
        // ftle.ShowFTLE(t);
        string outputFTLE = MATLAB_FTLE_Path + '/' + string.Format("ftle-{0}.txt", t);
        string outputLCS = MATLAB_LCS_Path + '/' + string.Format("lcs-{0}.txt", t);
        string outputClassification = string.Format("./data/LCS/class-{0}.txt", t);
        LCS lcs = new LCS(cFTLE.GetFTLE(t), isNormalize);
        // lcs.GaussianFilter(1);
        // lcs.SobelFilter2D();
        // lcs.LcsByHessian(0.01f, 0.1f, isSkeletonize, 5);
        // lcs.LcsByThreshold();
        // lcs.WriteLCS(outputLCS, cFTLE.GetOriginalPos());
        // lcs.WriteFTLE(outputFTLE, cFTLE.GetOriginalPos());
        // lcs.WriteClasscification(outputClassification, cFTLE.GetOriginalPos());
        cFTLE.WriteFTLE(outputFTLE, t);
        Console.WriteLine("End FTLE Calculation");
      }
    }

    static void Sample()
    {
      string folderPath = string.Format("./data/Buoyancy_VC_256x256x1/velocity");
      // string folderPath = string.Format("./data/ConstVelField_128x128x1_Wall");
      FTLE ftle = new FTLE(folderPath, 1000, -1, 256);
      bool isNormalize = false;
      bool isSkeletonize = true;

      for (int t = 100; t < 951; t += 50)
      {
        Console.WriteLine("Start FTLE Calculation : t = {0}", t);
        ftle.CalcFTLE(t);
        // ftle.ShowFTLE(t);
        string outputFTLE = MATLAB_FTLE_Path + '/' + string.Format("ftle-{0}.txt", t);
        string outputLCS = MATLAB_LCS_Path + '/' + string.Format("lcs-{0}.txt", t);
        string outputClassification = string.Format("./data/LCS/class-{0}.txt", t);
        LCS lcs = new LCS(ftle.GetFTLE(t), isNormalize);
        // lcs.GaussianFilter(5);
        // lcs.SobelFilter2D();
        // lcs.WriteFTLE(outputFTLE, ftle.GetOriginalPos());
        // lcs.LcsByHessian(0.01f, 0.1f, isSkeletonize, 10);
        // lcs.LcsByThreshold(0.5f);
        // lcs.WriteLCS(outputLCS, ftle.GetOriginalPos());
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
        FTLE ftle = new FTLE(folderPath, 500, -1, 128); //calculation backward
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
          lcs.GaussianFilter(5);
          lcs.LcsByHessian(0.01f, 0.5f, true, 20);
          lcs.WriteLCS(outputLCS, ftle.GetOriginalPos());
          ftle.WriteFTLE(outputFTLE, t);
          Console.WriteLine("End FTLE Calculation");
        }
        Console.WriteLine("End No.{0} Calculation", num);
      }
    }

    static void InteractiveSystem()
    {
      string input;
      Console.Write("Data Path ? : ");
      input = Console.ReadLine();
      string folderPath = input;
      ClearFTLE cFTLE = new ClearFTLE(folderPath, 1000, -1, 256);
      bool isNormalize = false;
      bool isSkeletonize = true;

      for (int t = 100; t < 951; t += 50)
      {
        Console.WriteLine("Start FTLE Calculation : t = {0}", t);
        cFTLE.CalcFTLE(t);
        // ftle.ShowFTLE(t);
        string outputFTLE = MATLAB_FTLE_Path + '/' + string.Format("ftle-{0}.txt", t);
        string outputLCS = MATLAB_LCS_Path + '/' + string.Format("lcs-{0}.txt", t);
        string outputClassification = string.Format("./data/LCS/class-{0}.txt", t);
        LCS lcs = new LCS(cFTLE.GetFTLE(t), isNormalize);
        // lcs.GaussianFilter(1);
        // lcs.SobelFilter2D();
        // lcs.LcsByHessian(0.01f, 0.1f, isSkeletonize, 5);
        // lcs.LcsByThreshold();
        // lcs.WriteLCS(outputLCS, cFTLE.GetOriginalPos());
        // lcs.WriteFTLE(outputFTLE, cFTLE.GetOriginalPos());
        // lcs.WriteClasscification(outputClassification, cFTLE.GetOriginalPos());
        cFTLE.WriteFTLE(outputFTLE, t);
        Console.WriteLine("End FTLE Calculation");
      }
    }
  }
}
