using LocResLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace LocresLib.Tests
{
    [TestClass()]
    public class LocalizationResourceFileTests
    {
        public static void LoadAndSaveTest(string filename, LocResVersion version)
        {
            var locres = TestHelper.LoadTestFile(filename);

            byte[] bytes;

            using (var ms = new MemoryStream())
            {
                locres.Save(ms, version);

                bytes = ms.ToArray();
            }

            using (var ms2 = new MemoryStream(bytes))
            {
                var locres2 = new LocResFile();
                locres2.Load(ms2);

                Assert.AreEqual(locres.CountStrings, locres2.CountStrings);
                Assert.AreEqual(locres.Version, locres2.Version);
            }
        }

        [TestMethod()]
        public void LoadLegacyTest()
        {
            var locres = TestHelper.LoadTestFile("legacy.locres");
            Console.WriteLine(locres.CountStrings);
        }

        [TestMethod()]
        public void LoadCompactTest()
        {
            var locres = TestHelper.LoadTestFile("compact.locres");
            Console.WriteLine(locres.CountStrings);
        }

        [TestMethod()]
        public void LoadOptimizedTest()
        {
            var locres = TestHelper.LoadTestFile("optimized.locres");
            Console.WriteLine(locres.CountStrings);
        }

        [TestMethod()]
        public void LoadOptimizedCityhashTest()
        {
            var locres = TestHelper.LoadTestFile("optimized_cityhash.locres");
            Console.WriteLine(locres.CountStrings);
            Assert.AreEqual(LocResVersion.Optimized_CityHash64_UTF16, locres.Version);
        }

        [TestMethod()]
        public void SaveLegacyTest()
        {
            LoadAndSaveTest("legacy.locres", LocResVersion.Legacy);
        }

        [TestMethod()]
        public void SaveCompactTest()
        {
            LoadAndSaveTest("compact.locres", LocResVersion.Compact);
        }

        [TestMethod()]
        public void SaveOptimizedTest()
        {
            LoadAndSaveTest("optimized.locres", LocResVersion.Optimized);
        }

        [TestMethod()]
        public void SaveOptimizedCityHashTest()
        {
            LoadAndSaveTest("optimized_cityhash.locres", LocResVersion.Optimized_CityHash64_UTF16);
        }
    }
}