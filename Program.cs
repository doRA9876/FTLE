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

    }

    static void FromJsonCalculation()
    {
      Parameter parameter = ReadJson("./parameter.json");
      CalculationByParameter(parameter);
    }

    static void InteractiveSystem()
    {
      Parameter p = new Parameter();

      string input_str;
      Console.Write("Data Path ? : ");
      input_str = Console.ReadLine();
      p.dataPath = input_str;

      Console.Write("Data Number ? : ");
      input_str = Console.ReadLine();
      p.dataNum = int.Parse(input_str);

      Console.Write("FTLE Resolution X ? : ");
      input_str = Console.ReadLine();
      p.ftleResolutionX = int.Parse(input_str);

      Console.Write("FTLE Resolution Y ? : ");
      input_str = Console.ReadLine();
      p.ftleResolutionY = int.Parse(input_str);

      Console.Write("FTLE Resolution Z ? : ");
      input_str = Console.ReadLine();
      p.ftleResolutionZ = int.Parse(input_str);

      ConsoleKeyInfo keyInfo;
      Console.Write("Backward Integration(Y / n) ? :");
      keyInfo = Console.ReadKey();
      Console.WriteLine();
      if (keyInfo.KeyChar == 'n')
      {
        p.direction = "Forward";
        Console.WriteLine("Forward Integration");
      }
      else
      {
        p.direction = "Backward";
        Console.WriteLine("Backward Integration");
      }

      CalculationByParameter(p);
    }

    static void CalculationByParameter(Parameter p)
    {
      FTLE ftle = new FTLE(p.dataPath, p.dataNum, p.deltaT, p.ftleResolutionX, p.ftleResolutionY, p.ftleResolutionZ);
      for (int t = p.startFrame; t <= p.endFrame; t += p.integralFrame)
      {
        Console.WriteLine("Start FTLE Calculation : t = {0}", t);
        ftle.CalcFTLE(t, p.direction, p.integralFrame, p.tau);
        string outputFTLEFile = p.outFTLEPath + '/' + string.Format("ftle-{0}.txt", t);
        string outputClassification = string.Format("./data/LCS/class-{0}.txt", t);
        ftle.WriteFTLE(outputFTLEFile, t);
        Console.WriteLine("End FTLE Calculation");
      }
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
  }
}
