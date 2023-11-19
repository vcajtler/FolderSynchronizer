using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

class FolderSynchronizer
{
    static void Main(string[] args)
    {
        if (args.Length != 4)
        {
            Console.WriteLine("Usage: FolderSynchronizer.exe <sourceFolderPath> <replicaFolderPath> <logFilePath> <syncIntervalInSeconds>");
            return;
        }

        string sourceFolderPath = args[0];
        string replicaFolderPath = args[1];
        string logFilePath = args[2];
        int syncIntervalInSeconds;

        if (!int.TryParse(args[3], out syncIntervalInSeconds) || syncIntervalInSeconds <= 0)
        {
            Console.WriteLine("Invalid sync interval. Please provide a positive integer for syncIntervalInSeconds.");
            return;
        }

        Console.WriteLine($"Synchronization started. Source: {sourceFolderPath}, Replica: {replicaFolderPath}, Sync Interval: {syncIntervalInSeconds} seconds");

        while (true)
        {
            if (!Directory.Exists(replicaFolderPath))
            {
                Directory.CreateDirectory(replicaFolderPath);
                LogToFile($"Created folder: {replicaFolderPath}", logFilePath);
            }

            SynchronizeFolders(sourceFolderPath, replicaFolderPath, logFilePath);
            Thread.Sleep(syncIntervalInSeconds*1000);
        }
    }

    static void SynchronizeFolders(string sourcePath, string replicaPath, string logFilePath)
    {
        try
        {
            // Synchronize files
            foreach (string sourceFile in Directory.GetFiles(sourcePath))
            {
                string destinationFile = Path.Combine(replicaPath, Path.GetFileName(sourceFile));

                // Copy or update file if it's different
                if (!File.Exists(destinationFile) || !AreFilesEqual(sourceFile, destinationFile))
                {
                    File.Copy(sourceFile, destinationFile, true);
                    LogToFile($"Copied: {sourceFile} to {destinationFile}", logFilePath);
                }
            }

            // Remove files from replica that are not in source
            foreach (string replicaFile in Directory.GetFiles(replicaPath))
            {
                string sourceFile = Path.Combine(sourcePath, Path.GetFileName(replicaFile));

                if (!File.Exists(sourceFile))
                {
                    File.Delete(replicaFile);
                    LogToFile($"Deleted: {replicaFile}", logFilePath);
                }
            }

            // Synchronize directories recursively
            foreach (string sourceFolder in Directory.GetDirectories(sourcePath))
            {
                string destinationFolder = Path.Combine(replicaPath, Path.GetFileName(sourceFolder));

                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                    LogToFile($"Created folder: {destinationFolder}", logFilePath);
                }

                SynchronizeFolders(sourceFolder, destinationFolder, logFilePath);
            }

            // Remove directories from replica that are not in source
            foreach (string replicaFolder in Directory.GetDirectories(replicaPath))
            {
                string sourceFolder = Path.Combine(sourcePath, Path.GetFileName(replicaFolder));

                if (!Directory.Exists(sourceFolder))
                {
                    Directory.Delete(replicaFolder, true);
                    LogToFile($"Deleted folder: {replicaFolder}", logFilePath);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }



    static bool AreFilesEqual(string file1, string file2)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream1 = File.OpenRead(file1))
            using (var stream2 = File.OpenRead(file2))
            {
                byte[] hash1 = md5.ComputeHash(stream1);
                byte[] hash2 = md5.ComputeHash(stream2);

                for (int i = 0; i < hash1.Length; i++)
                {
                    if (hash1[i] != hash2[i])
                        return false;
                }
            }
        }

        return true;
    }

    static void LogToFile(string logMessage, string logFilePath)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string logEntry = $"{timestamp} - {logMessage}";

        Console.WriteLine(logEntry);

        try
        {
            if (!File.Exists(logFilePath))
            {
                // If the log file doesn't exist, attempt to create it
                using (File.Create(logFilePath)) { }
            }

            File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to log file: {ex.Message}");
        }
    }
}
