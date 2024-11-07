using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using parallel.Models;

namespace parallel
{
    public class ThreadsController
    {
        private ConcurrentBag<VideoInfo> globalVideoInfos = new ConcurrentBag<VideoInfo>();


        public async Task<IEnumerable<VideoInfo>> ProcessFileThreadSingle(string path)
        {

            var tasks = new List<Task>();

            object lockObj = new object();

            foreach (string file in Directory.GetFiles(path))
            {


                tasks.Add(Task.Run(() =>
                {
                    DateTime loadStart = DateTime.Now;

                    ProcessSingleFile(file);

                    DateTime loadEnd = DateTime.Now;


                    lock (lockObj)
                    {

                        Console.WriteLine(DateTime.Now.ToString() + " | File: " + System.IO.Path.GetFileName(path) + " | " + (loadEnd - loadStart).TotalMilliseconds + " ms");

                    }

                }));
            }

            await Task.WhenAll(tasks);
            DateTime endTime = DateTime.Now;
            Console.WriteLine("Last File End: " + endTime.ToString());

            return globalVideoInfos;

        }


        private void ProcessSingleFile(string filePath)
        {

            foreach (string line in File.ReadAllLines(filePath))
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


        public async Task<IEnumerable<VideoInfo>> ProcessFileThreadChunks(string path, int chunkSize = 100)
        {
            var tasks = new List<Task>();

            foreach (string file in Directory.GetFiles(path))
            {
                tasks.Add(Task.Run(() => ProcessFileInChunksMulticore(file, chunkSize)));
            }

            await Task.WhenAll(tasks);

            return globalVideoInfos;
        }

        private void ProcessFileInChunksMulticore(string filePath, int chunkSize)
        {
            var lines = File.ReadAllLines(filePath);
            int totalLines = lines.Length;

   
            var chunkRanges = new List<(int start, int end)>();
            for (int i = 0; i < totalLines; i += chunkSize)
            {
                int start = i;
                int end = Math.Min(i + chunkSize, totalLines);
                chunkRanges.Add((start, end));
            }

            Parallel.ForEach(chunkRanges, range =>
            {

                int threadId = Thread.CurrentThread.ManagedThreadId;
                // Imprimir Hilos trabjando
                //Console.WriteLine($"Thread {threadId} processing lines {range.start}-{range.end - 1} of file {Path.GetFileName(filePath)}");

                for (int j = range.start; j < range.end; j++)
                {
                    var video = ParseLine(lines[j], filePath);
                    if (video != null)
                    {
                        globalVideoInfos.Add(video);
                    }
                }
            });
        }

    }
}
