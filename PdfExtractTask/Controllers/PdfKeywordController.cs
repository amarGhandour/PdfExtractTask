using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Text.Json;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using PdfExtractTask.Dtos;

namespace PdfExtractTask.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfKeywordController : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] PdfUploadRequest request)
        {
            if (request.ZipFile == null || request.ZipFile.Length == 0)
                return BadRequest("Invalid ZIP file.");

            if (string.IsNullOrEmpty(request.Json))
                return BadRequest("Missing JSON with keywords.");

            // Parse JSON object to extract keywords
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var keywordsObject = JsonSerializer.Deserialize<KeywordRequest>(request.Json, options);
            if (keywordsObject?.Keywords == null || !keywordsObject.Keywords.Any())
                return BadRequest("Invalid or missing keywords.");

            var keywordOccurrences = new Dictionary<string, Dictionary<string, int>>();

            try
            {
                // Extract ZIP file contents
                using var zipStream = new MemoryStream();
                await request.ZipFile.CopyToAsync(zipStream);
                zipStream.Seek(0, SeekOrigin.Begin);
                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        using var entryStream = entry.Open();
                        using var pdfStream = new MemoryStream();
                        await entryStream.CopyToAsync(pdfStream);
                        pdfStream.Seek(0, SeekOrigin.Begin);

                        var occurrences = CountKeywordsInPdf(pdfStream, keywordsObject.Keywords);
                        keywordOccurrences[entry.Name] = occurrences;
                    }
                }

                return Ok(keywordOccurrences);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private Dictionary<string, int> CountKeywordsInPdf(Stream pdfStream, IEnumerable<string> keywords)
        {
            var keywordCount = keywords.ToDictionary(keyword => keyword, keyword => 0);

            using var pdfReader = new PdfReader(pdfStream);
            using var pdfDocument = new PdfDocument(pdfReader);

            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                var text = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(i));

                foreach (var keyword in keywords)
                {
                    var occurrences = CountOccurrences(text, keyword);
                    keywordCount[keyword] += occurrences;
                }
            }

            return keywordCount;
        }

        private int CountOccurrences(string text, string keyword)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(keyword))
                return 0;

            var count = 0;
            var index = text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);

            while (index != -1)
            {
                count++;
                index = text.IndexOf(keyword, index + keyword.Length, StringComparison.OrdinalIgnoreCase);
            }

            return count;
        }

        public class KeywordRequest
        {
            public List<string> Keywords { get; set; }
        }
    }
}
