namespace Arihara.GuideSmoke
{
  public class Parameter
  {
    public string dataPath { get; set; }
    public string outputFTLEPath { get; set; }
    public string outputLCSPath { get; set; }
    public int startT { get; set; }
    public int endT { get; set; }
    public int integralT { get; set; }
    public int ftleResolution { get; set; }
    public int direction { get; set; }
    public int skeletonizeNum { get; set; }
    public int gaussianNum { get; set; }
    public int sobelNum { get; set; }
    public int LcsMethod { get; set; }
    public bool isLcsCalculation { get; set; }
    public bool isNormalize { get; set; }
    public bool isSkeletonize { get; set; }
    public float lcsThreshold { get; set; }
    public float kappa { get; set; }
  }
}