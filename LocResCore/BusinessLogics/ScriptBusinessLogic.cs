using LocResCore.Commands;
using LocResCore.Translations;
using LocResLib;
using Serilog;
using System;
using System.IO;

namespace LocResCore.BusinessLogics
{
    public static class ScriptBusinessLogic
    {
        public static int Apply(string outputPath, string targetPath, string sourcePath, string culture, LocResEncoding encoding, IScriptReaderPool scriptReaderPool)
        {
            if (scriptReaderPool is null)
                throw new ArgumentNullException(nameof(scriptReaderPool));

            string outputExtension = Path.GetExtension(outputPath);

            if (string.IsNullOrEmpty(outputExtension))
            {
                Log.Error($"Invalid or missing extension [{outputExtension}] on file [{Path.GetFileName(outputPath)}]!");
                return 1;
            }

            if (!string.Equals(outputExtension, ".locres"))
            {
                Log.Error($"File [{Path.GetFileName(outputPath)}] should have .locres extension!");
                return 1;
            }

            string targetExtension = Path.GetExtension(targetPath);

            if (string.IsNullOrEmpty(targetExtension))
            {
                Log.Error($"Invalid or missing extension [{targetExtension}] on file [{Path.GetFileName(targetPath)}]!");
                return 1;
            }

            if (!string.Equals(targetExtension, ".locres"))
            {
                Log.Error($"File [{Path.GetFileName(targetPath)}] should have .locres extension!");
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

            string sourceExtension = Path.GetExtension(sourcePath);

            if (string.IsNullOrEmpty(sourceExtension))
            {
                Log.Error($"Invalid or missing extension [{sourceExtension}] on file [{Path.GetFileName(sourcePath)}]!");
                return 1;
            }

            if (!scriptReaderPool.TryGetReader(sourceExtension, out IScriptReader scriptReader))
            {
                Log.Error($"Format not supported [{sourceExtension}]!");
                return 1;
            }

            try
            {
                Log.Information($"Subject [{culture}] culture.");
                FileBusinessLogic.Load(out LocResFile targetFile, targetPath);
                scriptReader.Read(out CommandPool commandPool, sourcePath);
                TranslationItem item = new TranslationItem(culture);
                ExportBusinessLogic.Export(item, targetFile);
                commandPool.Apply(item);
                ImportBusinessLogic.Import(targetFile, item);
                FileBusinessLogic.Save(outputPath, targetFile, encoding);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception!");
                return 1;
            }

            return 0;
        }

        public static int Template(string templatePath, IScriptWriterPool scriptWriterPool)
        {
            if (scriptWriterPool is null)
                throw new ArgumentNullException(nameof(scriptWriterPool));

            string templateExtension = Path.GetExtension(templatePath);

            if (string.IsNullOrEmpty(templateExtension))
            {
                Log.Error($"Invalid or missing extension [{templateExtension}] on file [{Path.GetFileName(templatePath)}]!");
                return 1;
            }

            if (!scriptWriterPool.TryGetWriter(templateExtension, out IScriptWriter scriptWriter))
            {
                Log.Error($"Format not supported [{templateExtension}]!");
                return 1;
            }

            try
            {
                CommandPool commandPool = new CommandPool();
                commandPool.Add(new InsertCommand("en", "ST_Inventory", "INVENTORY_WEAPON_RIFLE", "Rifle A", "Rifle A"));
                commandPool.Add(new UpdateCommand("en", "ST_Inventory", "INVENTORY_WEAPON_RIFLE", "Rifle M1"));
                commandPool.Add(new UpsertCommand("en", "ST_Inventory", "INVENTORY_WEAPON_PISTOL", "Pistol B", "Pistol B"));
                commandPool.Add(new DeleteCommand("en", "ST_Inventory", "INVENTORY_WEAPON_DELETE"));
                commandPool.Add(new ReplaceCommand("*", "*", "INVENTORY_WEAPON_PISTOL", "B", "M3"));
                scriptWriter.Write(templatePath, commandPool);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception!");
                return 1;
            }

            return 0;
        }

        public static int Create(string outputPath, string targetPath, IScriptWriterPool scriptWriterPool, ITranslationReaderPool readerPool)
        {
            if (scriptWriterPool is null)
                throw new ArgumentNullException(nameof(scriptWriterPool));
            if (readerPool is null)
                throw new ArgumentNullException(nameof(readerPool));

            string outputExtension = Path.GetExtension(outputPath);

            if (string.IsNullOrEmpty(outputExtension))
            {
                Log.Error($"Invalid or missing extension [{outputExtension}] on file [{Path.GetFileName(outputPath)}]!");
                return 1;
            }

            if (!scriptWriterPool.TryGetWriter(outputExtension, out IScriptWriter scriptWriter))
            {
                Log.Error($"Format not supported [{outputExtension}]!");
                return 1;
            }

            if (!File.Exists(targetPath))
            {
                Log.Error($"Target file [{Path.GetFileName(targetPath)}] does not exist!");
                return 1;
            }

            string targetExtension = Path.GetExtension(targetPath);

            if (string.IsNullOrEmpty(targetExtension))
            {
                Log.Error($"Invalid or missing extension [{targetExtension}] on file [{Path.GetFileName(targetPath)}]!");
                return 1;
            }

            if (!readerPool.TryGetReader(targetExtension, out ITranslationReader reader))
            {
                Log.Error($"Format not supported [{targetExtension}]!");
                return 1;
            }

            try
            {
                CommandPool commandPool = new CommandPool();
                reader.Read(out TranslationPool pool, targetPath);
                foreach (TranslationItem item in pool)
                {
                    foreach (TranslationValue value in item)
                    {
                        if (value.IsModified && !value.IsEmpty)
                        {
                            ICommand command = new UpdateCommand(item.Culture, value.NameSpace, value.Key, value.EscapeValue);
                            commandPool.Add(command);
                        }
                    }
                }
                scriptWriter.Write(outputPath, commandPool);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception!");
                return 1;
            }

            return 0;
        }

    }
}
