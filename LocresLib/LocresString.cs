using System;
using LocResLib.Hashing;

namespace LocResLib
{
    public class LocResString
    {
        /// <summary>
        /// The key.
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// The value.
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// The <see cref="Crc.StrCrc32"/> hash of original value.
        /// </summary>
        public uint Hash { get; }
        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="hash">The <see cref="Crc.StrCrc32"/> hash of original value.</param>
        public LocResString(string key, string value, uint hash)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Hash = hash;
        }
    }
}
