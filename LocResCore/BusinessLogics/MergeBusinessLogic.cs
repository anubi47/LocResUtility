using LocResLib;
using Serilog;
using System;
using System.IO;

namespace LocResCore.BusinessLogics
{
    public enum MergeMode
    {
        Update = 1,
        Insert = 2,
        InsertAndUpdate = 3
    }

    public static class MergeBusinessLogic
    {

        public static int Merge(string outputPath, string targetPath, string sourcePath, string culture, MergeMode mode, LocResEncoding encoding)
        {
            string extension = Path.GetExtension(outputPath);

            if (!string.Equals(extension, ".locres"))
            {
                Log.Error($"Output file extension should be .locres!");
                return 1;
            }

            if (!File.Exists(targetPath))
            {
                Log.Error($"Target file [{Path.GetFileName(targetPath)}] does not exist!");
                return 1;
            }

            if (!File.Exists(sourcePath))
            {
                Log.Error($"Source file [{Path.GetFileName(sourcePath)}] does not exist!");
                return 1;
            }

            //if (!suppressPrompt && string.Equals(Path.GetFullPath(opt.OutputPath), Path.GetFileName(opt.TargetPath), StringComparison.OrdinalIgnoreCase))
            //{
            //    Console.Write($"Output and Target file have same path. Overwrite? (Y/N) ");
            //    ConsoleKeyInfo cki = Console.ReadKey();
            //    Console.WriteLine();
            //    Console.WriteLine();
            //    if (cki.Key.ToString().ToUpper() != "Y")
            //    {
            //        return 1;
            //    }
            //}

            //if (!suppressPrompt && File.Exists(outputPath))
            //{
            //    Console.Write($"File [{Path.GetFileName(outputPath)}] already exist. Overwrite? (Y/N) ");
            //    ConsoleKeyInfo cki = Console.ReadKey();
            //    Console.WriteLine();
            //    Console.WriteLine();
            //    if (cki.Key.ToString().ToUpper() != "Y")
            //    {
            //        return 1;
            //    }
            //}

            try
            {
                Log.Information($"Merge [{culture}] culture.");
                FileBusinessLogic.Load(out LocResFile targetFile, targetPath);
                FileBusinessLogic.Load(out LocResFile sourceFile, sourcePath);
                Merge(targetFile, sourceFile, mode);
                FileBusinessLogic.Save(outputPath, targetFile, encoding);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception!");
                return 1;
            }

            return 0;
        }

        private static void Merge(LocResFile targetFile, LocResFile sourceFile, MergeMode mode)
        {
            if (targetFile is null)
                throw new ArgumentNullException(nameof(targetFile));
            if (sourceFile is null)
                throw new ArgumentNullException(nameof(sourceFile));

            int add = 0;
            int edit = 0;
            int sourceOnly = 0;

            Log.Information($"Target has [{targetFile.CountStrings}] resources.");
            Log.Information($"Source has [{sourceFile.CountStrings}] resources.");

            foreach (var targetNamespace in targetFile)
            {
                if (!sourceFile.TryGetNamespace(targetNamespace.Name, out LocResNamespace sourceNamespace))
                {
                    Log.Debug($"[{targetNamespace.Name}] namespace present in target only!");
                    continue;
                }

                foreach (var targetString in targetNamespace)
                {
                    if (!sourceNamespace.TryGetString(targetString.Key, out LocResString sourceString))
                    {
                        Log.Debug($"[{targetNamespace.Name}/{targetString.Key}] present in target only!");
                        continue;
                    }

                    if (mode == MergeMode.Update || mode == MergeMode.InsertAndUpdate)
                    {
                        Log.Debug($"[{targetNamespace.Name}/{targetString.Key}] [{targetString.Value}] < [{sourceString.Value}]");
                        targetString.Value = sourceString.Value;
                        edit++;
                    }
                }
            }

            foreach (var sourceNamespace in sourceFile)
            {
                if (!targetFile.TryGetNamespace(sourceNamespace.Name, out LocResNamespace targetNamespace))
                {
                    if (mode == MergeMode.Insert || mode == MergeMode.InsertAndUpdate)
                    {
                        Log.Debug($"[{sourceNamespace.Name}] namespace added in target!");
                        targetNamespace = new LocResNamespace(sourceNamespace.Name);
                        targetFile.Add(targetNamespace);
                    }
                    else
                    {
                        Log.Debug($"[{sourceNamespace.Name}] namespace present in source only!");
                    }
                }

                foreach (var sourceString in sourceNamespace)
                {
                    if (!targetNamespace.TryGetString(sourceString.Key, out LocResString targetString))
                    {
                        sourceOnly++;
                        if (mode == MergeMode.Insert || mode == MergeMode.InsertAndUpdate)
                        {
                            Log.Debug($"[{sourceNamespace.Name}/{sourceString.Key}] added in target!");
                            targetString = new LocResString(sourceString.Key, sourceString.Value, sourceString.Hash);
                            targetNamespace.Add(targetString);
                            add++;
                        }
                        else
                        {
                            Log.Debug($"[{sourceNamespace.Name}/{sourceString.Key}] present in source only!");
                            continue;
                        }
                    }
                }
            }

            Log.Information($"Found [{sourceOnly}] resources present in source only.");
            Log.Information($"Added [{add}] new resources from source to target.");
            Log.Information($"Applied [{edit}] values from source to target.");
        }

    }
}
