using System.Collections.Generic;

namespace LocResCore.Translations
{
    public interface ITranslationWriterPool
    {
        /// <summary>
        /// Get all the supported extensions.
        /// </summary>
        /// <returns>A list of extensions.</returns>
        IEnumerable<string> SupportedExtensions();
        /// <summary>
        /// Register a writer inside the Pool.
        /// </summary>
        /// <param name="writer">The writer.</param>
        void RegisterWriter(ITranslationWriter writer);
        /// <summary>
        /// Try get the writer for the given extension.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <param name="writer">The writer.</param>
        /// <returns>True if writer is found; False otherwise.</returns>
        bool TryGetWriter(string extension, out ITranslationWriter writer);
    }
}
