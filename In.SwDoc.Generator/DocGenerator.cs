﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using log4net;
using log4net.Repository.Hierarchy;

namespace In.SwDoc.Generator
{
    public class DocumentGenerationException : Exception
    {
        public DocumentGenerationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class DocGenerator
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(DocGenerator));
        private readonly string _swaggerCli = @"..\swaggercli\swagger2markup-cli-1.3.3.jar";
        private readonly string _tempDirectory = @"..\swaggercli\temp";
        private readonly byte[] _newLine = Encoding.UTF8.GetBytes(Environment.NewLine);

        internal DocGenerator()
        {
            if (!Directory.Exists(_tempDirectory))
            {
                Directory.CreateDirectory(_tempDirectory);
            }
        }

        public Stream ConvertJsonToAscii(string data)
        {
            _log.Info("Start converting to ascii doc");
            var jsonName = Guid.NewGuid().ToString("N");
            var asciiName = Guid.NewGuid().ToString("N");
            var jsonPath = Path.Combine(_tempDirectory, jsonName);
            var asciiPath = Path.Combine(_tempDirectory, asciiName);
            try
            {
                File.WriteAllText(jsonPath, data);

                ConverJsonToAscii(jsonPath, asciiPath);

                var memory = new MemoryStream();
                var files = Directory.GetFiles(asciiPath);
                foreach (var file in files)
                {
                    using (var stream = File.OpenRead(file))
                    {
                        stream.CopyTo(memory);
                    }
                    memory.Write(_newLine, 0, _newLine.Length);
                }
                Directory.Delete(asciiPath, true);
                File.Delete(jsonPath);
                memory.Position = 0;
                return memory;
            }
            catch (Exception e)
            {
                _log.Error("Unable to generate ascii document", e);
                throw new DocumentGenerationException("Unable to generate ascii document", e);
            }
        }

        public Stream ConvertJsonToPdf(string data)
        {
            var adocName = Guid.NewGuid().ToString("N");
            var pdfName = Guid.NewGuid().ToString("N");
            var adocPath = Path.Combine(_tempDirectory, adocName);
            var pdfPath = Path.Combine(_tempDirectory, pdfName);
            try
            {
                using (var stream = ConvertJsonToAscii(data))
                using (var file = File.Create(adocPath))
                {
                    stream.CopyTo(file);
                }

                ConverAsciiToPdf(adocPath, pdfPath);

                File.Delete(adocPath);
                var memory = new MemoryStream();
                using (var pdf = File.OpenRead(pdfPath))
                {
                    pdf.CopyTo(memory);
                }

                File.Delete(pdfPath);
                memory.Position = 0;
                return memory;
            }
            catch (DocumentGenerationException)
            {
                throw;
            }
            catch (Exception e)
            {
                _log.Error("Unable to generate pdf document", e);
                throw new DocumentGenerationException("Unable to generate ascii document", e);
            }
        }

        public void ConverJsonToAscii(string jsonPath, string asciiPath)
        {
            var cmd = $"/C java -jar \"{_swaggerCli}\" convert -i \"{jsonPath}\" -d \"{asciiPath}\"";

            var process = new Process();
            var startInfo = new ProcessStartInfo();
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = cmd;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }


        public void ConverAsciiToPdf(string asciiPath, string pdfPath)
        {
            var cmd = $"/C asciidoctor-pdf -o \"{pdfPath}\" \"{asciiPath}\"";

            var process = new Process();
            var startInfo = new ProcessStartInfo();
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = cmd;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }
    }
}
