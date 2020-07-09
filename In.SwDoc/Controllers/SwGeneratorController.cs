using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using In.SwDoc.Generator;
using In.SwDoc.Model;
using log4net;
using Microsoft.AspNetCore.Mvc;

namespace In.SwDoc.Controllers
{
    [Route("api/sw-generator")]
    public class SwGeneratorController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(DocGenerator));
        private readonly DocumentStorage _storage;
        private readonly DocGenerator _generator;

        public SwGeneratorController()
        {
            _storage = DocumentStorageFactory.Get();
            _generator = DocGeneratorFactory.Get();
        }

        [HttpPost("url")]
        public IActionResult GetDocumentByUrl([FromBody]UrlForm data)
        {
            try
            {
                var request = WebRequest.Create(data.Url);
                request.Method = "GET";
                if (string.IsNullOrEmpty(data.Format))
                {
                    data.Format = "pdf";
                }
                using (var responce = request.GetResponse())
                using (var stream = responce.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    var content = reader.ReadToEnd();
                    var d = _generator.ConvertJsonToFormat(content, data.OpenApi,data.Format.ToLower());
                    var id = _storage.SaveDocument(d);
                    return Ok(new
                    {
                        id,
                        error = (string) null
                    });
                }
            }
            catch (WebException e)
            {
                return Ok(new
                {
                    id = (string) null,
                    error = "WebException"
                });
            }
            catch (DocumentGenerationException e)
            {
                return Ok(new
                {
                    id = (string) null,
                    error = "GenerationError"
                });
            }
            catch (Exception e)
            {
                _log.Error("Unknown exception", e);
                return Ok(new
                {
                    id = (string) null,
                    error = "UnknownException"
                });
            }
        }

        [HttpPost("spec")]
        public IActionResult GetDocumentBySpec([FromBody]SpecForm data)
        {
            try
            {
                if (string.IsNullOrEmpty(data.Format)){
                    data.Format = "pdf";
                }
                var d = _generator.ConvertJsonToFormat(data.Text, data.OpenApi,data.Format.ToLower());
                var id = _storage.SaveDocument(d);
                return Ok(new
                {
                    id,
                    error = (string) null
                });
            }
            catch (DocumentGenerationException e)
            {
                return Ok(new
                {
                    id = (string) null,
                    error = "GenerationError"
                });
            }
            catch (Exception e)
            {
                _log.Error("Unknown exception", e);
                return Ok(new
                {
                    id = (string) null,
                    error = "UnknownException"
                });
            }

        }

        [HttpGet("document/{id}/{format}")]
        public IActionResult DownloadDocument(string id, string format)
        {
            var stream = _storage.GetDocument(id);

            if (string.IsNullOrEmpty(format))
            {
                format = "pdf";
            }

            if (format.ToLower() == "pdf")
                return File(stream, "application/pdf", "api-documentation.pdf");
            else
                return File(stream, "text/html", "api-documentation.html");

        }
    }
}
