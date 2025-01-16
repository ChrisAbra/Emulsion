using NetVips;

namespace Emulsion.Core;

public interface IImageOperation
{
    public void Process(ref Image image);

}
