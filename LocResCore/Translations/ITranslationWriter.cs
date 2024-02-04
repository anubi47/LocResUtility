using LocResCore.Files;

namespace LocResCore.Translations
{
    public interface ITranslationWriter : IExtensions
    {
        /// <summary>
        /// Write translation.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="pool">The translation pool.</param>
        void Write(string filePath, TranslationPool pool);
        /// <summary>
        /// Write translation.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="item">The translation item.</param>
        void Write(string filePath, TranslationItem item);
    }
}
