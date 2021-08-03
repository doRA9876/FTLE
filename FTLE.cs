using System;
using System.Numerics;


/*
Using MAC Grid
*/

namespace Arihara.GuideSmoke
{
  public class FTLE
  {
    const int FORWARD = 1;
    const int BACKWARD = -1;

    private int direction = BACKWARD;
    private int dimension = 2;

    Vector3[,,] originalPosition;
    Vector3[][,,] velocityField;
    float[][,,] ftleField;

    int x_min, x_max, y_min, y_max, z_min, z_max;


    int ftleResolution = 128;
    int delta_t = 1;
    int integral_T = 10;

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


    public FTLE(string folderPath, int fileNum, int d)
    {
      Constructor(folderPath, fileNum, d);
    }

    public FTLE(string folderPath, int fileNum)
    {
      Constructor(folderPath, fileNum, FORWARD);
    }

    private void Constructor(string folderPath, int fileNum, int d)
    {
      if (d > 0) direction = FORWARD;
      else direction = BACKWARD;

      velocityField = new Vector3[fileNum][,,];
      ftleField = new float[fileNum][,,];

      for (int n = 0; n < fileNum; n++)
      {
        string path = string.Format(folderPath + "/vel-{0}.txt", n);
        velocityField[n] = FileIO.ReadVelocityFile(path);
      }

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
      if (dimension == 3) ftleField[t] = FTLE3D(t);
    }

    public void WriteFTLE(string path, int t)
    {
      FileIO.WriteFTLEFile(path, t, originalPosition, ftleField[t], ftleResolution, ftleResolution, 1);
    }

    #region Implementation for 2D FTLE
    float[,,] FTLE2D(int t)
    {
      float[,,] ftle = new float[ftleResolution, ftleResolution, 1];
      Vector3[,,] pos = new Vector3[ftleResolution, ftleResolution, 1];
      Vector3[,,] new_pos = new Vector3[ftleResolution, ftleResolution, 1];
      bool[,,] leftDomain = new bool[ftleResolution, ftleResolution, 1];
      bool[,,] calcFTLE = new bool[ftleResolution, ftleResolution, 1];
      originalPosition = new Vector3[ftleResolution, ftleResolution, 1];
      for (int ix = 0; ix < ftleResolution; ix++)
      {
        for (int iy = 0; iy < ftleResolution; iy++)
        {
          float x = (float)ix * x_max / (ftleResolution - 1);
          float y = (float)iy * y_max / (ftleResolution - 1);
          originalPosition[ix, iy, 0] = new Vector3(x, y, 0);
          pos[ix, iy, 0] = new Vector3(x, y, 0);
          new_pos[ix, iy, 0] = new Vector3(x, y, 0);
          leftDomain[ix, iy, 0] = false;
          calcFTLE[ix, iy, 0] = false;
        }
      }

      for (int t_integration = 0; t_integration < integral_T - delta_t; t_integration = t_integration + delta_t)
      {
        int t0 = t + t_integration * direction;
        int t1 = t0 + delta_t * direction;

        if (t0 < 0) continue;

        for (int x = 0; x < ftleResolution; x++)
        {
          for (int y = 0; y < ftleResolution; y++)
          {
            new_pos[x, y, 0] = Trace2D(t0, t1, delta_t, pos[x, y, 0]);
          }
        }

        for (int x = 0; x < ftleResolution; x++)
        {
          for (int y = 0; y < ftleResolution; y++)
          {
            if (leftDomain[x, y, 0] == false)
            {
              if (((new_pos[x, y, 0].X - x_min) * (new_pos[x, y, 0].X - x_max)) >= 0 || ((new_pos[x, y, 0].Y - y_min) * (new_pos[x, y, 0].Y - y_max)) >= 0)
              {
                leftDomain[x, y, 0] = true;
                if (calcFTLE[x, y, 0] == false)
                {
                  ftle[x, y, 0] = CalculateFTLE2D(pos, x, y);
                  calcFTLE[x, y, 0] = true;

                  if (x > x_min && calcFTLE[x - 1, y, 0] == false)
                  {
                    ftle[x - 1, y, 0] = CalculateFTLE2D(pos, x - 1, y);
                    calcFTLE[x - 1, y, 0] = true;
                  }
                  if (x < x_max && calcFTLE[x + 1, y, 0] == false)
                  {
                    ftle[x + 1, y, 0] = CalculateFTLE2D(pos, x + 1, y);
                    calcFTLE[x + 1, y, 0] = true;
                  }
                  if (y > y_min && calcFTLE[x, y - 1, 0] == false)
                  {
                    ftle[x, y - 1, 0] = CalculateFTLE2D(pos, x, y - 1);
                    calcFTLE[x, y - 1, 0] = true;
                  }
                  if (y < y_max && calcFTLE[x, y + 1, 0] == false)
                  {
                    ftle[x, y + 1, 0] = CalculateFTLE2D(pos, x, y + 1);
                    calcFTLE[x, y + 1, 0] = true;
                  }
                }
              }
            }
            else
            {
              new_pos[x, y, 0] = pos[x, y, 0];
            }
          }
        }

        for (int x = 0; x < ftleResolution; x++)
        {
          for (int y = 0; y < ftleResolution; y++)
          {
            if (leftDomain[x, y, 0] == false)
            {
              pos[x, y, 0] = new_pos[x, y, 0];
            }
          }
        }
      }

      for (int x = 0; x < ftleResolution; x++)
      {
        for (int y = 0; y < ftleResolution; y++)
        {
          if (calcFTLE[x, y, 0] == false)
          {
            ftle[x, y, 0] = CalculateFTLE2D(pos, x, y);
            calcFTLE[x, y, 0] = true;
          }
        }
      }
      return ftle;
    }


    float CalculateFTLE2D(Vector3[,,] flowmap, int ix, int iy)
    {
      if ((ix * (ix - (ftleResolution - 1))) < 0 && (iy * (iy - (ftleResolution - 1))) < 0)
      {
        float scaleX = (float)(x_max + 1) / ftleResolution;
        float scaleY = (float)(y_max + 1) / ftleResolution;
        /*
        00 01
        10 11
        */
        float a00 = (flowmap[ix + 1, iy, 0].X - flowmap[ix - 1, iy, 0].X) / (2 * scaleX);
        float a01 = (flowmap[ix, iy + 1, 0].X - flowmap[ix, iy - 1, 0].X) / (2 * scaleY);
        float a10 = (flowmap[ix + 1, iy, 0].Y - flowmap[ix - 1, iy, 0].Y) / (2 * scaleX);
        float a11 = (flowmap[ix, iy + 1, 0].Y - flowmap[ix, iy - 1, 0].Y) / (2 * scaleY);

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

      Vector3 velocity = Lerp2D(t_start, position);
      // Vector3 velocity = RungeKutta(t_start, pos);
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
      int neiborPoint = GetNeighborPoint(x0, y0);

      if (neiborPoint == 3)
      {
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

      if (neiborPoint == 2)
      {
        int x1, y1;
        if (x0 <= 0 || x_max <= x0)
        {
          if (x0 <= 0)
          {
            x0 = 0;
            x1 = 1;
          }
          else
          {
            x0 = x_max;
            x1 = x_max - 1;
          }

          if ((y0 - y) < 0)
            y1 = y0 + 1;
          else
            y1 = y0 - 1;
        }
        else
        {
          if (y0 <= 0)
          {
            y0 = 0;
            y1 = 1;
          }
          else
          {
            y0 = y_max;
            y1 = y_max - 1;
          }

          if ((x0 - x) < 0)
            x1 = x0 + 1;
          else
            x1 = x0 - 1;
        }
        Vector3 v00 = velocityField[t][x0, y0, 0];
        Vector3 v01 = velocityField[t][x0, y1, 0];
        Vector3 v10 = velocityField[t][x1, y0, 0];

        float delta_x = Math.Abs(x - x1);
        float delta_y = Math.Abs(y - y1);
        float dx = Math.Abs(x1 - x0);
        float dy = Math.Abs(y1 - y0);

        return Vector3.Zero;
      }

      else
      {
        int x1, y1;
        if (x0 <= 0)
        {
          x0 = 0;
          x1 = 1;
        }
        else
        {
          x0 = x_max;
          x1 = x_max - 1;
        }
        if (y0 <= 0)
        {
          y0 = 0;
          y1 = 1;
        }
        else
        {
          y0 = y_max;
          y1 = y_max - 1;
        }

        Vector3 v00 = velocityField[t][x0, y0, 0];
        Vector3 v01 = velocityField[t][x0, y1, 0];
        Vector3 v10 = velocityField[t][x1, y0, 0];

        float delta_x = Math.Abs(x - x1);
        float delta_y = Math.Abs(y - y1);
        float dx = Math.Abs(x1 - x0);
        float dy = Math.Abs(y1 - y0);

        return Vector3.Zero;
      }

      /*
      velocity field
      1 2 2 ... 2 2 1
      2 3 3 ... 3 3 2
      2 3 3 ... 3 3 2
      . . . ... . . .
      2 3 3 ... 3 3 2
      2 3 3 ... 3 3 2
      1 2 2 ... 2 2 1
      */
      int GetNeighborPoint(int _x, int _y)
      {
        if (0 < _x && _x < x_max && 0 < _y && _y < y_max) return 3;
        else if ((0 < _x && _x < x_max) || 0 < _y && _y < y_max) return 2;
        else return 1;
      }
    }
    #endregion


    #region Implementation for 3D FTLE
    float[,,] FTLE3D(int t)
    {
      float[,,] ftle = new float[ftleResolution, ftleResolution, ftleResolution];
      Vector3[,,] pos = new Vector3[ftleResolution, ftleResolution, ftleResolution];
      Vector3[,,] new_pos = new Vector3[ftleResolution, ftleResolution, ftleResolution];
      bool[,,] leftDomain = new bool[ftleResolution, ftleResolution, ftleResolution];
      bool[,,] calcFTLE = new bool[ftleResolution, ftleResolution, ftleResolution];
      originalPosition = new Vector3[ftleResolution, ftleResolution, ftleResolution];
      for (int ix = 0; ix < ftleResolution; ix++)
      {
        for (int iy = 0; iy < ftleResolution; iy++)
        {
          for (int iz = 0; iz < ftleResolution; iz++)
          {
            float x = (float)ix * x_max / (ftleResolution - 1);
            float y = (float)iy * y_max / (ftleResolution - 1);
            float z = (float)iz * z_max / (ftleResolution - 1);
            originalPosition[ix, iy, iz] = new Vector3(x, y, z);
            pos[ix, iy, iz] = new Vector3(x, y, z);
            new_pos[ix, iy, iz] = new Vector3(x, y, z);
            leftDomain[ix, iy, iz] = false;
            calcFTLE[ix, iy, iz] = false;
          }
        }
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
            for (int iz = 0; iz < ftleResolution; iz++)
            {
              new_pos[ix, iy, iz] = Trace3D(t0, t1, delta_t, pos[ix, iy, iz]);
            }
          }
        }

        for (int ix = 0; ix < ftleResolution; ix++)
        {
          for (int iy = 0; iy < ftleResolution; iy++)
          {
            for (int iz = 0; iz < ftleResolution; iz++)
            {
              if (leftDomain[ix, iy, iz] == false)
              {
                if (((new_pos[ix, iy, iz].X - x_min) * (new_pos[ix, iy, iz].X - x_max)) >= 0 || ((new_pos[ix, iy, iz].Y - y_min) * (new_pos[ix, iy, iz].Y - y_max)) >= 0 || ((new_pos[ix, iy, iz].Z - z_min) * (new_pos[ix, iy, iz].Z - z_max)) >= 0)
                {
                  leftDomain[ix, iy, iz] = true;
                  if (calcFTLE[ix, iy, iz] == false)
                  {
                    ftle[ix, iy, iz] = CalculateFTLE3D(pos, ix, iy, iz);
                    calcFTLE[ix, iy, iz] = true;

                    if (ix > x_min && calcFTLE[ix - 1, iy, iz] == false)
                    {
                      ftle[ix - 1, iy, iz] = CalculateFTLE3D(pos, ix - 1, iy, iz);
                      calcFTLE[ix - 1, iy, iz] = true;
                    }
                    if (ix < x_max && calcFTLE[ix + 1, iy, iz] == false)
                    {
                      ftle[ix + 1, iy, iz] = CalculateFTLE3D(pos, ix + 1, iy, iz);
                      calcFTLE[ix + 1, iy, iz] = true;
                    }
                    if (iy > y_min && calcFTLE[ix, iy - 1, iz] == false)
                    {
                      ftle[ix, iy - 1, iz] = CalculateFTLE3D(pos, ix, iy - 1, iz);
                      calcFTLE[ix, iy - 1, iz] = true;
                    }
                    if (iy < y_max && calcFTLE[ix, iy + 1, iz] == false)
                    {
                      ftle[ix, iy + 1, iz] = CalculateFTLE3D(pos, ix, iy + 1, iz);
                      calcFTLE[ix, iy + 1, iz] = true;
                    }
                    if (iz > z_min && calcFTLE[ix, iy, iz - 1] == false)
                    {
                      ftle[ix, iy, iz - 1] = CalculateFTLE3D(pos, ix, iy, iz - 1);
                      calcFTLE[ix, iy, iz - 1] = true;
                    }
                    if (iz < z_max && calcFTLE[ix, iy, iz + 1] == false)
                    {
                      ftle[ix, iy, iz + 1] = CalculateFTLE3D(pos, ix, iy, iz + 1);
                      calcFTLE[ix, iy, iz + 1] = true;
                    }
                  }
                }
              }
              else
              {
                new_pos[ix, iy, iz] = pos[ix, iy, iz];
              }
            }
          }
        }

        for (int ix = 0; ix < ftleResolution; ix++)
        {
          for (int iy = 0; iy < ftleResolution; iy++)
          {
            for (int iz = 0; iz < ftleResolution; iz++)
            {
              if (leftDomain[ix, iy, iz] == false)
              {
                pos[ix, iy, iz] = new_pos[ix, iy, iz];
              }
            }
          }
        }
      }

      for (int ix = 0; ix < ftleResolution; ix++)
      {
        for (int iy = 0; iy < ftleResolution; iy++)
        {
          for (int iz = 0; iz < ftleResolution; iz++)
          {
            if (calcFTLE[ix, iy, iz] == false)
            {
              ftle[ix, iy, iz] = CalculateFTLE3D(pos, ix, iy, iz);
              calcFTLE[ix, iy, iz] = true;
            }
          }
        }
      }
      return ftle;
    }

    /*
    refer:http://georgehaller.com/reprints/annurev-fluid-010313-141322.pdf
    p.10
    */
    float CalculateFTLE3D(Vector3[,,] flowmap, int ix, int iy, int iz)
    {
      if ((ix * (ix - (ftleResolution - 1))) < 0 && (iy * (iy - (ftleResolution - 1))) < 0)
      {
        float scaleX = (float)(x_max + 1) / ftleResolution;
        float scaleY = (float)(y_max + 1) / ftleResolution;
        float scaleZ = (float)(z_max + 1) / ftleResolution;
        /*
        00 01 02
        10 11 12
        20 21 22
        */
        float a00 = (flowmap[ix + 1, iy, iz].X - flowmap[ix - 1, iy, iz].X) / (2 * scaleX);
        float a01 = (flowmap[ix, iy + 1, iz].X - flowmap[ix, iy - 1, iz].X) / (2 * scaleY);
        float a02 = (flowmap[ix, iy, iz + 1].X - flowmap[ix, iy, iz - 1].X) / (2 * scaleZ);
        float a10 = (flowmap[ix + 1, iy, iz].Y - flowmap[ix - 1, iy, iz].Y) / (2 * scaleX);
        float a11 = (flowmap[ix, iy + 1, iz].Y - flowmap[ix, iy - 1, iz].Y) / (2 * scaleY);
        float a12 = (flowmap[ix, iy, iz + 1].Y - flowmap[ix, iy, iz - 1].Y) / (2 * scaleZ);
        float a20 = (flowmap[ix + 1, iy, iz].Z - flowmap[ix - 1, iy, iz].Z) / (2 * scaleX);
        float a21 = (flowmap[ix, iy + 1, iz].Z - flowmap[ix, iy - 1, iz].Z) / (2 * scaleY);
        float a22 = (flowmap[ix, iy, iz + 1].Z - flowmap[ix, iy, iz - 1].Z) / (2 * scaleZ);

        float[,] tensor3D = new float[3, 3];
        tensor3D[0, 0] = (float)Math.Pow(a00, 2) + (float)Math.Pow(a01, 2) + (float)Math.Pow(a02, 2);
        tensor3D[1, 1] = (float)Math.Pow(a10, 2) + (float)Math.Pow(a11, 2) + (float)Math.Pow(a12, 2);
        tensor3D[2, 2] = (float)Math.Pow(a20, 2) + (float)Math.Pow(a21, 2) + (float)Math.Pow(a22, 2);
        tensor3D[1, 0] = tensor3D[0, 1] = a00 * a10 + a01 * a11 + a02 * a12;
        tensor3D[2, 0] = tensor3D[0, 2] = a00 * a20 + a01 * a21 + a02 * a22;
        tensor3D[2, 1] = tensor3D[1, 2] = a10 * a20 + a11 * a21 + a12 * a22;

        double result = (float)Math.Log(Eigen.GetMaxEigenValue3x3(tensor3D)) / Math.Abs(integral_T / delta_t);

        if (result != double.NegativeInfinity) return (float)result;
        else return 0;
      }
      else
      {
        return 0;
      }
    }

    //TODO
    Vector3 Trace3D(int t_start, int t_end, float h, Vector3 position)
    {
      int delta = (t_end - t_start);

      Vector3 velocity = Lerp2D(t_start, position);
      // Vector3 velocity = RungeKutta(t_start, pos);
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

    //TODO
    Vector3 Lerp3D(int t, Vector3 pos)
    {
      float x = pos.X;
      float y = pos.Y;
      float z = pos.Z;
      int x0 = (int)Math.Round(x);
      int y0 = (int)Math.Round(y);
      int z0 = (int)Math.Round(z);

      int x1, y1, z1;
      if ((x - x0) < 0)
        x1 = x0 - 1;
      else
        x1 = x0 + 1;

      if ((y - y0) < 0)
        y1 = y0 - 1;
      else
        y1 = y0 + 1;

      if ((z - z0) < 0)
        z1 = z0 - 1;
      else
        z1 = z0 + 1;

      Vector3 v000 = velocityField[t][x0, y0, z0];
      Vector3 v100 = velocityField[t][x1, y0, z0];
      Vector3 v010 = velocityField[t][x0, y1, z0];
      Vector3 v001 = velocityField[t][x0, y0, z1];

      float delta_x = Math.Abs(x - x1);
      float delta_y = Math.Abs(y - y1);
      float delta_z = Math.Abs(z - z1);
      float dx = Math.Abs(x1 - x0);
      float dy = Math.Abs(y1 - y0);
      float dz = Math.Abs(z1 - z0);

      return v000 + (v100 - v000) / dx * delta_x + (v010 - v000) / dy * delta_y + (v001 - v000) / dz * delta_z;
    }
    #endregion
  }
}
