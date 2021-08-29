using System;
using System.Numerics;

namespace Arihara.GuideSmoke
{
  public class ClearFTLE
  {
    const int FORWARD = 1;
    const int BACKWARD = -1;

    private int direction = BACKWARD;
    private int dimension = 2;

    Vector3[,,] originalPosition;
    Vector3[][,,] velocityField;
    float[][,,] ftleField;
    bool[] isCalculatedTime;
    bool[] isLoadFile;

    int x_min, x_max, y_min, y_max, z_min, z_max;


    int ftleResolution = 128;
    int delta_t = 1;
    int integral_T = 10;
    float perturbation = 0.3f;

    string dataFolderPath;
    int dataNum;


    #region Accessor
    public int FtleResolution
    {
      set { ftleResolution = value; }
    }
    public int Delta_t
    {
      set { delta_t = value; }
    }
    public int Integral_T
    {
      set { integral_T = value; }
    }
    #endregion


    public ClearFTLE(string folderPath, int fileNum, int d)
    {
      Constructor(folderPath, fileNum, d);
    }

    public ClearFTLE(string folderPath, int fileNum)
    {
      Constructor(folderPath, fileNum, FORWARD);
    }

    private void Constructor(string folderPath, int fileNum, int d)
    {
      if (d > 0) direction = FORWARD;
      else direction = BACKWARD;

      velocityField = new Vector3[fileNum][,,];
      ftleField = new float[fileNum][,,];
      isCalculatedTime = new bool[fileNum];
      isLoadFile = new bool[fileNum];

      dataNum = fileNum;
      dataFolderPath = folderPath;
      for (int n = 0; n < fileNum; n++)
      {
        isCalculatedTime[n] = false;
        isLoadFile[n] = false;
      }

      LoadData(0);
      x_min = 0; y_min = 0; z_min = 0;
      x_max = velocityField[0].GetLength(0) - 1;
      y_max = velocityField[0].GetLength(1) - 1;
      z_max = velocityField[0].GetLength(2) - 1;

      if (z_max == 0) dimension = 2;
      if (z_max > 0) dimension = 3;
    }

    public void CalcFTLE(int t)
    {
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
      if (!isCalculatedTime[t]) CalcFTLE(t);
      if (dimension == 2)
        FileIO.WriteFTLEFile(path, t, originalPosition, ftleField[t], ftleResolution, ftleResolution, 1);
      if (dimension == 3)
        FileIO.WriteFTLEFile(path, t, originalPosition, ftleField[t], ftleResolution, ftleResolution, ftleResolution);
    }

    public void ShowFTLE(int t)
    {
      if (!isCalculatedTime[t]) CalcFTLE(t);
      for (int ix = 0; ix < ftleResolution; ix++)
      {
        for (int iy = 0; iy < ftleResolution; iy++)
        {
          if (dimension == 2)
          {
            Vector3 pos = originalPosition[ix, iy, 0];
            Console.WriteLine("{0} {1} {2}", pos.X, pos.Y, ftleField[t][ix, iy, 0]);
          }

          if (dimension == 3)
          {
            for (int iz = 0; iz < ftleResolution; iz++)
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
      if (!FileIO.ReadVelocityFile(path, ref velocityField[t])) Environment.Exit(0);
      isLoadFile[t] = true;
    }

    #region Implementation for 2D FTLE
    float[,,] FTLE2D(int t)
    {
      float[,,] ftle = new float[ftleResolution, ftleResolution, 1];
      Vector3[,,,] pos = new Vector3[ftleResolution, ftleResolution, 1, 4];
      Vector3[,,,] new_pos = new Vector3[ftleResolution, ftleResolution, 1, 4];
      bool[,,] leftDomain = new bool[ftleResolution, ftleResolution, 1];
      originalPosition = new Vector3[ftleResolution, ftleResolution, 1];
      for (int ix = 0; ix < ftleResolution; ix++)
      {
        for (int iy = 0; iy < ftleResolution; iy++)
        {
          float x = (float)ix * x_max / (ftleResolution - 2) + 0.5f;
          float y = (float)iy * y_max / (ftleResolution - 2) + 0.5f;
          originalPosition[ix, iy, 0] = new Vector3(x, y, 0);
          pos[ix, iy, 0, 0] = new Vector3(x + perturbation, y, 0); // Pr=(i+τ, j)
          pos[ix, iy, 0, 1] = new Vector3(x - perturbation, y, 0); // Pl=(i-τ, j)
          pos[ix, iy, 0, 2] = new Vector3(x, y + perturbation, 0); // Pu=(i, j+τ)
          pos[ix, iy, 0, 3] = new Vector3(x, y - perturbation, 0); // Pd=(i, j-τ)
          new_pos[ix, iy, 0, 0] = new Vector3(x + perturbation, y, 0);
          new_pos[ix, iy, 0, 1] = new Vector3(x - perturbation, y, 0);
          new_pos[ix, iy, 0, 2] = new Vector3(x, y + perturbation, 0);
          new_pos[ix, iy, 0, 3] = new Vector3(x, y - perturbation, 0);
          leftDomain[ix, iy, 0] = false;
        }
      }

      for (int ti = 0; ti < integral_T; ti++)
      {
        int t0 = t + ti * direction;
        LoadData(t0);
      }

      for (int t_integration = 0; t_integration < integral_T - delta_t; t_integration = t_integration + delta_t)
      {
        int t0 = t + t_integration * direction;
        int t1 = t0 + delta_t * direction;

        if (t0 < 0) continue;

        for (int ix = 0; ix < ftleResolution; ix++)
        {
          for (int iy = 0; iy < ftleResolution; iy++)
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

        for (int ix = 0; ix < ftleResolution; ix++)
        {
          for (int iy = 0; iy < ftleResolution; iy++)
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

        for (int ix = 0; ix < ftleResolution; ix++)
        {
          for (int iy = 0; iy < ftleResolution; iy++)
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

      for (int ix = 0; ix < ftleResolution; ix++)
      {
        for (int iy = 0; iy < ftleResolution; iy++)
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
      if ((ix * (ix - (ftleResolution - 1))) < 0 && (iy * (iy - (ftleResolution - 1))) < 0)
      {
        /*
        00 01
        10 11
        */
        float a00 = (flowmap[ix, iy, 0, 0].X - flowmap[ix, iy, 0, 1].X) / (2 * perturbation);
        float a01 = (flowmap[ix, iy, 0, 2].X - flowmap[ix, iy, 0, 3].X) / (2 * perturbation);
        float a10 = (flowmap[ix, iy, 0, 0].Y - flowmap[ix, iy, 0, 1].Y) / (2 * perturbation);
        float a11 = (flowmap[ix, iy, 0, 2].Y - flowmap[ix, iy, 0, 3].Y) / (2 * perturbation);

        float[,] tensor2D = new float[2, 2];
        tensor2D[0, 0] = (float)Math.Pow(a00, 2) + (float)Math.Pow(a01, 2);
        tensor2D[1, 0] = tensor2D[0, 1] = a00 * a01 + a10 * a11;
        tensor2D[1, 1] = (float)Math.Pow(a11, 2) + (float)Math.Pow(a10, 2);

        double result = (float)Math.Log(Eigen.GetMaxEigenValue2x2(tensor2D)) / Math.Abs(integral_T / delta_t);

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
      int delta = (t_end - t_start);

      // Vector3 velocity = Lerp2D(t_start, position);
      Vector3 velocity = RungeKutta(t_start, position);
      return position + velocity * direction * delta * h;

      Vector3 RungeKutta(int t, Vector3 pos)
      {
        float dt = delta_t * direction;
        Vector3 k1 = Lerp2D(t, pos);
        Vector3 x2 = pos + k1 * dt / 2;
        Vector3 k2 = Lerp2D((int)(t + dt / 2), x2);
        Vector3 x3 = pos + k2 * dt / 2;
        Vector3 k3 = Lerp2D((int)(t + dt / 2), x3);
        Vector3 x4 = pos + k3 * dt;
        Vector3 k4 = Lerp2D((int)(t + dt), x4);
        return (k1 + k2 + k3 + k4) / 6;
      }
    }

    Vector3 Lerp2D(int t, Vector3 pos)
    {
      float x = pos.X;
      float y = pos.Y;
      int x0 = (int)Math.Round(x);
      int y0 = (int)Math.Round(y);

      if ((x - x_min) * (x - x_max) >= 0 || (y - y_min) * (y - y_max) >= 0) return Vector3.Zero;

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
  }
}