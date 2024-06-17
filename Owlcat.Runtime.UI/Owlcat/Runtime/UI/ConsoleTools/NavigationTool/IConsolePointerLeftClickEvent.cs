using UniRx;

namespace Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

public interface IConsolePointerLeftClickEvent : IConsoleNavigationEntity, IConsoleEntity
{
	ReactiveCommand PointerLeftClickCommand { get; }
}
