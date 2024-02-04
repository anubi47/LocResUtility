using System;
using System.Collections.Generic;
using System.Text;

namespace LocResCore.Commands
{
    public interface IScriptWriterPool
    {
        /// <summary>
        /// Get all the supported extensions.
        /// </summary>
        /// <returns>A list of extensions.</returns>
        IEnumerable<string> SupportedExtensions();
        /// <summary>
        /// Register a script writer inside the Pool.
        /// </summary>
        /// <param name="writer">The writer.</param>
        void RegisterWriter(IScriptWriter writer);
        /// <summary>
        /// Try get the script writer for the given extension.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <param name="writer">The writer.</param>
        /// <returns>True if writer is found; False otherwise.</returns>
        bool TryGetWriter(string extension, out IScriptWriter writer);
    }
}
