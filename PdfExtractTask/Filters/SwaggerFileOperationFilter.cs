using Microsoft.OpenApi.Models;
using PdfExtractTask.Dtos;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PdfExtractTask.Filters
{
    public class SwaggerFileOperationFilter: IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileParameters = context.MethodInfo
                .GetParameters()
                .Where(p => p.ParameterType == typeof(IFormFile) || p.ParameterType == typeof(PdfUploadRequest));

            foreach (var parameter in fileParameters)
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content =
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties =
                                {
                                    ["zipFile"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "binary"
                                    },
                                    ["json"] = new OpenApiSchema
                                    {
                                        Type = "string"
                                    }
                                },
                                Required = new HashSet<string> { "zipFile", "json" }
                            }
                        }
                    }
                };
            }
        }
    }
}
