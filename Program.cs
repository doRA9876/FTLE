using System;
using System.IO;
using System.Text.Json;
using System.Text;
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

    const int LCS_Hessian = 0;
    const int LCS_Threshod = 1;
    const int LCS_Median = 2;

    static void Main(string[] args)
    {
      Console.Write("Interactive System (0) or From json calculation (1) ? : ");
      string input = Console.ReadLine();
      int select = int.Parse(input);

      switch (select)
      {
        case 1:
          FromJsonCalculation();
          break;

        default:
          InteractiveSystem();
          break;
      }

      // Sample();
      // SampleClear();
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

    static void FromJsonCalculation()
    {
      Parameter parameter = ReadJson("./parameter.json");
      CalculationByParameter(parameter);
    }

    static Parameter ReadJson(string filePath)
    {
      string jsonStr;
      using (StreamReader sr = new StreamReader(filePath, Encoding.GetEncoding("utf-8")))
      {
        jsonStr = sr.ReadToEnd();
      }

      Console.WriteLine(jsonStr);

      Parameter parameter = new Parameter();
      parameter = JsonSerializer.Deserialize<Parameter>(jsonStr);

      return parameter;
    }

    static void InteractiveSystem()
    {
      Parameter parameter = new Parameter();

      string input_str;
      Console.Write("Data Path ? : ");
      input_str = Console.ReadLine();
      parameter.dataPath = input_str;

      Console.Write("FTLE Resolution ? : ");
      input_str = Console.ReadLine();
      parameter.ftleResolution = int.Parse(input_str);

      ConsoleKeyInfo keyInfo;
      Console.Write("Backward Integration(Y / n) ? :");
      keyInfo = Console.ReadKey();
      Console.WriteLine();
      if (keyInfo.KeyChar == 'n')
      {
        parameter.direction = 1;
        Console.WriteLine("Forward Integration");
      }
      else
      {
        parameter.direction = -1;
        Console.WriteLine("Backward Integration");
      }

      Console.Write("LCS Calculation(y / N) ? :");
      keyInfo = Console.ReadKey();
      Console.WriteLine();
      if (keyInfo.KeyChar == 'y')
      {
        parameter.isLcsCalculation = true;
        Console.WriteLine("LCS Calculation : Y");

        Console.Write("Normalize(y / N) ? :");
        keyInfo = Console.ReadKey();
        Console.WriteLine();
        if (keyInfo.KeyChar == 'y')
        {
          parameter.isNormalize = true;
          Console.WriteLine("Normalize : Y");
        }
        else
        {
          parameter.isNormalize = false;
          Console.WriteLine("Normalize : N");
        }

        Console.Write("LCS Method ? (Hessian:0, Threshold:1, Median:2):");
        input_str = Console.ReadLine();
        parameter.LcsMethod = int.Parse(input_str);

        Console.Write("How many times do you apply the Gaussian Filter ? :");
        input_str = Console.ReadLine();
        parameter.gaussianNum = int.Parse(input_str);

        Console.Write("How many times do you apply the Sobel Filter ? :");
        input_str = Console.ReadLine();
        parameter.sobelNum = int.Parse(input_str);

        if (parameter.LcsMethod == 0)
        {
          Console.Write("Skeletonize(y / N) ? :");
          keyInfo = Console.ReadKey();
          Console.WriteLine();
          if (keyInfo.KeyChar == 'y')
          {
            parameter.isSkeletonize = true;
            Console.WriteLine("Skeletonize : Y");

            Console.Write("How many Skeletonize Num ?:");
            input_str = Console.ReadLine();
            parameter.skeletonizeNum = int.Parse(input_str);
          }
          else
          {
            parameter.isSkeletonize = false;
            Console.WriteLine("Skeletonize : N");
          }

          Console.Write("How many kappa value ? :");
          input_str = Console.ReadLine();
          parameter.kappa = float.Parse(input_str);

          Console.Write("How many threshold are there ? :");
          input_str = Console.ReadLine();
          parameter.lcsThreshold = float.Parse(input_str);
        }

        if (parameter.LcsMethod == 1)
        {
          Console.Write("How many threshold are there ?:");
          input_str = Console.ReadLine();
          parameter.lcsThreshold = float.Parse(input_str);
        }
      }
      else
      {
        parameter.isLcsCalculation = false;
        Console.WriteLine("LCS Calculation : N");
      }

      CalculationByParameter(parameter);
    }

    static void CalculationByParameter(Parameter param)
    {
      ClearFTLE cFTLE = new ClearFTLE(param.dataPath, 1000, param.direction, param.ftleResolution);
      for (int t = param.startT; t <= param.endT; t += param.integralT)
      {
        Console.WriteLine("Start FTLE Calculation : t = {0}", t);
        cFTLE.CalcFTLE(t);
        // ftle.ShowFTLE(t);
        string outputFTLEFile = param.outputFTLEPath + '/' + string.Format("ftle-{0}.txt", t);
        string outputLCSFile = param.outputLCSPath + '/' + string.Format("lcs-{0}.txt", t);
        string outputClassification = string.Format("./data/LCS/class-{0}.txt", t);
        if (param.isLcsCalculation)
        {
          LCS lcs = new LCS(cFTLE.GetFTLE(t), param.isNormalize);
          lcs.GaussianFilter(param.gaussianNum);
          if (param.sobelNum > 0) lcs.SobelFilter2D();
          switch (param.LcsMethod)
          {
            case 1:
              lcs.LcsByThreshold(param.lcsThreshold);
              break;

            case 2:
              lcs.LcsByThreshold();
              break;

            default:
              lcs.LcsByHessian(param.kappa, param.lcsThreshold, param.isSkeletonize, param.skeletonizeNum);
              break;
          }
          lcs.WriteLCS(outputLCSFile, cFTLE.GetOriginalPos());
        }
        // lcs.WriteFTLE(outputFTLE, cFTLE.GetOriginalPos());
        // lcs.WriteClasscification(outputClassification, cFTLE.GetOriginalPos());
        cFTLE.WriteFTLE(outputFTLEFile, t);
        Console.WriteLine("End FTLE Calculation");
      }
    }
  }
}
