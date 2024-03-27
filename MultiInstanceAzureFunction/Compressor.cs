using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiInstanceAzureFunction
{
    internal class Compressor
    {
        static void SplitZipFile(string zipFilePath, string outputDirectory, int maxFileSizeMB)
        {
            zipFilePath = @"D:\Archive\input";
            outputDirectory = @"D:\Archive\input";
            int partNumber = 1;
            long maxFileSizeBytes = 1024 * 1024 * 1024;

            using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
            {
                long totalSize = 0;
                int currentPartSize = 0;

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (totalSize + entry.Length > maxFileSizeBytes || currentPartSize == 0)
                    {
                        // Start a new part
                        string partFilePath = Path.Combine(outputDirectory, $"part{partNumber}.zip");
                        using (FileStream partFileStream = new FileStream(partFilePath, FileMode.Create))
                        using (ZipArchive partArchive = new ZipArchive(partFileStream, ZipArchiveMode.Create))
                        {
                            partNumber++;
                            currentPartSize = 0;
                        }
                    }

                    // Add entry to current part
                    string currentPartFilePath = Path.Combine(outputDirectory, $"part{partNumber - 1}.zip");
                    using (FileStream partFileStream = new FileStream(currentPartFilePath, FileMode.Open))
                    using (ZipArchive partArchive = new ZipArchive(partFileStream, ZipArchiveMode.Update))
                    {
                        ZipArchiveEntry newEntry = partArchive.CreateEntry(entry.FullName, CompressionLevel.Optimal);
                        using (Stream entryStream = entry.Open())
                        using (Stream newEntryStream = newEntry.Open())
                        {
                            entryStream.CopyTo(newEntryStream);
                        }
                    }

                    totalSize += entry.Length;
                    currentPartSize += (int)entry.Length;
                }
            }
        }
    }
}
