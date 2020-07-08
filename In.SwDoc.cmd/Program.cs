using In.SwDoc.Generator;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace In.SwDoc.cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count()==0 || args.Contains("--h"))
            {
                //Help
                var help = "In.SwDoc.cmd:" + Environment.NewLine+
                    "   Convert swagger xml Markup to PDF or HTML" + Environment.NewLine +
                    "Usage: " + Environment.NewLine +
                    "   dotnet.exe In.SwDoc.cmd.dll [options]" + Environment.NewLine +
                    "Options:" + Environment.NewLine +
                    "   --h                 Shows this Help" + Environment.NewLine +
                    "   --xml \"path\"        Path of the Swagger XML to Convert" + Environment.NewLine +
                    "   --format            Type of Output, html or pdf; if not specified pdf is assumed " + Environment.NewLine +
                    "   --output \"path\"     Path of the outputfile, if not specified the xml parameter will be used changing the extension" ;

                Console.WriteLine(help);
            }
            else
            {
                string path = null;
                string outputFormat = "pdf";
                string outputPath = null;

                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--xml")
                    {
                        path = args[i + 1];
                    }
                    if (args[i] == "--format")
                    {
                        outputFormat = args[i + 1].ToLower();
                    }
                    if (args[i] == "--output")
                    {
                        outputPath = args[i + 1].ToLower();
                    }
                }

                var generator = DocGeneratorFactory.Get();
                var data = File.ReadAllText(path);
                var output = generator.ConvertJsonToFormat(data, false, outputFormat);
                if (outputPath == null)
                {
                    outputPath = Path.Combine(Path.GetDirectoryName(path),
                        Path.GetFileNameWithoutExtension(path) + "." + outputFormat
                        );
                }

                using (var file = File.Create(outputPath))
                {
                    output.CopyTo(file);
                }

                //A bit of Padding
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("");

                Console.WriteLine("File Created " + outputPath);
            }

           
#if DEBUG
            Console.ReadLine();
#endif
        }
    }
}
