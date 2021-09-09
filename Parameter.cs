namespace Arihara.GuideSmoke
{
  public class Parameter
  {
    public string dataPath { get; set; }
    public string outFTLEPath { get; set; }
    public int dataNum{ get; set; }
    public int startFrame { get; set; }
    public int endFrame { get; set; }
    public int integralFrame { get; set; }
    public float deltaT{ get; set; }
    public int ftleResolutionX { get; set; }
    public int ftleResolutionY { get; set; }
    public int ftleResolutionZ { get; set; }
    public string direction{ get; set; }
    public float tau{ get; set; }
  }
}