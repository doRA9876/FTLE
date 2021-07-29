using System;
using System.Numerics;


/*
Using MAC Grid
*/

namespace FTLE
{
  class Program
  {
    const int FORWARD = 1;
    const int BACKWARD = -1;

    static int fileNum = 1000;

    static int velResolution = 32;
    static int ftleResolution =32;

    static int direction = BACKWARD;

    static float delta_t = 2f;
    static float integral_T = 2;
    static float perturbation = 0.1f;

    static Vector3[][,,] velocityField;
    static float[][,,] ftleField;

    static void Main(string[] args)
    {
      velocityField = new Vector3[fileNum][,,];
      ftleField = new float[fileNum][,,];

      for (int n = 0; n < fileNum; n++)
      // for (int n = 0; n < 60; n++)
      {
        string path = string.Format("./data/vel-{0}.txt", n);
        velocityField[n] = FileIO.ReadFile(path);
      }

      // for (int x = 0; x < velocityField.GetLength(0); x++)
      // {
      //   for (int y = 0; y < velocityField.GetLength(1); y++)
      //   {
      //     for (int z = 0; z < velocityField.GetLength(2); z++)
      //     {
      // 			Console.WriteLine(velocityField[x, y, z]);
      // 		}
      //   }
      // }
      ftleField[500] = FTLE2D(500);
      // for (int x = 0; x < ftleResolution; x++)
      // {
      //   for (int y = 0; y < ftleResolution; y++)
      //   {
      //     for (int z = 0; z < 1; z++)
      //     {
      //       Console.WriteLine(ftleField[10][x, y, z]);
      //     }
      //   }
      // }
      FileIO.WriteFile2D(500, ftleField[500]);

      // Vector3 v = new Vector3(8.1f, 11, 0);
      // Trace2D(50, ref v);
      // Console.WriteLine(v);
    }

    static float[,,] FTLE2D(int t)
    {
      float[,,] ftle = new float[ftleResolution, ftleResolution, 1];
      for (int x = 0; x < ftleResolution; x++)
      {
        for (int y = 0; y < ftleResolution; y++)
        {
          Vector3 pl = new Vector3(x - perturbation, y, 0);
          Vector3 pr = new Vector3(x + perturbation, y, 0);
          Vector3 pu = new Vector3(x, y + perturbation, 0);
          Vector3 pd = new Vector3(x, y - perturbation, 0);

          for (int i = 0; i < (int)(integral_T / delta_t); i++)
          // for (int i = 0; i < 1; i++)
          {
            if (t - i < 0) continue;
            if (isInRegion(pl)) Trace2D(t - i, ref pl);
            if (isInRegion(pr)) Trace2D(t - i, ref pr);
            if (isInRegion(pu)) Trace2D(t - i, ref pu);
            if (isInRegion(pd)) Trace2D(t - i, ref pd);
          }
          float[,] flowmap2D = new float[2, 2];
          /*
          0,0 0,1
          1,0 1,1
          */
          flowmap2D[0, 0] = (pr.X - pl.X) / (2 * perturbation);
          flowmap2D[1, 0] = (pr.Y - pl.Y) / (2 * perturbation);
          flowmap2D[0, 1] = (pu.X - pd.X) / (2 * perturbation);
          flowmap2D[1, 1] = (pu.Y - pl.Y) / (2 * perturbation);

          float[,] tensor2D = new float[2, 2];
          tensor2D[0, 0] = (float)Math.Pow(flowmap2D[0, 0], 2) + (float)Math.Pow(flowmap2D[1, 0], 2);
          tensor2D[1, 0] = tensor2D[0, 1] = flowmap2D[0, 0] * flowmap2D[1, 0] + flowmap2D[0, 1] * flowmap2D[1, 1];
          tensor2D[1, 1] = (float)Math.Pow(flowmap2D[1, 1], 2) + (float)Math.Pow(flowmap2D[0, 1], 2);

          ftle[x, y, 0] = (float)Math.Log(Eigen.GetMaxEigenValue2x2(tensor2D)) / Math.Abs(integral_T);
        }
      }
      return ftle;
    }

    static bool isInRegion(Vector3 p)
    {
      if (p.X < 0 || p.Y < 0 || p.Z < 0 || ftleResolution < p.X || ftleResolution < p.Y || ftleResolution < p.Z) return false;
      else return true;
    }

    static void Trace2D(int t, ref Vector3 pos)
    {
      // LinearPrediction(t, ref pos);
      RungeKutta(t, ref pos);

      void LinearPrediction(int t, ref Vector3 pos)
      {
        float dt = delta_t * direction;
        Vector3 vel = Lerp2D(t, pos);
        pos.X = pos.X + vel.X * dt;
        pos.Y = pos.Y + vel.Y * dt;
      }

      void RungeKutta(int t, ref Vector3 pos)
      {
        float dt = delta_t * direction;
        Vector3 k1 = Lerp2D(t, pos);
        Vector3 x2 = pos + k1 * dt / 2;
        Vector3 k2 = Lerp2D((int)(t + dt / 2), x2);
        Vector3 x3 = pos + k2 * dt / 2;
        Vector3 k3 = Lerp2D((int)(t + dt / 2), x3);
        Vector3 x4 = pos + k3 * dt;
        Vector3 k4 = Lerp2D((int)(t + dt), x4);
        pos = pos + (k1 + k2 + k3 + k4) / 6 * dt;
      }
    }

    static Vector3 Lerp2D(int t, Vector3 pos)
    {
      // int domain = GetLerpDomain(pos);
      // if (domain == 0)
      // {
      //   // Console.WriteLine("x:{0} y:{1} z:{2}", pos.X, pos.Y, pos.Z);

      //   int x0 = (int)Math.Round(pos.X);
      //   int y0 = (int)Math.Round(pos.Y);

      //   int x1, y1;

      //   if ((pos.X - x0) < 0)
      //     x1 = x0 - 1;
      //   else
      //     x1 = x0 + 1;

      //   if (x1 < 0) x1 = 0;
      //   if (ftleResolution - 1 < x1) x1 = ftleResolution - 1;

      //   if ((pos.Y - y0) < 0)
      //     y1 = y0 - 1;
      //   else
      //     y1 = y0 + 1;

      //   if (y1 < 0) y1 = 0;
      //   if (ftleResolution - 1 < y1) y1 = ftleResolution - 1;

      //   Vector3 v00 = velocityField[t][x0, y0, 0];
      //   Vector3 v10 = velocityField[t][x1, y0, 0];
      //   Vector3 v01 = velocityField[t][x0, y1, 0];

      //   float up = Math.Abs(pos.X - x1);
      //   float vp = Math.Abs(pos.Y - y1);

      //   return v00 + up * (v10 - v00) + vp * (v01 - v00);
      // }
      // else
      // {
      //   return Vector3.Zero;
      // }

      int x0 = (int)Math.Round(pos.X / (ftleResolution - 1) * velResolution);
      int y0 = (int)Math.Round(pos.Y / (ftleResolution - 1) * velResolution);
      int neiborPoint = GetNeighborPoint(x0, y0);
      // Console.WriteLine("x:{0} y:{1} z:{2}", pos.X, pos.Y, pos.Z);
      // Console.WriteLine("p:{0}", neiborPoint);
      if (neiborPoint == 3)
      {
        int x1, y1;
        if ((pos.X - x0) < 0)
          x1 = x0 - 1;
        else
          x1 = x0 + 1;

        if ((pos.Y - y0) < 0)
          y1 = y0 - 1;
        else
          y1 = y0 + 1;

        Vector3 v00 = velocityField[t][x0, y0, 0];
        Vector3 v10 = velocityField[t][x1, y0, 0];
        Vector3 v01 = velocityField[t][x0, y1, 0];

        float delta_x = Math.Abs(pos.X - x1);
        float delta_y = Math.Abs(pos.Y - y1);
        float dx = Math.Abs(x1 - x0);
        float dy = Math.Abs(y1 - y0);

        return v00 + (v10 - v00) / dx * delta_x + (v01 - v00) / dy * delta_y;
      }

      if (neiborPoint == 2)
      {
        int x1, y1;
        if (x0 < 0 || (velResolution - 1) < x0)
        {
          if (x0 < 0)
          {
            x0 = 0;
            x1 = 1;
          }
          else
          {
            x0 = velResolution - 1;
            x1 = velResolution - 2;
          }

          if ((y0 - pos.Y) < 0)
            y1 = y0 + 1;
          else
            y1 = y0 - 1;
        }
        else
        {
          if (y0 < 0)
          {
            y0 = 0;
            y1 = 1;
          }
          else
          {
            y0 = velResolution - 1;
            y1 = velResolution - 2;
          }

          if ((x0 - pos.Y) < 0)
            x1 = x0 + 1;
          else
            x1 = x0 - 1;
        }
        Vector3 v00 = velocityField[t][x0, y0, 0];
        Vector3 v01 = velocityField[t][x0, y1, 0];
        Vector3 v10 = velocityField[t][x1, y0, 0];

        float delta_x = Math.Abs(pos.X - x1);
        float delta_y = Math.Abs(pos.Y - y1);
        float dx = Math.Abs(x1 - x0);
        float dy = Math.Abs(y1 - y0);

        return v00 + (v00 - v10) / dx * delta_x + (v00 - v01) / dy * delta_y;
      }

      if (neiborPoint == 1)
      {
        int x1, y1;
        if (x0 < 0)
        {
          x0 = 0;
          x1 = 1;
        }
        else
        {
          x0 = velResolution - 1;
          x1 = velResolution - 2;
        }
        if (y0 < 0)
        {
          y0 = 0;
          y1 = 1;
        }
        else
        {
          y0 = velResolution - 1;
          y1 = velResolution - 2;
        }
        Vector3 v00 = velocityField[t][x0, y0, 0];
        Vector3 v01 = velocityField[t][x0, y1, 0];
        Vector3 v10 = velocityField[t][x1, y0, 0];

        float delta_x = Math.Abs(pos.X - x1);
        float delta_y = Math.Abs(pos.Y - y1);
        float dx = Math.Abs(x1 - x0);
        float dy = Math.Abs(y1 - y0);

        return v00 + (v00 - v10) / dx * delta_x + (v00 - v01) / dy * delta_y;
      }
      return Vector3.Zero;

      /* Domain
        3 \      2         \ 3
      --------------------------
          \                \
          \ velocity field \
        1 \        0       \  1
          \                \
          \                \
      --------------------------
        3 \        2       \ 3
      */
      int GetLerpDomain(Vector3 pos)
      {
        if (0 <= pos.X && pos.X <= (ftleResolution - 1) && 0 <= pos.Y && pos.Y <= (ftleResolution - 1))
          return 0;
        else
        {
          if (pos.X < 0 || (ftleResolution - 1) < pos.X)
            if (pos.Y < 0 || (ftleResolution - 1) < pos.Y)
              return 2;
            else
              return 1;
          else
            return 1;
        }
      }

      /*
      velociyt field
      1 2 2 ... 2 2 1
      2 3 3 ... 3 3 2
      2 3 3 ... 3 3 2
      . . . ... . . .
      2 3 3 ... 3 3 2
      2 3 3 ... 3 3 2
      1 2 2 ... 2 2 1
      */

      int GetNeighborPoint(int x0, int y0)
      {
        if (0 < x0 && x0 < (velResolution - 1) && 0 < y0 && y0 < (velResolution - 1)) return 3;
        else if ((0 < x0 && x0 < (velResolution - 1)) || 0 < y0 && y0 < (velResolution - 1)) return 2;
        else return 1;
      }
    }
  }
}
