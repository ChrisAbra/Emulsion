using System.Collections.Generic;

namespace Emulsion.Core;

public record class Pipeline()
{
    public List<IImageOperation> Operations = [];

}
