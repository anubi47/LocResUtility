using LocResCore.Translations;

namespace LocResCore.Commands
{
    public interface ICommand
    {
        void Apply(TranslationItem item);
        void Apply(TranslationPool pool);
    }
}
