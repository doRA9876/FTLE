using System;
using System.Linq;
using System.Numerics;

namespace Arihara.GuideSmoke
{
  public class LCS
  {
    private int lenX, lenY, lenZ;
    private float[,,] ftleField;
    private int[,,] lcsField = null;
    private int[,] classification = null;

    public LCS(float[,,] ftle, bool doNormalization)
    {
      lenX = ftle.GetLength(0);
      lenY = ftle.GetLength(1);
      lenZ = ftle.GetLength(2);
      this.ftleField = ftle;
      if (doNormalization) NormalizeFTLE();
    }

    private void NormalizeFTLE()
    {
      float min = ftleField[0, 0, 0];
      float max = min;
      for (int ix = 0; ix < lenX; ix++)
      {
        for (int iy = 0; iy < lenY; iy++)
        {
          for (int iz = 0; iz < lenZ; iz++)
          {
            float num = ftleField[ix, iy, iz];
            if (num < min) min = num;
            if (max < num) max = num;
          }
        }
      }

      for (int ix = 0; ix < lenX; ix++)
      {
        for (int iy = 0; iy < lenY; iy++)
        {
          for (int iz = 0; iz < lenZ; iz++)
          {
            ftleField[ix, iy, iz] = (ftleField[ix, iy, iz] - min) / (max - min);
          }
        }
      }
    }

    public void LcsByThreshold(float threshold)
    {
      lcsField = new int[lenX, lenY, lenZ];
      for (int ix = 0; ix < lenX; ix++)
      {
        for (int iy = 0; iy < lenY; iy++)
        {
          for (int iz = 0; iz < lenZ; iz++)
          {
            lcsField[ix, iy, iz] = (ftleField[ix, iy, iz] < threshold) ? 0 : 1;
          }
        }
      }
    }

    public void LcsByThreshold()
    {
      float median = CalcMedian();
      LcsByThreshold(median);
      Console.WriteLine("Used Median Value: {0}", median);
    }

    public void LcsByHessian(int filterApplyIter, float kappa, bool doSkeltonize, int skeletonIter)
    {
      FtleClassification(filterApplyIter, kappa);
      if (doSkeltonize) Skeletonization(skeletonIter);

      lcsField = new int[lenX, lenY, 1];
      for (int ix = 0; ix < lenX; ix++)
      {
        for (int iy = 0; iy < lenY; iy++)
        {
          lcsField[ix, iy, 0] = classification[ix, iy];
        }
      }
    }

    /*
     * refer : https://ieeexplore.ieee.org/document/5613499
     */
    public void FtleClassification(int filterCount, float kappa)
    {
      if (lenZ > 1)
      {
        Console.WriteLine("This function only supports 2D.");
        return;
      }

      classification = new int[lenX, lenY];

      for (int i = 0; i < filterCount; i++)
      {
        GaussianFilter2D();
      }

      float[,,] secondPartialDerivative = new float[lenX, lenY, 3]; //[lenX, lenY, (dx^2, dy^2, dxdy)]
      for (int ix = 0; ix < lenX; ix++)
      {
        for (int iy = 0; iy < lenY; iy++)
        {
          // refer : https://www.ics.nara-wu.ac.jp/~kako/teaching/na/chap15.pdf
          float dxx = (GetCoordValue2D(ix + 1, iy) + GetCoordValue2D(ix - 1, iy) - 2 * GetCoordValue2D(ix, iy)) / 1;
          float dyy = (GetCoordValue2D(ix, iy + 1) + GetCoordValue2D(ix, iy - 1) - 2 * GetCoordValue2D(ix, iy)) / 1;
          float dxdy = (GetCoordValue2D(ix + 1, iy + 1) - GetCoordValue2D(ix - 1, iy + 1)
                      - GetCoordValue2D(ix + 1, iy - 1) + GetCoordValue2D(ix - 1, iy - 1)) / 1;

          secondPartialDerivative[ix, iy, 0] = dxx;
          secondPartialDerivative[ix, iy, 1] = dyy;
          secondPartialDerivative[ix, iy, 2] = dxdy;
        }
      }

      float[,] maxEigenValue = new float[lenX, lenY];
      for (int ix = 0; ix < lenX; ix++)
      {
        for (int iy = 0; iy < lenY; iy++)
        {
          float[,] hessian = new float[2, 2];
          hessian[0, 0] = secondPartialDerivative[ix, iy, 0];
          hessian[1, 1] = secondPartialDerivative[ix, iy, 1];
          hessian[0, 1] = hessian[1, 0] = secondPartialDerivative[ix, iy, 2];
          maxEigenValue[ix, iy] = Eigen.GetMaxEigenValue2x2(hessian);
        }
      }

      float maxFTLE = 0;
      for (int ix = 0; ix < lenX; ix++)
      {
        for (int iy = 0; iy < lenY; iy++)
        {
          if (ftleField[ix, iy, 0] > maxFTLE) maxFTLE = ftleField[ix, iy, 0];
        }
      }

      for (int ix = 0; ix < lenX; ix++)
      {
        for (int iy = 0; iy < lenY; iy++)
        {
          if (maxEigenValue[ix, iy] <= kappa && (maxFTLE * 0.3f) <= ftleField[ix, iy, 0]) classification[ix, iy] = 1;
          else classification[ix, iy] = 0;
        }
      }
    }

    public void Skeletonization(int iteration)
    {
      const int N = 0b0001;
      const int E = 0b0010;
      const int S = 0b0100;
      const int W = 0b1000;
      const int NW = N | W;
      const int SE = S | E;
      const int NE = N | E;
      const int SW = S | W;

      int[] dir = { NW, SE, NE, SW };
      int[,] Template1 = {
        {  0,  0,  0},
        {  2,  1,  2},
        {  2,  1,  2},
      };
      int[,] Template2 = {
        {  0,  2,  2},
        {  0,  1,  1},
        {  0,  2,  2},
      };
      int[,] Template3 = {
        {  0,  0, -1},
        {  0,  1,  1},
        { -1,  1, -1},
      };

      for (int n = 1; n < iteration; n++)
      {
        int[,] bounderPixel = new int[lenX, lenY];
        for (int ix = 0; ix < lenX; ix++)
        {
          for (int iy = 0; iy < lenY; iy++)
          {
            if (classification[ix, iy] == 0) continue;
            if (classification[ix, iy + 1] == 0) bounderPixel[ix, iy] |= N;
            if (classification[ix, iy - 1] == 0) bounderPixel[ix, iy] |= S;
            if (classification[ix + 1, iy] == 0) bounderPixel[ix, iy] |= E;
            if (classification[ix - 1, iy] == 0) bounderPixel[ix, iy] |= W;
          }
        }

        /* i = 0:NW, 1:SE, 2:NE, 3:SW */
        for (int i = 0; i < 4; i++)
        {
          for (int ix = 0; ix < lenX; ix++)
          {
            for (int iy = 0; iy < lenY; iy++)
            {
              if ((bounderPixel[ix, iy] & dir[i]) == 0) continue;
              if (isMatchTemplate(ix, iy, Rotation(i, Template1)))
              {
                classification[ix, iy] = 0;
                continue;
              }

              if (isMatchTemplate(ix, iy, Rotation(i, Template2)))
              {
                classification[ix, iy] = 0;
                continue;
              }

              if (isMatchTemplate(ix, iy, Rotation(i, Template3)))
              {
                classification[ix, iy] = 0;
                continue;
              }
            }
          }
        }
      }


      bool isMatchTemplate(int cx, int cy, int[,] template)
      {
        bool isWildcardX = false;
        for (int ix = -1; ix < 2; ix++)
        {
          for (int iy = -1; iy < 2; iy++)
          {
            int x = cx + ix;
            int y = cy + iy;
            int templateNum = template[ix + 1, iy + 1];
            switch (templateNum)
            {
              case 0:
              case 1:
                if (classification[x, y] == templateNum) continue;
                else return false;

              case 2:
                if (classification[x, y] == 1) isWildcardX = true;
                continue;

              default:
                continue;
            }
          }
        }
        return isWildcardX;
      }

      int[,] Rotation(int num, int[,] src)
      {
        int[,] dst = src;

        for (int i = 0; i < num; i++)
        {
          int[,] tmp = new int[lenX, lenY];
          tmp[2, 0] = dst[0, 0];
          tmp[1, 0] = dst[0, 1];
          tmp[0, 0] = dst[0, 2];
          tmp[2, 1] = dst[1, 0];
          tmp[1, 1] = dst[1, 1];
          tmp[0, 1] = dst[1, 2];
          tmp[2, 2] = dst[2, 0];
          tmp[1, 2] = dst[2, 1];
          tmp[0, 2] = dst[2, 2];
          dst = tmp;
        }
        return dst;
      }
    }

    public void ShowFTLE()
    {
      for (int ix = 0; ix < lenX; ix++)
      {
        for (int iy = 0; iy < lenY; iy++)
        {
          for (int iz = 0; iz < lenZ; iz++)
          {
            Console.WriteLine("[{0} {1} {2}] : {3}", ix, iy, iz, ftleField[ix, iy, iz]);
          }
        }
      }
    }

    public void WriteLCS(string path, Vector3[,,] pos)
    {
      if (lcsField == null)
      {
        Console.WriteLine("LCS is not calulated.");
        return;
      }
      FileIO.WriteLCSFile(path, pos, lcsField, lenX, lenY, lenZ);
    }

    public void WriteFTLE(string path, Vector3[,,] pos)
    {
      FileIO.WriteFTLEFile(path, 1000, pos, this.ftleField, lenX, lenY, lenZ);
    }

    public void WriteClasscification(string path, Vector3[,,] pos)
    {
      if (classification == null)
      {
        Console.WriteLine("Classification is not calculated.");
        return;
      }

      int[,,] classify = new int[lenX, lenY, 1];
      for (int ix = 0; ix < lenX; ix++)
      {
        for (int iy = 0; iy < lenY; iy++)
        {
          classify[ix, iy, 0] = classification[ix, iy];
        }
      }
      FileIO.WriteLCSFile(path, pos, classify, lenX, lenY, 1);
    }

    private float CalcMedian()
    {
      float[] arr = new float[lenX * lenY * lenZ];
      for (int ix = 0; ix < lenX; ix++)
      {
        for (int iy = 0; iy < lenY; iy++)
        {
          for (int iz = 0; iz < lenZ; iz++)
          {
            arr[ix * (lenY * lenZ) + iy * lenZ + iz] = ftleField[ix, iy, iz];
          }
        }
      }
      Array.Sort(arr);
      return arr[(lenX * lenY * lenZ) / 2];
    }

    private void GaussianFilter2D()
    {
      if (lenZ > 1) return;

      float[,] filter = {
        { 1f / 256,  4f / 256,  6f / 256,  4f / 256, 1f / 256 },
        { 4f / 256, 16f / 256, 24f / 256, 16f / 256, 4f / 256 },
        { 6f / 256, 24f / 256, 36f / 256, 24f / 256, 6f / 256 },
        { 4f / 256, 16f / 256, 24f / 256, 16f / 256, 4f / 256 },
        { 1f / 256,  4f / 256,  6f / 256,  4f / 256, 1f / 256 },
      };

      float[,,] tmp = new float[lenX, lenY, 1];

      for (int ix = 0; ix < lenX; ix++)
      {
        for (int iy = 0; iy < lenY; iy++)
        {
          for (int fx = -2; fx < 3; fx++)
          {
            for (int fy = -2; fy < 3; fy++)
            {
              tmp[ix, iy, 0] += filter[fx + 2, fy + 2] * GetCoordValue2D(ix + fx, iy + fy);
            }
          }
        }
      }
      ftleField = tmp;
    }

    private float GetCoordValue2D(int ix, int iy)
    {
      if ((ix * (ix - lenX)) >= 0 || (iy * (iy - lenY)) >= 0) return 0;
      else return ftleField[ix, iy, 0];
    }
  }
}