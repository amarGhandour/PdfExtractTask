using Microsoft.AspNetCore.Mvc;

namespace PdfExtractTask.Dtos
{
    public class PdfUploadRequest
    {
        [FromForm(Name = "zipFile")]
        public IFormFile ZipFile { get; set; }

        [FromForm(Name = "json")]
        public string Json { get; set; }
    }
}
