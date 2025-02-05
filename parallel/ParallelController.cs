﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using parallel.Models;
namespace parallel
{
    public class ParallelController
    {


        private List<string> outputMessages { get; set; } = new List<string>();

     
        private ConcurrentBag<VideoInfo> globalVideoInfos = new ConcurrentBag<VideoInfo>();

        public List<string> ProcessFile(string[] args)
        {

            // -----------------------------------------------------------------------------------------------
            //	Mock args para testing

            //	string[] argsTest = new string[] { "-folder", "D:\ParallelTestData", "-s"};

            //	args = argsTest;

            // -----------------------------------------------------------------------------------------------



            // Estado del programa
            int status = -1; // -1 => Sin procesar, 0 => Exitoso, 1 => Error

            // Variable para calcular el tiempo de ejecucion del programa
            DateTime start = DateTime.Now;

            outputMessages.Add("------------------------------------------------");
            outputMessages.Add("Program Start time: " + start.ToString());
            outputMessages.Add("------------------------------------------------");
            // Tomar los argumentos ingresados
            var arguments = ReadArguments(args);


       
            // Verificacion de que la direccion del folder es correcta
            if (arguments.Path == null)
            {
                throw new ArgumentException("The path is not set");
            }
            else
            {
                if (!System.IO.Directory.Exists(arguments.Path))
                {
                    throw new ArgumentException("The path is not a valid directory");
                }
            }

            if (arguments.ExecutionType == null)
            {
                throw new ArgumentException("The execution type is not set");
            }

            // Si ExecutionType = SINGLE
            if (arguments.ExecutionType == "SINGLE")
            {
                status = ProcessFileSingle(arguments.Path);
            }

            // Si ExecutionType = MULTIPLE
            if (arguments.ExecutionType == "MULTIPLE")
            {
                status = ProcessFileMultiple(arguments.Path);
            }
            // Si ExecutionType = SEQUENCIAL
            if (arguments.ExecutionType == "SEQUENCIAL")
            {
                status = ProcessFileSequencial(arguments.Path);
            }
            if (arguments.ExecutionType == "SINGLETHREADS")
            {
                status = ProcessFileThreadSingle(arguments.Path).Result;
            }
            if (arguments.ExecutionType == "TEST")
            {
                checkFiles(arguments.Path);
                status = 0;
            }




            // Calcular el tiempo transcurrido
            DateTime end = DateTime.Now;
            TimeSpan elapsed = end - start;

            outputMessages.Add("------------------------------------------------");
            outputMessages.Add("Elapsed time: " + elapsed.TotalMilliseconds.ToString() + " milliseconds");
            outputMessages.Add("Status: " + status.ToString());
            outputMessages.Add("------------------------------------------------");


            outputMessages.Add("--xxx---");
            outputMessages.Add(globalVideoInfos.Count().ToString());

            return outputMessages;



        }

        #region NoThreads

        private int ProcessFileMultiple(string path)
        {
            try
            {
                outputMessages.Add("------------------------------------------------");
                outputMessages.Add("Execution Type: MULTIPLE ");


                DateTime file0start = DateTime.Now;

                outputMessages.Add("First File Start: " + file0start.ToString());


                Parallel.ForEach(System.IO.Directory.GetFiles(path), file =>
                {
                    DateTime loadStart = DateTime.Now;
                    var FileContentList = new List<string>();
                    foreach (string line in System.IO.File.ReadAllLines(file))
                    {
                        FileContentList.Add(line);

                    }
                    DateTime loadEnd = DateTime.Now;

                    // Agregar al output la hora, el nombre del archivo y lo que se demoro en cargar
                    outputMessages.Add(DateTime.Now.ToString() + " | File: " + System.IO.Path.GetFileName(file) + " | " + (loadEnd - loadStart).Milliseconds + " ms");

                });


                outputMessages.Add("Last File End: " + DateTime.Now.ToString());
                return 0;

            }
            catch (Exception ex)
            {
                outputMessages.Add(ex.Message);
                return 1;
            }
        }

        private int ProcessFileSingle(string path)
        {
            try
            {
                outputMessages.Add("------------------------------------------------");
                outputMessages.Add("Execution Type: SINGLE ");


                DateTime file0start = DateTime.Now;

                outputMessages.Add("First File Start: " + file0start.ToString());

                // Afinidad para single core
                Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)1;

                Parallel.ForEach(System.IO.Directory.GetFiles(path), file =>
                {
                    DateTime loadStart = DateTime.Now;
                    var FileContentList = new List<string>();
                    foreach (string line in System.IO.File.ReadAllLines(file))
                    {
                        FileContentList.Add(line);

                    }
                    DateTime loadEnd = DateTime.Now;

                    // Agregar al output la hora, el nombre del archivo y lo que se demoro en cargar
                    outputMessages.Add(DateTime.Now.ToString() + " | File: " + System.IO.Path.GetFileName(file) + " | " + (loadEnd - loadStart).Milliseconds + " ms");

                });


                outputMessages.Add("Last File End: " + DateTime.Now.ToString());
                return 0;

            }
            catch (Exception ex)
            {

                outputMessages.Add(ex.Message);
                return 1;
            }

        }

        private int ProcessFileSequencial(string path)
        {
            try
            {


                outputMessages.Add("------------------------------------------------");
                outputMessages.Add("Execution Type: SEQUENCIAL ");


                DateTime file0start = DateTime.Now;

                outputMessages.Add("First File Start: " + file0start.ToString());


                Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)2;

                foreach (string file in System.IO.Directory.GetFiles(path))
                {

                    DateTime loadStart = DateTime.Now;
                    var FileContentList = new List<string>();
                    foreach (string line in System.IO.File.ReadAllLines(file))
                    {
                        FileContentList.Add(line);

                    }
                    DateTime loadEnd = DateTime.Now;

                    // Agregar al output la hora, el nombre del archivo y lo que se demoro en cargar
                    outputMessages.Add(DateTime.Now.ToString() + " | File: " + System.IO.Path.GetFileName(file) + " | " + (loadEnd - loadStart).Milliseconds + " ms");


                }
                outputMessages.Add("Last File End: " + DateTime.Now.ToString());


                return 0;
            }
            catch (Exception ex)
            {

                outputMessages.Add(ex.Message);
                return 1;
            }



        }

        #endregion



        #region Threads


        private async Task<int> ProcessFileThreadSingle(string path)
        {
            try
            {
                outputMessages.Add("------------------------------------------------");
                outputMessages.Add("Execution Type: SINGLE THREADS");

                DateTime startTime = DateTime.Now;
                outputMessages.Add("First File Start: " + startTime.ToString());


                var tasks = new List<Task>();


                object lockObj = new object();

                foreach (string file in System.IO.Directory.GetFiles(path))
                {

                    tasks.Add(Task.Run(() =>
                    {
                        DateTime loadStart = DateTime.Now;

                        ProcessSingleFile(file, lockObj);

                        DateTime loadEnd = DateTime.Now;

                        lock (lockObj)
                        {
                            
                            outputMessages.Add(DateTime.Now.ToString() + " | File: " + System.IO.Path.GetFileName(path) + " | " + (loadEnd - loadStart).TotalMilliseconds + " ms");
                         
                        }

                    }));
                }



                // Wait for all tasks to complete
                await Task.WhenAll(tasks);

                DateTime endTime = DateTime.Now;
                outputMessages.Add("Last File End: " + endTime.ToString());
                //DisplaySummaryResults();


                


                return 0;
            }
            catch (Exception ex)
            {
                outputMessages.Add(ex.Message);
                return 1;
            }



        }

        private void checkFiles(string path)
        {
            var fileContentList = new List<string>();
            int ok = 0;
            int notok = 0;
            foreach (string file in System.IO.Directory.GetFiles(path))
            {
                foreach (string line in System.IO.File.ReadAllLines(file))
                {


                    fileContentList.Add(line);


                    var video = ParseLine(line, path);
                    if (video == null)
                    {
                        notok++;
                    }
                    else
                    {
                        ok++;
                    }


                }
                Console.WriteLine(file);
                Console.WriteLine("Rows ok: " + ok);
                Console.WriteLine("Rows not ok: " + notok);
                Console.WriteLine("Error percentage: " + ((float)notok / (float)ok) * 100 + "%");
                Console.WriteLine("--------------------------");






            }


        }


        private void ProcessSingleFile(string filePath, object lockObj)
        {

            var videoResponse = new List<VideoInfo>();
     
            foreach (string line in System.IO.File.ReadAllLines(filePath))
            {


                var video = ParseLine(line, filePath);
                if (video != null)
                {
                   globalVideoInfos.Add(video);
                }
               

            }
                       

        }


        private VideoInfo ParseLine(string line, string FileName)
        {
            try
            {
                var columns = line.Split(',');
                return new VideoInfo
                {
                    FileName = FileName,
                    Title = columns[2],
                    Views = int.Parse(columns[7])
                };
            }
            catch (Exception ex)
            {
                return null;

            }



        }


        #endregion



        // Argumentos de ejecucion
        private Arguments ReadArguments(string[] args)
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




    }

}
