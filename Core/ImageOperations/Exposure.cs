using NetVips;

namespace Emulsion.Core.ImageOperations;

public record class Exposure : IImageOperation
{
    public float ExposureMultiplier { get; set; }
    public Exposure(float ExposureMultiplier = 1f)
    {
        this.ExposureMultiplier = ExposureMultiplier;
    }

    public void Process(ref Image image)
    {
        image *= ExposureMultiplier;
    }


}
