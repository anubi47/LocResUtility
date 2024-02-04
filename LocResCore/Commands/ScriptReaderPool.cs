using System.Collections.Generic;
using System.Linq;

namespace LocResCore.Commands
{
    public class ScriptReaderPool : IScriptReaderPool
    {
        #region Private fields
        private readonly Dictionary<string, IScriptReader> _scriptReaderDic = new Dictionary<string, IScriptReader>();
        #endregion

        #region Interface methods
        public void RegisterReader(IScriptReader scriptReader)
        {
            foreach (string extension in scriptReader.Extensions)
            {
                _scriptReaderDic[extension] = scriptReader;
            }
        }
        public IEnumerable<string> SupportedExtensions()
        {
            return _scriptReaderDic.Keys.ToList();
        }
        public bool TryGetReader(string extension, out IScriptReader scriptReader)
        {
            return _scriptReaderDic.TryGetValue(extension, out scriptReader);
        }
        #endregion
    }
}
