using System;
using System.IO;
using System.Numerics;

namespace FTLE
{
  static class FileIO
  {
    public static Vector3[,,] ReadFile(string path)
    {
      if (!File.Exists(path))
      {
        Console.WriteLine("Target File \"{0}\" is not exists.", path);
        return null;
      }

      string fileData = string.Empty;
      using (StreamReader sr = new StreamReader(path))
      {
        fileData = sr.ReadToEnd();
      }
      fileData = fileData.Replace(" ", "");
      fileData = fileData.Replace("[", "");
      fileData = fileData.Replace("]", "");
      fileData = fileData.Replace("=", ",");
      string[] culums = fileData.Split('\n');
      string[][] datas = new string[culums.Length - 1][];
      for (int i = 0; i < culums.Length - 1; i++)
      {
        datas[i] = culums[i].Split(',');

        // for (int j = 0; j < 6; j++)
        // {
        // 	// Console.Write(string.Format("i:{0}, j:{1}", i, j) + datas[i][j] + " ");
        // 	Console.Write(datas[i][j] + " ");
        // }
        // Console.WriteLine();
      }

      int maxX = 0, maxY = 0, maxZ = 0;
      for (int i = 0; i < culums.Length - 1; i++)
      {
        int x = int.Parse(datas[i][0]);
        int y = int.Parse(datas[i][1]);
        int z = int.Parse(datas[i][2]);
        if (maxX < x) maxX = x;
        if (maxY < y) maxY = y;
        if (maxZ < z) maxZ = z;
      }
      maxX += 1; maxY += 1; maxZ += 1;

      Vector3[,,] velocityField = new Vector3[maxX, maxY, maxZ];
      for (int i = 0; i < culums.Length - 1; i++)
      {
        int x = int.Parse(datas[i][0]);
        int y = int.Parse(datas[i][1]);
        int z = int.Parse(datas[i][2]);
        velocityField[x, y, z] = new Vector3(float.Parse(datas[i][3]), float.Parse(datas[i][4]), float.Parse(datas[i][5]));
      }

      // for (int x = 0; x < maxX; x++)
      // {
      //   for (int y = 0; y < maxY; y++)
      //   {
      //     for (int z = 0; z < maxZ; z++)
      //     {
      // 			Console.WriteLine(velocityField[x, y, z]);
      // 		}
      //   }
      // }
      return velocityField;
    }

    public static void WriteFile2D(int t, float[,,] ftleField)
    {
      string path = string.Format("./FTLE/ftle-{0}.txt", t);
      using (StreamWriter sw = new StreamWriter(path))
      {
        for (int x = 0; x < ftleField.GetLength(0); x++)
        {
          for (int y = 0; y < ftleField.GetLength(1); y++)
          {
            sw.WriteLine(string.Format("{0} {1} {2}", x, y, ftleField[x, y, 0]));
          }
        }
      }
    }
  }
}