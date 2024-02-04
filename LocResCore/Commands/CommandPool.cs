using LocResCore.Translations;
using System;
using System.Collections.Generic;

namespace LocResCore.Commands
{
    public class CommandPool : List<ICommand>, ICommand
    {
        public void Apply(TranslationItem item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));
            foreach (var command in this)
            {
                command.Apply(item);
            }
        }
        public void Apply(TranslationPool pool)
        {
            if (pool is null)
                throw new ArgumentNullException(nameof(pool));
            foreach (TranslationItem item in pool)
            {
                Apply(item);
            }
        }
    }
}
