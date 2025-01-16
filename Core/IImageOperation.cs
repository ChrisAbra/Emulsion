using NetVips;

namespace Emulsion.Core;

public interface IImageOperation
{
    public string Label {get;}
    public void Process(ref Image image);

}
