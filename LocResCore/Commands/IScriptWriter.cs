using LocResCore.Files;

namespace LocResCore.Commands
{
    public interface IScriptWriter : IExtensions
    {
        /// <summary>
        /// Write the script.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="commandPool">The command pool.</param>
        void Write(string filePath, CommandPool commandPool);
    }
}
