using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace XmlTransformer
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            var parser = new CommandLineParser(new CommandLineParserSettings(Console.Error));
            if(!parser.ParseArguments(args, options))
                Environment.Exit(1);

            XmlTransformer.TransformFile(options.SourcePath, options.TransformPath);

            Environment.Exit(1);
        }

        private sealed class Options : CommandLineOptionsBase
        {
            [Option("s", "sourcePath", HelpText = "The path to the source file", Required=true)]
            public string SourcePath { get; set; }

            [Option("t", "transformPath", HelpText = "The path to the transform file", Required = true)]
            public string TransformPath { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                var help = new HelpText
                {
                    Heading = new HeadingInfo("XmlTransformer", "1.0"),
                    AdditionalNewLineAfterOption = true,
                    AddDashesToOption = true
                };

                HandleParsingErrorsInHelp(help);

                return help;
            }

            private void HandleParsingErrorsInHelp(HelpText help)
            {
                if (this.LastPostParsingState.Errors.Count > 0)
                {
                    var errors = help.RenderParsingErrorsText(this, 2); // indent with two spaces
                    if (!string.IsNullOrEmpty(errors))
                    {
                        help.AddPreOptionsLine(string.Concat(Environment.NewLine, "ERROR(S):"));
                        help.AddPreOptionsLine(errors);
                    }
                }
            }
        }
    }
}
