using NetVips;

namespace Emulsion.Core.ImageOperations;

public record class Exposure : IImageOperation
{
    public string Label => "Exposure";

    public float ExposureMultiplier
    {
        get => _exposureMultipler;
        set
        {
            if (_exposureMultipler == value) return;
            _exposureMultipler = value;
        }
    }


    private float _exposureMultipler;
    public Exposure(float ExposureMultiplier = 1f)
    {
        this.ExposureMultiplier = ExposureMultiplier;
    }

    public void Process(ref Image image)
    {
        image *= ExposureMultiplier;
    }


}
