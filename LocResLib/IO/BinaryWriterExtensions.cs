using System.IO;
using System.Linq;
using System.Text;

namespace LocResLib.IO
{
    public static class BinaryWriterExtensions
    {
        /// <summary>
        /// Writes Unreal format string
        /// </summary>
        /// <param name="value">String to save</param>
        /// <param name="type">0 = ASCII, 1 = UNICODE, 2 = Forced ASCII</param>
        /// <returns></returns>
        internal static void WriteUnrealString(this BinaryWriter writer, string value, LocResEncoding type = LocResEncoding.Auto)
        {
            value += "\0";
            if (( type == LocResEncoding.Auto && value.All(ch => ch < 128) ) || type == LocResEncoding.ForceASCII)
            {
                // ASCII
                if (type == LocResEncoding.ForceASCII) {
                    // This strips all non-ASCII characters from a string
                    value = Encoding.ASCII.GetString(
                        Encoding.Convert(
                            Encoding.UTF8,
                            Encoding.GetEncoding(
                                Encoding.ASCII.EncodingName,
                                new EncoderReplacementFallback(string.Empty),
                                new DecoderExceptionFallback()
                                ),
                            Encoding.UTF8.GetBytes(value)
                        )
                    );
                }
                var data = Encoding.ASCII.GetBytes(value);
                writer.Write(data.Length);
                writer.Write(data);
            }
            else
            {
                // UCS2 as UTF-16-LE
                var data = Encoding.Unicode.GetBytes(value);
                writer.Write(-1 * value.Length);
                writer.Write(data);
            }
        }
    }
}
