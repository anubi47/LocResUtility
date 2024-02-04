using System.Collections.Generic;

namespace LocResCore.Commands
{
    public interface IScriptReaderPool
    {
        /// <summary>
        /// Get all the supported extensions.
        /// </summary>
        /// <returns>A list of extensions.</returns>
        IEnumerable<string> SupportedExtensions();
        /// <summary>
        /// Register a script reader inside the Pool.
        /// </summary>
        /// <param name="reader">The reader.</param>
        void RegisterReader(IScriptReader reader);
        /// <summary>
        /// Try get the script reader for the given extension.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <param name="reader">The reader.</param>
        /// <returns>True if reader is found; False otherwise.</returns>
        bool TryGetReader(string extension, out IScriptReader reader);
    }
}
