using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

using Ogdi.Data.DataLoader;
using System.Diagnostics;

namespace Ogdi.Data.DataLoaderConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime start = DateTime.Now;

            try
            {
                if (args.Length == 1)
                {
                    var r = File.OpenText(args[0]);

                    while (!r.EndOfStream)
                    {
                        var cmd = r.ReadLine();
                        LoadDataset(cmd.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries));
                    }
                }
                else
                {
                    LoadDataset(args);
                }
            }
            catch (Exception ex)
            {
                PrintException(ex, false);
            }

            long elapsedTicks = DateTime.Now.Ticks - start.Ticks;
            TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
            Console.WriteLine("\tProcessing time: {0:N2} seconds", elapsedSpan.TotalSeconds);
            Console.WriteLine("\nHit enter to exit...");
            Console.ReadLine();
        }

        private static void LoadDataset(string[] args)
        {
            var cmdLineParams = CommandLineParser.ParseCommandLine(args);

            if (cmdLineParams != null)
            {
                Console.WriteLine("Loading {0}...", cmdLineParams.FileSetName);
                if (cmdLineParams.RefreshLastUpdateDate)
                {
                    RefreshLastUpdatedDate(cmdLineParams.FileSetName + DataLoaderConstants.FileExtConfig);
                }

                IDataLoader loader = DataLoaderFactory.CreateDataLoader(cmdLineParams.DataType, cmdLineParams.LoadingTarget, cmdLineParams.FileSetName, cmdLineParams.OverwriteMode,cmdLineParams.SourceOrder);

                if (cmdLineParams.LoadingTarget == DataLoadingTarget.Console)
                {
                    loader.Load(null,null);
                }
                else
                {
                    loader.Load((tc, cc) => 
                    {
                        if ((tc - cc) % 100 == 0 || tc <= 500)
                            Console.WriteLine("\t{0} entities remain to be processed...\r", tc - cc); 
                    }, ExceptionNotifier);
                    Console.WriteLine();
                }
            }
        }

        private static void PrintException(Exception ex, bool isOnContinue)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ex.GetType());
            sb.Append("\r\n");
            while (ex != null)
            {
                sb.Append(ex.Message);
                sb.Append("\r\n\t");
                sb.Append(ex.StackTrace);
                sb.Append("\r\n\t");
                ex = ex.InnerException;
            }

            Console.ForegroundColor = (isOnContinue) ? ConsoleColor.Yellow : ConsoleColor.Red;
            Console.WriteLine(sb.ToString());            
            Console.ResetColor();
            Trace.WriteLine(sb.ToString());
        }

        private static void RefreshLastUpdatedDate(string fileName)
        {
            XElement cfg;
            using (var r = File.OpenText(fileName))
            {
                using (var xr = XmlReader.Create(r))
                {
                    cfg = XElement.Load(xr);
                    var e = cfg.Descendants("LastUpdateDate");
                    e.First().SetValue(DateTime.Now.Date);
                }
            }

            using (var w = File.CreateText(fileName))
            {
                w.Write(cfg.ToString());
            }
        }

        private static void ExceptionNotifier(Exception ex)
        {
            PrintException(ex, true);
        }
    }
}