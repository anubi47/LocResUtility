using LocResLib;
using Serilog;
using System;
using System.IO;

namespace LocResCore.BusinessLogics
{
    public static class FileBusinessLogic
    {
        public static void Load(out LocResFile locresFile, string filePath)
        {
            locresFile = new LocResFile();
            try
            {
                if (File.Exists(filePath))
                    using (var file = File.OpenRead(filePath))
                        locresFile.Load(file);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to load [{Path.GetFileName(filePath)}] file!");
                Log.Debug(ex, $"Full path of file is [{Path.GetFullPath(filePath)}]!");
                throw;
            }
            Log.Information($"Loaded file [{Path.GetFileName(filePath)}].");
        }

        public static void Save(string filePath, LocResFile locresFile, LocResEncoding encoding)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                using (var file = File.Create(filePath))
                    locresFile.Save(file, locresFile.Version, encoding);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to create [{Path.GetFileName(filePath)}] file!");
                Log.Debug(ex, $"Full path of file is [{Path.GetFullPath(filePath)}]!");
                throw;
            }
            Log.Information($"Saved file [{Path.GetFileName(filePath)}]. (Encoding: {encoding})");
        }
    }
}
