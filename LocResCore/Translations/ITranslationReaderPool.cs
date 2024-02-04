using System.Collections.Generic;

namespace LocResCore.Translations
{
    public interface ITranslationReaderPool
    {
        /// <summary>
        /// Get all the supported extensions.
        /// </summary>
        /// <returns>A list of extensions.</returns>
        IEnumerable<string> SupportedExtensions();
        /// <summary>
        /// Register a reader inside the Pool.
        /// </summary>
        /// <param name="reader">The reader.</param>
        void RegisterReader(ITranslationReader reader);
        /// <summary>
        /// Try get the reader for the given extension.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <param name="reader">The reader.</param>
        /// <returns>True if reader is found; False otherwise.</returns>
        bool TryGetReader(string extension, out ITranslationReader reader);
    }
}
