using LocResCore.Translations;
using LocResLib;
using Serilog;
using System;
using System.IO;
using System.Linq;

namespace LocResCore.BusinessLogics
{
    public static class ImportBusinessLogic
    {
        public static int Import(string outputPath, string targetPath, string sourcePath, string culture, bool clear, LocResEncoding encoding, ITranslationReaderPool translationReaderPool)
        {
            if (translationReaderPool is null)
                throw new ArgumentNullException(nameof(translationReaderPool));

            string extension = Path.GetExtension(sourcePath);

            if (string.IsNullOrEmpty(extension))
            {
                Log.Error($"Invalid or missing extension [{extension}] on file [{Path.GetFileName(sourcePath)}]!");
                return 1;
            }

            if (!translationReaderPool.TryGetReader(extension, out ITranslationReader reader))
            {
                Log.Error($"Format not supported [{extension}]!");
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
                Log.Information($"Import [{culture}] culture.");
                FileBusinessLogic.Load(out LocResFile targetFile, targetPath);
                if (clear)
                {
                    targetFile.Clear();
                    Log.Information($"Cleared all resources!");
                }
                reader.Read(out TranslationItem item, sourcePath, culture);
                Import(targetFile, item);
                FileBusinessLogic.Save(outputPath, targetFile, encoding);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception!");
                return 1;
            }

            return 0;
        }

        public static void Import(LocResFile lrFile, TranslationItem item)
        {
            if (lrFile is null)
                throw new ArgumentNullException(nameof(lrFile));
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            var items = item.Where(_ => _.IsModified && !_.IsEmpty).ToList();

            Log.Information($"Found [{items.Count}] modified resources.");

            uint add = 0;
            uint upd = 0;

            foreach (TranslationValue value in items)
            {
                if (!lrFile.TryGetNamespace(value.NameSpace, out LocResNamespace locresNamespace))
                {
                    locresNamespace = new LocResNamespace(value.NameSpace);
                    lrFile.Add(locresNamespace);
                    Log.Debug($"Import.");
                    Log.Debug($"  Culture   [{item.Culture}]");
                    Log.Debug($"  Namespace [{value.NameSpace}] (added)");
                }

                if (!locresNamespace.TryGetString(value.Key, out LocResString locresString))
                {
                    locresString = new LocResString(value.Key, value.Value, value.Hash);
                    locresNamespace.Add(locresString);
                    add++;
                    Log.Debug($"Import.");
                    Log.Debug($"  Culture   [{item.Culture}]");
                    Log.Debug($"  Namespace [{value.NameSpace}]");
                    Log.Debug($"  Key       [{value.Key}] (added)");
                    Log.Debug($"  Hash      [{value.Hash}] (added)");
                    Log.Debug($"  Value     [{value.EscapeValue}]");
                }
                else
                {
                    locresString.Value = value.Value;
                    upd++;
                    Log.Debug($"Import.");
                    Log.Debug($"  Culture   [{item.Culture}]");
                    Log.Debug($"  Namespace [{value.NameSpace}]");
                    Log.Debug($"  Key       [{value.Key}]");
                    Log.Debug($"  Hash      [{value.Hash}]");
                    Log.Debug($"  Original  [{value.EscapeOriginalValue}]");
                    Log.Debug($"  Value     [{value.EscapeValue}]");
                }
            }

            Log.Information($"Import. Added [{add}] new resources.");
            Log.Information($"Import. Updated [{upd}] existing resources.");
        }
    }
}
