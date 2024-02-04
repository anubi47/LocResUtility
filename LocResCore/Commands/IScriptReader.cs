using LocResCore.Files;

namespace LocResCore.Commands
{
    public interface IScriptReader : IExtensions
    {
        /// <summary>
        /// Read the script.
        /// </summary>
        /// <param name="commandPool">The command pool.</param>
        /// <param name="filePath">The file path.</param>
        void Read(out CommandPool commandPool, string filePath);
    }
}
