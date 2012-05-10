using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Ogdi.Data.DataLoader;

namespace Ogdi.Data.DataLoaderConsoleApp
{
    static class CommandLineParser
    {
        private static readonly string mode_restriction = "Use /mode=create for /type=dbf+kml.";
        private static readonly string s_use = "Use: AfdDataLoader.exe /type=dbf+kml|csv /fsname=file_set_name /target=console|tables [/date] [/mode=create|add|update] [/sourceorder].\nInput file set must be co-located with this executable.";

        public static CommandLineParams ParseCommandLine(string[] args)
        {
            int count = 3;

            if (args.Length >= 1)
            {
                var parsedArgs = ParseArguments(args);

                if (!parsedArgs.ContainsKey("help") && !parsedArgs.ContainsKey("?"))
                {
                    var cmdLineArgs = new CommandLineParams();

                    foreach (var s in parsedArgs)
                    {
                        if (s.Key.Equals("type"))
                        {
                            if (s.Value.ToLowerInvariant().Equals("dbf+kml"))
                            {
                                cmdLineArgs.DataType = SourceDataType.DbfAndKml;
                            }
                            else if (s.Value.ToLowerInvariant().Equals("csv"))
                            {
                                cmdLineArgs.DataType = SourceDataType.Csv;
                            }
                            else
                            {
                                goto UseMessage;
                            }

                            count--;
                        }
                        else if (s.Key.Equals("target"))
                        {
                            if (s.Value.ToLowerInvariant().Equals("console"))
                            {
                                cmdLineArgs.LoadingTarget = DataLoadingTarget.Console;
                            }
                            else if (s.Value.ToLowerInvariant().Equals("tables"))
                            {
                                cmdLineArgs.LoadingTarget = DataLoadingTarget.Tables;
                            }
                            else
                            {
                                goto UseMessage;
                            }

                            count--;
                        }
                        else if (s.Key.Equals("fsname"))
                        {
                            cmdLineArgs.FileSetName = s.Value;
                            count--;
                        }
                        else if (s.Key.Equals("date"))
                        {
                            cmdLineArgs.RefreshLastUpdateDate = true;
                        }
                        else if (s.Key.Equals("mode"))
                        {
                            if (s.Value.ToLowerInvariant().Equals("create"))
                            {
                                cmdLineArgs.OverwriteMode = TableOverwriteMode.Create;
                            }
                            else if (s.Value.ToLowerInvariant().Equals("add"))
                            {
                                cmdLineArgs.OverwriteMode = TableOverwriteMode.Add;
                            }
                            else if (s.Value.ToLowerInvariant().Equals("update"))
                            {
                                cmdLineArgs.OverwriteMode = TableOverwriteMode.Update;
                            }
                            else
                            {
                                goto UseMessage;
                            }
                        }
                        else if (s.Key.Equals("sourceorder"))
                        {
                            cmdLineArgs.SourceOrder = true;
                        }
                        else
                        {
                            goto UseMessage;
                        }
                    }

                    if (count == 0)
                    {
                        if (cmdLineArgs.DataType == SourceDataType.DbfAndKml && cmdLineArgs.OverwriteMode != TableOverwriteMode.Create)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(mode_restriction);
                            Console.ResetColor();
                            goto UseMessage;
                        }
                        return cmdLineArgs;
                    }
                }
            }

        UseMessage:
            Console.WriteLine(s_use);
            return null;
        }

        // parsing logic, with minor modifications, came from http://www.codeproject.com/KB/recipes/command_line.aspx
        private static Dictionary<string, string> ParseArguments(string[] args)
        {
            var parameters = new Dictionary<string, string>();
            var spliter = new Regex(@"^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var remover = new Regex(@"^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            string parameter = null;
            string[] parts;

            // Valid parameters forms:
            // {-,/,--}param{ ,=,:}((",')value(",'))
            // Examples: 
            // -param1 value1 --param2 /param3:"Test-:-work" 
            //   /param4=happy -param5 '--=nice=--'
            foreach (string txt in args)
            {
                // Look for new parameters (-,/ or --) and a
                // possible enclosed value (=,:)
                parts = spliter.Split(txt, 3);

                switch (parts.Length)
                {
                    // Found a value (for the last parameter 
                    // found (space separator))
                    case 1:
                        if (parameter != null)
                        {
                            if (!parameters.ContainsKey(parameter))
                            {
                                parts[0] =
                                    remover.Replace(parts[0], "$1");

                                parameters.Add(parameter.ToLowerInvariant(), parts[0]);
                            }

                            parameter = null;
                        }

                        // else Error: no parameter waiting for a value (skipped)
                        break;

                    // Found just a parameter
                    case 2:
                        // The last parameter is still waiting. 
                        // With no value, set it to true.
                        if (parameter != null)
                        {
                            if (!parameters.ContainsKey(parameter))
                            {
                                parameters.Add(parameter.ToLowerInvariant(), "true");
                            }
                        }

                        parameter = parts[1];
                        break;

                    // Parameter with enclosed value
                    case 3:
                        // The last parameter is still waiting. 
                        // With no value, set it to true.
                        if (parameter != null)
                        {
                            if (!parameters.ContainsKey(parameter))
                            {
                                parameters.Add(parameter.ToLowerInvariant(), "true");
                            }
                        }

                        parameter = parts[1];

                        // Remove possible enclosing characters (",')
                        if (!parameters.ContainsKey(parameter))
                        {
                            parts[2] = remover.Replace(parts[2], "$1");
                            parameters.Add(parameter.ToLowerInvariant(), parts[2]);
                        }

                        parameter = null;
                        break;
                }
            }

            // In case a parameter is still waiting
            if (parameter != null)
            {
                if (!parameters.ContainsKey(parameter))
                {
                    parameters.Add(parameter.ToLowerInvariant(), "true");
                }
            }

            return parameters;
        }
    }
}
