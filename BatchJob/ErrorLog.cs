using Microsoft.Extensions.Options;
using Sponge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchJob
{
    public class ErrorLog : IDisposable
    {
        private readonly string webRootPath = "E:\\Sponge\\Excel";


        void IDisposable.Dispose()
        {
        }
        public void LogErrorInTextFile(Exception ex)
        {

            string logFolderPath = Path.Combine(webRootPath, "ErrorLog");
            string logFilePath = Path.Combine(logFolderPath, "ErrorLog.txt");

            string message = $"Time: {DateTime.Now:dd/MM/yyyy hh:mm:ss tt}\n";
            message += "-----------------------------------------------------------\n";
            message += $"Message: {ex.Message}\n";
            message += $"StackTrace: {ex.StackTrace}\n";
            message += $"Source: {ex.Source}\n";
            message += $"TargetSite: {ex.TargetSite?.ToString()}\n";
            message += $"InnerException: {ex.InnerException}\n";
            message += "-----------------------------------------------------------\n";

            Directory.CreateDirectory(logFolderPath);
            File.AppendAllText(logFilePath, message);
        }
        public void LogTextInTextFile(string st)
        {
            string logFolderPath = Path.Combine(webRootPath, "ErrorLog");
            string logFilePath = Path.Combine(logFolderPath, "ErrorLog.txt");

            string message = $"Time: {DateTime.Now:dd/MM/yyyy hh:mm:ss tt}\n";
            message += "-----------------------------------------------------------\n";
            message += $"Message: {st}\n";
           
            message += "-----------------------------------------------------------\n";

            Directory.CreateDirectory(logFolderPath);
            File.AppendAllText(logFilePath, message);
        }
        public ErrorLog()
        {

        }
        public void LogErrorInTextFile(Exception ex,string ErrorDetails)
        {
            string logFolderPath = Path.Combine(webRootPath, "ErrorLog");
            string logFilePath = Path.Combine(logFolderPath, "ErrorLog.txt");

            string message = $"Time: {DateTime.Now:dd/MM/yyyy hh:mm:ss tt}\n";
            message += "-----------------------------------------------------------\n";
            message += $"Message: {ex.Message}\n";
            message += $"StackTrace: {ex.StackTrace}\n";
            message += $"Source: {ex.Source}\n";
            message += $"TargetSite: {ex.TargetSite?.ToString()}\n";
            message += $"InnerException: {ex.InnerException}\n";
            message += "-----------------------------------------------------------\n";
            message += $"Details: {ErrorDetails}\n";
            Directory.CreateDirectory(logFolderPath);
            File.AppendAllText(logFilePath, message);
        }
    }
}
