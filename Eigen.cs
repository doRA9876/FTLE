using System;

namespace FTLE
{
  static class Eigen
  {
    static public float GetMaxEigenValue2x2(float[,] tensor)
    {
      float b = tensor[0, 0] + tensor[1, 1];
      float c = tensor[0, 0] * tensor[1, 1] - tensor[1, 0] * tensor[0, 1];
      float d = b * b - 4 * c;

      if (d < 0) return 0;
      d = (float)Math.Sqrt(d);

      float e1 = 0.5f * (b + d);
      float e2 = 0.5f * (b - d);

      if (e1 < e2) return e2;
      else return e1;
    }
  }
}