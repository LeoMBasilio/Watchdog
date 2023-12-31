﻿using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;

namespace Watchdog
{
    public class program
    {
        //---> variables

        static string nome = "", status = "";

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        const int SW_HIDE = 0;

        const string uri = @"caminho do programa a ser aberto";
        private const string uriOut = @"caminho de saida em json";
        static string output = "", output1 = "", output2 = "", output3 = "";
        


        //---> main
        public static void Main(string[] args)
        {
            string json = "";
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            while (true)
            {
                Task tracertTask = Task.Run(() => json = tracert());
                Task verifyTask = Task.Run(() => verify());
                Task.WaitAll(tracertTask);

                using (StreamWriter sw = new StreamWriter(uriOut))
                {
                    sw.WriteLine(json);
                }

                Thread.Sleep(3000);
            }
            
        }

        private static string tracert()
        {
            String[] _gateway = {"enderecos para verificacao"};
            ProcessStartInfo startInfo;
            Process process;

            startInfo = new ProcessStartInfo("cmd.exe", "/c tracert -h 1 " + _gateway[0]);
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            process = Process.Start(startInfo);
            process.WaitForExit();
            output = process.StandardOutput.ReadToEnd();

            startInfo = new ProcessStartInfo("cmd.exe", "/c tracert -h 1 " + _gateway[1]);
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            process = Process.Start(startInfo);
            process.WaitForExit();
            output1 = process.StandardOutput.ReadToEnd();

            startInfo = new ProcessStartInfo("cmd.exe", "/c tracert -h 1 " + _gateway[2]);
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            process = Process.Start(startInfo);
            process.WaitForExit();
            output2 = process.StandardOutput.ReadToEnd();

            startInfo = new ProcessStartInfo("cmd.exe", "/c tracert -h 1 " + _gateway[3]);
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            process = Process.Start(startInfo);
            process.WaitForExit();
            output3 = process.StandardOutput.ReadToEnd();

            return "{\"provedor\":\"" + output + "\",\"provedor\":\"" + output1 + "\",\"provedor\":\"" + output2 + "\",\"provedor\":\"" + output3 + "\"}";
        }

        private static void verify()
        {
            try
            {
                string _nomeProcesso = "processo a ser monitorado";

                // Obtém todos os processos com o nome do atual
                Process[] processes = Process.GetProcessesByName(_nomeProcesso);
        
                NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
                NetworkInterface adapter = adapters[0];

                nome = adapter.Description;
                status = adapter.OperationalStatus.ToString();

                Console.WriteLine(status);
                Console.WriteLine(processes.Length);

                if ((nome.Contains("Fortinet SSL VPN Virtual Ethernet Adapter") != true))
                {
                    System.Diagnostics.Process.Start("powershell.exe", "taskkill -f -im " + _nomeProcesso).WaitForExit();
                }
                else if ((nome.Contains("Fortinet SSL VPN Virtual Ethernet Adapter") == true) && (status.Contains("Up") == true))
                {
                    if (processes.Length > 0)
                    {
                        Console.WriteLine("Processo já está em execução");
                    }
                    else
                    {
                        Process process = Process.Start(uri);
                        process.WaitForInputIdle();
                        while (!process.HasExited)
                        {
                            process.WaitForExit(3000);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

    }
}
