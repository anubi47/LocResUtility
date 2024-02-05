using LocResLib;
using System;
using System.IO;
using System.Linq;

namespace LocresLib.Tests
{
    public static class TestHelper
    {
        public static string GetTestDataDirectory()
        {
            string startupPath = AppDomain.CurrentDomain.BaseDirectory;
            var pathItems = startupPath.Split(Path.DirectorySeparatorChar);
            var pos = pathItems.Reverse().ToList().FindIndex(x => string.Equals("bin", x));
            string projectPath = string.Join(Path.DirectorySeparatorChar.ToString(), pathItems.Take(pathItems.Length - pos - 1));
            return Path.Combine(projectPath, "Data");
        }

        public static LocResFile LoadTestFile(string filename)
        {
            var filePath = Path.Combine(GetTestDataDirectory(), filename);
            using (var file = File.OpenRead(filePath))
            {
                var locres = new LocResFile();
                locres.Load(file);

                return locres;
            }
        }
    }
}
