using System.Security.Cryptography;
using System.Text;

namespace UserTracker.HistoryFileTesterConsole
{
    public static class ZipPartHandler
    {
        public static void Initialize(string baseFolder)
        {
            BaseFolder = baseFolder;
            PendingPartsFolder = Path.Combine(BaseFolder, "PendingZipParts");
            FinishedPartsFolder = Path.Combine(BaseFolder, "FinishedZipParts");
            RepackFolder = Path.Combine(BaseFolder, "Repack");

            Console.WriteLine($"Initializing ZipPartHandler with base folder: {baseFolder}");
            Console.WriteLine($"  - PendingPartsFolder: {PendingPartsFolder}");
            Console.WriteLine($"  - FinishedPartsFolder: {FinishedPartsFolder}");
            Console.WriteLine($"  - RepackFolder: {RepackFolder}");

            if (!Directory.Exists(PendingPartsFolder))
            {
                Directory.CreateDirectory(PendingPartsFolder);
                Console.WriteLine($"Created PendingPartsFolder");
            }
            if (!Directory.Exists(FinishedPartsFolder))
            {
                Directory.CreateDirectory(FinishedPartsFolder);
                Console.WriteLine($"Created FinishedPartsFolder");
            }

            ExecuteRepackOfZips();
        }

        private static string BaseFolder { get; set; } = string.Empty;
        private static string PendingPartsFolder { get; set; } = string.Empty;
        private static string FinishedPartsFolder { get; set; } = string.Empty;
        private static string RepackFolder { get; set; } = string.Empty;

        private static string GenerateHashForFiles(string tempFolder, List<string> files)
        {
            var sortedFileNames = files
                .Select(f =>
                {
                    var relativePath = Path.GetRelativePath(tempFolder, f);
                    var parentDir = Path.GetDirectoryName(relativePath);
                    var fileName = Path.GetFileName(f);
                    
                    if (!string.IsNullOrEmpty(parentDir))
                    {
                        var parentFolderName = parentDir.Replace(Path.DirectorySeparatorChar, '_').Replace(Path.AltDirectorySeparatorChar, '_');
                        return $"{parentFolderName}_{fileName}";
                    }
                    return fileName;
                })
                .OrderBy(f => f)
                .ToList();

            var combined = string.Join("|", sortedFileNames);
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
            return Convert.ToHexString(hash)[..16];
        }

        public static async Task Handle(string path)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Handling zip part: {fileNameWithoutExtension}");

            var targetLocation = path[..^4];
            var finishedPartPath = Path.Combine(FinishedPartsFolder, $"{fileNameWithoutExtension}.zip");
            var backupZipPath = path + ".backup";

            bool zipExtracted = false;
            bool originalZipMoved = false;
            bool finishedZipCreated = false;

            try
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Extracting zip to: {targetLocation}");
                await System.IO.Compression.ZipFile.ExtractToDirectoryAsync(path, targetLocation);
                zipExtracted = true;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Extraction complete");

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Moving original zip to backup");
                File.Move(path, backupZipPath);
                originalZipMoved = true;

                //var historyFolder = new HistoryFolderClass(BaseFolder, targetLocation);
                //await historyFolder.Start();

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Creating finished zip");
                await System.IO.Compression.ZipFile.CreateFromDirectoryAsync(targetLocation, finishedPartPath, System.IO.Compression.CompressionLevel.Optimal, false);
                finishedZipCreated = true;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Finished zip created");

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Cleaning up temporary files");
                Directory.Delete(targetLocation, true);
                File.Delete(backupZipPath);

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✓ Finished handling zip part: {fileNameWithoutExtension}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✗ Error handling zip part {fileNameWithoutExtension}: {ex.Message}");
                Console.WriteLine("Initiating rollback...");

                try
                {
                    if (finishedZipCreated && File.Exists(finishedPartPath))
                    {
                        File.Delete(finishedPartPath);
                        Console.WriteLine($"  - Deleted finished zip");
                    }

                    if (originalZipMoved && File.Exists(backupZipPath))
                    {
                        File.Move(backupZipPath, path);
                        Console.WriteLine($"  - Restored original zip");
                    }

                    if (zipExtracted && Directory.Exists(targetLocation))
                    {
                        Directory.Delete(targetLocation, true);
                        Console.WriteLine($"  - Deleted extracted files");
                    }

                    Console.WriteLine($"✓ Rollback completed for {fileNameWithoutExtension}");
                }
                catch (Exception rollbackEx)
                {
                    Console.WriteLine($"✗ Critical: Rollback failed for {fileNameWithoutExtension}: {rollbackEx.Message}");
                    Console.WriteLine($"Manual intervention may be required. Backup location: {backupZipPath}");
                }

                throw;
            }
        }

        public static List<string> GetPendingParts()
        {
            var zipFiles = Directory.GetFiles(PendingPartsFolder, "*.zip");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Found {zipFiles.Length} pending zip parts");
            return zipFiles.ToList();
        }

        public static void ExecuteRepackOfZips()
        {
            if (!Directory.Exists(RepackFolder))
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] RepackFolder does not exist, skipping repack");
                return;
            }

            var zipFiles = Directory.GetFiles(RepackFolder, "*.zip");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Starting repack of {zipFiles.Length} zip file(s)");

            int processedCount = 0;
            foreach (var zipFile in zipFiles)
            {
                try
                {
                    processedCount++;
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(zipFile);
                    Console.WriteLine($"\n[{DateTime.Now:HH:mm:ss}] [{processedCount}/{zipFiles.Length}] Processing: {fileNameWithoutExtension}");

                    var tempFolder = Path.Combine(RepackFolder, fileNameWithoutExtension);

                    if (Directory.Exists(tempFolder))
                    {
                        Console.WriteLine($"  - Cleaning existing temp folder");
                        Directory.Delete(tempFolder, true);
                    }

                    Console.WriteLine($"  - Extracting zip...");
                    System.IO.Compression.ZipFile.ExtractToDirectory(zipFile, tempFolder);

                    var files = Directory.GetFiles(tempFolder, "*", SearchOption.AllDirectories);
                    var jsonFiles = files.Where(f => Path.GetExtension(f).Equals(".json", StringComparison.OrdinalIgnoreCase)).ToArray();
                    Console.WriteLine($"  - Found {jsonFiles.Length} JSON files out of {files.Length} total files");

                    const long maxBatchSize = 1000 * 1024 * 1024;
                    long currentBatchSize = 0;
                    int batchNumber = 0;
                    List<string> currentBatchFiles = new();

                    foreach (var file in jsonFiles)
                    {
                        var fileInfo = new FileInfo(file);
                        var fileSize = fileInfo.Length;

                        if (currentBatchSize + fileSize > maxBatchSize && currentBatchFiles.Count > 0)
                        {
                            var batchHash = GenerateHashForFiles(tempFolder, currentBatchFiles);
                            Console.WriteLine($"  - Creating batch {batchNumber} (hash: {batchHash}) with {currentBatchFiles.Count} files ({currentBatchSize / (1024 * 1024)} MB)");
                            CreateBatchZip(tempFolder, batchHash, currentBatchFiles);
                            batchNumber++;
                            currentBatchFiles.Clear();
                            currentBatchSize = 0;
                        }

                        currentBatchFiles.Add(file);
                        currentBatchSize += fileSize;
                    }

                    if (currentBatchFiles.Count > 0)
                    {
                        var batchHash = GenerateHashForFiles(tempFolder, currentBatchFiles);
                        Console.WriteLine($"  - Creating final batch {batchNumber} (hash: {batchHash}) with {currentBatchFiles.Count} files ({currentBatchSize / (1024 * 1024)} MB)");
                        CreateBatchZip(tempFolder, batchHash, currentBatchFiles);
                    }

                    Console.WriteLine($"  - Cleaning up temp folder");
                    Directory.Delete(tempFolder, true);
                    File.Delete(zipFile);
                    Console.WriteLine($"  ✓ Completed processing {fileNameWithoutExtension} - created {batchNumber + 1} batch(es)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ✗ Error repacking zip {Path.GetFileName(zipFile)}: {ex.Message}");
                    Console.WriteLine($"     Stack trace: {ex.StackTrace}");
                }
            }

            Console.WriteLine($"\n[{DateTime.Now:HH:mm:ss}] Repack complete: {processedCount} zip file(s) processed");
        }

        private static void CreateBatchZip(string tempFolder, string zipName, List<string> files)
        {
            var batchZipPath = Path.Combine(PendingPartsFolder, $"{zipName}.zip");

            if (File.Exists(batchZipPath))
            {
                Console.WriteLine($"    - Skipping {Path.GetFileName(batchZipPath)} (already exists)");
                return;
            }

            int addedCount = 0;
            long totalSize = 0;

            using var zipArchive = System.IO.Compression.ZipFile.Open(batchZipPath, System.IO.Compression.ZipArchiveMode.Create);
            foreach (var file in files)
            {
                if (!Path.GetExtension(file).Equals(".json", StringComparison.OrdinalIgnoreCase))
                    continue;

                var relativePath = Path.GetRelativePath(tempFolder, file);
                var parentDir = Path.GetDirectoryName(relativePath);
                var fileName = Path.GetFileName(file);
                
                string entryName;
                if (!string.IsNullOrEmpty(parentDir))
                {
                    var parentFolderName = parentDir.Replace(Path.DirectorySeparatorChar, '_').Replace(Path.AltDirectorySeparatorChar, '_');
                    entryName = $"{parentFolderName}_{fileName}";
                }
                else
                {
                    entryName = fileName;
                }

                using var entryStream = zipArchive.CreateEntry(entryName, System.IO.Compression.CompressionLevel.Optimal).Open();
                using var fileStream = File.OpenRead(file);
                totalSize += fileStream.Length;
                fileStream.CopyTo(entryStream);
                addedCount++;
            }

            Console.WriteLine($"    - Created {Path.GetFileName(batchZipPath)}: {addedCount} files, {totalSize / (1024 * 1024)} MB uncompressed");
        }
    }
}