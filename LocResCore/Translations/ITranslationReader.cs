using LocResCore.Files;

namespace LocResCore.Translations
{
    public interface ITranslationReader : IExtensions
    {
        /// <summary>
        /// Read translation.
        /// </summary>
        /// <param name="pool">The translation pool.</param>
        /// <param name="filePath">The file path.</param>
        void Read(out TranslationPool pool, string filePath);
        /// <summary>
        /// Read translation.
        /// </summary>
        /// <param name="item">The translation item.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="culture">The culture.</param>
        void Read(out TranslationItem item, string filePath, string culture);
    }
}
