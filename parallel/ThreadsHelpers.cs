using parallel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parallel
{
    public class ThreadsHelpers
    {

        public Arguments ReadArguments(string[] args)
        {
            var arguments = new Arguments();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-folder")
                {
                    // Verificar que "path" no este ya asignado
                    if (arguments.Path != null)
                    {
                        throw new ArgumentException("The path can't be set twice");
                    }
                    else if (i + 1 < args.Length)
                    {
                        arguments.Path = args[i + 1];
                    }
                }

                if (args[i] == "-s")
                {
                    // Verificar que "ExecutionType" no este ya asignado
                    if (arguments.ExecutionType != null)
                    {
                        throw new ArgumentException("The execution type can't be set twice");
                    }
                    arguments.ExecutionType = "SINGLE";
                }
                if (args[i] == "-st")
                {
                    // Verificar que "ExecutionType" no este ya asignado
                    if (arguments.ExecutionType != null)
                    {
                        throw new ArgumentException("The execution type can't be set twice");
                    }
                    arguments.ExecutionType = "SINGLETHREADS";
                }
                if (args[i] == "-mt")
                {
                    // Verificar que "ExecutionType" no este ya asignado
                    if (arguments.ExecutionType != null)
                    {
                        throw new ArgumentException("The execution type can't be set twice");
                    }
                    arguments.ExecutionType = "MULTIPLETHREADS";
                }
                if (args[i] == "-m")
                {
                    // Verificar que "ExecutionType" no este ya asignado
                    if (arguments.ExecutionType != null)
                    {
                        throw new ArgumentException("The execution type can't be set twice");
                    }
                    arguments.ExecutionType = "MULTIPLE";
                }
                if (args[i] == "-t")
                {
                    // Verificar que "ExecutionType" no este ya asignado
                    if (arguments.ExecutionType != null)
                    {
                        throw new ArgumentException("The execution type can't be set twice");
                    }
                    arguments.ExecutionType = "TEST";
                }

            }
            // Si "ExecutionType" no esta asignado se asigna "SEQUENCIAL"
            if (arguments.ExecutionType == null)
            {
                arguments.ExecutionType = "SEQUENCIAL";
            }

            return arguments;
        }


        public void PrintResults(IEnumerable<VideoInfo> globalVideos) {


            //foreach (VideoInfo v in globalVideos)
            //{

            //    Console.WriteLine(v.Views);
            //};

            // 1. Most popular video overall
                

            var mostPopularOverall = globalVideos
                .Where(v => v.Views.HasValue) 
                .OrderByDescending(v => v.Views)
                .FirstOrDefault();
            Console.WriteLine("--------------------------------------------------------------------------------------------------------");
            Console.WriteLine($"Most popular video overall: {mostPopularOverall?.Title} \nwith {mostPopularOverall?.Views} views");
            Console.WriteLine("--------------------------------------------------------------------------------------------------------");

            // 2. Least popular video overall
            var leastPopularOverall = globalVideos
                .Where(v => v.Views.HasValue)
                .OrderBy(v => v.Views)
                .FirstOrDefault();
            Console.WriteLine("--------------------------------------------------------------------------------------------------------");
            Console.WriteLine($"Least popular video overall: {leastPopularOverall?.Title} \nwith {leastPopularOverall?.Views} views");
            Console.WriteLine("--------------------------------------------------------------------------------------------------------");

            // 3. Most popular video per file
            var mostPopularPerFile = globalVideos
                .Where(v => v.Views.HasValue)
                .GroupBy(v => v.FileName)
                .Select(g => g.OrderByDescending(v => v.Views).FirstOrDefault());
            Console.WriteLine("\nMost popular video per region:");
            foreach (var video in mostPopularPerFile)
            {
                Console.WriteLine($"Region: {video?.FileName.Substring(32).Substring(0,2)}, Video: {video?.Title}, Views: {video?.Views}");
            }

            // 4. Least popular video per file
            var leastPopularPerFile = globalVideos
                .Where(v => v.Views.HasValue)
                .GroupBy(v => v.FileName)
                .Select(g => g.OrderBy(v => v.Views).FirstOrDefault());
            Console.WriteLine("\nLeast popular video per region:");
            foreach (var video in leastPopularPerFile)
            {
                Console.WriteLine($"Region: {video?.FileName.Substring(32).Substring(0, 2)}, Video: {video?.Title}, Views: {video?.Views}");
            }


        }

    }
}
