# LocResUtility
UnrealEngine 4 `TextLocalizationResource` library and export/import tool built with C#.

Can read/write every locres version up to 3 (latest)

## Download
Go to [releases](https://github.com/anubi47/LocResUtility/releases/latest) and download `LocResUtilityCli-v[version].7z`

## Usage
`LocResUtilityCli` is a command line tool. You should use it in command line (cmd, powershell, etc.)

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
