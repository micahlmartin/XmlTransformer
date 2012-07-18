using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace XmlTransformer
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options(args);
            if (!options.Parse())
                Environment.Exit(1);

            if (string.IsNullOrWhiteSpace(options.DestinationFile))
            {
                options.DestinationFile = options.SourceFile;
                Console.Write("No destination file specified. Will save to source file");
            }

            if (options.Kind == TransformType.Merge)
                XmlTransformer.MergeFile(options.SourceFile, options.TransformFile, options.DestinationFile);
            else
                XmlTransformer.TransformXml(options.SourceFile, options.TransformFile, options.DestinationFile);

            Environment.Exit(0);
        }

        private class Options
        {
            private string[] _cmdArgs;
            private static Regex OptionRegex = new Regex("/(?<param>((s|source|sourcefile|t|transform|transformfile|d|dest|destination|destinationfile|k|kind))):(?<value>.+)", RegexOptions.IgnoreCase);

            public Options(string[] cmdArgs)
            {
                _cmdArgs = cmdArgs;
            }

            public bool Parse()
            {
                var success = true;
                var error = "";

                foreach (var arg in _cmdArgs)
                {
                    var match = OptionRegex.Match(arg);
                    if (!match.Success)
                    {
                        error = string.Format("Uknown parameter: '{0}'", arg);
                        success = false;
                        break;
                    }

                    var param = match.Groups["param"].Value;
                    var value = match.Groups["value"].Value;

                    switch (param.ToUpper())
                    {
                        case "S":
                        case "SOURCE":
                        case "SOURCEFILE":
                            SourceFile = value;
                            break;
                        case "T":
                        case "TRANSFORM":
                        case "TRANSFORMFILE":
                            TransformFile = value;
                            break;
                        case "D":
                        case "DEST":
                        case "DESTINATION":
                        case "DESTINATIONFILE":
                            DestinationFile = value;
                            break;
                        case "K":
                        case "KIND":
                            TransformType kind;
                            if (!Enum.TryParse<TransformType>(value, out kind))
                            {
                                error = string.Format("Uknown kind: '{0}'", value);
                                success = false;
                            }
                            else
                                Kind = kind;
                            break;
                        default:
                            error = string.Format("Uknown parameter: '{0}'", param);
                            success = false;
                            break;
                    }

                    if (!success)
                        break;
                }

                if (string.IsNullOrEmpty(SourceFile))
                {
                    error = "Source file is required";
                    success = false;
                }
                else if (string.IsNullOrEmpty(TransformFile))
                {
                    error = "Transform file is required";
                    success = false;
                }

                if (!success)
                {
                    Console.Error.Write(error);
                    Console.Write(HelpText);
                }

                return success;
            }

            public string SourceFile { get; set; }
            public string TransformFile { get; set; }
            public string DestinationFile { get; set; }
            public TransformType Kind { get; set; } 

            

            public string HelpText
            {
                get
                {
                    return @"
Usage: XmlTransformer /s:[sourceFile] /t:[transformFile] /d:[destinationFile] /k:[ind]

Description:        Transforms xml files

Params:

  /s[ourceFile]:<fileName>

                    The file that you want to apply the transformation to


  /t[ransformFile]:<fileName> 

                    The name of the file that contains the transformations


  /d[estinationFile]:<fileName> (optional)

                    The name of the file where the transformations
                    will be saved to. This will default to the source
                    file if not specified.


  /k[ind]:<Transform,Merge>
                    The type of transformation to apply. Merge will merge 
                    the files and transform will transform them using 
                    the transformation directives.
";
                }
            }
        }
    }
}
