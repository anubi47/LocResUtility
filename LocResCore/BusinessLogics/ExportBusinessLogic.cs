using LocResCore.Translations;
using LocResLib;
using Serilog;
using System;
using System.IO;

namespace LocResCore.BusinessLogics
{
    public static class ExportBusinessLogic
    {
        public static int Export(string outputPath, string targetPath, string sourcePath, string culture, ITranslationWriterPool translationWriterPool)
        {
            if (translationWriterPool is null)
                throw new ArgumentNullException(nameof(translationWriterPool));

            string extension = Path.GetExtension(outputPath);

            if (string.IsNullOrEmpty(extension))
            {
                Log.Error($"Invalid or missing extension [{extension}] on file [{Path.GetFileName(outputPath)}]!");
                return 1;
            }

            if (!translationWriterPool.TryGetWriter(extension, out ITranslationWriter writer))
            {
                Log.Error($"Format not supported [{extension}]!");
                return 1;
            }

            if (!File.Exists(targetPath))
            {
                Log.Error($"Target file [{Path.GetFileName(targetPath)}] does not exist!");
                return 1;
            }

            if (!string.IsNullOrEmpty(sourcePath) && !File.Exists(sourcePath))
            {
                Log.Error($"Source file [{Path.GetFileName(sourcePath)}] does not exist!");
                return 1;
            }

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
                Log.Information($"Export [{culture}] culture.");
                FileBusinessLogic.Load(out LocResFile targetFile, targetPath);
                FileBusinessLogic.Load(out LocResFile sourceFile, sourcePath);
                TranslationItem item = new TranslationItem(culture);
                Export(item, targetFile);
                Update(item, sourceFile);
                writer.Write(outputPath, item);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception!");
                return 1;
            }

            return 0;
        }

        public static void Export(TranslationItem item, LocResFile lrFile)
        {
            if (lrFile is null)
                throw new ArgumentNullException(nameof(lrFile));
            foreach (LocResNamespace lrNamespace in lrFile)
            {
                foreach (LocResString lrString in lrNamespace)
                {
                    TranslationValue value = new TranslationValue(
                        lrNamespace.Name ?? string.Empty,
                        lrString.Key ?? string.Empty,
                        lrString.Hash,
                        lrString.Value ?? string.Empty,
                        lrString.Value ?? string.Empty
                    );
                    item.Add(value);
                }
            }
        }

        public static void Update(TranslationItem item, LocResFile lrFile)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));
            if (lrFile is object)
            {
                foreach (TranslationValue value in item)
                {
                    if (lrFile is object &&
                        lrFile.TryGetNamespace(value.NameSpace, out LocResNamespace lrNamespace) &&
                        lrNamespace is object &&
                        lrNamespace.TryGetString(value.Key, out LocResString lrString) &&
                        lrString is object &&
                        !string.IsNullOrEmpty(lrString.Value))
                    {
                        value.Value = lrString.Value;
                    }
                }
            }
        }
    }
}
