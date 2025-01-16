using NetVips;

namespace Emulsion.Core;

public class PipelineProcessor
{

    Image outputImage;
    
    //Pipelines arent run when the operations are called, they are enqued and only run when the image is output to memory or file, or 
    //an action which requires calculation such as average or max is run

    public ref Image Run(Pipeline pipeline, ref readonly Image baseImage){

        outputImage = baseImage;

        foreach (IImageOperation operation in pipeline.Operations){
            operation.Process(ref outputImage);
        }

        return ref outputImage;
    }

}
