﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchJob
{
    public class ErrorLog : IDisposable
    {         
        void IDisposable.Dispose()
        {
        }
        public void LogErrorInTextFile(Exception ex)
        {
            string webRootPath = Directory.GetCurrentDirectory();

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

        public ErrorLog()
        {

        }
    }
}
