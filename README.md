# LocResUtility
UnrealEngine 4 `TextLocalizationResource` library and export/import tool built with C#.

Can read/write every locres version up to 3 (latest).

## Download
Go to [releases](https://github.com/anubi47/LocResUtility/releases/latest) and download `LocResUtilityCli-v[version].7z`

## Usage
`LocResUtilityCli` is a command line tool. You should use it in command line (cmd, powershell, etc.).

### Export
```
DESCRIPTION:
    Export the <Target> resources.

USAGE:
    LocResUtilityCli.exe export <outputPath> <targetPath> [sourcePath] [OPTIONS]

EXAMPLES:
    LocResUtilityCli.exe export .\Output.xlsx .\Target.locres
    LocResUtilityCli.exe export .\Output.xlsx .\Target.locres .\Source.locres
    LocResUtilityCli.exe export .\Output.xlsx .\Target.locres .\Source.locres -y -c en-US
  
ARGUMENTS:
    <outputPath>    Output file
    <targetPath>    Target .locres file
    [sourcePath]    Source .locres file

OPTIONS:
                     DEFAULT    
    -h, --help                  Prints help information
    -v, --version               Prints version information
    -y                          Suppresses prompting to confirm that you want to overwrite an existing file
    -c, --culture    unknown    The culture
                                Used when dealing with multiple cultures
```

### Import
```
DESCRIPTION:
    Import the modified resources from <Source> into <Target>.

USAGE:
    LocResUtilityCli.exe import <outputPath> <targetPath> <sourcePath> [OPTIONS]

EXAMPLES:
    LocResUtilityCli.exe import .\Output.locres .\Target.locres .\Source.xlsx
    LocResUtilityCli.exe import .\Output.locres .\Target.locres .\Source.xlsx -y -c en-US

ARGUMENTS:
    <outputPath>    Output .locres file
    <targetPath>    Target .locres file
    <sourcePath>    Source file

OPTIONS:
                      DEFAULT
    -h, --help                   Prints help information
    -v, --version                Prints version information
    -y                           Suppresses prompting to confirm that you want to overwrite an existing file
    -c, --culture     unknown    The culture
                                 Used when dealing with multiple cultures
    -e, --encoding    0          Encoding of the output strings
                                 0 = Auto
                                 1 = UTF-16
                                 2 = Force ASCII
    -d                           Delete all resources
```

### Merge
```
DESCRIPTION:
    Merge resources from <Source> into <Target>.

USAGE:
    LocResUtilityCli.exe merge <outputPath> <targetPath> <sourcePath> [OPTIONS]

EXAMPLES:
    LocResUtilityCli.exe merge .\Output.locres .\Target.locres .\Source.locres
    LocResUtilityCli.exe merge .\Output.locres .\Target.locres .\Source.locres -y -m 3

ARGUMENTS:
    <outputPath>    Output .locres file
    <targetPath>    Target .locres file
    <sourcePath>    Source .locres file

OPTIONS:
                      DEFAULT
    -h, --help                   Prints help information
    -v, --version                Prints version information
    -y                           Suppresses prompting to confirm that you want to overwrite an existing file
    -c, --culture     unknown    The culture
                                 Used when dealing with multiple cultures
    -e, --encoding    0          Encoding of the output strings
                                 0 = Auto
                                 1 = UTF-16
                                 2 = Force ASCII
    -m, --mode        1          Merge mode
                                 1 = Update. Update common <Source> resources into <Target>
                                 2 = Insert. Add exclusive <Source> resources into <Target>
                                 3 = Upsert. Update if present, insert otherwise
```

### Run script
```
DESCRIPTION:
    Run <Source> scripts into <Target>.

USAGE:
    LocResUtilityCli.exe script run <outputPath> <targetPath> <sourcePath> [OPTIONS]

EXAMPLES:
    LocResUtilityCli.exe script run .\Output.locres .\Target.locres .\Source.json
    LocResUtilityCli.exe script run .\Output.locres .\Target.locres .\Source.xlsx -y -c en-US

ARGUMENTS:
    <outputPath>    Output .locres file
    <targetPath>    Target .locres file
    <sourcePath>    Source .json file

OPTIONS:
                      DEFAULT
    -h, --help                   Prints help information
    -v, --version                Prints version information
    -y                           Suppresses prompting to confirm that you want to overwrite an existing file
    -c, --culture     unknown    The culture
                                 Used when dealing with multiple cultures
    -e, --encoding    0          Encoding of the output strings
                                 0 = Auto
                                 1 = UTF-16
                                 2 = Force ASCII
```

### Template script
```
DESCRIPTION:
    Create a script <Template> file.

USAGE:
    LocResUtilityCli.exe script template <templatePath> [OPTIONS]

EXAMPLES:
    LocResUtilityCli.exe script template .\Template.json
    LocResUtilityCli.exe script template .\Template.xlsx -y

ARGUMENTS:
    <templatePath>    Template file

OPTIONS:
    -h, --help       Prints help information
    -v, --version    Prints version information
    -y               Suppresses prompting to confirm that you want to overwrite an existing file
```

### Create script
```
DESCRIPTION:
    Create an update script from <Target>.

USAGE:
    LocResUtilityCli.exe script create <outputPath> <targetPath> [OPTIONS]

EXAMPLES:
    LocResUtilityCli.exe script create .\Output.json .\Target.xlsx -y

ARGUMENTS:
    <outputPath>    Output file
    <targetPath>    Target file

OPTIONS:
    -h, --help       Prints help information
    -v, --version    Prints version information
    -y               Suppresses prompting to confirm that you want to overwrite an existing file
```

### Hash
```
DESCRIPTION:
    Calculate the hash for the given <Text>.

USAGE:
    LocResUtilityCli.exe hash <text> [OPTIONS]

EXAMPLES:
    LocResUtilityCli.exe hash "Text to hash."

ARGUMENTS:
    <text>    The text to hash

OPTIONS:
    -h, --help       Prints help information
    -v, --version    Prints version information
```

## Example workflow
The goal is to edit existing game localization, for all the available game languages, obtaining edited .locres files.

1. Understand if you need to merge the localized .locres with default language .locres
2. Export all .locres files into something human readable
3. Edit localizations
4. Import changes into .locres

### Merge
Let's consider that our game has english default language. It might happen that localized .locres files do not contain all the default resources.
Example:

```
en - 2 resources
  NS_Namespace/RK_Yes|Yes
  NS_Namespace/RK_No|No
de-DE - 2 resources
  NS_Namespace/RK_Yes|Ja
  NS_Namespace/RK_No|Nein
it-IT - 1 resource
  NS_Namespace/RK_Yes|Si
```

English is default language. German localizes both resources. Italian localizes RK_Yes only. It might happen that you need to have 'merged' .locres files like this:

```
en - 2 resources
  NS_Namespace/RK_Yes|Yes
  NS_Namespace/RK_No|No
de-DE - 2 resources
  NS_Namespace/RK_Yes|Ja
  NS_Namespace/RK_No|Nein
it-IT - 2 resources
  NS_Namespace/RK_Yes|Si
  NS_Namespace/RK_No|No
```

Here a script to automatically merge all the necessary .locres files.

```
.\LocResUtilityCli.exe merge ".\merge\GAME\de-DE\Game.locres"   ".\original\GAME\en\Game.locres" ".\original\GAME\de-DE\Game.locres"   -y --mode 3 --culture de-DE
.\LocResUtilityCli.exe merge ".\merge\GAME\es-419\Game.locres"  ".\original\GAME\en\Game.locres" ".\original\GAME\es-419\Game.locres"  -y --mode 3 --culture es-419
.\LocResUtilityCli.exe merge ".\merge\GAME\es-ES\Game.locres"   ".\original\GAME\en\Game.locres" ".\original\GAME\es-ES\Game.locres"   -y --mode 3 --culture es-ES
.\LocResUtilityCli.exe merge ".\merge\GAME\fr-FR\Game.locres"   ".\original\GAME\en\Game.locres" ".\original\GAME\fr-FR\Game.locres"   -y --mode 3 --culture fr-FR
.\LocResUtilityCli.exe merge ".\merge\GAME\it-IT\Game.locres"   ".\original\GAME\en\Game.locres" ".\original\GAME\it-IT\Game.locres"   -y --mode 3 --culture it-IT
.\LocResUtilityCli.exe merge ".\merge\GAME\ja-JP\Game.locres"   ".\original\GAME\en\Game.locres" ".\original\GAME\ja-JP\Game.locres"   -y --mode 3 --culture ja-JP
.\LocResUtilityCli.exe merge ".\merge\GAME\ko-KR\Game.locres"   ".\original\GAME\en\Game.locres" ".\original\GAME\ko-KR\Game.locres"   -y --mode 3 --culture ko-KR
.\LocResUtilityCli.exe merge ".\merge\GAME\pl-PL\Game.locres"   ".\original\GAME\en\Game.locres" ".\original\GAME\pl-PL\Game.locres"   -y --mode 3 --culture pl-PL
.\LocResUtilityCli.exe merge ".\merge\GAME\pt-BR\Game.locres"   ".\original\GAME\en\Game.locres" ".\original\GAME\pt-BR\Game.locres"   -y --mode 3 --culture pt-BR
.\LocResUtilityCli.exe merge ".\merge\GAME\ru-RU\Game.locres"   ".\original\GAME\en\Game.locres" ".\original\GAME\ru-RU\Game.locres"   -y --mode 3 --culture ru-RU
.\LocResUtilityCli.exe merge ".\merge\GAME\tr-TR\Game.locres"   ".\original\GAME\en\Game.locres" ".\original\GAME\tr-TR\Game.locres"   -y --mode 3 --culture tr-TR
.\LocResUtilityCli.exe merge ".\merge\GAME\zh-Hans\Game.locres" ".\original\GAME\en\Game.locres" ".\original\GAME\zh-Hans\Game.locres" -y --mode 3 --culture zh-Hans
.\LocResUtilityCli.exe merge ".\merge\GAME\zh-Hant\Game.locres" ".\original\GAME\en\Game.locres" ".\original\GAME\zh-Hant\Game.locres" -y --mode 3 --culture zh-Hant
```

### Export

Here a script to automatically export all the necessary .locres files into a single Excel.

```
.\LocResUtilityCli.exe export ".\export\GAME.xlsx" ".\original\GAME\en\Game.locres"   -y --culture en
.\LocResUtilityCli.exe export ".\export\GAME.xlsx" ".\merge\GAME\de-DE\Game.locres"   -y --culture de-DE
.\LocResUtilityCli.exe export ".\export\GAME.xlsx" ".\merge\GAME\es-419\Game.locres"  -y --culture es-419
.\LocResUtilityCli.exe export ".\export\GAME.xlsx" ".\merge\GAME\es-ES\Game.locres"   -y --culture es-ES
.\LocResUtilityCli.exe export ".\export\GAME.xlsx" ".\merge\GAME\fr-FR\Game.locres"   -y --culture fr-FR
.\LocResUtilityCli.exe export ".\export\GAME.xlsx" ".\merge\GAME\it-IT\Game.locres"   -y --culture it-IT
.\LocResUtilityCli.exe export ".\export\GAME.xlsx" ".\merge\GAME\ja-JP\Game.locres"   -y --culture ja-JP
.\LocResUtilityCli.exe export ".\export\GAME.xlsx" ".\merge\GAME\ko-KR\Game.locres"   -y --culture ko-KR
.\LocResUtilityCli.exe export ".\export\GAME.xlsx" ".\merge\GAME\pl-PL\Game.locres"   -y --culture pl-PL
.\LocResUtilityCli.exe export ".\export\GAME.xlsx" ".\merge\GAME\pt-BR\Game.locres"   -y --culture pt-BR
.\LocResUtilityCli.exe export ".\export\GAME.xlsx" ".\merge\GAME\ru-RU\Game.locres"   -y --culture ru-RU
.\LocResUtilityCli.exe export ".\export\GAME.xlsx" ".\merge\GAME\tr-TR\Game.locres"   -y --culture tr-TR
.\LocResUtilityCli.exe export ".\export\GAME.xlsx" ".\merge\GAME\zh-Hans\Game.locres" -y --culture zh-Hans
.\LocResUtilityCli.exe export ".\export\GAME.xlsx" ".\merge\GAME\zh-Hant\Game.locres" -y --culture zh-Hant
```

### Import

Here a script to automatically import edited Excel into .locres files.

```
.\LocResUtilityCli.exe import ".\edit\GAME\en\Game.locres"      ".\original\GAME\en\Game.locres"   ".\export\GAME.xlsx" -y --culture en
.\LocResUtilityCli.exe import ".\edit\GAME\de-DE\Game.locres"   ".\merge\GAME\de-DE\Game.locres"   ".\export\GAME.xlsx" -y --culture de-DE
.\LocResUtilityCli.exe import ".\edit\GAME\es-419\Game.locres"  ".\merge\GAME\es-419\Game.locres"  ".\export\GAME.xlsx" -y --culture es-419
.\LocResUtilityCli.exe import ".\edit\GAME\es-ES\Game.locres"   ".\merge\GAME\es-ES\Game.locres"   ".\export\GAME.xlsx" -y --culture es-ES
.\LocResUtilityCli.exe import ".\edit\GAME\fr-FR\Game.locres"   ".\merge\GAME\fr-FR\Game.locres"   ".\export\GAME.xlsx" -y --culture fr-FR
.\LocResUtilityCli.exe import ".\edit\GAME\it-IT\Game.locres"   ".\merge\GAME\it-IT\Game.locres"   ".\export\GAME.xlsx" -y --culture it-IT
.\LocResUtilityCli.exe import ".\edit\GAME\ja-JP\Game.locres"   ".\merge\GAME\ja-JP\Game.locres"   ".\export\GAME.xlsx" -y --culture ja-JP
.\LocResUtilityCli.exe import ".\edit\GAME\ko-KR\Game.locres"   ".\merge\GAME\ko-KR\Game.locres"   ".\export\GAME.xlsx" -y --culture ko-KR
.\LocResUtilityCli.exe import ".\edit\GAME\pl-PL\Game.locres"   ".\merge\GAME\pl-PL\Game.locres"   ".\export\GAME.xlsx" -y --culture pl-PL
.\LocResUtilityCli.exe import ".\edit\GAME\pt-BR\Game.locres"   ".\merge\GAME\pt-BR\Game.locres"   ".\export\GAME.xlsx" -y --culture pt-BR
.\LocResUtilityCli.exe import ".\edit\GAME\ru-RU\Game.locres"   ".\merge\GAME\ru-RU\Game.locres"   ".\export\GAME.xlsx" -y --culture ru-RU
.\LocResUtilityCli.exe import ".\edit\GAME\tr-TR\Game.locres"   ".\merge\GAME\tr-TR\Game.locres"   ".\export\GAME.xlsx" -y --culture tr-TR
.\LocResUtilityCli.exe import ".\edit\GAME\zh-Hans\Game.locres" ".\merge\GAME\zh-Hans\Game.locres" ".\export\GAME.xlsx" -y --culture zh-Hans
.\LocResUtilityCli.exe import ".\edit\GAME\zh-Hant\Game.locres" ".\merge\GAME\zh-Hant\Game.locres" ".\export\GAME.xlsx" -y --culture zh-Hant
```

## LocResLib

### Sample usage

```cs
using LocResLib;

var locres = new LocResFile();

using (var file = File.OpenRead(inputPath))
{
    locres.Load(file);
}

foreach (var locresNamespace in locres)
{
    foreach (var stringEntry in locresNamespace)
    {
        string key = stringEntry.Key;
        string val = stringEntry.Value;

        // work with stringEntry
    }
}

using (var file = File.Create(outputPath))
{
    locres.Save(file, LocResVersion.Optimized);
}
```
