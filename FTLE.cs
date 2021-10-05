using System;
using System.Numerics;

namespace Arihara.GuideSmoke
{
  public class FTLE : IDisposable
  {
    private int direction = 0;
    private int dimension = 2;

    Vector3[,,] originalPosition;
    Vector3[][,,] velocityField;
    float[][,,] ftleField;
    bool[] isCalculatedTime;
    bool[] isLoadFile;

    int x_min, x_max, y_min, y_max, z_min, z_max;


    int ftleResolutionX = 256;
    int ftleResolutionY = 256;
    int ftleResolutionZ = 256;
    float delta_t = 0.1f;
    float integral_Frame = 5;
    float tau = 0.5f;

    string dataFolderPath;
    int dataNum;


    public FTLE(string folderPath, int fileNum, float dt, int lenX, int lenY, int lenZ)
    {
      this.ftleResolutionX = lenX;
      this.ftleResolutionY = lenY;
      this.ftleResolutionZ = lenZ;
      Constructor(folderPath, fileNum, dt);
    }

    public FTLE(string folderPath, int fileNum, float dt)
    {
      Constructor(folderPath, fileNum, dt);
    }

    private void Constructor(string folderPath, int fileNum, float dt)
    {
      velocityField = new Vector3[fileNum][,,];
      ftleField = new float[fileNum][,,];
      isCalculatedTime = new bool[fileNum];
      isLoadFile = new bool[fileNum];

      dataNum = fileNum;
      dataFolderPath = folderPath;
      delta_t = dt;
      for (int n = 0; n < fileNum; n++)
      {
        isCalculatedTime[n] = false;
        isLoadFile[n] = false;
      }

      LoadData(fileNum - 1);
      x_min = 0; y_min = 0; z_min = 0;
      x_max = velocityField[fileNum - 1].GetLength(0) - 1;
      y_max = velocityField[fileNum - 1].GetLength(1) - 1;
      z_max = velocityField[fileNum - 1].GetLength(2) - 1;

      if (z_max == 0) dimension = 2;
      if (z_max > 0) dimension = 3;
    }

    public void CalcFTLE(int t, string d, int integralF, float pt)
    {
      direction = 0;
      if (string.Equals(d, "Forward")) direction = 1;
      if (string.Equals(d, "Backward")) direction = -1;
      if (direction == 0)
      {
        Console.WriteLine($"Specified direction (d = {d}) is incorrect");
        return;
      }
      this.integral_Frame = integralF;
      this.tau = pt;

      if (dimension == 2) ftleField[t] = FTLE2D(t);
      isCalculatedTime[t] = true;
    }

    public float[,,] GetFTLE(int t)
    {
      return ftleField[t];
    }

    public Vector3[,,] GetOriginalPos()
    {
      return originalPosition;
    }

    public void WriteFTLE(string path, int t)
    {
      if (!isCalculatedTime[t])
      {
        Console.WriteLine($"Specified time (t = {t} has not been calculated.");
        return;
      }
      FileIO.WriteFTLEFile(path, originalPosition, ftleField[t], ftleResolutionX, ftleResolutionY, ftleResolutionZ);
    }

    public void ShowFTLE(int t)
    {
      if (!isCalculatedTime[t])
      {
        Console.WriteLine($"Specified time (t = {t} has not been calculated.");
        return;
      }
      for (int ix = 0; ix < ftleResolutionX; ix++)
      {
        for (int iy = 0; iy < ftleResolutionY; iy++)
        {
          if (dimension == 2)
          {
            Vector3 pos = originalPosition[ix, iy, 0];
            Console.WriteLine("{0} {1} {2}", pos.X, pos.Y, ftleField[t][ix, iy, 0]);
          }

          if (dimension == 3)
          {
            for (int iz = 0; iz < ftleResolutionZ; iz++)
            {
              Vector3 pos = originalPosition[ix, iy, iz];
              Console.WriteLine("{0} {1} {2} {3}", pos.X, pos.Y, pos.Z, ftleField[t][ix, iy, iz]);
            }
          }
        }
      }
    }

    private void LoadData(int t)
    {
      if (isLoadFile[t]) return;
      if (t < 0 || dataNum - 1 < t) return;

      string path = string.Format(dataFolderPath + "/vel-{0}.txt", t);
      if (!FileIO.LoadVelocityFile(path, ref velocityField[t])) Environment.Exit(0);
      isLoadFile[t] = true;
    }

    #region Implementation for 2D FTLE
    float[,,] FTLE2D(int t)
    {
      float[,,] ftle = new float[ftleResolutionX, ftleResolutionY, 1];
      Vector3[,,,] pos = new Vector3[ftleResolutionX, ftleResolutionY, 1, 4];
      Vector3[,,,] new_pos = new Vector3[ftleResolutionX, ftleResolutionY, 1, 4];
      bool[,,] leftDomain = new bool[ftleResolutionX, ftleResolutionY, 1];
      originalPosition = new Vector3[ftleResolutionX, ftleResolutionY, 1];
      for (int ix = 0; ix < ftleResolutionX; ix++)
      {
        for (int iy = 0; iy < ftleResolutionY; iy++)
        {
          float x = (float)ix * x_max / (ftleResolutionX - 2) + 0.5f;
          float y = (float)iy * y_max / (ftleResolutionY - 2) + 0.5f;
          originalPosition[ix, iy, 0] = new Vector3(x, y, 0);
          pos[ix, iy, 0, 0] = new Vector3(x + tau, y, 0); // Pr=(i+τ, j)
          pos[ix, iy, 0, 1] = new Vector3(x - tau, y, 0); // Pl=(i-τ, j)
          pos[ix, iy, 0, 2] = new Vector3(x, y + tau, 0); // Pu=(i, j+τ)
          pos[ix, iy, 0, 3] = new Vector3(x, y - tau, 0); // Pd=(i, j-τ)
          new_pos[ix, iy, 0, 0] = new Vector3(x + tau, y, 0);
          new_pos[ix, iy, 0, 1] = new Vector3(x - tau, y, 0);
          new_pos[ix, iy, 0, 2] = new Vector3(x, y + tau, 0);
          new_pos[ix, iy, 0, 3] = new Vector3(x, y - tau, 0);
          leftDomain[ix, iy, 0] = false;
        }
      }

      for (int t_integration = 0; t_integration < integral_Frame; t_integration++)
      {
        int t0 = t + t_integration * direction;
        int t1 = t0 + direction;

        if (t0 < 0) continue;

        for (int ix = 0; ix < ftleResolutionX; ix++)
        {
          for (int iy = 0; iy < ftleResolutionY; iy++)
          {
            if (!leftDomain[ix, iy, 0])
            {
              new_pos[ix, iy, 0, 0] = Trace2D(t0, t1, delta_t, pos[ix, iy, 0, 0]);
              new_pos[ix, iy, 0, 1] = Trace2D(t0, t1, delta_t, pos[ix, iy, 0, 1]);
              new_pos[ix, iy, 0, 2] = Trace2D(t0, t1, delta_t, pos[ix, iy, 0, 2]);
              new_pos[ix, iy, 0, 3] = Trace2D(t0, t1, delta_t, pos[ix, iy, 0, 3]);
            }
          }
        }

        for (int ix = 0; ix < ftleResolutionX; ix++)
        {
          for (int iy = 0; iy < ftleResolutionY; iy++)
          {
            if (leftDomain[ix, iy, 0] == false)
            {
              if (isLeftDomain2D(new_pos[ix, iy, 0, 0]) ||
                  isLeftDomain2D(new_pos[ix, iy, 0, 1]) ||
                  isLeftDomain2D(new_pos[ix, iy, 0, 2]) ||
                  isLeftDomain2D(new_pos[ix, iy, 0, 3]))
              {
                leftDomain[ix, iy, 0] = true;
                ftle[ix, iy, 0] = CalculateFTLE2D(pos, ix, iy);
              }
            }
            else
            {
              new_pos[ix, iy, 0, 0] = pos[ix, iy, 0, 0];
              new_pos[ix, iy, 0, 1] = pos[ix, iy, 0, 1];
              new_pos[ix, iy, 0, 2] = pos[ix, iy, 0, 2];
              new_pos[ix, iy, 0, 3] = pos[ix, iy, 0, 3];
            }
          }
        }

        for (int ix = 0; ix < ftleResolutionX; ix++)
        {
          for (int iy = 0; iy < ftleResolutionY; iy++)
          {
            if (leftDomain[ix, iy, 0] == false)
            {
              pos[ix, iy, 0, 0] = new_pos[ix, iy, 0, 0];
              pos[ix, iy, 0, 1] = new_pos[ix, iy, 0, 1];
              pos[ix, iy, 0, 2] = new_pos[ix, iy, 0, 2];
              pos[ix, iy, 0, 3] = new_pos[ix, iy, 0, 3];
            }
          }
        }
      }

      for (int ix = 0; ix < ftleResolutionX; ix++)
      {
        for (int iy = 0; iy < ftleResolutionY; iy++)
        {
          ftle[ix, iy, 0] = CalculateFTLE2D(pos, ix, iy);
        }
      }
      return ftle;
    }

    bool isLeftDomain2D(Vector3 pos)
    {
      if (((pos.X - x_min) * (pos.X - x_max)) >= 0 || ((pos.Y - y_min) * (pos.Y - y_max)) >= 0) return true;
      else return false;
    }

    float CalculateFTLE2D(Vector3[,,,] flowmap, int ix, int iy)
    {
      if ((ix * (ix - (ftleResolutionX - 1))) < 0 && (iy * (iy - (ftleResolutionY - 1))) < 0)
      {
        /*
        a b
        c d
        */
        float a = (flowmap[ix, iy, 0, 0].X - flowmap[ix, iy, 0, 1].X) / (2 * tau);
        float b = (flowmap[ix, iy, 0, 2].X - flowmap[ix, iy, 0, 3].X) / (2 * tau);
        float c = (flowmap[ix, iy, 0, 0].Y - flowmap[ix, iy, 0, 1].Y) / (2 * tau);
        float d = (flowmap[ix, iy, 0, 2].Y - flowmap[ix, iy, 0, 3].Y) / (2 * tau);

        float[,] tensor2D = new float[2, 2];
        tensor2D[0, 0] = (float)Math.Pow(a, 2) + (float)Math.Pow(c, 2);
        tensor2D[1, 0] = tensor2D[0, 1] = a * b + c * d;
        tensor2D[1, 1] = (float)Math.Pow(b, 2) + (float)Math.Pow(d, 2);

        double result = (float)Math.Log(Math.Sqrt(Eigen.GetMaxEigenValue2x2(tensor2D))) / Math.Abs(integral_Frame * delta_t);

        if (result != double.NegativeInfinity) return (float)result;
        else return 0;
      }
      else
      {
        return 0;
      }
    }

    Vector3 Trace2D(int t_start, int t_end, float h, Vector3 position)
    {
      int delta = Math.Abs((t_end - t_start));

      // Vector3 velocity = Lerp2D(t_start, position) * direction;
      Vector3 velocity = RungeKutta(t_start, position);
      return position + velocity * delta * h;

      Vector3 RungeKutta(int t, Vector3 pos)
      {
        Vector3 v1 = Lerp2D(t, pos) * direction; //Lerp(t, pos)
        Vector3 v2 = Lerp2D(t, pos + (v1 * h) / 2) * direction; //Lerp(t, pos)
        Vector3 v3 = Lerp2D(t + direction, pos + (v2 * h) / 2) * direction; //Lerp(t + direction, pos)
        Vector3 v4 = Lerp2D(t + direction, pos + (v3 * h)) * direction; //Lerp(t + direction, pos)
        return (v1 + 2 * v2 + 2 * v3 + v4) / 6;
      }
    }

    Vector3 Lerp2D(int t, Vector3 pos)
    {
      float x = pos.X;
      float y = pos.Y;
      int x0 = (int)Math.Round(x);
      int y0 = (int)Math.Round(y);

      if ((x - x_min) * (x - x_max) >= 0 || (y - y_min) * (y - y_max) >= 0) return Vector3.Zero;
      if (!isLoadFile[t]) LoadData(t);

      int x1, y1;
      if ((x - x0) < 0)
        x1 = x0 - 1;
      else
        x1 = x0 + 1;

      if ((y - y0) < 0)
        y1 = y0 - 1;
      else
        y1 = y0 + 1;

      Vector3 v00 = velocityField[t][x0, y0, 0];
      Vector3 v10 = velocityField[t][x1, y0, 0];
      Vector3 v01 = velocityField[t][x0, y1, 0];

      float delta_x = Math.Abs(x - x1);
      float delta_y = Math.Abs(y - y1);
      float dx = Math.Abs(x1 - x0);
      float dy = Math.Abs(y1 - y0);

      return v00 + (v10 - v00) / dx * delta_x + (v01 - v00) / dy * delta_y;
    }
    #endregion

    public void Dispose() { }
  }
}