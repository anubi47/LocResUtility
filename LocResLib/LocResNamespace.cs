using System;
using System.Collections;
using System.Collections.Generic;

namespace LocResLib
{
    public class LocResNamespace : IEnumerable<LocResString>
    {
        private readonly string _name;
        private readonly Dictionary<string, LocResString> _stringDic;

        public string Name
        {
            get {  return _name; }
        }

        public int Count
        {
            get { return _stringDic.Count; }
        }

        public LocResNamespace(string name)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            _stringDic = new Dictionary<string, LocResString>();
            _name = name;
        }

        public void Add(LocResString locresString)
        {
            if (locresString is null)
                throw new ArgumentNullException(nameof(locresString));

            _stringDic[locresString.Key] = locresString;
        }

        public void Clear()
        {
            _stringDic.Clear();
        }

        public bool TryGetString(string key, out LocResString locresString)
        {
            return _stringDic.TryGetValue(key, out locresString);
        }

        public IEnumerator<LocResString> GetEnumerator()
        {
            return _stringDic.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
