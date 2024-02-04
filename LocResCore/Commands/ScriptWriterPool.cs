using System.Collections.Generic;
using System.Linq;

namespace LocResCore.Commands
{
    public class ScriptWriterPool : IScriptWriterPool
    {
        #region Private fields
        private readonly Dictionary<string, IScriptWriter> _scriptWriterDic = new Dictionary<string, IScriptWriter>();
        #endregion

        #region Interface methods
        public void RegisterWriter(IScriptWriter scriptWriter)
        {
            foreach (string extension in scriptWriter.Extensions)
            {
                _scriptWriterDic[extension] = scriptWriter;
            }
        }
        public IEnumerable<string> SupportedExtensions()
        {
            return _scriptWriterDic.Keys.ToList();
        }
        public bool TryGetWriter(string extension, out IScriptWriter scriptWriter)
        {
            return _scriptWriterDic.TryGetValue(extension, out scriptWriter);
        }
        #endregion
    }
}
