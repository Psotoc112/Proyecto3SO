using parallel;
using parallel.Models;
using System;
using System.Runtime.InteropServices;

// Instancia de ParallelController
ParallelController MyController = new ParallelController();

// Ejecucion del programa eimpresion de la respuesta 

/*

foreach (string outputMessage in MyController.ProcessFile(Environment.GetCommandLineArgs()))
{
	Console.WriteLine(outputMessage);

};
Console.ReadLine();


*/



// Estado del programa
int status = -1; // -1 => Sin procesar, 0 => Exitoso, 1 => Error

// Variable para calcular el tiempo de ejecucion del programa
DateTime start = DateTime.Now;

// Instances of ny clases
ThreadsHelpers helper = new ThreadsHelpers();
ThreadsController controller = new ThreadsController();


Arguments arguments = helper.ReadArguments(Environment.GetCommandLineArgs());

IEnumerable<VideoInfo> videoInfos = new List<VideoInfo>();


Console.WriteLine("------------------------------------------------");
Console.WriteLine("Program Start time: " + start.ToString());
Console.WriteLine("------------------------------------------------");


// Controlll the submition of a path
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



switch (arguments.ExecutionType)
{
    case "SINGLETHREADS":
        videoInfos = await controller.ProcessFileThreadSingle(arguments.Path);
        if (videoInfos != null)
        {
            status = 0;
        }
        break;
    case "MULTIPLETHREADS":
        videoInfos = await controller.ProcessFileThreadChunks(arguments.Path);
        if (videoInfos != null)
        {
            status = 0;
        }
        break;

}




//Console.WriteLine(videoInfos.Count().ToString());


helper.PrintResults(videoInfos);


DateTime end = DateTime.Now;
TimeSpan elapsed = end - start;

Console.WriteLine("------------------------------------------------");
Console.WriteLine("Elapsed time: " + elapsed.TotalMilliseconds.ToString() + " milliseconds");
Console.WriteLine("Status: " + status.ToString());
Console.WriteLine("------------------------------------------------");

//Console.WriteLine(arguments.Path);