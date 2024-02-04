using LocResCore.BusinessLogics;
using LocResCore.Commands;
using LocResCore.Csv.Commands;
using LocResCore.Csv.Translations;
using LocResCore.Excel.Commands;
using LocResCore.Excel.Translations;
using LocResCore.Json.Commands;
using LocResCore.Json.Translations;
using LocResCore.Po.Translations;
using LocResCore.Translations;
using LocResLib;
using LocResLib.Hashing;
using Microsoft.Extensions.Configuration;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Help;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LocresConsole
{
    internal class Program
    {
        private readonly static IScriptReaderPool _scriptReaderPool = new ScriptReaderPool();
        private readonly static IScriptWriterPool _scriptWriterPool = new ScriptWriterPool();
        private readonly static ITranslationReaderPool _translationReaderPool = new TranslationReaderPool();
        private readonly static ITranslationWriterPool _translationWriterPool = new TranslationWriterPool();

        internal class CommonSettings : CommandSettings
        {
            [Description("Suppresses prompting to confirm that you want to overwrite an existing file.")]
            [CommandOption("-y")]
            [DefaultValue(false)]
            public bool SuppressPrompt { get; init; }
        }

        internal class CultureSettings : CommonSettings
        {
            [Description("The culture\nUsed when dealing with multiple cultures")]
            [CommandOption("-c|--culture")]
            [DefaultValue("unknown")]
            public string Culture { get; init; }
        }

        internal class SavingSettings : CultureSettings
        {
            [Description("Encoding of the output strings\n0 = Auto\n1 = UTF-16\n2 = Force ASCII")]
            [CommandOption("-e|--encoding")]
            [DefaultValue(0)]
            public int Encoding { get; init; }

            public override ValidationResult Validate()
            {
                ValidationResult result = base.Validate();
                if (result.Successful)
                {
                    int[] validEncodings = { (int)LocResEncoding.Auto, (int)LocResEncoding.UTF16, (int)LocResEncoding.ForceASCII };

                    if (!validEncodings.Contains(Encoding))
                    {
                        result = ValidationResult.Error($"Wrong encoding! [{Encoding}]");
                    }
                }
                return result;
            }
        }

        internal class ExportSettings : CultureSettings
        {
            [Description("Output file.")]
            [CommandArgument(0, "<outputPath>")]
            public string OutputPath { get; init; }

            [Description("Target .locres file.")]
            [CommandArgument(1, "<targetPath>")]
            public string TargetPath { get; init; }

            [Description("Source .locres file.")]
            [CommandArgument(2, "[sourcePath]")]
            public string SourcePath { get; init; }

            public override ValidationResult Validate()
            {
                string extension = Path.GetExtension(OutputPath);

                if (string.IsNullOrEmpty(extension))
                {
                    Log.Error($"Invalid or missing extension [{extension}] on file [{Path.GetFileName(OutputPath)}]!");
                    return ValidationResult.Error($"Invalid or missing extension [{extension}] on file [{Path.GetFileName(OutputPath)}]!");
                }

                if (!_translationWriterPool.TryGetWriter(extension, out ITranslationWriter writer))
                {
                    Log.Error($"Format not supported [{extension}]!");
                    return ValidationResult.Error($"Format not supported [{extension}]!");
                }

                if (!File.Exists(TargetPath))
                {
                    Log.Error($"Target file [{Path.GetFileName(TargetPath)}] does not exist!");
                    return ValidationResult.Error($"Target file [{Path.GetFileName(TargetPath)}] does not exist!");
                }

                if (!string.IsNullOrEmpty(SourcePath) && !File.Exists(SourcePath))
                {
                    Log.Error($"Source file [{Path.GetFileName(SourcePath)}] does not exist!");
                    return ValidationResult.Error($"Source file [{Path.GetFileName(SourcePath)}] does not exist!");
                }

                return base.Validate();
            }
        }

        internal class ImportSettings : SavingSettings
        {
            [Description("Output .locres file.")]
            [CommandArgument(0, "<outputPath>")]
            public string OutputPath { get; init; }

            [Description("Target .locres file.")]
            [CommandArgument(1, "<targetPath>")]
            public string TargetPath { get; init; }

            [Description("Source file.")]
            [CommandArgument(2, "<sourcePath>")]
            public string SourcePath { get; init; }

            [Description("Delete all resources.")]
            [CommandOption("-d")]
            [DefaultValue(false)]
            public bool Clear { get; init; }

            public override ValidationResult Validate()
            {
                string extension = Path.GetExtension(SourcePath);

                if (string.IsNullOrEmpty(extension))
                {
                    Log.Error($"Invalid or missing extension!");
                    return ValidationResult.Error($"Invalid or missing extension!");
                }

                if (!_translationReaderPool.TryGetReader(extension, out ITranslationReader reader))
                {
                    Log.Error($"Format not supported [{extension}]!");
                    return ValidationResult.Error($"Format not supported [{extension}]!");
                }

                if (!File.Exists(TargetPath))
                {
                    Log.Error($"Target file [{Path.GetFileName(TargetPath)}] does not exist!");
                    return ValidationResult.Error($"Target file [{Path.GetFileName(TargetPath)}] does not exist!");
                }

                if (!File.Exists(SourcePath))
                {
                    Log.Error($"Source file [{Path.GetFileName(SourcePath)}] does not exist!");
                    return ValidationResult.Error($"Source file [{Path.GetFileName(SourcePath)}] does not exist!");
                }

                return base.Validate();
            }
        }

        internal class MergeSettings : SavingSettings
        {
            [Description("Output .locres file.")]
            [CommandArgument(0, "<outputPath>")]
            public string OutputPath { get; init; }

            [Description("Target .locres file.")]
            [CommandArgument(1, "<targetPath>")]
            public string TargetPath { get; init; }

            [Description("Source .locres file.")]
            [CommandArgument(2, "<sourcePath>")]
            public string SourcePath { get; init; }

            [Description("Merge mode\n1 = Update. Update common <Source> resources into <Target>\n2 = Insert. Add exclusive <Source> resources into <Target>\n3 = Upsert. Update if present, insert otherwise")]
            [CommandOption("-m|--mode")]
            [DefaultValue(1)]
            public int Mode { get; init; }

            public override ValidationResult Validate()
            {
                string extension = Path.GetExtension(OutputPath);

                if (!string.Equals(extension, ".locres"))
                {
                    Log.Error($"Output file extension should be .locres!");
                    return ValidationResult.Error($"Output file extension should be .locres!");
                }

                if (!File.Exists(TargetPath))
                {
                    Log.Error($"Target file [{Path.GetFileName(TargetPath)}] does not exist!");
                    return ValidationResult.Error($"Target file [{Path.GetFileName(TargetPath)}] does not exist!");
                }

                if (!File.Exists(SourcePath))
                {
                    Log.Error($"Source file [{Path.GetFileName(SourcePath)}] does not exist!");
                    return ValidationResult.Error($"Source file [{Path.GetFileName(SourcePath)}] does not exist!");
                }

                int[] validModes = { 1, 2, 3 };
                if (!validModes.Contains(Mode))
                {
                    Log.Error($"Wrong mode! [{Mode}]");
                    return ValidationResult.Error($"Wrong mode! [{Mode}]");
                }

                return base.Validate();
            }
        }

        internal class ScriptRunSettings : SavingSettings
        {
            [Description("Output .locres file.")]
            [CommandArgument(0, "<outputPath>")]
            public string OutputPath { get; init; }

            [Description("Target .locres file.")]
            [CommandArgument(1, "<targetPath>")]
            public string TargetPath { get; init; }

            [Description("Source .json file.")]
            [CommandArgument(2, "<sourcePath>")]
            public string SourcePath { get; init; }

            public override ValidationResult Validate()
            {
                string extension = Path.GetExtension(OutputPath);

                if (string.IsNullOrEmpty(extension))
                {
                    Log.Error($"Invalid or missing extension [{extension}] on file [{Path.GetFileName(OutputPath)}]!");
                    return ValidationResult.Error($"Invalid or missing extension [{extension}] on file [{Path.GetFileName(OutputPath)}]!");
                }

                if (!File.Exists(TargetPath))
                {
                    Log.Error($"Target file [{Path.GetFileName(TargetPath)}] does not exist!");
                    return ValidationResult.Error($"Target file [{Path.GetFileName(TargetPath)}] does not exist!");
                }

                if (!File.Exists(SourcePath))
                {
                    Log.Error($"Source file [{Path.GetFileName(SourcePath)}] does not exist!");
                    return ValidationResult.Error($"Source file [{Path.GetFileName(SourcePath)}] does not exist!");
                }

                return base.Validate();
            }
        }

        internal class ScriptTemplateSettings : CommonSettings
        {
            [Description("Template file.")]
            [CommandArgument(0, "<templatePath>")]
            public string TemplatePath { get; init; }
        }

        internal class ScriptCreateSettings : CommonSettings
        {
            [Description("Output file.")]
            [CommandArgument(0, "<outputPath>")]
            public string OutputPath { get; init; }

            [Description("Target file.")]
            [CommandArgument(0, "<targetPath>")]
            public string TargetPath { get; init; }
        }

        internal class HashSettings : CommandSettings
        {
            [Description("The text to hash.")]
            [CommandArgument(0, "<text>")]
            public string Text { get; init; }
        }

        internal sealed class ExportCommand : Command<ExportSettings>
        {
            public override int Execute([NotNull] CommandContext context, [NotNull] ExportSettings settings)
            {
                if (!settings.SuppressPrompt && File.Exists(settings.OutputPath))
                    if (!AnsiConsole.Confirm($"File [[{Path.GetFileName(settings.OutputPath)}]] already exist. Overwrite?"))
                        return 1;

                return ExportBusinessLogic.Export(
                    settings.OutputPath,
                    settings.TargetPath,
                    settings.SourcePath,
                    settings.Culture,
                    _translationWriterPool);
            }
        }

        internal sealed class ImportCommand : Command<ImportSettings>
        {
            public override int Execute([NotNull] CommandContext context, [NotNull] ImportSettings settings)
            {
                if (!settings.SuppressPrompt && File.Exists(settings.OutputPath))
                    if (!AnsiConsole.Confirm($"File [[{Path.GetFileName(settings.OutputPath)}]] already exist. Overwrite?"))
                        return 1;

                return ImportBusinessLogic.Import(
                    settings.OutputPath,
                    settings.TargetPath,
                    settings.SourcePath,
                    settings.Culture,
                    settings.Clear,
                    (LocResEncoding)settings.Encoding,
                    _translationReaderPool);
            }
        }

        internal sealed class MergeCommand : Command<MergeSettings>
        {
            public override int Execute([NotNull] CommandContext context, [NotNull] MergeSettings settings)
            {
                if (!settings.SuppressPrompt && File.Exists(settings.OutputPath))
                    if (!AnsiConsole.Confirm($"File [[{Path.GetFileName(settings.OutputPath)}]] already exist. Overwrite?"))
                        return 1;

                return MergeBusinessLogic.Merge(
                    settings.OutputPath,
                    settings.TargetPath,
                    settings.SourcePath,
                    settings.Culture,
                    (MergeMode)settings.Mode,
                    (LocResEncoding)settings.Encoding);
            }
        }

        internal sealed class ScriptRunCommand : Command<ScriptRunSettings>
        {
            public override int Execute([NotNull] CommandContext context, [NotNull] ScriptRunSettings settings)
            {
                if (!settings.SuppressPrompt && File.Exists(settings.OutputPath))
                    if (!AnsiConsole.Confirm($"File [[{Path.GetFileName(settings.OutputPath)}]] already exist. Overwrite?"))
                        return 1;

                return ScriptBusinessLogic.Apply(
                    settings.OutputPath,
                    settings.TargetPath,
                    settings.SourcePath,
                    settings.Culture,
                    (LocResEncoding)settings.Encoding,
                    _scriptReaderPool);
            }
        }

        internal sealed class ScriptTemplateCommand : Command<ScriptTemplateSettings>
        {
            public override int Execute([NotNull] CommandContext context, [NotNull] ScriptTemplateSettings settings)
            {
                if (!settings.SuppressPrompt && File.Exists(settings.TemplatePath))
                    if (!AnsiConsole.Confirm($"File [[{Path.GetFileName(settings.TemplatePath)}]] already exist. Overwrite?"))
                        return 1;

                return ScriptBusinessLogic.Template(
                    settings.TemplatePath,
                    _scriptWriterPool);
            }
        }

        internal sealed class ScriptCreateCommand : Command<ScriptCreateSettings>
        {
            public override int Execute([NotNull] CommandContext context, [NotNull] ScriptCreateSettings settings)
            {
                if (!settings.SuppressPrompt && File.Exists(settings.OutputPath))
                    if (!AnsiConsole.Confirm($"File [[{Path.GetFileName(settings.OutputPath)}]] already exist. Overwrite?"))
                        return 1;

                return ScriptBusinessLogic.Create(
                    settings.OutputPath,
                    settings.TargetPath,
                    _scriptWriterPool,
                    _translationReaderPool);
            }
        }

        internal sealed class HashCommand : Command<HashSettings>
        {
            public override int Execute([NotNull] CommandContext context, [NotNull] HashSettings settings)
            {
                uint hash = Crc.StrCrc32(settings.Text);
                Log.Information($"Value = {settings.Text}");
                Log.Information($"Hash = {hash}");
                return 0;
            }
        }

        internal sealed class CustomHelpProvider(ICommandAppSettings settings) : HelpProvider(settings)
        {
            public override IEnumerable<IRenderable> GetHeader(ICommandModel model, ICommandInfo command)
            {
                string version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
                string translationReaderExtensions = string.Join(", ", _translationReaderPool.SupportedExtensions());
                string translationWriterExtensions = string.Join(", ", _translationWriterPool.SupportedExtensions());
                string scriptReaderExtensions = string.Join(", ", _scriptReaderPool.SupportedExtensions());
                string scriptWriterExtensions = string.Join(", ", _scriptWriterPool.SupportedExtensions());
                return new[]
                {
                    new Text($"LocRes Utility CLI v{version}"), Text.NewLine,
                    new Text($"Supported Export/Import extensions."), Text.NewLine,
                    new Text($"  (Read)  : {translationReaderExtensions}"), Text.NewLine,
                    new Text($"  (Write) : {translationWriterExtensions}"), Text.NewLine,
                    new Text($"Supported Script extensions."), Text.NewLine,
                    new Text($"  (Read)  : {scriptReaderExtensions}"), Text.NewLine,
                    new Text($"  (Write) : {scriptWriterExtensions}"), Text.NewLine,
                    Text.NewLine,
                };
            }
        }

        static int Main(string[] args)
        {
            if (File.Exists("serilog.json"))
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("serilog.json")
                    .Build();

                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger();
            }

            _scriptReaderPool.RegisterReader(new CsvScriptReader());
            _scriptReaderPool.RegisterReader(new JsonScriptReader());
            _scriptReaderPool.RegisterReader(new ExcelScriptReader());

            _scriptWriterPool.RegisterWriter(new CsvScriptWriter());
            _scriptWriterPool.RegisterWriter(new JsonScriptWriter());
            _scriptWriterPool.RegisterWriter(new ExcelScriptWriter());

            _translationReaderPool.RegisterReader(new CsvTranslationReader());
            _translationReaderPool.RegisterReader(new ExcelTranslationReader());
            _translationReaderPool.RegisterReader(new JsonTranslationReader());
            _translationReaderPool.RegisterReader(new PoTranslationReader());

            _translationWriterPool.RegisterWriter(new CsvTranslationWriter());
            _translationWriterPool.RegisterWriter(new ExcelTranslationWriter());
            _translationWriterPool.RegisterWriter(new JsonTranslationWriter());
            _translationWriterPool.RegisterWriter(new PoTranslationWriter());

            var app = new CommandApp();
            app.Configure(_ =>
            {
                //_.PropagateExceptions();
                _.SetApplicationName("LocResUtilityCli.exe");
                _.SetHelpProvider(new CustomHelpProvider(_.Settings));
                _.AddCommand<ExportCommand>("export")
                    .WithDescription("Export the <Target> resources.")
                    .WithExample("export", @".\Output.xlsx", @".\Target.locres")
                    .WithExample("export", @".\Output.xlsx", @".\Target.locres", @".\Source.locres")
                    .WithExample("export", @".\Output.xlsx", @".\Target.locres", @".\Source.locres", "-y", "-c", "en-US");
                _.AddCommand<ImportCommand>("import")
                    .WithDescription("Import the modified resources from <Source> into <Target>.")
                    .WithExample("import", @".\Output.locres", @".\Target.locres", @".\Source.xlsx")
                    .WithExample("import", @".\Output.locres", @".\Target.locres", @".\Source.xlsx", "-y", "-c", "en-US");
                _.AddCommand<MergeCommand>("merge")
                    .WithDescription("Merge resources from <Source> into <Target>.")
                    .WithExample("merge", @".\Output.locres", @".\Target.locres", @".\Source.locres")
                    .WithExample("merge", @".\Output.locres", @".\Target.locres", @".\Source.locres", "-y", "-m", "3");
                _.AddCommand<HashCommand>("hash")
                    .WithDescription("Calculate the hash for the given <Text>.")
                    .WithExample("hash", @"""Text to hash.""");
                _.AddBranch("script", command =>
                {
                    command.AddCommand<ScriptRunCommand>("run")
                        .WithDescription("Run <Source> script into <Target>.")
                        .WithExample("script", "run", @".\Output.locres", @".\Target.locres", @".\Source.json")
                        .WithExample("script", "run", @".\Output.locres", @".\Target.locres", @".\Source.xlsx", "-y", "-c", "en-US");
                    command.AddCommand<ScriptTemplateCommand>("template")
                        .WithDescription("Create a script <Template> file.")
                        .WithExample("script", "template", @".\Template.json")
                        .WithExample("script", "template", @".\Template.xlsx", "-y");
                    command.AddCommand<ScriptCreateCommand>("create")
                        .WithDescription("Create an update script from <Target>.")
                        .WithExample("script", "create", @".\Output.json", @".\Target.xlsx", "-y");
                });
            });

            try
            {
                return app.Run(args);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception!");
                AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}