using System;
using System.Diagnostics;
using System.IO;

namespace RestartManager
{
    internal class Program
    {
        private Process proc;

        private static void Main(string[] args) => new Program().Run();

        public void Run()
        {
            Console.WriteLine("Generating new Process");
            proc = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "run -f Release",
                    WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), @"../Ruby Rose/"),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                }
            };

            Console.WriteLine("Starting Process");
            proc.Start();
            Console.WriteLine("Process Started");
            proc.Exited += Proc_Exited;
            proc.WaitForExit();
            Console.WriteLine("Process Exited");
        }

        private void Proc_Exited(object sender, EventArgs e)
        {
            if (proc.ExitCode == 1)
            {
                Console.WriteLine("Received ExitCode 1. Restarting..");
                Run();
            }
        }
    }
}