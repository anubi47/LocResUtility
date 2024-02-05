using LocResLib.Hashing;
using LocResLib.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LocResLib
{
    public class LocResFile : IEnumerable<LocResNamespace>
    {
        private static byte[] LOCRES_MAGIC = new byte[]{
            0x0E, 0x14, 0x74, 0x75, 0x67, 0x4A, 0x03, 0xFC, 0x4A, 0x15, 0x90, 0x9D, 0xC3, 0x37, 0x7F, 0x1B
        };

        private readonly Dictionary<string, LocResNamespace> _namespaceDic = new Dictionary<string, LocResNamespace>();

        public int Count
        {
            get { return _namespaceDic.Count; }
        }

        public int CountNamespaces
        {
            get { return _namespaceDic.Count; }
        }

        public int CountStrings
        {
            get
            {
                int count = 0;
                foreach (var lrNamespace in _namespaceDic)
                {
                    count += lrNamespace.Value.Count;
                }
                return count;
            }
        }

        public LocResVersion Version
        {
            get; private set;
        }

        public void Add(LocResNamespace locresNameSpace)
        {
            if (locresNameSpace is null)
                throw new ArgumentNullException(nameof(locresNameSpace));

            _namespaceDic.Add(locresNameSpace.Name, locresNameSpace);
        }

        public void Clear()
        {
            _namespaceDic.Clear();
        }

        public bool ContainsNamespace(string name)
        {
            return _namespaceDic.ContainsKey(name);
        }

        public void Remove(LocResNamespace locresNamespace)
        {
            if (locresNamespace is null)
                throw new ArgumentNullException(nameof(locresNamespace));

            _namespaceDic.Remove(locresNamespace.Name);
        }

        public bool TryGetNamespace(string name, out LocResNamespace locresNamespace)
        {
            return _namespaceDic.TryGetValue(name, out locresNamespace);
        }

        #region Stream

        public void Load(Stream stream)
        {
            if (!stream.CanSeek)
                throw new ArgumentException("Stream must be seekable.");

            if (!stream.CanRead)
                throw new ArgumentException("Stream must be readable.");

            Clear();

            using (var reader = new BinaryReader(stream))
            {
                byte[] magic = reader.ReadBytes(0x10);

                if (LOCRES_MAGIC.SequenceEqual(magic))
                {
                    Version = (LocResVersion)reader.ReadByte();
                }
                else
                {
                    Version = LocResVersion.Legacy;
                    reader.BaseStream.Position = 0;
                }

                string[] localizedStringArray = null;

                if (Version >= LocResVersion.Compact)
                {
                    long localizedStringArrayOffset = reader.ReadInt64();
                    long tempOffset = reader.BaseStream.Position;
                    reader.BaseStream.Position = localizedStringArrayOffset;

                    int localizedStringCount = reader.ReadInt32();
                    localizedStringArray = new string[localizedStringCount];

                    if (Version >= LocResVersion.Optimized)
                    {
                        for (int i = 0; i < localizedStringCount; i++)
                        {
                            localizedStringArray[i] = reader.ReadUnrealString();
                            reader.ReadInt32(); //refCount
                        }
                    }
                    else
                    {
                        for (int i = 0; i < localizedStringCount; i++)
                        {
                            localizedStringArray[i] = reader.ReadUnrealString();
                        }
                    }

                    reader.BaseStream.Position = tempOffset;
                }

                if (Version >= LocResVersion.Optimized)
                    reader.ReadInt32(); // entriesCount

                int namespaceCount = reader.ReadInt32();

                for (int i = 0; i < namespaceCount; i++)
                {
                    if (Version >= LocResVersion.Optimized)
                        reader.ReadUInt32(); // namespaceKeyHash

                    string namespaceKey = reader.ReadUnrealString();

                    int keyCount = reader.ReadInt32();

                    var ns = new LocResNamespace(namespaceKey);

                    for (int j = 0; j < keyCount; j++)
                    {
                        uint stringKeyHash;
                        if (Version >= LocResVersion.Optimized)
                        {
                            stringKeyHash = reader.ReadUInt32();
                        }

                        string stringKey = reader.ReadUnrealString();
                        uint sourceStringHash = reader.ReadUInt32();

                        string localizedString;

                        if (Version >= LocResVersion.Compact)
                        {
                            int stringIndex = reader.ReadInt32();
                            localizedString = localizedStringArray[stringIndex];
                        }
                        else
                        {
                            localizedString = reader.ReadUnrealString();
                        }

                        ns.Add(new LocResString(stringKey, localizedString, sourceStringHash));
                    }

                    Add(ns);
                }
            }
        }

        public void Save(Stream stream, LocResVersion outputVersion = LocResVersion.Compact, LocResEncoding outputEncoding = LocResEncoding.Auto)
        {
            if (!stream.CanSeek)
                throw new ArgumentException("Stream must be seekable.");

            if (!stream.CanWrite)
                throw new ArgumentException("Stream must be writeable.");

            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                if (outputVersion == LocResVersion.Legacy)
                {
                    SaveLegacy(writer);
                    return;
                }

                writer.Write(LOCRES_MAGIC);                  // byte LOCRES_MAGIC[16]
                writer.Write((byte)outputVersion);           // byte version
                long arrayOffset = writer.BaseStream.Position;
                writer.Write((long)0);                       // long localizedStringArrayOffset

                if (outputVersion >= LocResVersion.Optimized)
                    writer.Write(0); // int localizedStringEntryCount

                writer.Write(Count); // int namespaceCount

                var stringTable = new List<StringTableEntry>();
                int localizedStringEntryCount = 0;

                foreach (var localizationNamespace in this)
                {
                    if (outputVersion == LocResVersion.Optimized_CityHash64_UTF16)
                        writer.Write(CityHash64_utf16_to_uint32(localizationNamespace.Name));
                    else if (outputVersion >= LocResVersion.Optimized)
                        writer.Write(Crc.StrCrc32(localizationNamespace.Name));

                    writer.WriteUnrealString(localizationNamespace.Name);
                    writer.Write(localizationNamespace.Count); // int localizaedStringCounnt

                    foreach (var localizedString in localizationNamespace)
                    {
                        if (outputVersion == LocResVersion.Optimized_CityHash64_UTF16)
                            writer.Write(CityHash64_utf16_to_uint32(localizedString.Key));
                        else if (outputVersion == LocResVersion.Optimized)
                            writer.Write(Crc.StrCrc32(localizedString.Key));

                        writer.WriteUnrealString(localizedString.Key);
                        writer.Write(localizedString.Hash);

                        int stringTableIndex = stringTable.FindIndex(x => x.Text == localizedString.Value);

                        if (stringTableIndex == -1)
                        {
                            stringTableIndex = stringTable.Count;
                            stringTable.Add(new StringTableEntry() { Text = localizedString.Value, RefCount = 1 });
                        }
                        else
                        {
                            stringTable[stringTableIndex].RefCount += 1;
                        }

                        writer.Write(stringTableIndex);
                        localizedStringEntryCount += 1;
                    }
                }

                long stringTableOffset = writer.BaseStream.Position;

                writer.Write(stringTable.Count);

                if (outputVersion >= LocResVersion.Optimized)
                {
                    foreach (var entry in stringTable)
                    {
                        writer.WriteUnrealString(entry.Text, outputEncoding);
                        writer.Write(entry.RefCount);
                    }
                }
                else
                {
                    foreach (var entry in stringTable)
                    {
                        writer.WriteUnrealString(entry.Text, outputEncoding);
                    }
                }

                writer.BaseStream.Position = arrayOffset;
                writer.Write(stringTableOffset); // long localizedStringArrayOffset

                if (outputVersion >= LocResVersion.Optimized)
                    writer.Write(localizedStringEntryCount);

                stream.Seek(0, SeekOrigin.End);
            }
        }

        private void SaveLegacy(BinaryWriter writer)
        {
            writer.Write(Count); // int namespaceCount

            foreach (var localizationNamespace in this)
            {
                writer.WriteUnrealString(localizationNamespace.Name, LocResEncoding.UTF16);
                writer.Write(localizationNamespace.Count);

                foreach (var localizedString in localizationNamespace)
                {
                    writer.WriteUnrealString(localizedString.Key);
                    writer.Write(localizedString.Hash);
                    writer.WriteUnrealString(localizedString.Value);
                }
            }
        }

        /// <summary>
        ///     Encode string with UTF-16-LE, calculate CityHash64 and get uint32 hash of cityhash.<br/>
        ///     uint64 to uint32 hash function: <br/>
        ///         https://github.com/EpicGames/UnrealEngine/blob/release/Engine/Source/Runtime/Core/Public/Templates/TypeHash.h#L81
        /// </summary>
        /// <param name="s">Input string</param>
        /// <returns>uint32 hash of CityHash64 hash of input string</returns>
        private static uint CityHash64_utf16_to_uint32(string s)
        {
            if (s.Length == 0)
                return 0;

            byte[] b = Encoding.Unicode.GetBytes(s);
            ulong h = CityHash.CityHash64(b);
            uint r = (uint)h + ((uint)(h >> 32) * 23);
            return r;
        }

        private sealed class StringTableEntry
        {
            public string Text { get; set; }
            public int RefCount { get; set; }
        }

        #endregion

        #region Enumerator

        public IEnumerator<LocResNamespace> GetEnumerator()
        {
            return _namespaceDic.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

    }
}
